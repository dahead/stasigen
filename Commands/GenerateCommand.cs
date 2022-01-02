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
			[CommandArgument(0, "<path>")]
			[Description("The path from where to import the Markdown files.")]
			public string InputPath { get; set; }

			[CommandArgument(1, "[output]")]
			[Description("The output path for the HTML files. If not specified, a folder called 'output' will be created in the input path.")]
			public string OutputPath { get; set; }

			[CommandOption("--createindex <VERBOSITY>")]
			[Description("Creaets an index.html file with links to all processed Markdown files.")]
			[TypeConverter(typeof(bool))]
			[DefaultValue(false)]
			public bool CreateIndex { get; set; }

			[CommandOption("-v|--verbosity <VERBOSITY>")]
			[Description("Set the verbosity level. Allowed values are q[grey]uiet[/], m[grey]inimal[/], n[grey]ormal[/], d[grey]etailed[/], and diag[grey]nostic[/].")]
			[TypeConverter(typeof(VerbosityConverter))]
			[DefaultValue(Verbosity.Quiet)]
			public Verbosity Verbosity { get; set; }

			public override ValidationResult Validate()
			{
				return !System.IO.Directory.Exists(InputPath)
					? ValidationResult.Error($"Input path {InputPath} must exist!")
					: ValidationResult.Success();
			}
		}

		public override int Execute(CommandContext context, Settings settings)
		{
			// options: dump settings
			if (settings.Verbosity > 0)
				SettingsDumper.Dump(settings);

			// Start generating output
			var result = Generator.Start(settings);

			// options: result
			if (settings.Verbosity > 0)
			{
				AnsiConsole.MarkupLine($"[green]Done! Parsed {result.ParsedMDFiles} *.md files.[/]");
			}

			return 0;
		}

	}

}