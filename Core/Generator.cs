using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Markdig;
using stasigen.Commands;
using Spectre.Console;
//using Markdig.SyntaxHighlighting;

namespace stasigen.Core
{
	public static class Generator
	{

		public static void Start(GenerateCommand.Settings opt)
		{

			if ((opt.Path == String.Empty) || (!Directory.Exists(opt.Path)))
			{
				return;
			}


			// Console.WriteLine("Hello, World!");
			// static site generator - stasigen
			// create html files from *.md files
			// stasigen --update /Examples/dh/
			//  durch jedes Verzeichnis und aus *.md Dateien *.html machen.
			// stasigen --add ??? oder lieber einfach manuell datei anlegen? simpler!

			// Configure the pipeline with all advanced extensions active
			// var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
			var pipeline = new MarkdownPipelineBuilder()
				.UseAdvancedExtensions()
				//.UseSyntaxHighlighting()
				.Build();

			// var result = Markdown.ToHtml("This is a text with some *emphasis*", pipeline);


			var files = FileHelper.GetFiles(opt.Path);
			var files_md = files.Where(t => t.EndsWith(".md"));
			var files_img = files.Where(t => t.EndsWith(".jpg") || t.EndsWith(".png"));
			var files_css = files.Where(t => t.EndsWith(".css"));

			// header, footer files (get them via ini / search / pattern?)
			var file_header = files_md.Where(t => t.Contains("header.md")).FirstOrDefault();
			var file_footer = files_md.Where(t => t.Contains("footer.md")).FirstOrDefault();

			string hc = string.Empty;
			string fc = string.Empty;

			if (file_header != null)
			{
				var header_content = FileHelper.GetLines(file_header);
				hc = string.Join("", header_content);
				hc = Markdown.ToHtml(hc, pipeline);
			}

			if (file_footer != null)
			{
				var footer_content = FileHelper.GetLines(file_footer);
				fc = string.Join("", footer_content);
				fc = Markdown.ToHtml(fc, pipeline);
			}

			// thru each md file
			foreach (var file in files_md)
			{

				// read content and split into single lines
				string content = File.ReadAllText(file);
				string[] lines = content.Split(Environment.NewLine);

				// create new html output file
				string newfn = Path.ChangeExtension(file, ".html");

				// convert markdown to html and write it in the output file
				using (StreamWriter sw = File.CreateText(newfn))
				{
					for (int i = 0; i < lines.Length; i++)
					{
						var line = lines[i];

						// check if line contains {{ xxx }} and replace with placeholders...
						if ((line.ToLower().Contains("$header")))
							line = line.Replace("$header", hc);
						if ((line.ToLower().Contains("$footer")))
							line = line.Replace("$footer", fc);

						// embed md file
						if ((line.ToLower().Contains("$dynamic:")))
						{
							foreach (var mdfile in files_md)
							{
								var mdfn = Path.GetFileName(mdfile);
								if (line.Contains(mdfn))
								{
									line = line.Replace("$dynamic:", string.Empty); // remove "command"
									var embed_content = File.ReadAllText(mdfile);
									var embed_result = Markdown.ToHtml(embed_content, pipeline);
									// AnsiConsole.WriteLine($"replacing line {line} with {embed_result} from {mdfile}");
									// replace line
									line = embed_result;
								}
							}
						}

						// images
						if ((line.Contains("$img:")))
						{
							foreach (var imgfile in files_img)
							{
								var imgfn = Path.GetFileName(imgfile);
								if (line.Contains(imgfn))
								{
									line = line.Replace("$img:", string.Empty); // remove "command"
									var pathrels = Path.GetRelativePath(Path.GetDirectoryName(file), Path.GetDirectoryName(imgfile)); // get relative path from current .md file to img-dir.
									line = line.Replace(imgfn, $"{pathrels}/{imgfn}");
								}
							}
						}

						// css style
						// $css:main.css >>> <link rel="stylesheet" href="styles.css">
						if ((line.Contains("$css:")))
						{
							foreach (var cssfile in files_css)
							{
								var cssfn = Path.GetFileName(cssfile);
								if (line.Contains(cssfn))
								{
									line = line.Replace("$css:", string.Empty); // remove "command"
									var pathrels = Path.GetRelativePath(Path.GetDirectoryName(file), Path.GetDirectoryName(cssfile)); // get relative path from current .md file to img-dir.
									cssfn = cssfn.Replace(cssfn, $"{pathrels}/{cssfn}");
									var csstag = $"<link rel='stylesheet' type='text/css' href='{cssfn}'>";
									csstag = csstag.Replace("'", "\"");
									// replace line
									line = csstag;
								}
							}
						}


						// $navbar
						// todo:


						// $posts.last(10)
						// todo:

						// $posts.random(100)
						// todo:

						// markdown
						var result = Markdown.ToHtml(line, pipeline);

						if (opt.Verbosity != Verbosity.Quiet)
						{
							AnsiConsole.WriteLine($"HTML { line } >>> { file }");
						}

						// write line to output
						sw.WriteLine(result);
					}
				}

			}
		}

	}
}
