using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SysCommand.ConsoleApp;

namespace IRO.FilesBackuper.SysCommandExtensions
{
    public abstract class BaseCommand : Command
    {
        public ConsoleWrapper Console => App.Console;

        public void WriteAsJson(object obj, ConsoleColor? fontColor = null)
        {
            var str = JsonConvert.SerializeObject(obj, Formatting.Indented);
            if (fontColor.HasValue)
                WriteWithColor(str, fontColor.Value);
            else
                Write(str);
        }

        protected void WrapAsync(Func<Task> func)
        {
            Task.Run(async () =>
            {
                try
                {
                    await func();
                }
                catch (Exception ex)
                {
                    WriteWithColor(ex.ToString(), ConsoleColor.Red);
                    Console.Read();
                    throw;
                }
            }).Wait(); ;
        }

        protected void WrapAsync(Action act)
        {
            try
            {
                act();
            }
            catch (Exception ex)
            {
                WriteWithColor(ex.ToString(), ConsoleColor.Red);
                Console.Read();
            }
        }

        protected virtual string Read()
            => Console.Read();

        protected virtual string Read(string label)
            => Console.Read(label, true);

        protected virtual void Write(object msg, bool forceWrite = false)
            => Console.Write(msg, true, forceWrite);

        protected virtual void Critical(object msg, bool forceWrite = false)
            => Console.Critical(msg, true, forceWrite);

        protected virtual void Error(object msg, bool forceWrite = false)
            => Console.Error(msg, true, forceWrite);

        protected virtual void Success(object msg, bool forceWrite = false)
            => Console.Success(msg, true, forceWrite);

        protected void Warning(object msg, bool forceWrite = false)
            => Console.Warning(msg, true, forceWrite);

        protected virtual void WriteWithColor(object obj, ConsoleColor fontColor)
            => Console.WriteWithColor(obj, true, fontColor);
    }
}
