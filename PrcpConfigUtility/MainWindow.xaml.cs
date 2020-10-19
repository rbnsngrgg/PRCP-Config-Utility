using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace PrcpConfigUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            List<string> someStrings = new List<string>() { "Test string" };
            ConfigDataGrid.ItemsSource = someStrings;
            XDocument document = XDocument.Load("916-0401-MGSSS2R6.xml");
            BuildTree(document);
        }
        // TODO: Modify with XAML code to use HierarchicalDataTemplate for a two-column TreeView. https://www.codemag.com/Article/1401031
        private void BuildTree(XDocument doc)
        {
            TreeViewItem treeNode = new TreeViewItem
            {
                Header = doc.Root.Name.LocalName,
                IsExpanded = true
            };
            
            BuildNodes(ref treeNode, doc.Root);
            ConfigTreeView.Items.Add(treeNode);
        }
        private void BuildNodes(ref TreeViewItem treeNode, XElement element)
        {
            foreach (XAttribute attr in element.Attributes())
            {
                TreeViewItem rootAttribute = new TreeViewItem
                {
                    Header = attr.Name.LocalName
                };
                treeNode.Items.Add(rootAttribute);
            }
            foreach (XNode child in element.Nodes())
            {
                switch (child.NodeType)
                {
                    case XmlNodeType.Element:
                        XElement childElement = child as XElement;
                        TreeViewItem newElement = new TreeViewItem { Header = childElement.Name.LocalName };
                        foreach (XAttribute attr in childElement.Attributes()) 
                        {
                            TreeViewItem childTreeNode = new TreeViewItem
                            {
                                Header = attr.Name.LocalName
                            };
                            newElement.Items.Add(childTreeNode);
                            Debug.WriteLine(attr.Name);
                        }
                        foreach (XElement subElement in childElement.Elements())
                        {
                            if(subElement.Name.LocalName.Contains("EXPORT"))
                            {
                                Debug.WriteLine("Breakpoint");
                            }
                            TreeViewItem childTreeNode = new TreeViewItem
                            {
                                Header = subElement.Name.LocalName,
                            };
                            BuildNodes(ref childTreeNode, subElement);
                            newElement.Items.Add(childTreeNode);
                        }
                        treeNode.Items.Add(newElement);
                        break;
                    case XmlNodeType.Text:
                        XText childText = child as XText;
                        treeNode.Items.Add(new TreeViewItem { Header = childText.Value });
                        break;
                }
            }
        }

        private void ConfigTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.WriteLine(ConfigTreeView.SelectedItem);
        }
    }
}
