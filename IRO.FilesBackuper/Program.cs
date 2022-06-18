using SysCommand.ConsoleApp;
using SysCommand.Mapping;

namespace IRO.FilesBackuper
{
    public class Program
    {
#if DEBUG
        static string[] TestCommands { get; } = new[]
        {
            //"list --root=\"D:\\cf\\CODE_PROJECTS\\IRO_TOOLS\\IRO.FilesBackuper\\TestFiles\"",
            //"list --root=\"D:\\cf\\CODE_PROJECTS\\IRO_TOOLS\\IRO.FilesBackuper\\TestFiles\" --rule=Ignored",
            //"list --root=\"D:\\cf\\CODE_PROJECTS\\IRO_TOOLS\\IRO.FilesBackuper\\TestFiles\" --rule=All",
            //"list --root=\"D:\\cf\\CODE_PROJECTS\\MauDay_ECom\\ECom.MauDauGW\" --count_size=1",
            //"list --root=\"D:\\cf\\CODE_PROJECTS\\MauDay_ECom\\ECom.MauDauGW\" --count_size=1 --rule=Ignored",
            //"list --root=\"D:\\cf\" --count_size=1 --skip_size=5000",
            //"list --root=\"D:\\cf\\CODE_PROJECTS\\IRO_TOOLS\" --count_size=1",
            //"list --root=\"D:\\cf\" --rule=Ignored",
            //"list --root=\"D:\\cf\\CODE_PROJECTS\\WORK_PROJECTS\\DiplomaStatsCounter\\Frontend\"",
            //"backup --root=\"D:\\cf\\Chromes\" --out=\"F:\\cf_Backup\\Chromes\""
            //"backup --root=\"D:\\cf\\CODE_PROJECTS\" --out=\"F:\\cf_Backup\\CODE_PROJECTS\"",
            //"backup --root=\"D:\\cf\\CODE_PROJECTS\" --out=\"D:\\cf_Backup\\CODE_PROJECTS\""
            //"backup --root=\"D:\\cf\\Chromes\" --out=\"D:\\cf_Backup\\Chromes\""            
            "list --root=\"D:\\cf\\Chromes\" --count_size=1 --skip_size=1024",
        };
#endif

        public static int Main(string[] args)
        {

            var myApp = new App();
#if DEBUG  
            if (TestCommands.Any())
            {
                Console.WriteLine("Will execute list of test commands.");
                foreach (var testCmdStr in TestCommands)
                {
                    Console.WriteLine("\n----------\nExecute: " + testCmdStr);
                    myApp.Run(testCmdStr);
                }
                Console.WriteLine("Test commands was executed. Press enter to exit.");
                Console.ReadLine();
                return myApp.Console.ExitCode;
            }
#endif

            if (args.Length == 0)
            {
                return App.RunApplication(() =>
                {
                    return myApp;
                });
            }
            else
            {
                myApp.Run(args);
                return myApp.Console.ExitCode;
            }
        }
    }

}