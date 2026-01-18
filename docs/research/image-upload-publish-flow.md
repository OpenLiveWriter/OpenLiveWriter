# Image Upload/Publish Flow - End-to-End Documentation

## Executive Summary

This document maps out the complete image upload and publish flow in OpenLiveWriter, from initial image insertion through final publishing to the blog.

**Date:** January 2026  
**Research Task:** Map out the image upload/publish flow end-to-end

---

## High-Level Overview

The image upload/publish flow in OpenLiveWriter consists of several distinct phases:

```
┌─────────────────────────────────────────────────────────────────┐
│                    IMAGE LIFECYCLE                              │
├─────────────────────────────────────────────────────────────────┤
│  1. INSERTION      → User adds image to post                    │
│  2. REGISTRATION   → Image registered with supporting files     │
│  3. PROCESSING     → Image resized, decorated, optimized        │
│  4. STORAGE        → Files stored in temp supporting directory  │
│  5. EDITING        → User can modify decorators, size, etc.     │
│  6. PUBLISHING     → Files uploaded to blog/FTP server          │
│  7. URL FIXING     → Local URLs replaced with server URLs       │
│  8. FINALIZATION   → Post published, URLs persisted             │
└─────────────────────────────────────────────────────────────────┘
```

---

## Phase 1: Image Insertion

### Entry Points

**User Actions:**
1. **Insert > Picture** menu command
2. **Drag & drop** image file onto editor
3. **Paste** image from clipboard
4. **Insert from web** (URL entry)

**Command Infrastructure:**
- `CommandInsertPicture` - Main insert command
- Menu path: Insert > Picture
- Triggers `InsertImageDialog`

### InsertImageDialog

**Location:** `src/managed/OpenLiveWriter.PostEditor/ImageInsertion/`

**Image Source Tabs:**
1. **From File** - Local file browser
2. **From Web** - URL entry for web images
3. **From Service** - Image service integration (if configured)

**Selection Process:**
```
User selects image source
   ↓
User chooses image file/URL
   ↓
Dialog validates image
   ├─ Check file exists
   ├─ Verify image format (JPEG, PNG, GIF, etc.)
   ├─ Check file size
   └─ Validate dimensions
   
   ↓
Image info returned to editor
   ├─ File path or URL
   ├─ Initial dimensions
   └─ Image format
```

### Initial HTML Insertion

**Process:**
1. Dialog returns image selection to editor
2. Editor inserts basic `<img>` tag into HTML:
   ```html
   <img src="file:///C:/path/to/image.jpg" />
   ```
3. Image displayed in editor with temporary local path
4. Image marked for initialization (no `BlogPostImageData` yet)

---

## Phase 2: Image Registration & Initialization

### ImageInsertionManager

**Location:** `src/managed/OpenLiveWriter.PostEditor/ImageInsertion/ImageInsertionManager.cs`

**Purpose:** Scans editor for new images and initializes them.

**Process:**

```csharp
ScanAndInitializeNewImages()
   ├─ Scan HTML DOM for <img> tags
   ├─ Identify uninitialized images (no associated BlogPostImageData)
   ├─ Create NewImageInfo for each new image
   ├─ Determine image properties (dimensions, format, classification)
   └─ Queue for async initialization
```

**NewImageInfo Structure:**
- Source image path/URL
- Target HTML element
- Initial dimensions
- Image classification (animated GIF, transparent PNG, etc.)
- Default decorators to apply

### Async Initialization

**ImageInitializationAsyncOperation** (background thread)

**Steps for Each Image:**

#### 1. Register Source Image

```csharp
ISupportingFile sourceFile = 
    supportingFileService.AddLinkedSupportingFileReference(
        imageFilePath
    );
```

- Creates `ISupportingFile` for original image
- File relationship: `ImageFileRelationship.Source`
- File stored/referenced in supporting files system
- Returns file ID and metadata

#### 2. Create Shadow File (Draft Copy)

**Purpose:** Downscaled copy for draft editing without affecting original.

```csharp
BlogPostImageData.InitShadowFile(supportingFileService)
   ├─ Load source image
   ├─ Resize to max 1280x960 (if larger)
   ├─ Preserve aspect ratio
   ├─ Save to temp storage as JPEG (quality 95%)
   └─ Register as ImageFileRelationship.SourceShadow
```

**When Used:**
- `GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ShadowImageForDrafts)` must be true
- Allows editing without accessing potentially slow/unavailable source
- Preserves quality for final publishing

#### 3. Create BlogPostImageData

**Master metadata object for the image:**

```csharp
BlogPostImageData imageData = new BlogPostImageData()
{
    ImageSourceFile = sourceImageFileData,
    ImageSourceShadowFile = shadowImageFileData,
    InlineImageFile = null,  // Created during processing
    LinkedImageFile = null,   // Created if linking enabled
    UploadInfo = new BlogPostImageServiceUploadInfo(),
    ImageDecoratorSettings = new BlogPostSettingsBag()
};
```

