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
using System.Text.RegularExpressions;

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
            PopulateFixtureTree();
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
            foreach(string fixtureConfig in Directory.GetFiles(config.PcuFixturesFolder))
            {
                PcuFixture fixture = new PcuFixture(fixtureConfig);
                FixtureTreeViewItem fixtureHeader = new FixtureTreeViewItem //First level TreeViewItem
                {
                    Header = fixture.FixtureName,
                    Path = fixture.FixtureConfigsLocation
                };
                if(fixture.SubFolders)
                {
                    foreach(string subFolder in Directory.GetDirectories(fixture.FixtureConfigsLocation))
                    {
                        FixtureTreeViewItem subFolderHeader = new FixtureTreeViewItem //Second level TreeViewItem
                        {
                            Header = System.IO.Path.GetFileName(subFolder),
                            Path = subFolder
                        };
                        foreach(string file in Directory.GetFiles(subFolder))
                        {
                            string fileName = System.IO.Path.GetFileName(file);
                            if (Regex.IsMatch(fileName,fixture.ConfigRegex))
                            {
                                FixtureTreeViewItem fileItem = new FixtureTreeViewItem //Third level TreeViewItem
                                {
                                    Header = fileName,
                                    Path = file
                                };
                                subFolderHeader.Items.Add(fileItem);
                            }
                        }
                        fixtureHeader.Items.Add(subFolderHeader);
                    }
                }
                else
                {
                    foreach (string file in Directory.GetFiles(fixture.FixtureConfigsLocation))
                    {
                        string fileName = System.IO.Path.GetFileName(file);
                        if (Regex.IsMatch(file, fixture.ConfigRegex))
                        {
                            FixtureTreeViewItem fileItem = new FixtureTreeViewItem //Third level TreeViewItem
                            {
                                Header = fileName,
                                Path = file
                            };
                            fixtureHeader.Items.Add(fileItem);
                        }
                    }
                }
                FixtureTreeView.Items.Add(fixtureHeader);
            }
        }
    }
}
