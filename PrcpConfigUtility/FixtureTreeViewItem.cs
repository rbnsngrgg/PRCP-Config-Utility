using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;

namespace PrcpConfigUtility
{
    public class FixtureTreeViewItem : TreeViewItem
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private bool isSelected;

        public new bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                if(isSelected)
                {
                    Background = Brushes.LightBlue;
                }
                else
                {
                    Background = Brushes.Transparent;
                }
            }
        }
        public string Path {get;set;}
        public PcuFixture Fixture { get; set; }

        public void DeselectChildren()
        {
            foreach(FixtureTreeViewItem item in Items)
            {
                item.IsSelected = false;
                item.DeselectChildren();
            }
        }
        public int GetChildrenSelectedCount()
        {
            int selected = 0;
            foreach(FixtureTreeViewItem child in Items)
            {
                if(child.IsSelected)
                {
                    selected += 1;
                }
                selected += child.GetChildrenSelectedCount();
            }
            return selected;
        }
        public List<FixtureTreeViewItem> GetChildrenSelectedList()
        {
            List<FixtureTreeViewItem> selected = new List<FixtureTreeViewItem>();
            foreach (FixtureTreeViewItem child in Items)
            {
                if (child.IsSelected)
                {
                    selected.Add(child);
                }
                selected.AddRange(child.GetChildrenSelectedList());
            }
            return selected;
        }
        public string GetAutoIncrementVersion()
        {
            if(!Fixture.Autoincrement) { return ""; }
            string match = "";
            string fileName = System.IO.Path.GetFileName(Path);
            foreach(Match regexMatch in Regex.Matches(fileName, Fixture.FileNameSections[Fixture.Grouping["IncrementBy"]]))
            {
                match = regexMatch.Value;
            }
            return match;
        }
        public string GetGroupMatchString() //Return substring of file name that matches the group matching parameters in config
        {
            string fileName = System.IO.Path.GetFileName(Path);
            Match match;
            if (Fixture.GroupByRegex)
            {
                match =  Regex.Match(fileName, Fixture.Grouping["GroupBy"]);
            }
            else
            {
                match = Regex.Match(fileName, Fixture.FileNameSections[Fixture.Grouping["GroupBy"]]);
            }
            if(match.Success)
            {
                return match.Value;
            }
            return "";
        }
        public string IncrementVersion() //return file name with new version
        {
            if (!File.Exists(Path)) { return ""; }
            string version = GetAutoIncrementVersion();
            string fileName = System.IO.Path.GetFileName(Path);
            string newFileName = "";
            string newVersion = "99";
            if(alpha.Contains(version.ToUpper()))
            {
                newVersion = alpha.ElementAt(alpha.IndexOf(version.ToUpper()) + 1).ToString();
            }
            else
            {
                newVersion = (int.Parse(version) + 1).ToString();
            }
            newFileName = Regex.Replace(fileName, Fixture.FileNameSections[Fixture.Grouping["IncrementBy"]], newVersion);
            return newFileName;
        }
        public bool IsAutoVersionGreaterOrEqual(string version)
        {
            int thisVersion;
            int compareVersion;
            if(alpha.Contains(version) & alpha.Contains(GetAutoIncrementVersion()))
            {
                if(alpha.IndexOf(GetAutoIncrementVersion()) >= alpha.IndexOf(version))
                {
                    return true;
                }
            }
            else
            {
                if(int.TryParse(GetAutoIncrementVersion(), out thisVersion) & int.TryParse(version, out compareVersion))
                {
                    if(thisVersion >= compareVersion)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
