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
				// option
				if (opt.Verbosity > 0)
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
			// List<string> files_dynamic = new List<string>();

			// generate html output for each md file
			foreach (var file in files_md)
			{
				string content = File.ReadAllText(file);
				string newfn = GetOutputFilename(file, opt.InputPath, opt.OutputPath);
				// ParseFile(file, files_css, files_img, opt, pipeline);

				// find all [command] or [command:filename.extension]
				string pattern = @"\[[^\]]*\]";
				foreach (Match match in Regex.Matches(content, pattern))
				{
					if (match.Value.Contains("[css:"))
						content = ParseCSSTag(files_css, newfn, content, match.Value);

					if (match.Value.Contains("[img:"))
						content = ParseImageTag(files_img, newfn, content, match.Value);

					// Problem: files imported with the dynamic tag also need to be parsed for img and css tags.
					// todo: parse img and css tags (and maybe other tags in the future)
					if (match.Value.Contains("[dynamic:"))
						content = ParseDynamicTag(pipeline, files_md, content, match.Value);
					// files_dynamic.Add(newfn);
				}

				// create new output file
				using (StreamWriter sw = File.CreateText(newfn))
				{
					var parsedMD = Markdown.ToHtml(content, pipeline);
					sw.Write(parsedMD);
				}

				// update result
				result.ParsedMDFiles += 1;
			}

			return result;
		}

		// private static void ParseFile(string file, IEnumerable<string> images, IEnumerable<string> css, GenerateCommand.Settings opt, MarkdownPipeline pipeline)
		// {
		// 	string content = File.ReadAllText(file);
		// 	string newfn = GetOutputFilename(file, opt.InputPath, opt.OutputPath);

		// 	// find all [command] or [command:filename.extension]
		// 	string pattern = @"\[[^\]]*\]";
		// 	foreach (Match match in Regex.Matches(content, pattern))
		// 	{
		// 		if (match.Value.Contains("[css:"))
		// 			content = ParseCSSTag(css, newfn, content, match.Value);

		// 		if (match.Value.Contains("[img:"))
		// 			content = ParseImageTag(images, newfn, content, match.Value);
		// 	}

		// 	// create new output file
		// 	using (StreamWriter sw = File.CreateText(newfn))
		// 	{
		// 		var result = Markdown.ToHtml(content, pipeline);
		// 		sw.Write(result);
		// 	}
		// }

		private static string ParseDynamicTag(MarkdownPipeline pipeline, IEnumerable<string> files, string content, string token)
		{
			// token: [dynamic:main.md]
			string fn = token.Substring(9, token.Length - 10);
			var file = files.Where(t => t.EndsWith(fn)).FirstOrDefault();
			var embed_content = File.ReadAllText(file);
			var embed_result = Markdown.ToHtml(embed_content, pipeline);
			content = content.Replace(token, embed_content);
			return content;
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
		}

		private static string ParseImageTag(IEnumerable<string> files, string outputfilename, string content, string token)
		{
			// token: [img:logo.img]
			string imgfn = token.Substring(5, token.Length - 6);
			var file = files.Where(t => t.EndsWith(imgfn)).FirstOrDefault();
			var pathrels = Path.GetRelativePath(Path.GetDirectoryName(outputfilename), Path.GetDirectoryName(file)); // get relative path from current .md file to img-dir.
			string fullimgfn = imgfn.Replace(imgfn, $"{pathrels}/{imgfn}");
			content = content.Replace(token, $"![{imgfn}]({fullimgfn})");
			return content;
		}

		// private static string GetContent(string pattern, MarkdownPipeline pipeline, IEnumerable<string> files)
		// {
		// 	var file_header = files.Where(t => t.Contains(pattern)).FirstOrDefault();
		// 	if (file_header != null)
		// 	{
		// 		var header_content = FileHelper.GetLines(file_header);
		// 		var result = string.Join("", header_content);
		// 		result = Markdown.ToHtml(result, pipeline);
		// 		return result;
		// 	}
		// 	return string.Empty;
		// }

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
