using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Markdig;
using stasigen.Commands;
using Spectre.Console;
using System.Text.RegularExpressions;
//using Markdig.SyntaxHighlighting;

namespace stasigen.Core
{
	public static class Generator
	{

		public record GeneratorResult
		{
			public string Error { get; set; }
			public int ParsedMDFiles { get; set; }
		}

		public static GeneratorResult Start(GenerateCommand.Settings opt)
		{
			// result
			GeneratorResult result = new GeneratorResult();

			// check for input path
			if ((opt.InputPath == String.Empty) || (!Directory.Exists(opt.InputPath)))
			{
				result.Error = $"Input path {opt.InputPath} not found!";
				return result;
			}

			// retrieve full path instead of maybe relative paths like "./Examples"
			var di = new System.IO.DirectoryInfo(opt.InputPath);
			opt.InputPath = di.FullName;
			if (!opt.InputPath.EndsWith("/"))
				opt.InputPath = opt.InputPath + "/";

			// Configure the pipeline with all advanced extensions active
			var pipeline = new MarkdownPipelineBuilder()
				.UseAdvancedExtensions()
				//.UseSyntaxHighlighting()
				.Build();

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

			if (!opt.OutputPath.EndsWith("/"))
				opt.OutputPath = opt.OutputPath + "/";

			// get the important files in separate lists
			var files = FileHelper.GetFiles(opt.InputPath);
			var files_md = files.Where(t => t.EndsWith(".md"));
			var files_img = files.Where(t => t.EndsWith(".jpg") || t.EndsWith(".png"));
			var files_css = files.Where(t => t.EndsWith(".css"));

			string headercontent, footercontent;
			headercontent = GetContent("header.md", pipeline, files);
			footercontent = GetContent("footer.md", pipeline, files);

			List<string> files_to_parse_later = new List<string>();


			foreach (var file in files_md)
			{
				string newfn = GetOutputFilename(file, opt.InputPath, opt.OutputPath);
				string content = File.ReadAllText(file);

				content = content.Replace("$header$", headercontent);
				content = content.Replace("$footer$", footercontent);

				// find all [$command:filename.extension]
				string pattern = @"\[[^\]]*\]";

				foreach (Match match in Regex.Matches(content, pattern))
				{
					if (match.Value.Contains("[css:"))
					{
						content = ParseCSSTag(files_css, newfn, content, match.Value);
					}
					if (match.Value.Contains("[img:"))
					{
						content = ParseImageTag(files_img, newfn, content, match.Value);
					}
					if (match.Value.Contains("[dynamic:"))
					{
						// content = content.Replace(match.Value, ParseDynamicTag(pipeline, files_md, content));
					}

					//content = content.Replace(item.Value, );
				}



				using (StreamWriter sw = File.CreateText(newfn))
				{
					// markdown
					var parsedMD = Markdown.ToHtml(content, pipeline);
					sw.WriteLine(parsedMD);
				}




			}



			// // thru each md file
			// foreach (var file in files_md)
			// {
			// 	// read content and split into single lines
			// 	string content = File.ReadAllText(file);
			// 	string[] lines = content.Split(Environment.NewLine);

			// 	// create new filename for output
			// 	string newfn = GetOutputFilename(file, opt.InputPath, opt.OutputPath);

			// 	// convert markdown to html and write it in the output file
			// 	using (StreamWriter sw = File.CreateText(newfn))
			// 	{
			// 		// iterate through each line
			// 		// we could also just do: 
			// 		// var result = Markdown.ToHtml(content, pipeline);
			// 		for (int i = 0; i < lines.Length; i++)
			// 		{
			// 			// get current line
			// 			var currentline = lines[i];

			// 			// header
			// 			if (!string.IsNullOrEmpty(headercontent))
			// 			{
			// 				if ((currentline.ToLower().Contains("$header")))
			// 					currentline = currentline.Replace("$header", headercontent);
			// 			}

			// 			// footer
			// 			if (!string.IsNullOrEmpty(footercontent))
			// 			{
			// 				if ((currentline.ToLower().Contains("$footer")))
			// 					currentline = currentline.Replace("$footer", footercontent);
			// 			}

			// 			// dynamic: embed md file
			// 			if ((currentline.Contains("$dynamic:")))
			// 			{
			// 				var newline = ParseDynamicTag(pipeline, files_md, currentline);
			// 				if (!string.IsNullOrEmpty(newline))
			// 					currentline = newline;
			// 			}

			// 			// images
			// 			if ((currentline.Contains("$img:")))
			// 			{
			// 				var newline = ParseImageTag(files_img, newfn, currentline);
			// 				if (!string.IsNullOrEmpty(newline))
			// 					currentline = newline;
			// 			}

			// 			// css style: $css:main.css >>> <link rel="stylesheet" href="styles.css">
			// 			if ((currentline.Contains("$css:")))
			// 			{
			// 				var newline = ParseCSSTag(files_css, newfn, currentline);
			// 				if (!string.IsNullOrEmpty(newline))
			// 					currentline = newline;
			// 			}

			// 			// markdown
			// 			var parsedMD = Markdown.ToHtml(currentline, pipeline);

			// 			// output
			// 			if (opt.Verbosity != Verbosity.Quiet)
			// 				AnsiConsole.WriteLine($"HTML { currentline } >>> { file }");

			// 			// write only not empty strings to output (is this a good idea?)
			// 			if (!string.IsNullOrEmpty(parsedMD))
			// 			{
			// 				sw.WriteLine(parsedMD);
			// 			}
			// 		}
			// 	}
			// 	// update result
			// 	result.ParsedMDFiles += 1;
			// }

			return result;
		}

