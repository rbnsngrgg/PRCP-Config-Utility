using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PrcpConfigUtility
{
    class FixtureTreeViewItem : TreeViewItem
    {
        public string Path {get;set;}
        public PcuFixture Fixture { get; set; }
        public string GetAutoIncrementVersion()
        {
            string match = "";
            string fileName = System.IO.Path.GetFileName(Path);
            foreach(Match regexMatch in Regex.Matches(fileName, Fixture.FileNameSections[Fixture.Grouping["IncrementBy"]]))
            {
                match = regexMatch.Value;
            }
            return match;
        }
        public string IncrementVersion() //return file name with new version
        {
            string version = GetAutoIncrementVersion();
            string fileName = System.IO.Path.GetFileName(Path);
            string newFileName = "";
            string newVersion = "99";
            string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if(alpha.Contains(version.ToUpper()))
            {
                newVersion = alpha.ElementAt(alpha.IndexOf(version.ToUpper()) + 1).ToString();
            }
            else
            {
                newVersion = (int.Parse(version) + 1).ToString();
            }
            newFileName = Regex.Replace(fileName, Fixture.FileNameSections[Fixture.Grouping["IncrementBy"]], newVersion);
            Debug.WriteLine($"{fileName}\n{newFileName}");
            return newFileName;
        }
    }
}
