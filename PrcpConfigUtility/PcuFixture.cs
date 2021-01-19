using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Linq;

namespace PrcpConfigUtility
{
    public class PcuFixture
    {
        public bool ArchivePreviousVersion { get; private set; }
        public bool Autoincrement { get; private set; }
        public string ConfigRegex { get; private set; }
        public Dictionary<string,string> FileNameSections { get; private set; }
        public Dictionary<string,string> Grouping { get; private set; }
        public string FilePath { get; private set; }
        public string FixtureConfigsLocation { get; private set; }
        public string FixtureName { get; private set; }
        public bool GroupByRegex { get; private set; }
        public bool SubFolders { get; private set; }
        public bool Success { get; private set; } = false;

        private XDocument PcuFixtureConfig;

        public PcuFixture(string configPath)
        {
            if (File.Exists(configPath))
            { 
                FilePath = configPath;
                FixtureName = Path.GetFileNameWithoutExtension(configPath);
            }
            try { PcuFixtureConfig = XDocument.Load(FilePath); }
            catch(System.Xml.XmlException ex)
            {
                MessageBox.Show(ex.Message, "System.Xml.XmlException", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            LoadPcuFixtureConfig();
        }

        private void LoadPcuFixtureConfig()
        {
            if (!File.Exists(FilePath)) { return; }
            XElement root = PcuFixtureConfig.Element("PcuFixture");
            ArchivePreviousVersion = root.Attribute("ArchivePreviousVersion").Value.ToLower() == "true";
            Autoincrement = root.Attribute("Autoincrement").Value.ToLower() == "true";
            FixtureConfigsLocation = root.Attribute("FixtureConfigsLocation").Value;
            SubFolders = root.Attribute("SubFolders").Value.ToLower() == "true";
            ConfigRegex = root.Attribute("ConfigRegex").Value;
            Dictionary<string, string> newFileNameSections = new Dictionary<string, string>();
            foreach(XAttribute attr in root.Element("FileNameSections").Attributes())
            {
                newFileNameSections[attr.Name.LocalName] = attr.Value;
            }
            FileNameSections = newFileNameSections;
            Dictionary<string, string> newGrouping = new Dictionary<string, string>();
            foreach(XAttribute attr in root.Element("Grouping").Attributes())
            {
                newGrouping[attr.Name.LocalName] = attr.Value;
            }
            Grouping = newGrouping;
            GroupByRegex = Grouping["GroupByRegex"].ToLower() == "true";
            Success = true;
        }
    }
}
