using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRO.Threading.AsyncLinq;
using MAB.DotIgnore;

namespace IRO.FilesBackuper.MainLogic
{
    public class GitignoreInspectService
    {
        private readonly FindFilesRule _findFilesRule;

        public event ProcessingMessageEventDelegate ProcessingMessageEvent;

        public GitignoreInspectService(FindFilesRule findFilesRule)
        {
            this._findFilesRule = findFilesRule;
        }

        public async Task<IList<string>> FindFiles(string rootFolderPath)
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
            await InspectRecursively(rootFolderPath, rootFolderPath, outputFiles, ignoreList);
            return outputFiles;
        }

        async Task InspectRecursively(
            string rootFolderPath,
            string currentFolderPath,
            List<string> outputFilesList,
            IgnoreList ignoreList
            )
        {
            //Skip folder if it ignored.
            var currentFolderRelativePath = ToRelativePath(rootFolderPath, currentFolderPath);
            RiseProcessingMessageEvent($"Inspecting directory '{currentFolderRelativePath}'.");
            if (_findFilesRule == FindFilesRule.Tracked && IsPathSkipped(ignoreList, currentFolderRelativePath, true))
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
            var filesPathFiltered = await filesPath.SelectAsync((fPath) =>
              {
                  var relFilePath = ToRelativePath(rootFolderPath, fPath);
                  RiseProcessingMessageEvent($"Inspecting file '{relFilePath}'.");
                  if (IsPathSkipped(ignoreList, relFilePath, false))
                      return null;
                  else
                      return fPath;
              });
            filesPathFiltered = filesPathFiltered.Where(r => r != null);
            lock (outputFilesList)
                outputFilesList.AddRange(filesPathFiltered);

            //Inspect subdirectories.
            var directories = Directory.GetDirectories(currentFolderPath);
            await directories.ForEachAsync(async (dirPath) =>
            {
                await InspectRecursively(rootFolderPath, dirPath, outputFilesList, ignoreList);
            });
        }

        string ToRelativePath(string rootFolderPath, string path)
        {
            var relFilePath = Path.GetRelativePath(rootFolderPath, path)
                  .Replace("\\", "/");
            return relFilePath;
        }

        bool IsPathSkipped(IgnoreList ignoreList, string path, bool pathIsDirectory)
        {
            //If root.
            if (path == ".")
                return false;

            if (_findFilesRule == FindFilesRule.Tracked)
            {
                return ignoreList.IsIgnored(path, pathIsDirectory);
            }
            else if (_findFilesRule == FindFilesRule.Ignored)
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

        protected void RiseProcessingMessageEvent(string msg)
        {
            ProcessingMessageEvent?.Invoke(msg);
        }
    }
}
