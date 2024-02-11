using ConsoleCommander;
using Csharp.Common.Utilities;
using Csharp.Common.Utilities.ConsoleCommander;
using Microsoft.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();
// setting up the required dependencies for the cli commander
serviceCollection.AddSingleton<ICommandLineProcessor, CommandLineProcessor>();
serviceCollection.AddSingleton<IConsoleCommandList, CommandList>();
serviceCollection.AddSingleton<IConsoleOutput, ConsoleOutput>();

// registering our cli class
serviceCollection.AddSingleton<ICli, BasicCli>();

var serviceProvider = serviceCollection.BuildServiceProvider();
var cli = serviceProvider.GetRequiredService<ICli>();

// running it
cli.RunCli();