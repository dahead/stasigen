using Spectre.Console;
using Spectre.Console.Cli;
using stasigen.Commands;

var app = new CommandApp();
app.Configure(config =>
{
	config.SetApplicationName("stasigen");
	config.AddExample(new[] { "generate", "--path", "~/Documents/Website/staticsite" });
	config.AddExample(new[] { "generate", "--verbosity", "3" });
	config.ValidateExamples();

	// Add Commands
	config.AddCommand<GenerateCommand>("generate");
});

return app.Run(args);