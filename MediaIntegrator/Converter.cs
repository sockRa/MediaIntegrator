using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace MediaIntegrator
{
    public static class Converter
    {
        public static bool ConverterActive { get; set; }
        public static string CsvPath { get; set; }
        public static string XmlPath { get; set; }
        
        private enum CsvMusicProductTypes
        {
            Name,
            Description,
            Price,
            Id,
            Type,
            Quantity,
            Sales,
            PlayTime,
            Artist,
            NotAvailable
        }
        
        
        /// <summary>
        /// Start watching the location of the csv files for change
        /// </summary>
        public static void Start()
        {
            using (var watcher = new FileSystemWatcher())
            {
                watcher.Path = CsvPath;
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Changed += OnChanged;

                watcher.EnableRaisingEvents = true;

                while (ConverterActive)
                {
                }
            }
        }
        
        
        /// <summary>
        /// Method is called whenever a csv file have been modified
        /// Only convert music products at the moment
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.Name == "Music.csv")
                ConvertProductsCsvToXml(e.FullPath);
        }
        
        
        /// <summary>
        /// Convert the products csv file to xml
        /// Code inspired from http://getcodesnippet.com/2014/05/15/how-to-convert-csv-file-to-xml-file-in-c/
        /// </summary>
        /// <param name="csvFilePath"></param>
        private static void ConvertProductsCsvToXml(string csvFilePath)
        {
            //var lines = File.ReadAllLines(csvFilePath).Skip(1);
            var lines = new List<string>();
            
            ReadCsvLines(csvFilePath, lines);
            
            CreateXmlFromCsvLines(lines).Save(XmlPath + "\\" + "simplemedia.xml");
        }

        private static XElement CreateXmlFromCsvLines(IEnumerable<string> lines)
        {
            var xml = new XElement("Inventory",
                from str in lines.Skip(1)
                let columns = str.Split(',')
                select new XElement("Item",
                    new XElement("Name", columns[(int) CsvMusicProductTypes.Name]),
                    new XElement("Count", columns[(int) CsvMusicProductTypes.Quantity]),
                    new XElement("Price", columns[(int) CsvMusicProductTypes.Price]),
                    new XElement("Comment", columns[(int) CsvMusicProductTypes.Description]),
                    new XElement("Artist", columns[(int) CsvMusicProductTypes.Artist]),
                    new XElement("Publisher", CsvMusicProductTypes.NotAvailable),
                    new XElement("Genre", CsvMusicProductTypes.NotAvailable),
                    new XElement("Year", "-1"),
                    new XElement("ProductID", columns[(int) CsvMusicProductTypes.Id])
                )
            );
            return xml;
        }

        private static void ReadCsvLines(string csvFilePath, ICollection<string> lines)
        {
            using (var stream = new FileStream(csvFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                }
            }
        }
    }
}