#### 4. Register with ImageDataList

```csharp
IBlogPostEditingContext.ImageDataList.AddImage(
    imageId,
    imageData
);
```

- Image ID derived from `<img>` element ID
- ImageDataList maintains all post images
- Persisted with post file

---

## Phase 3: Image Processing & Transformation

### ImageInsertHandler

**Location:** `src/managed/OpenLiveWriter.PostEditor/ImageInsertion/ImageInsertHandler.cs`

**Purpose:** Applies decorators, creates web-optimized versions.

### Processing Flow

```
BlogPostImageData (source)
   ↓
Apply Image Decorators
   ├─ HtmlImageResizeDecorator (resize/scale)
   ├─ ImageBorderDecorator (borders, shadows)
   ├─ ImageSliceBorderDecorator (custom border images)
   ├─ HtmlImageTargetDecorator (link targets)
   └─ Custom decorators (plugins)
   
   ↓
Generate Inline Image (web-optimized)
   ├─ Load source or shadow file
   ├─ Apply decorators (resize, border, effects)
   ├─ Optimize for web (quality settings)
   ├─ Save to temp storage
   └─ Create ImageFileData (ImageFileRelationship.Inline)
   
   ↓
Generate Linked Image (optional, full-size)
   ├─ Load source file
   ├─ Apply minimal decorators (if any)
   ├─ Save to temp storage
   └─ Create ImageFileData (ImageFileRelationship.Linked)
   
   ↓
Update HTML
   ├─ <img src="temp://inline_image.jpg" />
   ├─ Wrapped in <a href="temp://linked_image.jpg"> if linking
   └─ Apply decorator HTML (border divs, etc.)
```

### Decorator System

**ImageDecorator Base Class:**
```csharp
abstract class ImageDecorator
{
    // Modify image bitmap
    Bitmap Decorate(Bitmap source, ImageDecoratorContext context);
    
    // Modify HTML output
    string DecorateHtml(string imgHtml, ImageDecoratorContext context);
}
```

**Common Decorators:**

1. **HtmlImageResizeDecorator**
   - Resizes image to target dimensions
   - Maintains aspect ratio
   - Quality settings for JPEG compression

2. **ImageBorderDecorator**
   - Adds borders around image
   - Shadow effects
   - Border color, width, style

3. **HtmlImageTargetDecorator**
   - Wraps `<img>` in `<a>` tag
   - Target options: `_blank`, `_self`, none
   - Click-through to full-size image

4. **ImageTiltDecorator** (example plugin)
   - Rotates image slightly
   - Polaroid-style effect

**Decorator Settings Persistence:**
- Settings stored in `BlogPostImageData.ImageDecoratorSettings`
- `BlogPostSettingsBag` (key-value pairs)
- XML serialization
- Allows re-applying decorators if source changes

### Image Format Optimization

**ImageHelper2.GetImageFormat()** determines format:

| Source Format | Inline Format | Reasoning |
|---------------|---------------|-----------|
| JPEG          | JPEG          | Already compressed |
| PNG (opaque)  | JPEG          | Smaller file size |
| PNG (transparent) | PNG       | Preserve transparency |
| GIF (static)  | JPEG or PNG   | Better compression |
| GIF (animated)| GIF           | Preserve animation |
| BMP           | JPEG          | Reduce file size |

**Quality Settings:**
- JPEG quality: 85-95 (user configurable)
- PNG: Optimal compression
- GIF: Original (no recompression)

---

## Phase 4: Supporting File Storage

### ISupportingFileService

**Interface:** `src/managed/OpenLiveWriter.PostEditor/`

**Purpose:** Manages all supporting files for a post (images, videos, attachments).

**Key Methods:**

```csharp
interface ISupportingFileService
{
    // Create new embedded file
    ISupportingFile CreateSupportingFile(string filename, Stream content);
    
    // Reference existing file (linked, not embedded)
    ISupportingFile AddLinkedSupportingFileReference(string filePath);
    
    // Retrieve by URI or ID
    ISupportingFile GetFileByUri(Uri uri);
    ISupportingFile GetFileById(string fileId);
    
    // Upload tracking
    void MarkFileUploaded(string fileId, string destinationContext, Uri uploadedUri);
}
```

### ISupportingFile

**Represents a single supporting file:**

```csharp
interface ISupportingFile
{
    string FileId { get; }           // Unique identifier
    string FileName { get; }         // Display filename
    int FileVersion { get; }         // Version number
    bool Embedded { get; }           // Embedded vs. linked
    
    Uri FileUri { get; }             // Local file URI
    
    // Upload tracking per destination
    ISupportingFileUploadInfo GetUploadInfo(string destinationContext);
    void MarkUploaded(string destinationContext, Uri uploadedUri);
    
    // Metadata storage
    BlogPostSettingsBag Settings { get; }
}
```

