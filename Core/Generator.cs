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

			if ((opt.InputPath == String.Empty) || (!Directory.Exists(opt.InputPath)))
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

			// output path defaults
			if (string.IsNullOrEmpty(opt.OutputPath))
			{
				opt.OutputPath = System.IO.Path.Combine(opt.InputPath, "output");
			}

			if (!System.IO.Directory.Exists(opt.OutputPath))
			{
				AnsiConsole.WriteLine($"Creating output path {opt.OutputPath}.");
				System.IO.Directory.CreateDirectory(opt.OutputPath);
			}

			// get the important files in separate lists
			var files = FileHelper.GetFiles(opt.InputPath);
			var files_md = files.Where(t => t.EndsWith(".md"));
			var files_img = files.Where(t => t.EndsWith(".jpg") || t.EndsWith(".png"));
			var files_css = files.Where(t => t.EndsWith(".css"));

			string headercontent, footercontent;
			headercontent = GetContent("header.md", pipeline, files);
			footercontent = GetContent("footer.md", pipeline, files);

			// thru each md file
			foreach (var file in files_md)
			{

				// read content and split into single lines
				string content = File.ReadAllText(file);
				string[] lines = content.Split(Environment.NewLine);

				string newfn = GetFilename(opt, file);

				// convert markdown to html and write it in the output file
				using (StreamWriter sw = File.CreateText(newfn))
				{
					// iterate through each line
					// we could also just do: 
					// var result = Markdown.ToHtml(content, pipeline);
					for (int i = 0; i < lines.Length; i++)
					{
						// get current line
						var line = lines[i];

						// header
						if ((line.ToLower().Contains("$header")))
							line = line.Replace("$header", headercontent);

						// footer
						if ((line.ToLower().Contains("$footer")))
							line = line.Replace("$footer", footercontent);

						// dynamic: embed md file
						if ((line.Contains("$dynamic:")))
						{
							var newline = ParseDynamicTag(pipeline, files_md, line);
							if (!string.IsNullOrEmpty(newline))
								line = newline;
						}

						// images
						if ((line.Contains("$img:")))
						{
							var newline = ParseImageTag(files_img, file, line);
							if (!string.IsNullOrEmpty(newline))
								line = newline;
						}

						// css style: $css:main.css >>> <link rel="stylesheet" href="styles.css">
						if ((line.Contains("$css:")))
						{
							var newline = ParseCSSTag(files_css, file, line);
							if (!string.IsNullOrEmpty(newline))
								line = newline;
						}

						// todo:
						// $navbar
						// $posts.last(10)
						// $posts.random(100)

						// markdown
						var result = Markdown.ToHtml(line, pipeline);

						// output
						if (opt.Verbosity != Verbosity.Quiet)
							AnsiConsole.WriteLine($"HTML { line } >>> { file }");

						// write line to output
						if (!string.IsNullOrEmpty(result))
						{
							sw.WriteLine(result);
						}

					}
				}

			}
		}

		private static string ParseDynamicTag(MarkdownPipeline pipeline, IEnumerable<string> files_md, string line)
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
			return line;
		}

		private static string ParseCSSTag(IEnumerable<string> files_css, string file, string line)
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
					return line;
				}
			}
			return string.Empty;
		}

		private static string ParseImageTag(IEnumerable<string> files_img, string file, string line)
		{
			foreach (var imgfile in files_img)
			{
				var imgfn = Path.GetFileName(imgfile);
				if (line.Contains(imgfn))
				{
					line = line.Replace("$img:", string.Empty); // remove "command"
					var pathrels = Path.GetRelativePath(Path.GetDirectoryName(file), Path.GetDirectoryName(imgfile)); // get relative path from current .md file to img-dir.
					line = line.Replace(imgfn, $"{pathrels}/{imgfn}");
					return line;
				}
			}
			return string.Empty;
		}

		private static string GetContent(string pattern, MarkdownPipeline pipeline, IEnumerable<string> files)
		{
			var file_header = files.Where(t => t.Contains(pattern)).FirstOrDefault();
			if (file_header != null)
			{
				var header_content = FileHelper.GetLines(file_header);
				var result = string.Join("", header_content);
				result = Markdown.ToHtml(result, pipeline);
				return result;
			}
			return string.Empty;
		}

		private static string GetFilename(GenerateCommand.Settings opt, string file)
		{
			// create new html output file
			string newfn = Path.GetFileName(file);
			newfn = Path.ChangeExtension(newfn, ".html");

			// Convert directory from
			//
			//	/home/dh/dev/test/site/generator/*
			//									|___ Blog
			//									|___ css
			//									|___ img
			//	to:
			//
			// 	/home/dh/sites/compiled/*
			//							|___ Blog
			//							|___ css
			//							|___ img

			string curdir = System.IO.Path.GetDirectoryName(file);
			string addondir = curdir.Replace(opt.InputPath, string.Empty); // remove 

			// settings: use output path
			string newoutputdir = System.IO.Path.Combine(opt.OutputPath, addondir);
			newfn = Path.Combine(newoutputdir, newfn);

			string newfndir = Path.GetDirectoryName(newfn);

			if (!System.IO.Directory.Exists(newfndir))
				System.IO.Directory.CreateDirectory(newfndir);

			return newfn;
		}
	}
}
