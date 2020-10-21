using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace PrcpConfigUtility
{
    /// <summary>
    /// Supporting logic for the PCU main window.
    /// </summary>
    public partial class MainWindow : Window
    {
        private string currentDocument = ""; //For keeping track of which document is displayed in the editor.

        private bool currentDocumentChangesMade = false;


        private void ArchiveCurrentItem()
        {
            string archiveFolder = Path.Combine(FixtureTreeItemFolder(), "archive");
            string currentItemPath = FixtureTreeItemPath();
            if (!Directory.Exists(archiveFolder))
            { Directory.CreateDirectory(archiveFolder); }
            string newPath = Path.Combine(archiveFolder, Path.GetFileName(currentItemPath));
            Debug.WriteLine($"Moving from: {currentItemPath}\nTo: {newPath}");
            File.Move(currentItemPath, newPath, true);
            PopulateFixtureTree();
        }

        private void DisplayXml(XDocument document)
        {
            ConfigEditorTextBox.Text = document.ToString();
            currentDocumentChangesMade = false;
        }

        private bool FixtureTreeIsFileSelected() //True if selected item is a config file
        {
            if (FixtureTreeView.SelectedItem != null)
            {
                FixtureTreeViewItem fixtureItem = FixtureTreeView.SelectedItem as FixtureTreeViewItem;
                if (File.Exists(fixtureItem.Path))
                { return true; }
            }
            return false;
        }

        private string FixtureTreeItemPath() //Empty string if no file selected.
        {
            if (FixtureTreeIsFileSelected())
            {
                return ((FixtureTreeViewItem)FixtureTreeView.SelectedItem).Path;
            }
            return "";
        }

        private string FixtureTreeItemFolder() //Path of the selected folder, or the folder containing selected file
        {
            if(FixtureTreeView.SelectedItem != null)
            {
                FixtureTreeViewItem fixtureItem = FixtureTreeView.SelectedItem as FixtureTreeViewItem;
                if (File.Exists(fixtureItem.Path))
                {
                    return Directory.GetParent(fixtureItem.Path).FullName;
                }
                else if(Directory.Exists(fixtureItem.Path))
                {
                    return fixtureItem.Path;
                }
            }
            return "";
        }

        private FixtureTreeViewItem FixtureTreeSelectedItem()
        {
            return FixtureTreeView.SelectedItem as FixtureTreeViewItem;
        }

        private string GetFileType(string file) //Return lowercase file type. Blank if folder
        {
            if (File.Exists(file))
            {
                string fileType = Path.GetFileName(file).Split('.').TakeLast(1).Last();
                return fileType.ToLower();
            }
            return "";
        }

        private List<FixtureTreeViewItem> GetItemsInGroup(FixtureTreeViewItem selectedItem)
        {
            bool matchParent = selectedItem.Fixture.FileNameSections[selectedItem.Fixture.Grouping["GroupBy"]] == "MatchParent";
            string parent = "";
            string matchRegex = "";
            if (matchParent) 
            { 
                parent = Path.GetFileName(Directory.GetParent(selectedItem.Path).FullName); 
            }
            else
            {
                matchRegex = selectedItem.Fixture.FileNameSections[selectedItem.Fixture.Grouping["GroupBy"]];
            }
            List<FixtureTreeViewItem> groupItems = new List<FixtureTreeViewItem>();
            if (!File.Exists(selectedItem.Path)) { return groupItems; }
            foreach(FixtureTreeViewItem listItem1 in FixtureTreeView.Items) //Foreach fixture
            {
                if (!selectedItem.Fixture.Grouping["GroupWithFixtures"].Contains((string)listItem1.Header)) //Check fixture grouping
                { continue; }
                foreach(FixtureTreeViewItem listItem2 in listItem1.Items) //Foreach subfolder or file
                {
                    if (listItem1.Fixture.SubFolders)
                    {
                        foreach (FixtureTreeViewItem listItem3 in listItem2.Items)
                        {
                            if (matchParent)
                            {
                                if (((string)listItem3.Header).Contains(parent))
                                {
                                    groupItems.Add(listItem3);
                                }
                            }
                            else
                            {
                                if (Regex.IsMatch((string)listItem3.Header, matchRegex))
                                {
                                    groupItems.Add(listItem3);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (matchParent)
                        {
                            if (((string)listItem2.Header).Contains(parent))
                            {
                                groupItems.Add(listItem2);
                            }
                        }
                        else
                        {
                            if(Regex.IsMatch((string)listItem2.Header,matchRegex))
                            {
                                groupItems.Add(listItem2);
                            }
                        }
                    }
                }
            }
            return groupItems;
        }

        /// <summary>
        /// Gets the text from the ConfigEditorTextBox in the main window, and parses to a new XDocument
        /// </summary>
        /// <returns>XDocument</returns>
        private XDocument GetXmlFromEditor()
        {
            return XDocument.Parse(ConfigEditorTextBox.Text);
        }

        private void PopulateFixtureTree()
        {
            FixtureTreeView.Items.Clear();
            foreach (string fixtureConfig in Directory.GetFiles(config.PcuFixturesFolder))
            {
                PcuFixture fixture = new PcuFixture(fixtureConfig);
                if (!fixture.Success) { continue; }
                FixtureTreeViewItem fixtureHeader = new FixtureTreeViewItem //First level TreeViewItem
                {
                    Header = fixture.FixtureName,
                    Path = fixture.FixtureConfigsLocation,
                    Fixture = fixture
                };
                if (fixture.SubFolders)
                {
                    foreach (string subFolder in Directory.GetDirectories(fixture.FixtureConfigsLocation))
                    {
                        FixtureTreeViewItem subFolderHeader = new FixtureTreeViewItem //Second level TreeViewItem
                        {
                            Header = System.IO.Path.GetFileName(subFolder),
                            Path = subFolder,
                            Fixture = fixture
                        };
                        foreach (string file in Directory.GetFiles(subFolder))
                        {
                            string fileName = System.IO.Path.GetFileName(file);
                            if (Regex.IsMatch(fileName, fixture.ConfigRegex))
                            {
                                FixtureTreeViewItem fileItem = new FixtureTreeViewItem //Third level TreeViewItem
                                {
                                    Header = fileName,
                                    Path = file,
                                    Fixture = fixture
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
                            FixtureTreeViewItem fileItem = new FixtureTreeViewItem //Second level TreeViewItem
                            {
                                Header = fileName,
                                Path = file,
                                Fixture = fixture
                            };
                            fixtureHeader.Items.Add(fileItem);
                        }
                    }
                }
                FixtureTreeView.Items.Add(fixtureHeader);
            }
        }

        private void SaveEditorText()
        {
            if(currentDocument != "")
            {
                if(GetFileType(currentDocument) == "xml")
                {
                    if(FixtureTreeSelectedItem().Fixture.Autoincrement)
                    {
                        string fileName = Path.GetFileName(FixtureTreeItemPath());
                    }
                    Debug.WriteLine(FixtureTreeSelectedItem().IncrementVersion());
                    XDocument document = GetXmlFromEditor();
                    document.Save(currentDocument);
                    currentDocumentChangesMade = false;
                }
            }
        }

        private void SetCurrentDocument(string document)
        {
            if (File.Exists(document))
            {
                currentDocument = document;
                CurrentFileTextBox.Text = document;
            }
        }

    }
}
