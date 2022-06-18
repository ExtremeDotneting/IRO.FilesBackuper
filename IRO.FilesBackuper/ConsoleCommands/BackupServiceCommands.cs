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

        [Action(Name = "backup")]
        public void ListIgnored(
            [Argument(LongName ="root" ,Help ="Path of folder to inspect.")]
            string rootDirPath = null,
            [Argument(LongName ="out" ,Help ="Directory path to save backup.")]
            string outputDirPath = null,
            bool verbose = true
            )
        {
            WrapAsync(async () =>
            {
                var backupService = new BackupService(FindFilesRule.Tracked);
                if (verbose)
                {
                    backupService.ProcessingMessageEvent += (msg) =>
                    {
                        Write(msg);
                    };
                }
                if (string.IsNullOrWhiteSpace(rootDirPath))
                {
                    rootDirPath = Environment.CurrentDirectory;
                }
                WriteWithColor($"Backuping directory: '{rootDirPath}'.", ConsoleColor.Yellow);
                await backupService.MakeBackup(rootDirPath, outputDirPath);
                WriteWithColor($"Backup finished.", ConsoleColor.Yellow);
            });
        }


        [Action(Name = "list")]
        public void ListIgnored(
            [Argument(LongName ="root" ,Help ="Path of folder to inspect.")]
            string rootDirPath = null,
            [Argument(Help ="Can be 'All', 'Ignored', 'Tracked'.")]
            FindFilesRule rule = FindFilesRule.Tracked,
            [Argument(LongName ="count_size", Help = "If set '1' will show total size of found files KB.")]
            bool countSize=false,
            [Argument(LongName ="skip_size", Help = "Value in KBytes. Files with size lower will be skipped in result.")]
            long skipSizeBelowKB=0,
            bool verbose = true
            )
        {
            WrapAsync(async () =>
            {
                var backupService = new BackupService(rule);
                if (verbose)
                {
                    backupService.ProcessingMessageEvent += (msg) =>
                    {
                        Write(msg);
                    };
                }

                if (string.IsNullOrWhiteSpace(rootDirPath))
                {
                    rootDirPath = Environment.CurrentDirectory;
                }
                WriteWithColor($"Inspecting directory: '{rootDirPath}'.", ConsoleColor.Yellow);
                var files = await backupService.FindFiles(rootDirPath);
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
                        .OrderBy(r => r.BytesSize)
                        .Reverse();
                    WriteAsJson(sizeFilteredFiles, ConsoleColor.Yellow);
                }
            });
        }

    }
}
