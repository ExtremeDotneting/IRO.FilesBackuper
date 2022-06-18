namespace IRO.FilesBackuper.MainLogic.Models
{
    internal class FileCopyPathInfo
    {
        public string SourceFile { get; set; }

        public string DestFile { get; set; }

        public string DestDir { get; set; }

        public string RelativePath { get; set; }
    }
}
