using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PrcpConfigUtility
{
    class PcuFixture
    {
        public string ConfigRegex { get; private set; }
        public string FilePath { get; private set; }
        public string FixtureConfigsLocation { get; private set; }
        public string FixtureName { get; private set; }
        public bool SubFolders { get; private set; }

        private XDocument PcuFixtureConfig;

        public PcuFixture(string configPath)
        {
            if (File.Exists(configPath))
            { 
                FilePath = configPath;
                FixtureName = Path.GetFileNameWithoutExtension(configPath);
            }
            PcuFixtureConfig = XDocument.Load(FilePath);
            LoadPcuFixtureConfig();
        }

        private void LoadPcuFixtureConfig()
        {
            if (!File.Exists(FilePath)) { return; }
            XElement root = PcuFixtureConfig.Element("PcuFixture");
            FixtureConfigsLocation = root.Attribute("FixtureConfigsLocation").Value;
            SubFolders = root.Attribute("SubFolders").Value == "True";
            ConfigRegex = root.Attribute("ConfigRegex").Value;
        }
    }
}
