using IRO.FilesBackuper.MainLogic.Models;
using IRO.Threading.AsyncLinq;
using IRO.Common.Collections;

namespace IRO.FilesBackuper.MainLogic
{
    public class BackupService : GitignoreInspectService
    {
        public BackupService(FindFilesRule findFilesRule) : base(findFilesRule)
        {
        }

        //public event FilesProcessingProgressDelegate CopyProgressEvent;

        public async Task MakeBackup(string rootFolderPath, string outputFolder)
        {
            //Knees bullet check.
            if (outputFolder is null)
            {
                throw new ArgumentNullException(nameof(outputFolder));
            }
            var relPathToCopyFolder = Path.GetRelativePath(rootFolderPath, outputFolder);
            if (!relPathToCopyFolder.StartsWith(".") && (rootFolderPath[0] == outputFolder[0]))
            {
                throw new Exception($"Destination directory '{outputFolder}' is subdirectory of inspected '{rootFolderPath}'.");
            }
            //--- 

            //Get files list
            var files = await FindFiles(rootFolderPath);

            var fileCopyInfoList = await files.SelectAsync((fPath) =>
            {
                var relPath = Path.GetRelativePath(rootFolderPath, fPath);
                var destFile = Path.Combine(outputFolder, relPath);
                var destDir = Path.GetDirectoryName(destFile);
                return new FileCopyPathInfo()
                {
                    RelativePath = relPath,
                    SourceFile = fPath,
                    DestFile = destFile,
                    DestDir = destDir
                };
            });

            //Remove old bakcup
            if (Directory.Exists(outputFolder))
            {
                RiseProcessingMessageEvent($"Remove old backup folder '{outputFolder}'.");
                DeleteDirectoryRecursively(outputFolder);
            }
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            //Create new folders for all files.
            var directoriesOfFiles = fileCopyInfoList
                .Select(r => r.DestDir)
                .Distinct();
            directoriesOfFiles.ForEach(dPath =>
            {
                CreateDirectoryRecursively(dPath);
            });

            //Copy files.
            int currentFileNum = 0;
            await fileCopyInfoList.ForEachAsync(async (fileCopyInfo) =>
            {
                try
                {
                    RiseProcessingMessageEvent($"Start copy file '{fileCopyInfo.RelativePath}.");
                    var persents = currentFileNum++ * 100 / files.Count;
                    RiseProcessingMessageEvent($"Copy progress {persents} %.");
                    File.Copy(fileCopyInfo.SourceFile, fileCopyInfo.DestFile);
                }
                catch (Exception ex)
                {
                    RiseProcessingMessageEvent($"File copy exception '{ex}'.\nPath: '{fileCopyInfo.RelativePath}'.");
                }
            });
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
                lock (locker)
                    totalSize += fileSizeInfo.BytesSize;

                RiseProcessingMessageEvent($"Count size {fileSizeInfo.BytesSize} Bytes of '{fileSizeInfo.Path}'.");
                return fileSizeInfo;
            });
            return (fileSizes, totalSize);
        }

        void CreateDirectoryRecursively(string path)
        {
            var pathParts = path
                .Replace("/", "\\")
                .Split('\\');

            for (int i = 0; i < pathParts.Length; i++)
            {
                if (i > 0)
                    pathParts[i] = Path.Combine(pathParts[i - 1], pathParts[i]);

                if (!Directory.Exists(pathParts[i]))
                    Directory.CreateDirectory(pathParts[i]);
            }
        }

        void DeleteDirectoryRecursively(string path)
        {
            try
            {
                var subdirs = Directory.GetDirectories(path);
                if (subdirs.Any())
                {
                    foreach (var dPath in subdirs)
                    {
                        RiseProcessingMessageEvent($"Removing old backup file '{dPath}'.");
                        DeleteDirectoryRecursively(dPath);
                    };
                }

                var subFiles = Directory.GetFiles(path);
                if (subFiles.Any())
                {
                    foreach (var fPath in subFiles)
                    {
                        RiseProcessingMessageEvent($"Removing old backup file '{fPath}'.");
                        File.Delete(fPath);
                    }

                }
                Directory.Delete(path, true);
            }
            catch (Exception ex)
            {
                RiseProcessingMessageEvent($"Error cleaning dir '{path}'.\n Error: '{ex}'.");
            }
        }

        /////// <summary> 
        /////// Fast file move with big buffers
        /////// https://www.codeproject.com/Tips/777322/A-Faster-File-Copy
        /////// </summary>
        /////// <param name="source">Source file path</param> 
        /////// <param name="destination">Destination file path</param> 
        //void FileCopy(string source, string destination)
        //{
        //    int array_length = (int)Math.Pow(2, 19);
        //    byte[] dataArray = new byte[array_length];
        //    using (FileStream fsread = new FileStream
        //    (source, FileMode.Open, FileAccess.Read, FileShare.None, array_length))
        //    {
        //        using (BinaryReader bwread = new BinaryReader(fsread))
        //        {
        //            using (FileStream fswrite = new FileStream
        //            (destination, FileMode.Create, FileAccess.Write, FileShare.None, array_length))
        //            {
        //                using (BinaryWriter bwwrite = new BinaryWriter(fswrite))
        //                {
        //                    for (; ; )
        //                    {
        //                        int read = bwread.Read(dataArray, 0, array_length);
        //                        if (0 == read)
        //                            break;
        //                        bwwrite.Write(dataArray, 0, read);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
