using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MAB.DotIgnore;

namespace IRO.FilesBackuper.MainLogic
{
    internal class BackupService
    {
        public IList<string> FindFiles(string rootFolderPath, FindFilesRule findFilesRule)
        {

            if (!Directory.Exists(rootFolderPath))
            {
                throw new Exception($"Can't find directory '{rootFolderPath}'.");
            }

            //Init ignore list
            var initialIgnoreFilePath = Path.Combine(rootFolderPath, BackuperConsts.InitialFileName);
            File.WriteAllText(initialIgnoreFilePath, "");
            var ignoreList = new IgnoreList(initialIgnoreFilePath);
            File.Delete(initialIgnoreFilePath);

            //Inspect
            var allFilesList = new List<string>();
            InspectRecursively(rootFolderPath, rootFolderPath, allFilesList, ignoreList);

            //Generate result
            if (findFilesRule == FindFilesRule.All)
            {
                return allFilesList;
            }
            else if (findFilesRule == FindFilesRule.Ignored)
            {
                var ignoredFilesList = new List<string>();
                foreach (var file in allFilesList)
                {
                    if (IsIgnored(ignoreList, file))
                    {
                        ignoredFilesList.Add(file);
                    }
                }
                return ignoredFilesList;
            }
            else if (findFilesRule == FindFilesRule.Tracked)
            {
                var trackedFilesList = new List<string>();
                foreach (var file in allFilesList)
                {
                    if (!IsIgnored(ignoreList, file))
                    {
                        trackedFilesList.Add(file);
                    }
                }
                return trackedFilesList;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(findFilesRule));
            }
        }

        //public IList<string> FullPathToRelative(ICollection<string> fullPathList, string relativeTo)
        //{
        //    var resList = new List<string>(fullPathList.Count);
        //    foreach (var path in fullPathList)
        //    {
        //        var newPath = Path.GetRelativePath(relativeTo, path);
        //        resList.Add(newPath);
        //    }
        //    return resList;
        //}

        void InspectRecursively(string rootFolderPath, string currentFolderPath, List<string> allFilesList, IgnoreList ignoreList)
        {
            //Add rules if template file exists.
            var templateFilePath = Path.Combine(currentFolderPath, BackuperConsts.TemplateFileName);
            if (File.Exists(templateFilePath))
            {
                var rules = File.ReadAllLines(templateFilePath);
                ApplyTemplateRules(rootFolderPath, currentFolderPath, rules, ignoreList);
            }

            //Add files to list.
            var filesPath = Directory.GetFiles(currentFolderPath);
            foreach (var filePath in filesPath)
            {
                var relFilePath = Path.GetRelativePath(rootFolderPath, filePath)
                    .Replace("\\", "/");
                allFilesList.Add(relFilePath);
            }

            //Inspect subdirectories.
            var directories = Directory.GetDirectories(currentFolderPath);
            foreach (var dirPath in directories)
            {
                InspectRecursively(rootFolderPath, dirPath, allFilesList, ignoreList);
            }
        }

        bool IsIgnored(IgnoreList ignoreList, string path)
        {         
            var isIgnored = ignoreList.IsIgnored(path, pathIsDirectory: false);
            return isIgnored;
        }

        void ApplyTemplateRules(string rootFolderPath, string currentFolderPath, IEnumerable<string> rules, IgnoreList ignoreList)
        {
            var relativePath = Path.GetRelativePath(rootFolderPath, currentFolderPath);
            var rulePrefix = relativePath.Replace("\\", "/");
            if (!rulePrefix.EndsWith("/"))
            {
                rulePrefix = rulePrefix + "/";
            }

            foreach (var r in rules)
            {
                if (string.IsNullOrWhiteSpace(r))
                    continue;
                var rule = r;
                bool isNotIgnoreRule = false;
                if (rule.StartsWith("!"))
                {
                    rule = rule.Substring(1);
                    isNotIgnoreRule = true;
                }
                if (rule.StartsWith("/") || rule.StartsWith("\\"))
                {
                    rule = rule.Substring(1);
                }


                if (rulePrefix != "./")
                    rule = rulePrefix + rule;
                if (isNotIgnoreRule)
                {
                    rule = "!" + rule;
                }

                ignoreList.AddRule(rule);
            }
        }
    }
}
