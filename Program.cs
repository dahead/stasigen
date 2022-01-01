using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Markdig;
using NLog;
using Spectre.Console;
using Spectre.Console.Cli;
using stasigen.Commands;

namespace stasigen
{

	class Program
	{
		private const string cAppName = "stasigen";
		private readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static async Task<int> Main(string[] args)
		{
			var app = new CommandApp();
			app.Configure(config =>
			{
				config.SetApplicationName("stasigen");
				config.SetApplicationVersion("1.0");
				config.AddExample(new[] { "generate", "~/Documents/Website/staticsite" });
				config.AddExample(new[] { "generate", "--verbosity", "3" });
				config.ValidateExamples();

				// Add Commands
				config.AddCommand<GenerateCommand>("generate")
						.WithAlias("gen")
						.WithAlias("g")
						.WithExample(new[] { "generate", "~/Documents/Website/staticsite" })
						.WithDescription("Generate html files from the input path given.");
				;
			});

			AnsiConsole.Render(new FigletText(cAppName).LeftAligned().Color(Color.SkyBlue1));
			AnsiConsole.MarkupLine($"[grey]v.{ FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion.ToString() }[/]");
			return app.Run(args);
		}
	}

}