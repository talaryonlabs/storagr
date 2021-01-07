using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Storagr.Shared;

namespace Storagr.CLI
{
    public interface IConsoleService
    {
        Task Wait(Func<CancellationToken, Task> waitingAction);

        string ReadPassword() => 
            ReadPassword(null);
        string ReadPassword(string title);

        string ReadLine() =>
            ReadLine(null);
        string ReadLine(string title);

        void Write(string p);
        void WriteLine();
        void WriteLine(string p);

        void WriteSuccess(string message);

        void WriteError(string errorMessage);
        void WriteError(StoragrError e);
        void WriteError(Exception e);

        void WriteView(View view);
        void WriteJson<T>(T obj) where T : new();
        void WriteJson<T>(IEnumerable<T> list) where T : new();
        void WriteObject<T>(T obj) where T : new();
        void WriteObject<T>(IEnumerable<T> list) where T : new();
    }
    
    public class ConsoleService : IConsoleService
    {
        private const ConsoleColor DefaultColor = ConsoleColor.White;
        
        private readonly IConsole _console;
        private readonly ConsoleRenderer _consoleRenderer;
        private readonly Region _region;

        public ConsoleService(InvocationContext invocationContext)
        {
            _console = invocationContext.Console;
            _consoleRenderer = new ConsoleRenderer(_console, invocationContext.BindingContext.OutputMode(), false);
            _region = new Region(0, 0, Console.WindowWidth, Console.WindowHeight);
        }

        public async Task Wait(Func<CancellationToken, Task> waitingAction)
        {
            var actionCancellation = new CancellationTokenSource();
            var spinnerCancellation = new CancellationTokenSource();
            var finalize = new Action(() =>
            {
                spinnerCancellation.Cancel();
                
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', 10));
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.CursorVisible = true;
            });

            Console.CursorVisible = false;
            Console.CancelKeyPress += (_, _) =>
            {
                actionCancellation.Cancel();
            };
            
            (new Task(() =>
            {
                var count = 1;
                while (!spinnerCancellation.IsCancellationRequested)
                {
                    if (count > 3)
                    {
                        count = 1;
                    }

                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string('.', count).PadRight(4));
                    Task.Delay(500, spinnerCancellation.Token).Wait(spinnerCancellation.Token);

                    count++;
                }
            }, spinnerCancellation.Token)).Start();

            try
            {
                await Task.Run(() => waitingAction(actionCancellation.Token), actionCancellation.Token);
            }
            catch (Exception)
            {
                
                finalize();
                throw;
            }

            finalize();
        }

        public string ReadPassword(string title)
        {
            if(title is not null)
                _console.Out.Write($"{title}: ");
            
            var password = "";

            Console.TreatControlCAsInput = true;
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
                _console.Out.Write($"{title}: ");

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

        public void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            
            _console.Error.WriteLine(message);
            
            Console.ForegroundColor = DefaultColor;
        }

        public void WriteError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            
            _console.Error.WriteLine($"-- Error --");
            _console.Error.WriteLine(errorMessage);
            
            Console.ForegroundColor = DefaultColor;
        }

        public void WriteError(StoragrError e) =>
            WriteError($"Code: {e.Code}\nMessage: {e.Message}");

        public void WriteError(Exception e) =>
            WriteError(e.Message);

        public void WriteView(View view)
        {
            var screen = new ScreenView(_consoleRenderer, _console) {Child = view};

            screen.Render(_region);
        }

        public void WriteJson<T>(T obj) where T : new()
        {
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings()
            {
                ContractResolver = new PropertyNameResolver(),
            });
            _console.Out.WriteLine(json);
        }

        public void WriteJson<T>(IEnumerable<T> list) where T : new()
        {
            WriteJson(list.ToList());
        }

        public void WriteObject<T>(T obj) where T : new()
        {
            var stackLayout = new StackLayoutView(Orientation.Vertical);

            foreach (var property in typeof(T).GetProperties())
            {
                stackLayout.Add(new ContentView($"{property.Name}: \"{property.GetValue(obj)}\""));
            }

            WriteView(stackLayout);
            _console.Out.WriteLine();
        }

        public void WriteObject<T>(IEnumerable<T> list) where T : new()
        {
            var table = new TableView<T>
            {
                Items = list.ToList()
            };
            foreach (var property in typeof(T).GetProperties())
            {
                table.AddColumn(v => $"{property.GetValue(v)}", property.Name);
            }

            WriteView(table);
        }
    }

    internal class PropertyNameResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            prop.PropertyName = member.Name;
            
            return prop;
        }
    }
}