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
        private readonly string _rootFolderPath;
        private readonly FindFilesRule _findFilesRule;

        protected AsyncLinqContext AsyncLinqCtx { get; } //= AsyncLinqContext.Create(5);

        public event ProcessingMessageEventDelegate ProcessingMessageEvent;

        public GitignoreInspectService(string rootFolderPath, FindFilesRule findFilesRule)
        {
            this._rootFolderPath = rootFolderPath;
            this._findFilesRule = findFilesRule;
        }

        public async Task<IList<string>> FindFiles()
        {
            if (!Directory.Exists(_rootFolderPath))
            {
                throw new Exception($"Can't find directory '{_rootFolderPath}'.");
            }

            //Init ignore list
            var initialIgnoreFilePath = Path.Combine(_rootFolderPath, BackuperConsts.InitialFileName);
            File.WriteAllText(initialIgnoreFilePath, "");
            var ignoreList = new IgnoreList(initialIgnoreFilePath);
            File.Delete(initialIgnoreFilePath);

            //Inspect
            var outputFiles = new List<string>();
            await InspectRecursively(_rootFolderPath, outputFiles, ignoreList);
            return outputFiles;
        }

        async Task InspectRecursively(
            string currentFolderPath,
            List<string> outputFilesList,
            IgnoreList ignoreList
            )
        {
            //Skip folder if it ignored.
            var currentFolderRelativePath = ToRelativePath(currentFolderPath);
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
                ignoreList = ApplyTemplateRules(_rootFolderPath, currentFolderPath, rules, ignoreList);
            }

            //Add files to list.
            var filesPath = Directory.GetFiles(currentFolderPath);
            var filesPathFiltered = await filesPath.SelectAsync((fPath) =>
              {
                  var relFilePath = ToRelativePath(fPath);
                  RiseProcessingMessageEvent($"Inspecting file '{relFilePath}'.");
                  if (IsPathSkipped(ignoreList, relFilePath, false))
                      return null;
                  else
                      return fPath;
              }, AsyncLinqCtx);
            filesPathFiltered = filesPathFiltered.Where(r => r != null);
            lock (outputFilesList)
                outputFilesList.AddRange(filesPathFiltered);

            //Inspect subdirectories.
            var directories = Directory.GetDirectories(currentFolderPath);
            foreach(var dirPath in directories)
            {
                await InspectRecursively(dirPath, outputFilesList, ignoreList);
            }
        }

        string ToRelativePath(string path)
        {
            var relFilePath = Path.GetRelativePath(_rootFolderPath, path)
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
