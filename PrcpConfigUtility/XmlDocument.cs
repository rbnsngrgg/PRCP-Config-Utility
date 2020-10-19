using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Xml;


namespace PrcpConfigUtility
{
    class XmlConfig
    {
        XmlDocument document = new XmlDocument();
        public XmlConfig(string path)
        {
            if (File.Exists(path)) { document.Load(path); }
        }
    }

}
