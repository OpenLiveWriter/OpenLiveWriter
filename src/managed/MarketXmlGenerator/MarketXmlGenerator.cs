using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace MarketXmlGenerator
{
    /// <summary>
    /// Summary description for MarketXmlGenerator.
    /// </summary>
    class MarketXmlGenerator
    {
        private const string FEATURES = "features";
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //1. input: path to where all the files are located, output file?
            string inputFilePath;
            string outputFile;
            if (args.Length != 2)
            {
                Console.WriteLine("incorrect number of arguments. Correct usage: xxx {inputFilePath} {outputFile}");
                Environment.Exit(1);
            }
            inputFilePath = args[0];
            outputFile = args[1];

            //inputFilePath = @"..\..\..\..\..\..\..\SD\working.client.writer\client\writer\intl\markets";
            //outputFile = @"c:\temp\output.xml";

            //2. generate blank xml root
            XmlDocument marketsXml = new XmlDocument();
            XmlDeclaration declaration = marketsXml.CreateXmlDeclaration("1.0", "utf-8", String.Empty);
            XmlElement entryNode = marketsXml.CreateElement(FEATURES);
            marketsXml.AppendChild(entryNode);
            marketsXml.InsertBefore(declaration, entryNode);
            //3. get all folders
            DirectoryInfo dir = new DirectoryInfo(inputFilePath);
            DirectoryInfo[] marketDirs = dir.GetDirectories();
            //4. open each folder and get the xml file from inside
            foreach (DirectoryInfo marketDir in marketDirs)
            {
                FileInfo[] files = marketDir.GetFiles("market.xml");
                //case 1: no market xml
                if (files.Length == 0)
                    continue;
                FileInfo xmlFile = files[0]; //defaulting to taking the first here...shouldn't be more than one tho!

                //5. scan xml file for correctness. if correct, move on, else log error
                //	5a. is actual XML document
                //	5b. XML structure: contains feature root, one market element, 0-many feature elements, each with 0 - many params
                //	5c. features have name and enabled attributes. parameters have name and value attributes.

                if (!ValidateXml(inputFilePath, xmlFile.FullName))
                {
                    Console.WriteLine("Validation Failed for file " + xmlFile.FullName);
                    Environment.Exit(1);
                }

                //6. take the market portion and copy into new xml document
                using (FileStream xmlStream = xmlFile.OpenRead())
                {
                    XmlDocument marketDocument = new XmlDocument();
                    marketDocument.Load(xmlStream);
                    XmlNode marketNode = marketDocument.SelectSingleNode("//features/market");
                    if (marketNode == null)
                    {
                        Console.WriteLine("Invalid marketizationXml.xml file detected");
                        Environment.Exit(1);
                    }
                    string marketName = marketNode.Attributes["name"].InnerText;
                    if (marketName.ToLower() != marketName)
                    {
                        Console.WriteLine("market name must be lower case");
                        Environment.Exit(1);
                    }
                    entryNode.AppendChild(marketsXml.ImportNode(marketNode, true));
                }
            }
            //7. output the xml into document (check for existence, create. Do not append)
            marketsXml.Save(outputFile);
        }

        private static bool succeed;

        private static bool ValidateXml(string inputFilePath, string xmlFile)
        {
            succeed = true;
            // TODO:OLW
            //XmlSchemaCollection sc = new XmlSchemaCollection();
            //sc.ValidationEventHandler += new ValidationEventHandler(ValidationEventHandle);

            //sc.Add(null, Path.Combine(inputFilePath, "validator.xsd"));
            //XmlTextReader tr = new XmlTextReader(xmlFile);
            //XmlValidatingReader rdr = new XmlValidatingReader(tr);

            //rdr.ValidationType = ValidationType.Schema;
            //rdr.Schemas.Add(sc);
            //rdr.ValidationEventHandler += new ValidationEventHandler(ValidationEventHandle);
            //while (rdr.Read());
            return succeed;
        }

        private static void ValidationEventHandle(object sender, ValidationEventArgs args)
        {
            Console.WriteLine("Validation error: " + args.Message);
            Console.WriteLine("Line " + args.Exception.LineNumber + " Pos " + args.Exception.LinePosition);
            succeed = false;
        }

    }
}
