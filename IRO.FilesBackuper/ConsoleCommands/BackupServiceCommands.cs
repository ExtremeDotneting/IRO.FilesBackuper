using IRO.FilesBackuper.MainLogic;
using IRO.FilesBackuper.MainLogic.Models;
using IRO.FilesBackuper.SysCommandExtensions;
using SysCommand.ConsoleApp;
using SysCommand.Mapping;
using IRO.Threading.AsyncLinq;

namespace IRO.FilesBackuper.ConsoleCommands
{
    public class BackupServiceCommands : BaseCommand
    {
        public BackupServiceCommands()
        {

        }


        [Action(Name = "list")]
        public void ListIgnored(
            [Argument(LongName ="path" ,Help ="Path of folder to inspect.")]
            string rootDirPath = null,
            [Argument(Help ="Can be 'All', 'Ignored', 'Tracked'.")]
            FindFilesRule rule = FindFilesRule.Tracked,
            [Argument(LongName ="count_size", Help = "If set '1' will show total size of found files KB.")]
            bool countSize=false,
            [Argument(LongName ="skip_size", Help = "Value in KBytes. Files with size lower will be skipped in result.")]
            long skipSizeBelowKB=0
            )
        {
            Task.Run(async () =>
            {
                if (string.IsNullOrWhiteSpace(rootDirPath))
                {
                    rootDirPath = Environment.CurrentDirectory;
                }
                WriteWithColor($"Inspecting directory: '{rootDirPath}'.", ConsoleColor.Yellow);
                var backupService = new BackupService(rootDirPath, rule);
                backupService.ProcessingMessageEvent += (msg) =>
                {
                    Write(msg);
                };
                var files = await backupService.FindFiles();
                //files = _backupService.FullPathToRelative(files, path);
                WriteAsJson(files, ConsoleColor.Yellow);

                IEnumerable<FileSizeInfo> fileSizesInfos = null;
                long totalBytes = 0;

                if (countSize)
                {
                    (fileSizesInfos, totalBytes) = await backupService.CountFilesSize(files);
                    long totalKB = totalBytes / 1024;
                    WriteWithColor($"Total files size is {totalKB} KB.", ConsoleColor.Yellow);
                }
                if (skipSizeBelowKB > 0)
                {
                    if (fileSizesInfos == null)
                        (fileSizesInfos, totalBytes) = await backupService.CountFilesSize(files);
                    WriteWithColor($"Skipped files lower than {skipSizeBelowKB} KB.", ConsoleColor.Yellow);
                    var skipSizeBelowBytes = skipSizeBelowKB * 1024;
                    var sizeFilteredFiles = fileSizesInfos
                    .Where(r => r.BytesSize > skipSizeBelowBytes)
                    .OrderBy(r=>r.BytesSize)
                    .Reverse();
                    WriteAsJson(sizeFilteredFiles, ConsoleColor.Yellow);
                }
            }).Wait();
        }

    }
}
