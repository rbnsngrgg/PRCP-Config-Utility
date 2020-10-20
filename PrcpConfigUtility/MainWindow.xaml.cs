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
using System.Printing;
using System.IO;

namespace PrcpConfigUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PcuConfig config = new PcuConfig();
        public MainWindow()
        {
            InitializeComponent();
            List<string> someStrings = new List<string>() { "Test string" };
            //ConfigDataGrid.ItemsSource = someStrings;
            XDocument document = XDocument.Load("916-0401-MGSSS2R6.xml");
            DisplayXml(document);
            XDocument document1 = GetXml();
        }

        private void DisplayXml(XDocument document)
        {
            ConfigEditorTextBox.Text = document.ToString();
        }
        /// <summary>
        /// Gets the text from the ConfigEditorTextBox in the main window, and parses to a new XDocument
        /// </summary>
        /// <returns>XDocument</returns>
        private XDocument GetXml()
        {
            return XDocument.Parse(ConfigEditorTextBox.Text);
        }
        private void PopulateFixtureTree()
        {
            foreach(string fixture in Directory.GetFiles(config.PcuFixturesFolder))
            {
                TreeViewItem fixtureHeader = new TreeViewItem //First level TreeViewItem
                {
                    Header = System.IO.Path.GetFileNameWithoutExtension(fixture),
                    Name = fixture
                };
                // TODO: Create PcuFixture class, load fixture configs into class.
            }
        }
    }
}
