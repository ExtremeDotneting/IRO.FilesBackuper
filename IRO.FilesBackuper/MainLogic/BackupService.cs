using IRO.FilesBackuper.MainLogic.Models;
using IRO.Threading.AsyncLinq;

namespace IRO.FilesBackuper.MainLogic
{
    public class BackupService : GitignoreInspectService
    {
        public BackupService(string rootFolderPath, FindFilesRule findFilesRule) : base(rootFolderPath, findFilesRule)
        {
        }

        //public event FilesProcessingProgressDelegate CopyProgressEvent;

        public void CopyFiles(string rootFolderPath, string outputFolder)
        {
            //Knees bullet check.
            if (!Directory.Exists(outputFolder))
            {
                throw new Exception($"Can't find directory '{outputFolder}'.");
            }
            var relPathToCopyFolder = Path.GetRelativePath(rootFolderPath, outputFolder);
            if (!relPathToCopyFolder.StartsWith("."))
            {
                throw new Exception($"Destination directory '{outputFolder}' is subdirectory of inspected '{rootFolderPath}'.");
            }

            var files = FindFiles();


        }

        public async Task<(IEnumerable<FileSizeInfo> List, long TotalBytesSize)> CountFilesSize(IEnumerable<string> filesPath)
        {
            RiseProcessingMessageEvent("Counting files size.");
            long totalSize = 0;
            var locker = new object();
            var fileSizes = await filesPath.SelectAsync((filePath) =>
            {
                var fileSizeInfo = new FileSizeInfo()
                {
                    BytesSize = new FileInfo(filePath).Length,
                    Path = filePath
                };
                lock(locker)
                    totalSize += fileSizeInfo.BytesSize;

                RiseProcessingMessageEvent($"Count size {fileSizeInfo.BytesSize} Bytes of '{fileSizeInfo.Path}'.");
                return fileSizeInfo;
            }, AsyncLinqCtx);
            return (fileSizes, totalSize);
        }

    }
}
