namespace IRO.FilesBackuper.MainLogic
{
    public class BackupService : GitignoreInspectService
    {
        public event FilesProcessingProgressDelegate CopyProgressEvent;

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

            var files = FindFiles(rootFolderPath, FindFilesRule.Tracked);


        }
    }
}
