// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenLiveWriter.Api;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.PostEditor.Video
{
    /// <summary>
    /// Summary description for VideoHelper.
    /// </summary>
    public class VideoHelper :IDisposable
    {
        public const int DEFAULT_TIMEOUT_MS = 15000;
        public const string WIDTH = "{width}";
        public const string HEIGHT = "{height}";
        private Bitmap testBitmap = null;
        private Color testColor;
        private double testPct;
        private RectTest rectTest;

        public static string GenerateEmbedHtml(string embedFormat, Size size)
        {
            string pattern = embedFormat.Replace(WIDTH, "{0}").Replace(HEIGHT, "{1}");
            return String.Format(pattern, size.Width, size.Height);
        }

        public Bitmap GetVideoSnapshot(VideoProvider provider, string embedHtml, Size videoSize)
        {
            try
            {
                string videoHtml = GenerateEmbedHtml(embedHtml, videoSize);
                if (provider != null && provider.UseBackgroundColor != String.Empty)
                {
                    videoHtml = String.Format(CultureInfo.InvariantCulture, "<div style=\"background-color:{0};\">{1}</div>", provider.UseBackgroundColor, videoHtml);
                }
                HtmlScreenCapture htmlScreenCapture = new HtmlScreenCapture(videoHtml, videoSize.Width);

                if (provider != null && provider.RectangleTest != null)
                {
                    rectTest = provider.RectangleTest;
                    htmlScreenCapture.HtmlScreenCaptureAvailable += new HtmlScreenCaptureAvailableHandler(htmlScreenCapture_HtmlScreenCaptureAvailable_RectangleTest);
                }
                else if (provider != null && provider.SnapshotLoadedOrigColor != Color.Empty)
                {
                    testColor = provider.SnapshotLoadedOrigColor;
                    testPct = provider.SnapshotLoadedColorPct;
                    htmlScreenCapture.HtmlScreenCaptureAvailable +=new HtmlScreenCaptureAvailableHandler(htmlScreenCapture_HtmlScreenCaptureAvailable_ColorTest);
                }
                else
                {
                    testBitmap = null;
                    htmlScreenCapture.HtmlScreenCaptureAvailable +=new HtmlScreenCaptureAvailableHandler(htmlScreenCapture_HtmlScreenCaptureAvailable_ChangeTest);
                }

                htmlScreenCapture.MaximumHeight = videoSize.Height;
                //we set our own limit to ensure a snapshot is always getting returned
                SetTimeout(DEFAULT_TIMEOUT_MS);
                Bitmap videoSnapshot = htmlScreenCapture.CaptureHtml(2 * DEFAULT_TIMEOUT_MS);

                // return the video
                return videoSnapshot ;
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                throw new VideoPluginException(Res.Get(StringId.Plugin_Video_Snapshot_Error_Title), String.Format(Res.Get(StringId.Plugin_Video_Snapshot_Error_Message), ex.Message)) ;
            }
        }

        void htmlScreenCapture_HtmlScreenCaptureAvailable_RectangleTest(object sender, HtmlScreenCaptureAvailableEventArgs e)
        {
            Bitmap bitmap = e.Bitmap ;
            int xRect =  (int)Math.Round(bitmap.Width * rectTest.X-(rectTest.Width / 2));
            int yRect =  (int)Math.Round(bitmap.Height * rectTest.Y - (rectTest.Height / 2));

            int nonMatchingPixelCount = 0;
            for (int x = xRect; x < xRect + rectTest.Width; x++)
            {
                for (int y = yRect; y < yRect + rectTest.Height; y++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    if (color.A != rectTest.Color.A ||
                        color.R != rectTest.Color.R ||
                        color.G != rectTest.Color.G ||
                        color.B != rectTest.Color.B)
                        nonMatchingPixelCount++;
                }
            }
            e.CaptureCompleted = nonMatchingPixelCount == 0 || TimedOut;
        }

        private DateTime _timeoutTime ;
        private void SetTimeout(int ms)
        {
            _timeoutTime = DateTime.Now.AddMilliseconds(ms) ;
        }

        private bool TimedOut
        {
            get
            {
                return DateTime.Now > _timeoutTime;
            }
        }

        private void htmlScreenCapture_HtmlScreenCaptureAvailable_ColorTest(object sender, HtmlScreenCaptureAvailableEventArgs e)
        {
            const int PLAYER_CONTROL_OFFSET = 30;
            // get the bitmap
            Bitmap bitmap = e.Bitmap ;

            int pixelCount = 0;
            for (int x=0; x<bitmap.Width; x++)
            {
                for ( int y=0; y<bitmap.Height - PLAYER_CONTROL_OFFSET; y++)
                {
                    if (bitmap.GetPixel(x, y) == testColor)
                        pixelCount++ ;
                }
            }

            int totalPixels = bitmap.Width*(bitmap.Height - PLAYER_CONTROL_OFFSET);

            double pctBackground = Convert.ToDouble(pixelCount)/Convert.ToDouble(totalPixels);
            //Trace.WriteLine("how much test Color: " + pctBackground.ToString());
            e.CaptureCompleted = pctBackground < testPct || TimedOut;
        }

        private void htmlScreenCapture_HtmlScreenCaptureAvailable_ChangeTest(object sender, HtmlScreenCaptureAvailableEventArgs e)
        {
            // get the bitmap
            Bitmap bitmap = e.Bitmap ;
            int totalPixels = 0;
            if (testBitmap == null)
                testBitmap = (Bitmap)e.Bitmap.Clone();

            int countPixels = 0 ;
            //only doing some pixels to speed this up!
            for (int x=0; x<bitmap.Width; x+=8)
            {
                for ( int y=0; y<bitmap.Height; y+=8)
                {
                    totalPixels++;
                    if (bitmap.GetPixel(x, y) == testBitmap.GetPixel(x, y))
                        countPixels++ ;
                }

            }

            // if more than 80% of all pixels the same as first look then
            // this is not a thumbnail
            double pctBackground = Convert.ToDouble(countPixels) / Convert.ToDouble(totalPixels) ;
            //Trace.WriteLine("difference in snapshots: " + pctBackground.ToString());
            e.CaptureCompleted = pctBackground < .80F || TimedOut;
        }

        #region IDisposable Members

        public void Dispose()
        {
            // TODO:  Add VideoHelper.Dispose implementation
        }

        #endregion

        //FIXME
        internal static bool SnapshotComparableToThumbnail(Bitmap videoSnapshot, Bitmap serviceThumbnail)
        {
            return true;
            //const int PLAYER_CONTROL_OFFSET = 30;

            //long pixelCount = 0;
            //long pixelValueTotal = 0;
            //for (int x = 0; x < videoSnapshot.Width; x++)
            //{
            //    for (int y = 0; y < videoSnapshot.Height - PLAYER_CONTROL_OFFSET; y++)
            //    {
            //        pixelValueTotal += videoSnapshot.GetPixel(x, y).ToArgb();
            //        pixelCount++;
            //    }
            //}
            //long videoSnapshotAverage = Math.Abs(pixelValueTotal / pixelCount);

            //pixelCount = 0;
            //pixelValueTotal = 0;
            //for (int x = 0; x < serviceThumbnail.Width; x++)
            //{
            //    for (int y = 0; y < serviceThumbnail.Height; y++)
            //    {
            //        pixelValueTotal += serviceThumbnail.GetPixel(x, y).ToArgb();
            //        pixelCount++;
            //    }
            //}
            //long serviceTHumbnailAverage = Math.Abs(pixelValueTotal / pixelCount);

            //if (videoSnapshotAverage < (serviceTHumbnailAverage * 1.1) && videoSnapshotAverage < (serviceTHumbnailAverage * 0.9))
            //    return true;

            //return false;
        }
    }
}
