using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Storagr.CLI
{
    public class TestCommand : StoragrCommand
    {
        private enum Test
        {
            Spinner,
            Options
        }

        private class LocalOptions
        {
            public Test RunTest { get; set; }
            public ulong SizeLimit { get; set; }
        }
        
        public TestCommand()
            : base("test", StoragrConstants.TestCommandDescription)
        {
            IsHidden = true;

            var runOption = new Option<Test>("--run");
            runOption.AddValidator(option =>
                !Enum.TryParse<Test>(option.Tokens[0].Value, out var value)
                    ? $"{option.Tokens[0].Value} is no test."
                    : null);
            runOption.Name = "RunTest";
            AddOption(runOption);
            
            AddOption(new SizeLimitOption());
            
            Handler = CommandHandler.Create<IHost, LocalOptions, GlobalOptions>(Run);
        }

        private static Task<int> Run(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            return options.RunTest switch
            {
                Test.Spinner => RunSpinner(host, globalOptions),
                Test.Options => RunOptions(host, options, globalOptions),
                _ => Task.FromResult(0)
            };
        }

        private static async Task<int> RunOptions(IHost host, LocalOptions options, GlobalOptions globalOptions)
        {
            var console = host.GetConsole();

            await Task.Delay(10);
            
            return Success(console, options);
        }

        private static async Task<int> RunSpinner(IHost host, GlobalOptions options)
        {
            var console = host.GetConsole();

            Console.WriteLine("waiting");

            try
            {
                await console.Wait(async token =>
                {
                    await Task.Delay(2000, token);
                    throw new Exception("hi!");
                });
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("abort");
                return -1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("finished");
            
            return 0;
        }
    }
}