### BlogPostSupportingFileStorage

**Location:** `src/managed/OpenLiveWriter.PostEditor/BlogPostSupportingFileStorage.cs`

**Purpose:** Physical storage manager for supporting files.

**Directory Structure:**
```
%TEMP%\OpenLiveWriter\Posts\{post-id}\
   ├─ Files\
   │  ├─ image_001.jpg          (source)
   │  ├─ image_001_shadow.jpg   (shadow copy)
   │  ├─ image_001_inline.jpg   (web-optimized)
   │  └─ image_001_linked.jpg   (full-size)
   └─ SupportingFiles.xml       (metadata)
```

**Key Operations:**

```csharp
class BlogPostSupportingFileStorage
{
    // Create unique filename
    string CreateFile(string suggestedName);
    
    // Add file with content
    string AddFile(Stream content, string suggestedName);
    
    // Get all files referenced in post HTML
    ISupportingFile[] GetSupportingFilesInPost(string html);
    
    // Storage path
    string StoragePath { get; }
}
```

### Upload Tracking

**ISupportingFileUploadInfo:**
- Key: `destinationContext` (blog ID or FTP URL)
- Value: `uploadedUri` (server URL)
- Prevents re-uploading same file

**Example:**
```csharp
supportingFile.MarkUploaded(
    destinationContext: "blog-123",
    uploadedUri: new Uri("https://blog.com/wp-content/uploads/image.jpg")
);

// Later, check if already uploaded:
var uploadInfo = supportingFile.GetUploadInfo("blog-123");
if (uploadInfo != null && uploadInfo.IsUploaded)
{
    // Skip upload, use existing URL
}
```

---

## Phase 5: Publishing - File Upload

### UpdateWeblogAsyncOperation

**Location:** `src/managed/OpenLiveWriter.PostEditor/`

**Purpose:** Coordinates post publishing and file uploads.

**Publishing Flow:**

```
User clicks "Publish"
   ↓
UpdateWeblogAsyncOperation.DoWork()
   ├─ Validate post (title, content)
   ├─ Get publishing context
   └─ Execute publish workflow
      ├─ Upload files BEFORE publish
      ├─ Publish post to blog API
      └─ Upload files AFTER publish (optional)
```

### LocalSupportingFileUploader

**Purpose:** Temporarily modifies post content for uploads, then restores.

**Process:**

```csharp
using (LocalSupportingFileUploader uploader = 
       new LocalSupportingFileUploader(blogPost, supportingFileService))
{
    // 1. Upload files before publishing
    uploader.UploadFilesBeforePublish(blogFileUploader, progressHost);
    
    // 2. Post content now has server URLs
    blog.NewPost(blogPost, publishingContext);
    
    // 3. Upload remaining files after publishing
    uploader.UploadFilesAfterPublish(blogFileUploader, postId, progressHost);
}
// 4. Original content restored on dispose
```

**Key Responsibilities:**
- Parse HTML for file references
- Coordinate with BlogFileUploader
- Replace local URLs with server URLs
- Restore original content after publishing

### BlogPostReferenceFixer

**Purpose:** Parses HTML to find file references that need uploading.

**Process:**

```csharp
class BlogPostReferenceFixer : LightWeightHTMLDocumentIterator
{
    protected override void OnBeginTag(BeginTag tag)
    {
        if (tag.NameEquals("img"))
        {
            // Extract src attribute
            string src = tag.GetAttributeValue("src");
            
            // Find corresponding ISupportingFile
            ISupportingFile file = supportingFileService.GetFileByUri(src);
            
            // Add to upload list
            filesToUpload.Add(file);
        }
        
        // Similar for <a href>, <video src>, etc.
    }
}
```

**Generates:**
- `SupportingFileReferenceList` - Files to upload
- `FileUploadWorker` - Upload coordinator

### FileUploadWorker

**Purpose:** Processes list of files to upload.

**Process:**

```csharp
class FileUploadWorker
{
    public void UploadFiles(
        BlogFileUploader uploader,
        SupportingFileReferenceList files,
        IProgressHost progress)
    {
        foreach (var fileRef in files)
        {
            // Check if already uploaded
            var uploadInfo = fileRef.File.GetUploadInfo(uploader.DestinationContext);
            if (uploadInfo != null && uploadInfo.IsUploaded)
            {
                // Use existing URL
                fileRef.UploadedUri = uploadInfo.UploadedUri;
                continue;
            }
            
            // Upload the file
            Uri uploadedUri = uploader.DoUploadWorkBeforePublish(
                new FileUploadContext(fileRef.File)
            );
            
            // Record upload
            fileRef.File.MarkUploaded(uploader.DestinationContext, uploadedUri);
            fileRef.UploadedUri = uploadedUri;
            
            // Update progress
            progress.UpdateProgress(currentFile, totalFiles);
        }
    }
}
```

