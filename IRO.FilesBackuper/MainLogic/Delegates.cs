using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRO.FilesBackuper.MainLogic.Delegates
{
    public delegate void FilesProcessingProgressDelegate(string rootFolderPath, int totalFilesCount, int processedFilesCount);
}
