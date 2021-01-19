using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace PrcpConfigUtility
{
    public class PcuConfig
    {
        //All regex matches result in full match
        private readonly string configName = "PCUConfig.xml";
        public string PcuFixturesFolder { get; private set; }
        public List<string> Fixtures { get; private set; } = new List<string>();
        private XDocument config;

        public PcuConfig()
        {
            if (File.Exists(configName))
            { config = XDocument.Load(configName); }
            else 
            { CreateConfig(); }
            LoadConfig();
        }
        private void CreateConfig()
        {
            config = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf - 8\" ?>\n" +
                "<PcuConfig PcuFixturesPath=\"PcuFixtures/\" Fixtures=\"\">\n\n" +
                "</PcuConfig>");
            config.Save(configName);
        }
        private void LoadConfig()
        {
            if(config == null) { return; }
            else
            {
                //Get fixture config folder location. Create folder if not exists
                PcuFixturesFolder = config.Element("PcuConfig").Attribute("PcuFixturesPath").Value;
                if (!Directory.Exists(PcuFixturesFolder)) { Directory.CreateDirectory(PcuFixturesFolder); }
                //Get list of fixtures
                string[] fixturesSplit = ((string)config.Element("PcuConfig").Attribute("Fixtures").Value).Split(',');
                if (fixturesSplit.Length != 0)
                { Fixtures.AddRange(fixturesSplit); }
            }
        }
    }
}
