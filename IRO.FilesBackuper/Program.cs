using SysCommand.ConsoleApp;
using SysCommand.Mapping;

namespace IRO.FilesBackuper
{
    public class Program
    {
#if DEBUG
        static string[] TestCommands { get; } = new[]
        {
            "list --path=\"D:\\cf\\CODE_PROJECTS\\IRO_TOOLS\\IRO.FilesBackuper\\TestFiles\"",
            "list --path=\"D:\\cf\\CODE_PROJECTS\\IRO_TOOLS\\IRO.FilesBackuper\\TestFiles\" --rule=Ignored",
            "list --path=\"D:\\cf\\CODE_PROJECTS\\IRO_TOOLS\\IRO.FilesBackuper\\TestFiles\" --rule=All",
            //"list --path=\"D:\\cf\\CODE_PROJECTS\\MauDay_ECom\\ECom.MauDauGW\" --count_size=1",
            //"list --path=\"D:\\cf\\CODE_PROJECTS\\MauDay_ECom\\ECom.MauDauGW\" --count_size=1 --rule=Ignored",
            //"list --path=\"D:\\cf\"",
            //"list --path=\"D:\\cf\" --rule=Ignored",
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