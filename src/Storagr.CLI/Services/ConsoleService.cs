using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;

namespace Storagr.CLI
{
    public interface IConsoleService
    {
        string ReadPassword() => 
            ReadPassword(null);
        string ReadPassword(string title);

        string ReadLine() =>
            ReadLine(null);
        string ReadLine(string title);

        void Write(string p);
        void WriteLine();
        void WriteLine(string p);

        void WriteError(string p);
        void WriteError(Exception e);
    }
    
    public class ConsoleService : IConsoleService
    {
        private readonly InvocationContext _invocationContext;
        private readonly IConsole _console;
        private readonly ConsoleRenderer _consoleRenderer;
        private readonly Region _region;
        
        public ConsoleService(InvocationContext invocationContext)
        {
            _invocationContext = invocationContext;
            _console = _invocationContext.Console;
            _consoleRenderer = new ConsoleRenderer(_console, invocationContext.BindingContext.OutputMode(), true);
            _region = new Region(0, 0, Console.WindowWidth, Console.WindowHeight);
        }
        
        public string ReadPassword(string title)
        {
            if(title is not null)
                _console.Out.Write($"{title}: ");
            
            var password = "";
            
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) && keyInfo.Key == ConsoleKey.C)
                    return null;

                if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    Console.Write("\b \b");
                    password = password[0..^1];
                }
                else if(!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    password += keyInfo.KeyChar;
                }
                    
            } while (keyInfo.Key != ConsoleKey.Enter);
                
            Console.WriteLine();
            return password;
        }

        public string ReadLine(string title)
        {
            if(title is not null)
                Console.Write($"{title}: ");

            return Console.ReadLine();
        }

        public void Write(string p)
        {
            _console.Out.Write(p);
        }

        public void WriteLine()
        {
            _console.Out.WriteLine();
        }

        public void WriteLine(string p)
        {
            _console.Out.WriteLine(p);
        }

        public void WriteError(string p)
        {
            var container = new ContainerSpan(
                ForegroundColorSpan.Red(),
                new ContentSpan(p),
                ForegroundColorSpan.Reset()
            );
            // container.WriteTo(_console.Error, OutputMode.Auto);
            
            
            _consoleRenderer.RenderToRegion(container, _region);
            
            // _console.Error.WriteLine(p);
        }

        public void WriteError(Exception e)
        {
            WriteError(e.Message);
        }
    }
}