		private static string ParseDynamicTag(MarkdownPipeline pipeline, IEnumerable<string> files_md, string line)
		{
			foreach (var mdfile in files_md)
			{
				var mdfn = Path.GetFileName(mdfile);
				if (line.Contains(mdfn))
				{
					var embed_content = File.ReadAllText(mdfile);
					var embed_result = Markdown.ToHtml(embed_content, pipeline);
					// AnsiConsole.WriteLine($"replacing line {line} with {embed_result} from {mdfile}");
					return embed_result;
				}
			}
			return line;
		}

		private static string ParseCSSTag(IEnumerable<string> files, string outputfilename, string content, string token)
		{
			// token: [css:main.css]
			string fn = token.Substring(5, token.Length - 6);
			var file = files.Where(t => t.EndsWith(fn)).FirstOrDefault();
			var pathrels = Path.GetRelativePath(Path.GetDirectoryName(outputfilename), Path.GetDirectoryName(file)); // get relative path from current .md file to img-dir.
			fn = fn.Replace(fn, $"{pathrels}/{fn}");
			var tag = $"<link rel='stylesheet' type='text/css' href='{fn}'>".Replace("'", "\"");
			content = content.Replace(token, tag);
			return content;

			// foreach (var file in files)
			// {
			// 	var shortfilename = Path.GetFileName(file);
			// 	if (content.Contains(shortfilename))
			// 	{
			// 		// line = line.Replace(token, string.Empty); // remove "command"
			// 		var pathrels = Path.GetRelativePath(Path.GetDirectoryName(outputfilename), Path.GetDirectoryName(file)); // get relative path from current .md file to img-dir.
			// 		shortfilename = shortfilename.Replace(shortfilename, $"{pathrels}/{shortfilename}");
			// 		var csstag = $"<link rel='stylesheet' type='text/css' href='{shortfilename}'>";  // formatting trick 1/2
			// 		csstag = csstag.Replace("'", "\""); // formatting trick 2/2
			// 		return csstag;
			// 	}
			// }
			// return string.Empty;
		}

		private static string ParseImageTag(IEnumerable<string> files, string outputfilename, string content, string token)
		{
			// token: [img:logo.img]
			string fn = token.Substring(5, token.Length - 6);
			var file = files.Where(t => t.EndsWith(fn)).FirstOrDefault();
			var pathrels = Path.GetRelativePath(Path.GetDirectoryName(outputfilename), Path.GetDirectoryName(file)); // get relative path from current .md file to img-dir.

			fn = fn.Replace(fn, $"{pathrels}/{fn}");

			content = content.Replace(token, $"!([]{fn}");
			return content;

			// foreach (var imgfile in files_img)
			// {
			// 	var imgfn = Path.GetFileName(imgfile);
			// 	if (line.Contains(imgfn))
			// 	{
			// 		// line = line.Replace("$img", string.Empty); // remove "command"
			// 		var pathrels = Path.GetRelativePath(Path.GetDirectoryName(file), Path.GetDirectoryName(imgfile)); // get relative path from current .md file to img-dir.
			// 		line = line.Replace(imgfn, $"{pathrels}/{imgfn}");
			// 		return line;
			// 	}
			// }
			// return string.Empty;
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

		private static string GetOutputFilename(string file, string intputpath, string outputpath)
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

			// create new filename
			string curdir = System.IO.Path.GetDirectoryName(file);
			string addondir = curdir.Replace(intputpath, outputpath); // replace the paths 

			newfn = Path.Combine(addondir, newfn);

			// new filename directory
			string newfndir = Path.GetDirectoryName(newfn);
			if (!System.IO.Directory.Exists(newfndir))
				System.IO.Directory.CreateDirectory(newfndir);

			return newfn;
		}
	}
}
