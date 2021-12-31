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
			[Description("")]
			public string Path { get; set; }

			[CommandOption("-r|--recursive")]
			[Description("")]
			[DefaultValue(true)]
			public bool NoRestore { get; set; }

			[CommandOption("-v|--verbosity <VERBOSITY>")]
			[Description("Set the verbosity level. Allowed values are q[grey]uiet[/], m[grey]inimal[/], n[grey]ormal[/], d[grey]etailed[/], and diag[grey]nostic[/].")]
			[TypeConverter(typeof(VerbosityConverter))]
			[DefaultValue(Verbosity.Quiet)]
			public Verbosity Verbosity { get; set; }
		}

		public override int Execute(CommandContext context, Settings settings)
		{
			// SettingsDumper.Dump(settings);

			if (settings.Verbosity > 0)
			{
				AnsiConsole.MarkupLine("[green]Done![/]");
			}

			if (string.IsNullOrEmpty(settings.Path))
			{
				settings.Path = System.Environment.CurrentDirectory;
				AnsiConsole.WriteLine($"No path given. Using current directory {settings.Path} instead.");
			}

			if (!System.IO.Directory.Exists(settings.Path))
			{
				AnsiConsole.WriteLine($"Path {settings.Path} not found! Exiting.");
				return -1;
			}

			// Start generating output
			Generator.Start(settings);

			// todo: return something useful?
			return 0;
		}

	}

}