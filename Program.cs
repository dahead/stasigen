using System.Threading.Tasks;
using NLog;
using Spectre.Console;
using Spectre.Console.Cli;
using stasigen.Commands;

namespace stasigen
{

	class Program
	{
		private const string cAppName = "DF2";
		private readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static async Task<int> Main(string[] args)
		{

			var app = new CommandApp();
			app.Configure(config =>
			{
				config.SetApplicationName("stasigen");
				config.SetApplicationVersion("1.0");
				// config.AddExample(new[] { "generate", "path", "~/Documents/Website/staticsite" });
				// config.AddExample(new[] { "generate", "--verbosity", "3" });
				// config.ValidateExamples();

				// Add Commands
				config.AddCommand<GenerateCommand>("generate")
					.WithAlias("gen")
					.WithAlias("g")
					// .WithExample(new[] { "generate", "--path", "~/Documents/Website/staticsite" })
					.WithDescription("Generate html files from the input path given.");
				;
			});

			return app.Run(args);
		}
	}

}