using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRO.FilesBackuper.MainLogic
{
    public delegate void ProgressEventDelegate(string rootFolderPath, int totalFilesCount, int processedFilesCount);

    public delegate void ProcessingMessageEventDelegate(string msg);
}
