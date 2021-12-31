using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using stasigen.Core;

namespace stasigen.Commands
{

	[Description("Generate output from scratch.")]
	public sealed class GenerateCommand : Command<GenerateCommand.Settings>
	{
		public sealed class Settings : CommandSettings
		{

			[CommandOption("-p|--path")]
			[Description("The path from where to import the Markdown files.")]
			public string InputPath { get; set; }

			[CommandOption("-o|--output")]
			[Description("The output path for the HTML files.")]
			public string OutputPath { get; set; }

			[CommandOption("-v|--verbosity <VERBOSITY>")]
			[Description("Set the verbosity level. Allowed values are q[grey]uiet[/], m[grey]inimal[/], n[grey]ormal[/], d[grey]etailed[/], and diag[grey]nostic[/].")]
			[TypeConverter(typeof(VerbosityConverter))]
			[DefaultValue(Verbosity.Quiet)]
			public Verbosity Verbosity { get; set; }
		}

		public override int Execute(CommandContext context, Settings settings)
		{
			// SettingsDumper.Dump(settings);


			// is input path given?
			if (string.IsNullOrEmpty(settings.InputPath))
			{
				settings.InputPath = System.Environment.CurrentDirectory;
				AnsiConsole.WriteLine($"No path given. Using current directory {settings.InputPath} instead.");
			}

			// does input path exist?
			if (!System.IO.Directory.Exists(settings.InputPath))
			{
				AnsiConsole.WriteLine($"Path {settings.InputPath} not found! Exiting.");
				return -1;
			}


			// Start generating output
			Generator.Start(settings);

			if (settings.Verbosity > 0)
			{
				AnsiConsole.MarkupLine("[green]Done![/]");
			}

			// todo: return something useful?
			return 0;
		}

	}

}