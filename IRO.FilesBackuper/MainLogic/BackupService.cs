using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

            //Inspect
            var outputFiles = new List<string>();
            var ignores = ImmutableList.Create<IgnoreList>();
            InspectRecursively(rootFolderPath, rootFolderPath, outputFiles, ignores, findFilesRule);

            return outputFiles;
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

        void InspectRecursively(
            string rootFolderPath,
            string currentFolderPath,
            List<string> outputFilesList,
            ImmutableList<IgnoreList> ignores,
            FindFilesRule findFilesRule)
        {
            //Skip folder if it ignored.
            var currentFolderRelativePath = ToRelativePath(rootFolderPath, currentFolderPath);
            if (IsPathSkipped(ignores, findFilesRule, currentFolderRelativePath, true))
            {
                return;
            }

            //Add rules if template file exists.
            var templateFilePath = Path.Combine(currentFolderPath, BackuperConsts.TemplateFileName);
            if (File.Exists(templateFilePath))
            {
                var newIgnoreList = new IgnoreList(templateFilePath);
                ignores = ignores.Add(newIgnoreList);
            }

            //Add files to list.
            var filesPath = Directory.GetFiles(currentFolderPath);
            foreach (var filePath in filesPath)
            {
                var relFilePath = ToRelativePath(rootFolderPath, filePath);
                if (!IsPathSkipped(ignores, findFilesRule, relFilePath, false))
                {
                    outputFilesList.Add(relFilePath);
                }
            }

            //Inspect subdirectories.
            var directories = Directory.GetDirectories(currentFolderPath);
            foreach (var dirPath in directories)
            {
                InspectRecursively(rootFolderPath, dirPath, outputFilesList, ignores, findFilesRule);
            }
        }

        string ToRelativePath(string rootFolderPath, string path)
        {
            var relFilePath = Path.GetRelativePath(rootFolderPath, path)
                  .Replace("\\", "/");
            return relFilePath;
        }

        bool IsPathSkipped(ImmutableList<IgnoreList> ignores, FindFilesRule findFilesRule, string path, bool pathIsDirectory)
        {
            if (findFilesRule == FindFilesRule.Tracked)
            {
                bool hasOneIsIgnored = false;
                foreach (var ignoreList in ignores)
                {
                    if (ignoreList.IsIgnored(path, pathIsDirectory))
                    {
                        hasOneIsIgnored = true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return hasOneIsIgnored;
            }
            else if (findFilesRule == FindFilesRule.Ignored)
            {
                foreach (var ignoreList in ignores)
                {
                    if (ignoreList.IsIgnored(path, pathIsDirectory))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
