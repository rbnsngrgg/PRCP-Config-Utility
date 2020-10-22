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
        private readonly string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
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
