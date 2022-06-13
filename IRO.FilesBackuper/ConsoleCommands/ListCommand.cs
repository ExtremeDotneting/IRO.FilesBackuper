﻿using IRO.FilesBackuper.MainLogic;
using IRO.FilesBackuper.SysCommandExtensions;
using SysCommand.ConsoleApp;
using SysCommand.Mapping;

namespace IRO.FilesBackuper.ConsoleCommands
{
    public class BackupServiceCommands : BaseCommand
    {
        readonly BackupService _backupService;

        public BackupServiceCommands()
        {
            _backupService = new BackupService();
        }


        [Action(Name = "list")]
        public void ListIgnored(
            [Argument(LongName ="path" ,Help ="Path of folder to inspect.")]
            string rootDirPath = null,
            [Argument(Help ="Can be 'All', 'Ignored', 'Tracked'.")]
            FindFilesRule rule = FindFilesRule.Tracked,
            [Argument(LongName ="count_size", Help = "If set '1' it will show total size of found files KB.")]
            bool countSize=false
            )
        {
            if (string.IsNullOrWhiteSpace(rootDirPath))
            {
                rootDirPath = Environment.CurrentDirectory;
            }
            Write($"Inspectiong directory: '{rootDirPath}'.");
            var files = _backupService.FindFiles(rootDirPath, rule);
            //files = _backupService.FullPathToRelative(files, path);
            WriteAsJson(files);

            if (countSize)
            {
                long totalLengthBytes = 0;
                foreach (var filePath in files)
                {
                    var fullFilePath = Path.Combine(rootDirPath, filePath);
                    var lengthBytes = new FileInfo(fullFilePath).Length;
                    totalLengthBytes += lengthBytes;
                }

                long totalKB = totalLengthBytes / 1024;
                WriteWithColor($"Total files size is {totalKB} KB.", ConsoleColor.Yellow);
            }
        }

    }
}