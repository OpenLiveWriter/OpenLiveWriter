using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace EmoticonGenerator
{
    class Program
    {
        /// <summary>
        /// Reads an input file for a list of emoticons and packs it into a single output PNG
        /// </summary>
        static void Main(string[] args)
        {
            string inputXml = string.Empty;
            string basePath = string.Empty;
            string outputPng = string.Empty;
            if (ParseCommandLine(args, ref inputXml, ref basePath, ref outputPng) == false)
            {
                Usage();
                return;
            }

            Console.Write("Reading and parsing input file {0}...", inputXml);
            StreamReader inputFile = new StreamReader(inputXml);
            string inputFileList = inputFile.ReadToEnd();
            inputFile.Close();

            string[] fileList = inputFileList.Split(Environment.NewLine.ToCharArray(),
                                                    StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine("done!");

            int emoticonSize = 19; // 19x19
            int x = 0;
            using (Bitmap pngStrip = new Bitmap(emoticonSize * fileList.Length, emoticonSize, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(pngStrip))
                {
                    Console.Write("Packing emoticons into one strip...");
                    foreach (string fileName in fileList)
                    {
                        if (fileName.Trim().StartsWith(";"))
                        {
                            // skip comments
                            continue;
                        }
                        string filePath = Path.Combine(basePath, fileName.Trim());
                        if (string.CompareOrdinal(Path.GetExtension(filePath).ToLowerInvariant(), ".png") != 0)
                        {
                            filePath += ".png";
                        }

                        try
                        {
                            using (Bitmap pngFile = new Bitmap(filePath))
                            {
                                g.DrawImage(pngFile, x, 0);
                                x += emoticonSize;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("\r\nError trying to read emoticon file {0}, {1}", filePath, e.Message);
                        }
                    }
                    Console.WriteLine("packed {0} emoticons!", (x / emoticonSize).ToString(CultureInfo.InvariantCulture));
                }

                Console.Write("Saving to {0}...", outputPng);
                pngStrip.Save(outputPng, ImageFormat.Png);
                Console.WriteLine("done!");
            }
        }

        static void Usage()
        {
            Console.WriteLine("Usage: EmoticonGenerator -i <input file> -b <base image path> -o <output png file>");
        }

        static bool ParseCommandLine(string[] args, ref string inputXml, ref string basePath, ref string outputPng)
        {
            int countFound = 0;
            try
            {
                for (int index = 0; index < args.Length; index++)
                {
                    string option = args[index].Trim().ToLowerInvariant();
                    if (string.CompareOrdinal(option, "-i") == 0)
                    {
                        inputXml = args[++index].Trim();
                        countFound++;
                    }
                    else if (string.CompareOrdinal(option, "-b") == 0)
                    {
                        basePath = args[++index].Trim();
                        countFound++;
                    }
                    else if (string.CompareOrdinal(option, "-o") == 0)
                    {
                        outputPng = args[++index].Trim();
                        countFound++;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            // 3 parameters expected
            return (countFound == 3);
        }
    }
}