---

## Phase 6: BlogFileUploader Implementations

### Base Class: BlogFileUploader

**Location:** `src/managed/OpenLiveWriter.PostEditor/BlogFileUploader.cs`

**Abstract Methods:**

```csharp
abstract class BlogFileUploader : IDisposable
{
    // Upload before post is published
    abstract Uri DoUploadWorkBeforePublish(IFileUploadContext context);
    
    // Upload after post is published (optional)
    virtual void DoUploadWorkAfterPublish(IFileUploadContext context) { }
    
    // Format server filename
    abstract string FormatUploadFileName(string filename, string conflictToken);
    
    // Connection management
    virtual void Connect() { }
    virtual void Disconnect() { }
}
```

**Filename Formatting:**

Template variables:
- `{FileName}` - Original filename
- `{FileNameWithoutExtension}` - Name without extension
- `{FileExtension}` - Extension only
- `{PostTitle}` - Post title (sanitized)
- `{PostRandomizer}` - Unique post ID component
- `{UploadDate}` - Upload date with custom format
- `{Randomizer}` - Short GUID
- `{FileNameConflictToken}` - Conflict resolution token
- `{OpenLiveWriter}` - Application name

**Example:**
```
Format: "{PostTitle}/{FileName}"
Result: "my-blog-post/vacation-photo.jpg"
```

### WeblogBlogFileUploader

**Purpose:** Upload via blog API (XML-RPC, ATOM, etc.)

**Implementation:**

```csharp
class WeblogBlogFileUploader : BlogFileUploader
{
    public override Uri DoUploadWorkBeforePublish(IFileUploadContext context)
    {
        // Format server filename
        string serverFileName = FormatUploadFileName(
            context.FileName,
            context.ConflictToken
        );
        
        // Read file content
        byte[] fileContent = File.ReadAllBytes(context.LocalFilePath);
        
        // Upload via blog client
        string uploadedUrl = blogClient.NewMediaObject(
            blogId,
            serverFileName,
            context.MimeType,
            fileContent
        );
        
        return new Uri(uploadedUrl);
    }
}
```

**Blog Client Integration:**
- `IBlogClient.NewMediaObject()` - Upload new media
- `IBlogClient.DoBeforePublishUploadWork()` - Batch upload
- Returns absolute or relative URL
- URL resolution handled by client

**Supported APIs:**
- XML-RPC `metaWeblog.newMediaObject`
- ATOM Publishing Protocol media upload
- Custom provider APIs (Blogger, WordPress, etc.)

### FTPBlogFileUploader

**Purpose:** Upload via FTP to separate file server.

**Implementation:**

```csharp
class FTPBlogFileUploader : BlogFileUploader
{
    private FileDestination ftpDestination;
    
    public override void Connect()
    {
        // Create FTP connection
        ftpDestination = new WinInetFTPFileDestination(
            ftpUrl,
            credentials
        );
        ftpDestination.Connect();
    }
    
    public override Uri DoUploadWorkBeforePublish(IFileUploadContext context)
    {
        // Format server path
        string serverPath = FormatUploadFileName(
            context.FileName,
            context.ConflictToken
        );
        
        // Upload file
        ftpDestination.UploadFile(
            context.LocalFilePath,
            serverPath
        );
        
        // Map FTP path to HTTP URL
        Uri httpUrl = urlMapping.MapFtpPathToUrl(serverPath);
        
        return httpUrl;
    }
}
```

**FTP Features:**
- Directory creation (auto-create paths)
- Conflict resolution (overwrite or rename)
- Binary/ASCII mode detection
- Passive/active mode support
- SSL/TLS support (FTPS)

**URL Mapping:**
- FTP path: `/public_html/images/photo.jpg`
- HTTP URL: `https://example.com/images/photo.jpg`
- Mapping defined in blog settings

### NullBlogFileUploader

**Purpose:** Throws exception when upload not supported.

```csharp
class NullBlogFileUploader : BlogFileUploader
{
    public override Uri DoUploadWorkBeforePublish(IFileUploadContext context)
    {
        throw new BlogClientFileUploadNotSupportedException(
            "Image upload is not supported for this blog."
        );
    }
}
```

### Factory Method

```csharp
static BlogFileUploader CreateFileUploader(
    BlogAccount account,
    IBlogClient client,
    BlogSettings settings)
{
    // Check if FTP upload configured
    if (settings.FtpUploadInfo != null)
    {
        return new FTPBlogFileUploader(
            settings.FtpUploadInfo.Url,
            settings.FtpUploadInfo.Credentials,
            settings.FtpUploadInfo.UrlMapping
        );
    }
    
    // Check if blog API supports upload
    if (client.SupportsFileUpload)
    {
        return new WeblogBlogFileUploader(
            client,
            account.BlogId,
            settings.FileUploadNameFormat
        );
    }
    
    // No upload support
    return new NullBlogFileUploader();
}
```

