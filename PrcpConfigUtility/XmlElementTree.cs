using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Transactions;
using System.Xml.Linq;

namespace PrcpConfigUtility
{
    class XmlTree
    {
        public XDocument Document { get; private set; }
        public XmlNode Root { get; private set; } = new XmlNode();
        public XmlTree( XDocument doc)
        {
            Document = doc;
        }

        private void BuildTree()
        {
            
        }
    }

    class XmlNode
    {
        public List<XAttribute> Attributes = new List<XAttribute>();
        public bool Changed { get; private set; } = false;
        public List<XmlNode> Children = new List<XmlNode>();
        public string Name { get; set; }
        public XmlTree ParentTree { get; private set; }
        public List<string> Path { get; private set; } = new List<string>();
        public string Value { get; set; }
        public XmlNode() //Creates a root node in the referenced tree.
        {
            Path.Add("ROOT");
        }
        public XmlNode(string name, string value, XmlNode parentNode)
        {
            Name = name;
            Value = value;
            Path.AddRange(parentNode.Path);
            Path.Add(Name);
        }
    }
}
