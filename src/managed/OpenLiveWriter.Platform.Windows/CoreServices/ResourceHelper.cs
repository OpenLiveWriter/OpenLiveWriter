// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Collections;
using OpenLiveWriter.Localization;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Resource helper class.
    /// </summary>
    public sealed class ResourceHelper
    {
        /// <summary>
        ///	The bitmap cache.
        /// </summary>
        [ThreadStatic]
        private static Dictionary<string, Bitmap> bitmapCache;

        /// <summary>
        /// The icon cache
        /// </summary>
        [ThreadStatic]
        private static Hashtable iconCache;

        /// <summary>
        /// Initializes a new instance of the ResourceHelper class.
        /// </summary>
        private ResourceHelper()
        {
        }

        /// <summary>
        /// Loads a Bitmap from the calling Assembly's resource stream.
        /// </summary>
        /// <param name="resource">The name of the Bitmap to load (i.e. "Image.png").</param>
        public static Bitmap LoadAssemblyResourceBitmap(string resource)
        {
            return LoadAssemblyResourceBitmap(Assembly.GetCallingAssembly(), null, resource, false);
        }

        public static Bitmap LoadAssemblyResourceBitmap(string resource, bool mirror)
        {
            return LoadAssemblyResourceBitmap(Assembly.GetCallingAssembly(), null, resource, mirror);
        }

        /// <summary>
        /// Loads a Bitmap from the calling Assembly's resource stream.
        /// </summary>
        /// <param name="path">The explicit path to the Bitmap, null to use the default.</param>
        /// <param name="resource">The name of the Bitmap to load (i.e. "Image.png").</param>
        public static Bitmap LoadAssemblyResourceBitmap(string path, string resource)
        {
            return LoadAssemblyResourceBitmap(Assembly.GetCallingAssembly(), path, resource, false);
        }

        /// <summary>
        /// Loads a Bitmap from an Assembly's resource stream.
        /// </summary>
        /// <param name="assembly">The Assembly to load the Bitmap from.</param>
        /// <param name="resource">The name of the Bitmap to load (i.e. "Image.png").</param>
        public static Bitmap LoadAssemblyResourceBitmap(Assembly assembly, string resource)
        {
            return LoadAssemblyResourceBitmap(assembly, null, resource, false);
        }

        /// <summary>
        /// Loads a Bitmap from an Assembly's resource stream.
        /// </summary>
        /// <param name="assembly">The Assembly to load the Bitmap from.</param>
        /// <param name="path">The explicit path to the Bitmap, null to use the default.</param>
        /// <param name="resource">The name of the Bitmap to load (i.e. "Image.png").</param>
        public static Bitmap LoadAssemblyResourceBitmap(Assembly assembly, string path, string resource, bool mirror)
        {
            //	If a path was not specified, default to using the name of the assembly.
            if (path == null)
                path = assembly.GetName().Name;

            //	Format the schema resource name that we will load from the assembly.
            string resourceName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", path, resource);
            string key = String.Format(CultureInfo.InvariantCulture, "{0},{1}", resourceName, mirror);
            //	Get the bitmap.
            Bitmap bitmap;
            lock (typeof(ResourceHelper))
            {
                //	Initialize the bitmap cache if necessary
                if (bitmapCache == null)
                    bitmapCache = new Dictionary<string, Bitmap>();

                //	Locate the resource name in the bitmap cache.  If found, return it.
                if (!bitmapCache.TryGetValue(key, out bitmap))
                {
                    //	Get a stream on the resource.  If this fails, null will be returned.  This means
                    //	that we could not find the resource in the assembly.
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {

                            //	Load the bitmap.
                            // Bitmaps require their underlying streams to remain open
                            // for as long as their bitmaps are in use.  We don't want to hold open the
                            // resource stream though, so copy it to a memory stream.
                            bitmap = new Bitmap(StreamHelper.CopyToMemoryStream(stream));

                            if (BidiHelper.IsRightToLeft && mirror)
                            {
                                Bitmap temp = BidiHelper.Mirror(bitmap);
                                bitmap.Dispose();
                                bitmap = temp;
                            }
                        }

                        //	Cache the bitmap.
                        bitmapCache.Add(key, bitmap);
                    }
                }

                //	Done!
                return bitmap;
            }
        }

        /// <summary>
        /// Loads a Bitmap embedded in Images.resx.
        /// </summary>
        /// <param name="resource">The name of the Bitmap to load (i.e. "InsertPhotoAlbum_SmallImage").</param>
        public static Bitmap LoadBitmap(string resource)
        {
            //	Get the bitmap.
            Bitmap bitmap;
            lock (typeof(ResourceHelper))
            {
                //	Initialize the bitmap cache if necessary
                if (bitmapCache == null)
                    bitmapCache = new Dictionary<string, Bitmap>();

                //	Locate the resource name in the bitmap cache.  If found, return it.
                if (!bitmapCache.TryGetValue(resource, out bitmap))
                {
                    bitmap = Images.ResourceManager.GetObject(resource) as Bitmap;
                    bitmapCache.Add(resource, bitmap);
                }

                //	Done!
                return bitmap;
            }
        }

        /// <summary>
        /// Loads an icon from an assembly resource.
        /// </summary>
        /// <param name="path">icon path.</param>
        /// <returns>Icon, or null if the icon could not be found.</returns>
        public static Icon LoadAssemblyResourceIcon(string path)
        {
            return LoadAssemblyResourceIcon(Assembly.GetCallingAssembly(), path);
        }

        /// <summary>
        /// Loads an icon from an assembly resource.
        /// </summary>
        /// <param name="assembly">The assembly that contains the resource.</param>
        /// <param name="path">icon path.</param>
        /// <returns>Icon, or null if the icon could not be found.</returns>
        public static Icon LoadAssemblyResourceIcon(Assembly assembly, string path)
        {
            return LoadAssemblyResourceIcon(assembly, path, 0, 0);
        }

        /// <summary>
        /// Loads an icon from an assembly resource.
        /// </summary>
        /// <param name="path">icon path.</param>
        /// <returns>Icon, or null if the icon could not be found.</returns>
        public static Icon LoadAssemblyResourceIcon(string path, int desiredWidth, int desiredHeight)
        {
            return LoadAssemblyResourceIcon(Assembly.GetCallingAssembly(), path, desiredWidth, desiredHeight);
        }

        /// <summary>
        /// Loads an icon from an assembly resource.
        /// </summary>
        /// <param name="path">icon path.</param>
        /// <returns>Icon, or null if the icon could not be found.</returns>
        private static Icon LoadAssemblyResourceIcon(Assembly callingAssembly, string path, int desiredWidth, int desiredHeight)
        {
            //	Get the calling assembly and its name.
            AssemblyName callingAssemblyName = callingAssembly.GetName();

            //	Format the schema resource name that we will load from the calling assembly.
            string resourceName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", callingAssemblyName.Name, path);
            string resourceNameKey = String.Format(CultureInfo.InvariantCulture, "{0}/{1}x{2}", resourceName, desiredWidth, desiredHeight);

            //	Get the icon
            Icon icon;
            lock (typeof(ResourceHelper))
            {
                //	Initialize the icon cache if necessary
                if (iconCache == null)
                    iconCache = new Hashtable();

                //	Get a stream on the resource.  If this fails, null will be returned.  This means
                //	If we have the icon cached, return it.
                icon = (Icon)iconCache[resourceNameKey];
                if (icon != null)
                    return icon;

                //	that we could not find the resource in the caller's assembly.  Throw an exception.
                using (Stream stream = callingAssembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        return null;

                    //	Load the icon
                    // Unlike Bitmaps, icons don't need their streams to remain open
                    if (desiredWidth > 0 && desiredHeight > 0)
                        icon = new Icon(stream, desiredWidth, desiredHeight);
                    else
                        icon = new Icon(stream);
                }

                //	Cache the icon
                iconCache[resourceNameKey] = icon;
            }

            //	Done!
            return icon;
        }

        /// <summary>
        /// Loads a raw stream from an assembly resource.
        /// </summary>
        /// <param name="resourcePath">Resource path.</param>
        /// <returns>Stream to the specified resource, or null if the resource could not be found.</returns>
        public static Stream LoadAssemblyResourceStream(string resourcePath)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            string resourceName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", assembly.GetName().Name, resourcePath);
            return assembly.GetManifestResourceStream(resourceName);
        }

        /// <summary>
        /// Saves an assembly resource to a file
        /// </summary>
        /// <param name="resourcePath">path to resource within assembly</param>
        /// <param name="filePath">full path to file to save</param>
        public static void SaveAssemblyResourceToFile(string resourcePath, string targetFilePath)
        {
            // Format the schema resource name that we will load from the assembly.
            Assembly assembly = Assembly.GetCallingAssembly();
            string resourceName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", assembly.GetName().Name, resourcePath);

            // transfer the resource into the file stream
            using (Stream fileStream = new FileStream(targetFilePath, FileMode.Create))
            {
                // Load stream and transfer it to the target stream
                using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    StreamHelper.Transfer(resourceStream, fileStream);
                }
            }
        }

        /// <summary>
        /// Saves an assembly resource to a stream
        /// </summary>
        /// <param name="resourcePath">path to resource within assembly</param>
        /// <param name="targetStream">target stream to save to</param>
        public static void SaveAssemblyResourceToStream(string resourcePath, Stream targetStream)
        {
            // Format the schema resource name that we will load from the assembly.
            Assembly assembly = Assembly.GetCallingAssembly();
            string resourceName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", assembly.GetName().Name, resourcePath);

            // Load stream and transfer it to the target stream
            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                StreamHelper.Transfer(resourceStream, targetStream);
            }
        }
    }
}