---

## Phase 7: Image Service Uploaders (Alternative)

### IImageServiceUploader

**Location:** `src/managed/OpenLiveWriter.Extensibility/ImageServices/`

**Purpose:** Plugin-based image hosting services (Flickr, Photobucket, etc.)

**Interface:**

```csharp
interface IImageServiceUploader
{
    void Connect();
    void Disconnect();
    IImageUploadResult[] UploadImages(IUploadImageContext context);
}
```

**Workflow:**

```
User configures image service (e.g., Flickr)
   ↓
Insert image with "Upload to Flickr" option
   ↓
BlogPostImageData.UploadInfo stores service ID
   ↓
During publishing:
   ├─ IImageService.CreateImageServiceUploader(settings)
   ├─ uploader.Connect()
   ├─ uploader.UploadImages(context)
   │  └─ Returns image URLs from service
   ├─ uploader.Disconnect()
   └─ URLs inserted into post HTML
```

### AtomMediaUploader

**Purpose:** ATOM Publishing Protocol media upload.

```csharp
class AtomMediaUploader : IImageServiceUploader
{
    public IImageUploadResult[] UploadImages(IUploadImageContext context)
    {
        var results = new List<IImageUploadResult>();
        
        foreach (var imageFile in context.UploadImageFiles)
        {
            // Build ATOM entry
            AtomEntry entry = new AtomEntry();
            entry.Title = imageFile.FilePath;
            entry.Content = new AtomContent();
            entry.Content.Type = GetMimeType(imageFile.FilePath);
            entry.Content.Value = File.ReadAllBytes(imageFile.FilePath);
            
            // POST to ATOM service
            AtomEntry posted = atomClient.CreateEntry(mediaCollectionUri, entry);
            
            // Extract media URL
            string imageUrl = posted.Content.Src;
            
            results.Add(new ImageUploadResult(imageUrl));
        }
        
        return results.ToArray();
    }
}
```

---

## Phase 8: URL Transformation

### HtmlReferenceFixer

**Purpose:** Replace local file URLs with server URLs in HTML.

**Process:**

```csharp
class HtmlReferenceFixer
{
    public string FixLocalFileReferences(
        string html,
        FileUploadWorker uploadWorker)
    {
        // Create transformer
        LocalFileTransformer transformer = new LocalFileTransformer(
            uploadWorker.UploadedFiles
        );
        
        // Parse HTML and transform URLs
        string fixedHtml = LightWeightHTMLDocument.Transform(
            html,
            transformer
        );
        
        return fixedHtml;
    }
}
```

### LocalFileTransformer

**Purpose:** Replaces local URLs with server URLs during HTML transformation.

```csharp
class LocalFileTransformer : LightWeightHTMLTransformer
{
    private Dictionary<Uri, Uri> urlMap;  // Local → Server
    
    protected override void OnBeginTag(BeginTag tag)
    {
        if (tag.NameEquals("img"))
        {
            string src = tag.GetAttributeValue("src");
            Uri localUri = new Uri(src);
            
            if (urlMap.ContainsKey(localUri))
            {
                // Replace with server URL
                Uri serverUri = urlMap[localUri];
                tag.SetAttributeValue("src", serverUri.ToString());
            }
        }
        
        base.OnBeginTag(tag);
    }
}
```

**Example Transformation:**

```html
<!-- Before -->
<img src="file:///C:/Temp/OpenLiveWriter/Posts/abc123/Files/photo.jpg" />
<a href="file:///C:/Temp/OpenLiveWriter/Posts/abc123/Files/photo_full.jpg">
    <img src="..." />
</a>

<!-- After -->
<img src="https://blog.com/wp-content/uploads/2026/01/photo.jpg" />
<a href="https://blog.com/wp-content/uploads/2026/01/photo_full.jpg">
    <img src="..." />
</a>
```

---

## Phase 9: Publishing the Post

### Blog.NewPost() / Blog.EditPost()

**Location:** `src/managed/OpenLiveWriter.BlogClient/Blog.cs`

**Process:**

```csharp
class Blog
{
    public string NewPost(
        BlogPost post,
        INewCategoryContext categoryContext,
        bool publish)
    {
        // Files already uploaded, URLs fixed
        
        // Validate post
        if (string.IsNullOrEmpty(post.Title))
            throw new BlogClientException("Post title is required");
        
        // Call blog API
        string postId = blogClient.NewPost(
            blogId,
            post,
            categoryContext,
            publish
        );
        
        // Record post result
        post.Id = postId;
        post.DatePublished = DateTime.Now;
        
        // Retrieve permalink if available
        if (blogClient.SupportsPermalinks)
        {
            post.Permalink = blogClient.GetPermalink(blogId, postId);
        }
        
        return postId;
    }
}
```

