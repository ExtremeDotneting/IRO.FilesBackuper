using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MAB.DotIgnore;

namespace IRO.FilesBackuper.MainLogic
{
    public class GitignoreInspectService
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
            var outputFiles = new List<string>();
            InspectRecursively(rootFolderPath, rootFolderPath, outputFiles, ignoreList, findFilesRule);



            return outputFiles;
        }

        void InspectRecursively(
            string rootFolderPath,
            string currentFolderPath,
            List<string> outputFilesList,
            IgnoreList ignoreList,
            FindFilesRule findFilesRule
            )
        {
            //Skip folder if it ignored.
            var currentFolderRelativePath = ToRelativePath(rootFolderPath, currentFolderPath);
            if (findFilesRule == FindFilesRule.Tracked && IsPathSkipped(ignoreList, findFilesRule, currentFolderRelativePath, true))
            {
                return;
            }

            //Add rules if template file exists.
            var templateFilePath = Path.Combine(currentFolderPath, BackuperConsts.TemplateFileName);
            if (File.Exists(templateFilePath))
            {
                var rules = File.ReadAllLines(templateFilePath);
                ignoreList = ApplyTemplateRules(rootFolderPath, currentFolderPath, rules, ignoreList);
            }

            //Add files to list.
            var filesPath = Directory.GetFiles(currentFolderPath);
            foreach (var filePath in filesPath)
            {
                var relFilePath = ToRelativePath(rootFolderPath, filePath);
                if (!IsPathSkipped(ignoreList, findFilesRule, relFilePath, false))
                {
                    outputFilesList.Add(relFilePath);
                }
            }

            //Inspect subdirectories.
            var directories = Directory.GetDirectories(currentFolderPath);
            foreach (var dirPath in directories)
            {
                InspectRecursively(rootFolderPath, dirPath, outputFilesList, ignoreList, findFilesRule);
            }
        }

        string ToRelativePath(string rootFolderPath, string path)
        {
            var relFilePath = Path.GetRelativePath(rootFolderPath, path)
                  .Replace("\\", "/");
            return relFilePath;
        }

        bool IsPathSkipped(IgnoreList ignoreList, FindFilesRule findFilesRule, string path, bool pathIsDirectory)
        {
            //If root.
            if (path == ".")
                return false;

            if (findFilesRule == FindFilesRule.Tracked)
            {
                return ignoreList.IsIgnored(path, pathIsDirectory);
            }
            else if (findFilesRule == FindFilesRule.Ignored)
            {
                return !ignoreList.IsIgnored(path, pathIsDirectory);
            }
            return false;
        }

        IgnoreList ApplyTemplateRules(string rootFolderPath, string currentFolderPath, IEnumerable<string> rules, IgnoreList ignoreList)
        {
            var relativePath = Path.GetRelativePath(rootFolderPath, currentFolderPath);
            var rulePrefix = relativePath.Replace("\\", "/");
            if (!rulePrefix.EndsWith("/"))
            {
                rulePrefix = rulePrefix + "/";
            }


            var clonedIngoreList = (IgnoreList)ignoreList.Clone();
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


                clonedIngoreList.AddRule(rule);
            }

            return clonedIngoreList;
        }
    }
}
