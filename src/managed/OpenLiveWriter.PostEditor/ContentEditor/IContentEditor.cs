// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using mshtml;
using OpenLiveWriter.Interop.Com.Ribbon;

namespace OpenLiveWriter.PostEditor
{
    // @SharedCanvas - Mail should save to file outside of the draft
    // @SharedCanvas - Block off forwared/replied code?
    // @SharedCanvas - Dont scan+init images in replies/forward
    // @SharedCanvas - Spell checking in plain text
    // @SharedCanvas - title should be decided by the editing context
    // @SharedCanvas - bring back default editing view
    // @SharedCanvas - when openning another post, the blogpostsupporting files are disposed with the old content in the editor
    //                 and then the blog is changed on the editor which causes the theme to reload.  This will mean
    //                 scan and initilize images is run, which throw an exception because it can't find the file.

    /// <summary>
    /// WYSIWYG editor that can edit arbitrary HTML.
    /// IContentEditor also implements IUICommandHandler even though the
    /// IDL from tlbexp does not show it
    /// http://msdn.microsoft.com/en-us/library/dd371491(VS.85).aspx - IUICommandHandler
    /// http://msdn.microsoft.com/en-us/library/46f8ac6z(VS.71).aspx - tlbexp details
    /// </summary>
    [Guid("D9A022CF-0FEF-4a6e-ADC4-C0A0036F77F7")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IContentEditor : IUICommandHandler, IUICommandHandlerOverride
    {
        /// <summary>
        /// Set the focus on the the editor again, this will move
        /// the input caret into the document
        /// </summary>
        void SetFocus();

        /// <summary>
        /// Provides a way for the hosting application to add html
        /// into the body
        /// </summary>
        void InsertHtml(string html, HtmlInsertOptions options);

        /// <summary>
        /// Moves the cursor to the beginning or end of the body.
        /// </summary>
        /// <param name="position">The position to move to</param>
        void ChangeSelection(SelectionPosition position);

        /// <summary>
        /// Saves the contents of the canvas to a serializable object
        /// the host can save.
        /// </summary>
        /// <param name="fileName">
        /// A filename to which the editor can save its payload that
        /// can later be passed into Load() to bring up the saved item
        /// </param>
        /// <param name="preserveDirty">
        /// True if thethe dirty start should be saved(autosave), false
        /// if the dirty state should be set to false(normal save).
        /// </param>
        /// <returns>true if success</returns>
        void Save(string fileName, bool preserveDirty);

        /// <summary>
        /// Runs any operation that needs to happen when the user
        /// requests the contents to be published (e.g. publish a
        /// blog post, or send an email). It will swap out local
        /// image paths.
        /// </summary>
        /// <returns>
        /// Returns the inner html of the element that contained
        /// {post-body} that can be viewed from other remote users
        /// </returns>
        string Publish(IPublishOperation imageConverter);

        /// <summary>
        /// Gets an IHTMLDocument2 that repersents the whole document
        /// where images are still pointing to local files on disk.
        /// </summary>
        /// <returns></returns>
        IHTMLDocument2 GetPublishDocument();

        /// <summary>
        /// Gets the dirty state of the canvas.
        /// </summary>
        /// <returns></returns>
        bool GetDirtyState();

        /// <summary>
        /// Set the current dirty state of the editor
        /// </summary>
        /// <returns></returns>
        void SetDirtyState(bool newState);

        /// <summary>
        /// Called to set the size the editor should take.
        /// </summary>
        /// <param name="width">New width of the editor.</param>
        /// <param name="height">New height of the editor.</param>
        void SetSize(int width, int height);

        /// <summary>
        /// Set the HTML that will be loaded into the editor.  The
        /// HTML is required to have a token {post-body} that will
        /// be the editable region.  Optionally the HTML can contain
        /// {post-title} which will be where the title of the document
        /// can be edited.
        /// </summary>
        /// <param name="wysiwygHTML">
        /// HTML that will be used when the user is editing their document.
        /// </param>
        void SetTheme(string wysiwygHTML);

        /// <summary>
        /// Changes what mode the editor is in.
        /// </summary>
        /// <param name="editingMode">"Wysiwyg", "Preview", "Source" </param>
        void ChangeView(EditingMode editingMode);

        /// <summary>
        /// Selling configuration
        /// </summary>
        /// <param name="bcp47Code">BCP47 culture code of the language to spell</param>
        /// <param name="sobitOptions">spelling options</param>
        /// <param name="useAutoCorrect">Auto correct spelling</param>
        void SetSpellingOptions(string bcp47Code, uint sobitOptions, bool useAutoCorrect);

        /// <summary>
        /// Turns off spelling checking--for example, user selected "None" for dictionary
        /// </summary>
        void DisableSpelling();

        /// <summary>
        /// Releases resources and shuts down the editor.
        /// </summary>
        void Dispose();

        void SetDefaultFont(string fontString);

        /// <summary>
        /// Specifies if text emoticons should be automatically replaced with emoticon graphics.
        /// </summary>
        void AutoreplaceEmoticons(bool enabled);
    }

    /// <summary>
    /// Each time the content editor publishes the html, the hosting application will have to provide
    /// one of these which it will use to display information to the user as well as change
    /// image location to non local URIs
    /// </summary>
    [Guid("9DC27B36-445D-41f8-9BBF-52DDAF278F12")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IPublishOperation
    {
        /// <summary>
        /// This method will be used when the canvas publishes the content to replace the
        /// local image paths with a URI from the final location. In Open Live Writer this would
        /// mean the image is uploaded to the blog service.
        /// </summary>
        /// <param name="localUri"></param>
        /// <returns></returns>
        string GetUriForImage(string localUri);
    }
}