**Blog APIs:**
- XML-RPC: `metaWeblog.newPost` / `metaWeblog.editPost`
- ATOM: `POST` / `PUT` to collection URI
- Custom: Provider-specific implementations

### Post Result Recording

**After successful publish:**

```csharp
// Update post metadata
localPost.Id = publishedPostId;
localPost.DatePublished = DateTime.UtcNow;
localPost.Permalink = permalink;
localPost.ETag = etag;  // For conditional updates

// Save local post file
postEditorFile.SavePost(localPost);

// Update supporting file upload info
foreach (var file in supportingFiles)
{
    file.MarkUploaded(destinationContext, uploadedUri);
}

// Save supporting files metadata
supportingFileStorage.SaveMetadata();
```

---

## Complete Flow Diagram

```
┌────────────────────────────────────────────────────────────────────┐
│                     IMAGE UPLOAD & PUBLISH FLOW                    │
└────────────────────────────────────────────────────────────────────┘

USER ACTION: Insert Image
   ↓
InsertImageDialog
   ├─ From File
   ├─ From Web
   └─ From Service
   ↓
   ↓ Returns image path/URL
   ↓
Editor inserts <img src="file:///..." />
   ↓
   ↓
ImageInsertionManager.ScanAndInitializeNewImages()
   ↓
   ↓ For each new image:
   ↓
ImageInitializationAsyncOperation (async)
   ├─ Register source: ISupportingFileService.AddLinkedReference()
   │  └─ Creates ImageFileData (Source)
   ├─ Create shadow: BlogPostImageData.InitShadowFile()
   │  └─ Creates ImageFileData (SourceShadow)
   ├─ Create BlogPostImageData
   └─ Add to ImageDataList
   ↓
   ↓ Image ready for editing
   ↓
USER ACTION: Edit Image (optional)
   ├─ Resize
   ├─ Add border
   ├─ Add link
   └─ Apply decorators
   ↓
   ↓ Decorators applied via:
   ↓
ImageInsertHandler.WriteImages()
   ├─ Apply decorators to bitmap
   ├─ Generate inline image (web-optimized)
   │  └─ Creates ImageFileData (Inline)
   ├─ Generate linked image (optional, full-size)
   │  └─ Creates ImageFileData (Linked)
   └─ Update HTML with temp URLs
   ↓
   ↓ Files stored in:
   ↓
BlogPostSupportingFileStorage
   └─ %TEMP%\OpenLiveWriter\Posts\{post-id}\Files\
   ↓
   ↓ User continues editing
   ↓
USER ACTION: Publish Post
   ↓
UpdateWeblogAsyncOperation.DoWork()
   ↓
   ├─ Create LocalSupportingFileUploader
   │  ↓
   │  ├─ BlogPostReferenceFixer parses HTML
   │  │  └─ Creates SupportingFileReferenceList
   │  │     (all files referenced in post)
   │  ↓
   │  ├─ UploadFilesBeforePublish()
   │  │  ↓
   │  │  └─ FileUploadWorker.UploadFiles()
   │  │     ↓
   │  │     └─ For each file:
   │  │        ├─ Check if already uploaded
   │  │        │  └─ ISupportingFile.GetUploadInfo(destinationContext)
   │  │        ├─ If not uploaded:
   │  │        │  ├─ BlogFileUploader.DoUploadWorkBeforePublish()
   │  │        │  │  ├─ Format server filename
   │  │        │  │  ├─ Upload via API/FTP
   │  │        │  │  └─ Return server URL
   │  │        │  └─ ISupportingFile.MarkUploaded(context, url)
   │  │        └─ Update progress
   │  ↓
   │  ├─ HtmlReferenceFixer.FixLocalFileReferences()
   │  │  └─ LocalFileTransformer replaces URLs
   │  │     ├─ file:///... → https://blog.com/...
   │  │     └─ Updates post.Contents
   │  ↓
   ├─ Blog.NewPost(post) or Blog.EditPost(post)
   │  └─ blogClient.NewPost() - API call
   │     └─ Returns post ID
   ↓
   ├─ UploadFilesAfterPublish() (optional)
   │  └─ For files that need post-publish upload
   ↓
   └─ LocalSupportingFileUploader.Dispose()
      └─ Restores original post content
   ↓
   ↓
Post published successfully
   ├─ Post ID recorded
   ├─ Permalink retrieved
   ├─ Upload info persisted
   └─ Local post file updated
```

---

## Key Data Structures

### BlogPostImageData

