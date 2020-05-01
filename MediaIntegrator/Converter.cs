using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MediaIntegrator
{
    public static class Converter
    {
        public static bool ConverterActive { get; set; }
        public static string CsvDirectoryPath { get; set; }
        public static string XmlDirectoryPath { get; set; }
        private static string XmlFullPath { get; set; }


        /// <summary>
        ///     Start watching the location of the csv files for change
        /// </summary>
        public static void Start()
        {
            XmlFullPath = XmlDirectoryPath + "\\" + "simplemedia.xml";

            using (var watcher = new FileSystemWatcher())
            {
                watcher.Path = CsvDirectoryPath;
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Changed += OnChanged;

                watcher.EnableRaisingEvents = true;

                while (ConverterActive)
                {
                }
            }
        }


        /// <summary>
        ///     Method is called whenever a csv file have been modified
        ///     Only convert music products at the moment
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.Name == "Products.csv")
                ConvertCsvToXml();
        }


        /// <summary>
        ///     Convert each csv file to one xml
        /// </summary>
        private static void ConvertCsvToXml()
        {
            var lines = new List<string>();
            var directoryInfo = new DirectoryInfo(CsvDirectoryPath);
            var xml = new XElement("Inventory");

            foreach (var csvFile in directoryInfo.GetFiles("*.csv"))
            {
                if (csvFile.Name == "Products.csv") continue;

                ReadCsvLines(csvFile.FullName, lines);
                ConvertCsvToXml(xml, lines, GetProductTypeFromName(csvFile.Name));
                lines.Clear();
            }

            xml.Save(XmlFullPath);
        }

        /// <summary>
        ///     Method that iterates over each csv-file
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="lines"></param>
        /// <param name="productType"></param>
        /// <returns></returns>
        private static void ConvertCsvToXml(XContainer xElement, IEnumerable<string> lines,
            CsvProductTypes productType)
        {
            // Skip the first line that contains the headers
            foreach (var line in lines.Skip(1))
            {
                var columns = line.Split(',');
                AddGenericXmlData(xElement, columns);
                AddProductSpecificData(xElement, columns, productType);
            }
        }


        /// <summary>
        ///     Append product specific data to already existing the xml file
        ///     The existing xml file contains generic product data that is common for every product in the store
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="columns"></param>
        /// <param name="retrieveProductTypeFromName"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static void AddProductSpecificData(XContainer xElement, IReadOnlyList<string> columns,
            CsvProductTypes retrieveProductTypeFromName)
        {
            var specificProduct = xElement
                .Elements("Item")
                .SingleOrDefault(x => x.Element("ProductID")?.Value == columns[(int) CsvProductItems.Id]);
            switch (retrieveProductTypeFromName)
            {
                case CsvProductTypes.Music:
                    specificProduct?.Add(new XElement("Artist", columns[(int) CsvMusicItems.Artist]),
                        new XElement("PlayTime", columns[(int) CsvMusicItems.PlayTime]));
                    break;
                case CsvProductTypes.Book:
                    specificProduct?.Add(new XElement("Author", columns[(int) CsvBookItems.Author]),
                        new XElement("ISBN", columns[(int) CsvBookItems.Isbn]),
                        new XElement("Pages", columns[(int) CsvBookItems.Pages]));
                    break;
                case CsvProductTypes.Game:
                    specificProduct?.Add(new XElement("AgeRestriction", columns[(int) CsvGameItems.AgeRestriction]));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(retrieveProductTypeFromName),
                        retrieveProductTypeFromName, null);
            }
        }


        /// <summary>
        ///     Add generic product data to an xml file
        ///     Code inspired from http://getcodesnippet.com/2014/05/15/how-to-convert-csv-file-to-xml-file-in-c/
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="columns"></param>
        private static void AddGenericXmlData(XContainer xElement, IReadOnlyList<string> columns)
        {
            xElement.Add(new XElement("Item",
                new XElement("Name", columns[(int) CsvProductItems.Name]),
                new XElement("Count", columns[(int) CsvProductItems.Quantity]),
                new XElement("Price", columns[(int) CsvProductItems.Price]),
                new XElement("Comment", columns[(int) CsvProductItems.Description]),
                new XElement("ProductID", columns[(int) CsvProductItems.Id]),
                new XElement("Type", columns[(int) CsvProductItems.Type]),
                new XElement("Sales", columns[(int) CsvProductItems.Sales]),
                new XElement("Publisher", CsvItemStatus.NotAvailable),
                new XElement("Genre", CsvItemStatus.NotAvailable),
                new XElement("Year", (int) CsvItemStatus.NotAvailable)));
        }


        /// <summary>
        ///     Method that compares the name of the csv file with different product types
        /// </summary>
        /// <param name="csvFileName"></param>
        /// <returns>Product Type</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static CsvProductTypes GetProductTypeFromName(string csvFileName)
        {
            switch (csvFileName.Split('.')[0])
            {
                case "Books":
                    return CsvProductTypes.Book;
                case "Music":
                    return CsvProductTypes.Music;
                case "Games":
                    return CsvProductTypes.Game;
                default:
                    throw new ArgumentOutOfRangeException(nameof(csvFileName), csvFileName, null);
            }
        }


        /// <summary>
        ///     Method that reads every line in a given csv file
        /// </summary>
        /// <param name="csvFilePath"></param>
        /// <param name="lines"></param>
        private static void ReadCsvLines(string csvFilePath, ICollection<string> lines)
        {
            using (var stream = new FileStream(csvFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null) lines.Add(line);
                }
            }
        }

        private enum CsvProductItems
        {
            Name,
            Description,
            Price,
            Id,
            Type,
            Quantity,
            Sales
        }

        private enum CsvMusicItems
        {
            Name,
            Description,
            Price,
            Id,
            Type,
            Quantity,
            Sales,
            PlayTime,
            Artist
        }

        private enum CsvBookItems
        {
            Name,
            Description,
            Price,
            Id,
            Type,
            Quantity,
            Sales,
            Author,
            Isbn,
            Pages
        }

        private enum CsvGameItems
        {
            Name,
            Description,
            Price,
            Id,
            Type,
            Quantity,
            Sales,
            AgeRestriction
        }

        private enum CsvProductTypes
        {
            Music,
            Book,
            Game
        }

        private enum CsvItemStatus
        {
            NotAvailable = -1
        }
    }
}