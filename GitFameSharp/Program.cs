using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GitFameSharp.Runner;
using Microsoft.Extensions.Configuration;

namespace GitFameSharp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Any(x => x.Equals("--version", StringComparison.OrdinalIgnoreCase)))
            {
                CommandLineOptions.PrintVersion(Console.WriteLine);
                return;
            }

            if (args.Any(x => x.Equals("--help", StringComparison.OrdinalIgnoreCase)))
            {
                CommandLineOptions.PrintUsage(Console.WriteLine);
                return;
            }

            var options = InitializeOptions(args);
            if (!options.Validate(Console.WriteLine))
            {
                return;
            }

            var runner = new GitFame();
            await runner.RunAsync(options).ConfigureAwait(false);

            if (Debugger.IsAttached)
            {
                Console.ReadLine();
            }
        }

        private static CommandLineOptions InitializeOptions(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var options = new CommandLineOptions();
            configuration.Bind(options);

            return options;
        }
    }
}