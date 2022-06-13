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

        public void WriteAsJson(object obj)
        {
            var str = JsonConvert.SerializeObject(obj,Formatting.Indented);
            Write(str, true);
        }

        public virtual string Read()
            => Console.Read();

        public virtual string Read(string label)
            => Console.Read(label, true);

        public virtual void Write(object msg, bool forceWrite = false)
            => Console.Write(msg, true, forceWrite);

        public virtual void Critical(object msg, bool forceWrite = false)
            => Console.Critical(msg, true, forceWrite);

        public virtual void Error(object msg, bool forceWrite = false)
            => Console.Error(msg, true, forceWrite);

        public virtual void Success(object msg, bool forceWrite = false)
            => Console.Success(msg, true, forceWrite);

        public void Warning(object msg, bool forceWrite = false)
            => Console.Warning(msg, true, forceWrite);

        public virtual void WriteWithColor(object obj, ConsoleColor fontColor)
            => Console.WriteWithColor(obj, true, fontColor);
    }
}
