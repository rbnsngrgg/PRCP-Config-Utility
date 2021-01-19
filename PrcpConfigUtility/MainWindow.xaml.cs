using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

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
            PopulateFixtureTree();
            Title = $"PRCP Config Manager {version}";
        }

        private void FixtureTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e) //Open selected xml in the editor
        {
            if(currentDocumentChangesMade & currentDocument !="")
            {
                switch (MessageBox.Show("Do you wish to save changes to the current document?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Information))
                {
                    case MessageBoxResult.Yes:
                        {
                            SaveEditorText();
                            break;
                        }
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }
            if(FixtureTreeIsFileSelected())
            {
                currentItem = FixtureTreeSelectedItem();
                string selectedPath = FixtureTreeItemPath();
                if (GetFileType(selectedPath) == "xml")
                {
                    XDocument xDocument = XDocument.Load(selectedPath);
                    DisplayXml(xDocument);
                    SetCurrentDocument(selectedPath, xDocument);
                }
            }
            //ForEachTreeItemVoid(WriteLineIfMatch);
            GetTreeViewItemParent(FixtureTreeSelectedItem());
        }

        private void EditorSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveEditorText();
        }

        private void ConfigEditorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentDocumentChangesMade = true;
        }

        private void FixtureTreeOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            string folder = FixtureTreeItemFolder();
            if (folder != "") { Process.Start("explorer", folder); }
        }

        private void FixtureTreeArchiveFile_Click(object sender, RoutedEventArgs e)
        {
            ArchiveCurrentItem(true);
        }

        private void FixtureTreeContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if(FixtureTreeIsFileSelected())
            {
                FixtureTreeArchiveFile.IsEnabled = true;
                FixtureTreeCopyFile.IsEnabled = true;
            }
            else
            {
                FixtureTreeArchiveFile.IsEnabled = false;
                FixtureTreeCopyFile.IsEnabled = false;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateFixtureTree();
        }

        private void FixtureTreeCopyFile_Click(object sender, RoutedEventArgs e)
        {
            FixtureTreeCopy();
        }

        private void FixtureTreeViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(FixtureTreeView.SelectedItem == null) { return; }
            MultiSelectTreeView senderTreeView = sender as MultiSelectTreeView;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if(((FixtureTreeViewItem)FixtureTreeView.SelectedItem).IsSelected)
                {
                    ((FixtureTreeViewItem)FixtureTreeView.SelectedItem).IsSelected = false;
                }
                else { ((FixtureTreeViewItem)FixtureTreeView.SelectedItem).IsSelected = true; }
            }
            else
            { 
                foreach(FixtureTreeViewItem item in FixtureTreeView.Items)
                {
                    item.IsSelected = false;
                    item.DeselectChildren();
                }
                ((FixtureTreeViewItem)FixtureTreeView.SelectedItem).IsSelected = true;
            }
            int selectedItems = 0;
            foreach (FixtureTreeViewItem item in FixtureTreeView.Items)
            {
                if(item.IsSelected)
                {
                    selectedItems += 1;
                }
                selectedItems += item.GetChildrenSelectedCount();
            }
        }

        private void FixtureTreeIncrementRev_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(((FixtureTreeViewItem)FixtureTreeView.SelectedItem).Header);
            Debug.WriteLine(((FixtureTreeViewItem)FixtureTreeView.SelectedItem).IncrementVersion());
        }
    }
}
