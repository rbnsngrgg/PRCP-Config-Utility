using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace PrcpConfigUtility
{
    /// <summary>
    /// Supporting logic for the PCU main window.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string version = "0.1.1-alpha";
        private string currentDocument = ""; //For keeping track of which document is displayed in the editor.
        private XDocument currentXDocument;
        private bool currentDocumentChangesMade = false;
        private FixtureTreeViewItem currentItem;

        private void ArchiveCurrentItem(bool newSelection = false)
        {
            List<FixtureTreeViewItem> items = new List<FixtureTreeViewItem>();
            if (newSelection)
            {
                //if (FixtureTreeIsFileSelected())
                //{ items.Add(FixtureTreeSelectedItem()); }
                //else { return; }
                foreach(FixtureTreeViewItem child in FixtureTreeView.Items)
                {
                    if(child.IsSelected)
                    {
                        items.Add(child);
                    }
                    items.AddRange(child.GetChildrenSelectedList());
                }
            }
            else
            { 
                if (currentItem == null) 
                { return; }
                items.Add(currentItem);
            }
            foreach (FixtureTreeViewItem item in items)
            {
                if (!File.Exists(item.Path)) { continue; }
                string archiveFolder = Path.Combine(Directory.GetParent(item.Path).FullName, "archive");
                if (!Directory.Exists(archiveFolder))
                { Directory.CreateDirectory(archiveFolder); }
                string newPath = Path.Combine(archiveFolder, Path.GetFileName(item.Path));
                File.Move(item.Path, newPath, true);
            }
            PopulateFixtureTree(newSelection);
        }

        private void DisplayXml(XDocument document)
        {
            ConfigEditorTextBox.Text = document.ToString();
            currentDocumentChangesMade = false;
        }

        private void FixtureTreeCopy()
        {
            if(FixtureTreeIsFileSelected())
            {
                FixtureTreeViewItem current = FixtureTreeSelectedItem();
                CopyFileDialog copyDialog = new CopyFileDialog();
                copyDialog.currentItem = current;
                copyDialog.CopyFileTextBox.Text = Path.Join(
                    Directory.GetParent(current.Path).FullName,
                    $"{Path.GetFileNameWithoutExtension(current.Path)}-Copy{Path.GetExtension(current.Path)}");
                if(copyDialog.ShowDialog() == true)
                {
                    if (!File.Exists(copyDialog.CopyFileTextBox.Text))
                    {
                        if (Directory.GetParent(copyDialog.CopyFileTextBox.Text).Exists)
                        { File.Copy(current.Path, copyDialog.CopyFileTextBox.Text); }
                    }
                    PopulateFixtureTree();
                }
            }
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
            if(currentItem != null)
            {
                return currentItem.Path;
            }
            return "";
        }

        private string FixtureTreeItemFolder() //Path of the selected folder, or the folder containing selected file
        {
            FixtureTreeViewItem selectedItem = FixtureTreeSelectedItem();
            if (selectedItem != null)
            {
                if(Directory.Exists(selectedItem.Path))
                {
                    return selectedItem.Path;
                }
                return Directory.GetParent(selectedItem.Path).FullName;
            }
            return "";
        }

        private FixtureTreeViewItem FixtureTreeSelectedItem()
        {
            return FixtureTreeView.SelectedItem as FixtureTreeViewItem;
        }

        private void ForEachTreeItemVoid(Func<FixtureTreeViewItem,bool> method) //Iterate through each FixtureTreeViewItem, run method on each
        {
            foreach(FixtureTreeViewItem level1Item in FixtureTreeView.Items)
            {
                foreach(FixtureTreeViewItem level2Item in level1Item.Items)
                {
                    if(level2Item.Items.Count > 0)
                    {
                        foreach(FixtureTreeViewItem level3Item in level2Item.Items)
                        {
                            if (method(level3Item)) { return; } ;
                        }
                    }
                    else
                    {
                        if (method(level2Item)) { return; } ;
                    }
                }
            }
        }

        private void WriteLineIfMatch(FixtureTreeViewItem item)
        {
            FixtureTreeViewItem currentItem = FixtureTreeSelectedItem();
            if(currentItem.GetGroupMatchString() == item.GetGroupMatchString())
            {
                Debug.WriteLine($"{item.Path}\nMATCHES\n{currentItem.Path}");
            }
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
                if (selectedItem.Fixture.Grouping["GroupName"] != listItem1.Fixture.Grouping["GroupName"]) //Check fixture grouping
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

        private string GetLatestFileVersion(FixtureTreeViewItem group)
        {
            bool resultIsAlpha = false;
            string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int latestIntVersion = 0;
            string latestStrVersion = "A";
            foreach(FixtureTreeViewItem file in group.Items)
            {
                int intVersion;
                string fileVersion = file.GetAutoIncrementVersion();
                if(int.TryParse(fileVersion, out intVersion)) //If file version is int
                {
                    if(intVersion > latestIntVersion)
                    {
                        latestIntVersion = intVersion;
                    }
                }
                else
                {
                    if(alpha.IndexOf(fileVersion) > alpha.IndexOf(latestStrVersion))
                    {
                        latestStrVersion = fileVersion;
                        resultIsAlpha = true;
                    }
                }
            }
            if(resultIsAlpha)
            {
                return latestStrVersion;
            }
            return latestIntVersion.ToString();
        }
#nullable enable
        private FixtureTreeViewItem? GetTreeViewItemParent(FixtureTreeViewItem item)
        {
            if(item == null) { return null; }
            DependencyObject parent = item.Parent;
            if (parent is FixtureTreeViewItem)
            {
                return parent as FixtureTreeViewItem; 
            }
            return null;
        }
        private XDocument? GetXmlFromEditor()
        {
            try
            { return XDocument.Parse(ConfigEditorTextBox.Text); }
            catch(System.Xml.XmlException ex)
            {
                MessageBox.Show($"{ex.Message}", "Xml Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
#nullable disable
        private void MarkOldFileVersions(ref FixtureTreeViewItem group)
        {
            if (!group.Fixture.Autoincrement) { return; }
            string latest = GetLatestFileVersion(group);
            foreach(FixtureTreeViewItem file in group.Items)
            {
                if (!file.IsAutoVersionGreaterOrEqual(latest))
                {
                    file.Foreground = Brushes.Orange;
                    file.ToolTip = "Consider archiving previous file version.\n";
                    group.Foreground = Brushes.Orange;
                    group.ToolTip = "Consider archiving previous file version.\n";
                }
                else
                {
                    file.Foreground = Brushes.Black;
                    file.ToolTip = null;
                }
            }
        }

        private List<FixtureTreeViewItem> GetLatestFilesInGroup(FixtureTreeViewItem group)
        {
            List<FixtureTreeViewItem> latestFiles = new List<FixtureTreeViewItem>();
            string latest = GetLatestFileVersion(group);
            foreach(FixtureTreeViewItem item in group.Items)
            {
                if(item.IsAutoVersionGreaterOrEqual(latest))
                {
                    latestFiles.Add(item);
                }
            }
            return latestFiles;
        }

        private void PopulateFixtureTree(bool selectionOverride = false)
        {
            string currentItemParent = "";
            if (selectionOverride)
            { currentItemParent = Directory.GetParent(FixtureTreeSelectedItem().Path).FullName; }
            else { currentItemParent = FixtureTreeItemFolder(); }
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
                        MarkOldFileVersions(ref subFolderHeader);
                        fixtureHeader.Items.Add(subFolderHeader);
                    }
                }
                else
                {
                    foreach (string file in Directory.GetFiles(fixture.FixtureConfigsLocation))
                    {
                        string fileName = System.IO.Path.GetFileName(file);
                        if (Regex.IsMatch(fileName, fixture.ConfigRegex))
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
                    MarkOldFileVersions(ref fixtureHeader);
                }
                FixtureTreeView.Items.Add(fixtureHeader);
            }
            if (currentItemParent != "") 
            { ScrollToItem(currentItemParent); }
        }

        private void SaveEditorText()
        {
            if(currentDocument != "")
            {
                if(GetFileType(currentDocument) == "xml")
                {
                    //FixtureTreeViewItem thisItem = FixtureTreeSelectedItem();
                    FixtureTreeViewItem thisItem = currentItem;
                    XDocument document = GetXmlFromEditor();
                    if (document == null) { return; }
                    if ((thisItem.Fixture.Autoincrement & currentDocumentChangesMade == true))
                    {
                        string incrementedPath = Path.Combine(Directory.GetParent(currentDocument).FullName, thisItem.IncrementVersion());
                        switch (MessageBox.Show($"Incrementing file version from: {currentDocument}\nTo: {incrementedPath}\n\nConfirm?","Confirm Autoincrement",
                            MessageBoxButton.YesNoCancel, MessageBoxImage.Information)) //Confirm the auto-increment
                        {
                            case (MessageBoxResult.Yes):
                                document.Save(incrementedPath);
                                if (thisItem.Fixture.ArchivePreviousVersion)
                                {
                                    ArchiveCurrentItem();
                                }
                                XDocument newXDoc = XDocument.Load(incrementedPath);
                                DisplayXml(newXDoc);
                                SetCurrentDocument(incrementedPath, newXDoc);
                                break;
                            case (MessageBoxResult.No):
                                document.Save(currentDocument);
                                break;
                            case (MessageBoxResult.Cancel):
                                return;
                        }
                    }
                    else
                    {
                        if(currentItem.Fixture.ArchivePreviousVersion)
                        {
                            string archivePath = Path.Join(Directory.GetParent(currentItem.Path).FullName, "archive");
                            string archiveFileName = $"{Path.GetFileNameWithoutExtension(currentItem.Path)}_{DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss")}.{Path.GetExtension(currentItem.Path)}";
                            if (!Directory.Exists(archivePath)) { Directory.CreateDirectory(archivePath); }
                            File.Copy(currentItem.Path, Path.Join(archivePath,archiveFileName));
                        }
                        document.Save(currentDocument);
                        SetCurrentDocument(currentDocument, document);
                    }
                    currentDocumentChangesMade = false;
                }
            }
        }

        private void ScrollToItem(string targetItemParent)
        {
            foreach (FixtureTreeViewItem level1Item in FixtureTreeView.Items)
            {
                foreach (FixtureTreeViewItem level2Item in level1Item.Items)
                {
                    
                    if (level2Item.Items.Count > 0)
                    {
                        foreach (FixtureTreeViewItem level3Item in level2Item.Items)
                        {
                            string currentItemParent = Directory.GetParent(level3Item.Path).FullName;
                            if (currentItemParent == targetItemParent)
                            {
                                level1Item.IsExpanded = true;
                                level2Item.IsExpanded = true;
                                level2Item.IsSelected = true;
                                level2Item.BringIntoView();
                                return;
                            };
                        }
                    }
                    else
                    {
                        string currentItemParent = Directory.GetParent(level2Item.Path).FullName;
                        if (currentItemParent == targetItemParent)
                        {
                            level1Item.IsExpanded = true;
                            level1Item.IsSelected = true;
                            level1Item.BringIntoView();
                            return;
                        };
                    }
                }
            }
        }

        private void SetCurrentDocument(string document, XDocument xdoc)
        {
            if (File.Exists(document))
            {
                currentItem = FixtureTreeSelectedItem();
                currentDocument = document;
                currentXDocument = xdoc;
                CurrentFileTextBox.Text = document;
            }
        }
    }
}