```csharp
class BlogPostImageData
{
    // Source images
    ImageFileData ImageSourceFile;         // Original
    ImageFileData ImageSourceShadowFile;   // Draft copy
    
    // Generated images
    ImageFileData InlineImageFile;         // Web-optimized
    ImageFileData LinkedImageFile;         // Full-size link target
    
    // Upload metadata
    BlogPostImageServiceUploadInfo UploadInfo;
    
    // Decorator settings
    BlogPostSettingsBag ImageDecoratorSettings;
}
```

### ImageFileData

```csharp
class ImageFileData
{
    int Width;
    int Height;
    string FilePath;
    ImageFileRelationship Relationship;  // Source, Inline, Linked, etc.
    ISupportingFile SupportingFile;
    
    Uri GetPublishedUri(string destinationContext);
}
```

### ISupportingFile

```csharp
interface ISupportingFile
{
    string FileId;
    string FileName;
    int FileVersion;
    bool Embedded;
    Uri FileUri;
    
    ISupportingFileUploadInfo GetUploadInfo(string destinationContext);
    void MarkUploaded(string destinationContext, Uri uploadedUri);
    
    BlogPostSettingsBag Settings;
}
```

### BlogPostImageDataList

```csharp
class BlogPostImageDataList : IEnumerable<BlogPostImageData>
{
    void AddImage(string imageId, BlogPostImageData imageData);
    BlogPostImageData GetImageData(string imageId);
    void RemoveImage(string imageId);
    
    // Serialization
    string ToXml();
    static BlogPostImageDataList FromXml(string xml);
}
```

---

## Upload Filename Formatting

### Template System

**Default Format:**
```
{PostTitle}_{PostRandomizer}/{FileName}
```

**Available Variables:**

| Variable | Description | Example |
|----------|-------------|---------|
| `{FileName}` | Original filename | `vacation-photo.jpg` |
| `{FileNameWithoutExtension}` | Name without extension | `vacation-photo` |
| `{AsciiFileName}` | ASCII-safe filename | `vacation-photo.jpg` |
| `{FileExtension}` | Extension only | `.jpg` |
| `{PostTitle}` | Post title (sanitized) | `my-summer-vacation` |
| `{PostRandomizer}` | Unique post ID component | `abc123def` |
| `{UploadDate:yyyy-MM}` | Upload date with format | `2026-01` |
| `{Randomizer}` | Short GUID | `k7m9p2` |
| `{FileNameConflictToken}` | Conflict resolution | `_1`, `_2`, etc. |
| `{OpenLiveWriter}` | Application name | `OpenLiveWriter` |

**Examples:**

```
Format: "{PostTitle}/{FileName}"
Result: "my-summer-vacation/photo001.jpg"

Format: "{UploadDate:yyyy/MM}/{FileNameWithoutExtension}_{Randomizer}{FileExtension}"
Result: "2026/01/photo001_k7m9p2.jpg"

Format: "{PostTitle}/{FileNameWithoutExtension}{FileNameConflictToken}{FileExtension}"
On conflict: "my-post/image_1.jpg", "my-post/image_2.jpg"
```

---

## Performance Optimizations

### 1. Upload Tracking
- Prevents re-uploading files
- Per-destination tracking (same file, different blogs)
- Stored in `ISupportingFile.UploadInfo`

### 2. Shadow Files
- Avoids accessing slow/unavailable source files
- Downscaled for draft editing
- Original preserved for final publish

### 3. Async Initialization
- Image processing in background thread
- Editor remains responsive
- Progress feedback to user

### 4. Batch Uploads
- Multiple files uploaded in sequence
- Progress reporting
- Error handling per file

### 5. Lazy Decorator Application
- Decorators only applied when needed
- Settings stored, bitmaps regenerated
- Avoids storing multiple decorated versions

---

## Error Handling

### Upload Failures

**Scenarios:**
1. Network timeout
2. Authentication failure
3. Insufficient permissions
4. Disk quota exceeded
5. Invalid file format
6. File too large

**Handling:**

```csharp
try
{
    uploadedUri = uploader.DoUploadWorkBeforePublish(context);
}
catch (BlogClientFileUploadNotSupportedException ex)
{
    // Blog doesn't support upload
    DisplayMessage.Show(MessageId.FileUploadNotSupported);
    // Offer alternatives (FTP, image service)
}
catch (BlogClientAuthenticationException ex)
{
    // Authentication failed
    // Prompt for credentials
}
catch (WebException ex)
{
    // Network error
    DisplayMessage.Show(MessageId.FileUploadNetworkError);
    // Offer retry
}
catch (Exception ex)
{
    // General upload error
    Log.Error("File upload failed", ex);
    // Show error to user
}
```

**Partial Upload Recovery:**
- Already-uploaded files tracked
- Retry only failed files
- User can continue or abort

### Post-Publish File Upload

**Scenario:** Some files uploaded after post publish (rare).

