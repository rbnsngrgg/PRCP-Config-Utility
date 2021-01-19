using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

namespace PrcpConfigUtility
{
    /// <summary>
    /// Interaction logic for CopyFileDialog.xaml
    /// </summary>
    public partial class CopyFileDialog : Window
    {
        public CopyFileDialog()
        {
            InitializeComponent();
        }

        public FixtureTreeViewItem currentItem;
        private void CopySelectOK_Click(object sender, RoutedEventArgs e)
        {
            if(!Regex.Match(CopyFileTextBox.Text,currentItem.Fixture.ConfigRegex).Success)
            {
                switch (MessageBox.Show($"The specified file name won't be recognized by the fixture's RegEx configuration. Continue with this file name?", "File Name RegEx",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning))
                {
                    case MessageBoxResult.No:
                    case MessageBoxResult.Cancel:
                    case MessageBoxResult.None:
                        return;
                }
            }
            this.DialogResult = true;
        }

        private void CopySelectCancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;
    }
}