**AfterPublishFileUploadFailedForm:**
- Shows list of files that failed
- Offers retry or ignore options
- Post already published, images may be broken
- User can manually fix or re-upload

---

## Security Considerations

### 1. File Validation
- Verify file format (magic bytes)
- Check file size limits
- Scan for malicious content (optional, via plugin)

### 2. URL Sanitization
- Prevent path traversal
- Validate server URLs before insertion
- Escape HTML attributes

### 3. FTP Security
- Credentials encrypted in storage
- SSL/TLS support (FTPS)
- No plaintext password logging

### 4. API Authentication
- Blog credentials securely stored
- OAuth tokens encrypted
- No credentials in HTML/URLs

### 5. Temp File Cleanup
- Temp files deleted after publishing
- Cleanup on application exit
- Cleanup on post close/discard

---

## Extensibility Points

### 1. Custom Image Services
- Implement `IImageService` interface
- Register via plugin system
- Provide custom upload logic

### 2. Custom Decorators
- Extend `ImageDecorator` base class
- Register in `ImageDecoratorsManager`
- Provide UI for settings

### 3. Custom File Uploaders
- Extend `BlogFileUploader`
- Implement custom protocols
- Register via blog provider

### 4. Upload Filters
- Pre-process files before upload
- Post-process URLs after upload
- Modify HTML during transformation

---

## Recommendations

### Short-Term Improvements

1. **Better Error Messages**
   - Specific error codes
   - Actionable suggestions
   - Troubleshooting links

2. **Upload Progress**
   - Per-file progress
   - Total progress
   - Estimated time remaining

3. **Retry Logic**
   - Automatic retry on transient errors
   - Exponential backoff
   - User-initiated retry

### Medium-Term Enhancements

1. **Background Upload**
   - Upload while user continues editing
   - Queue-based upload system
   - Notification on completion

2. **Smart Caching**
   - Cache uploaded URLs longer
   - Sync across posts
   - Deduplicate identical files

3. **Compression Options**
   - User-selectable quality
   - Format conversion (WebP support)
   - Automatic optimization

### Long-Term Vision

1. **Cloud Storage Integration**
   - Direct upload to CDN
   - Cloud storage services (Azure, AWS S3)
   - Better performance and reliability

2. **Modern Image Formats**
   - WebP, AVIF support
   - Automatic format selection
   - Responsive images (srcset)

3. **Advanced Features**
   - Image editing within application
   - Batch upload optimization
   - Deduplication across blogs

---

## Technical References

### Implementation Files

**Image Insertion:**
- `src/managed/OpenLiveWriter.PostEditor/ImageInsertion/ImageInsertionManager.cs`
- `src/managed/OpenLiveWriter.PostEditor/ImageInsertion/ImageInsertHandler.cs`
- `src/managed/OpenLiveWriter.PostEditor/ImageInsertion/InsertImageDialog.cs`

**Data Structures:**
- `src/managed/OpenLiveWriter.PostEditor/BlogPostImageData.cs`
- `src/managed/OpenLiveWriter.PostEditor/BlogPostImageDataList.cs`
- `src/managed/OpenLiveWriter.PostEditor/ImageFileData.cs`

**Supporting Files:**
- `src/managed/OpenLiveWriter.PostEditor/BlogPostSupportingFileStorage.cs`
- `src/managed/OpenLiveWriter.PostEditor/SupportingFileService.cs`

**File Upload:**
- `src/managed/OpenLiveWriter.PostEditor/BlogFileUploader.cs`
- `src/managed/OpenLiveWriter.PostEditor/LocalSupportingFileUploader.cs`
- `src/managed/OpenLiveWriter.PostEditor/BlogPostReferenceFixer.cs`

**Image Services:**
- `src/managed/OpenLiveWriter.Extensibility/ImageServices/ImageService.cs`
- `src/managed/OpenLiveWriter.BlogClient/Clients/AtomMediaUploader.cs`

**Decorators:**
- `src/managed/OpenLiveWriter.PostEditor/ImageDecorator*.cs`

---

## Conclusion

The image upload/publish flow in OpenLiveWriter is a sophisticated, multi-phase process that:

1. **Registers** images with supporting file system
2. **Processes** images (resize, optimize, decorate)
3. **Stores** files temporarily with metadata
4. **Uploads** files to blog/FTP during publishing
5. **Transforms** HTML to use server URLs
6. **Publishes** post with corrected references

**Key Strengths:**
- Robust metadata tracking
- Multiple upload destinations supported
- Decorator extensibility
- Upload tracking prevents duplicates
- Flexible filename formatting

**Potential Improvements:**
- Background upload
- Better error recovery
- Modern image format support
- Cloud storage integration

The architecture is well-designed for extensibility and reliability, with clear separation of concerns between image processing, storage, and upload mechanisms.

---

**End of Research Document**
