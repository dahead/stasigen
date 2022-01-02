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

		public record TagValueAttribute
		{
			public string Tag { get; set; }
			public string Value { get; set; }
		}

		public static int Start(GenerateCommand.Settings opt)
		{

			// check for input path
			if ((opt.InputPath == String.Empty) || (!Directory.Exists(opt.InputPath)))
				return -1;

			// retrieve full path instead of maybe relative paths like "./Examples"
			var di = new System.IO.DirectoryInfo(opt.InputPath);
			opt.InputPath = di.FullName;

			// make sure we deal with full paths
			if (!opt.InputPath.EndsWith("/"))
				opt.InputPath = opt.InputPath + "/";

			// Configure the pipeline with all advanced extensions active
			var pipeline = new MarkdownPipelineBuilder()
				.UseAdvancedExtensions()
				//.UseSyntaxHighlighting()
				.Build();

			// output path defaults
			if (string.IsNullOrEmpty(opt.OutputPath))
				opt.OutputPath = System.IO.Path.Combine(opt.InputPath, "output");

			if (!System.IO.Directory.Exists(opt.OutputPath))
			{
				// option
				if (opt.Verbosity > 0)
					AnsiConsole.WriteLine($"Creating output path {opt.OutputPath}.");
				// create output path
				System.IO.Directory.CreateDirectory(opt.OutputPath);
			}

			// make sure we deal with full paths
			if (!opt.OutputPath.EndsWith("/"))
				opt.OutputPath = opt.OutputPath + "/";

			// get the important files in separate lists
			var files = FileHelper.GetFiles(opt.InputPath).ToList();
			var files_md = files.Where(t => t.EndsWith(".md")).ToList();

			// generate html output for each md file
			foreach (var file in files_md)
			{
				string content = File.ReadAllText(file);
				string newfn = GetOutputFilename(file, opt.InputPath, opt.OutputPath);
				string pattern = @"\[[^\]]*\]"; // find all [command] or [command:filename.extension]
				foreach (Match match in Regex.Matches(content, pattern))
				{
					var result = ParseTag(files, newfn, content, match.Value);
					if (!string.IsNullOrEmpty(result))
					{
						content = content.Replace(match.Value, result);
					}
				}

				// create new output file
				using (StreamWriter sw = File.CreateText(newfn))
				{
					var parsedMD = Markdown.ToHtml(content, pipeline);
					sw.Write(parsedMD);
				}
			}

			return 0;
		}

		private static TagValueAttribute GetTagValueAttribute(string token)
		{
			// token: [tokenname:filename.extension]
			if (token.StartsWith("[") && token.EndsWith("]") && token.Contains(":"))
			{
				string[] parts = token.Split(":");
				string p1 = parts[0];
				p1 = p1.ToString().Substring(1, p1.Length - 1);

				string p2 = parts[1];
				p2 = p2.ToString().Substring(0, p2.Length - 1);

				return new TagValueAttribute() { Tag = p1, Value = p2 };
			}
			else
				return null;
		}

		private static string TagValueToFilename(string value, IEnumerable<string> files)
		{
			var match = files.Where(t => t.EndsWith(value)).FirstOrDefault();
			if (match != null)
				return match;
			return string.Empty;
		}

		private static string ParseTag(IEnumerable<string> files, string outputfilename, string content, string token)
		{
			// token: [css:main.css]
			var tag = GetTagValueAttribute(token); // token.Substring(5, token.Length - 6);
			if (tag == null)
				return string.Empty;

			var foundfile = files.Where(t => t.EndsWith(tag.Value)).FirstOrDefault();
			if (string.IsNullOrEmpty(foundfile))
				return string.Empty;

			var pathrels = Path.GetRelativePath(Path.GetDirectoryName(outputfilename), Path.GetDirectoryName(foundfile)); // get relative path from current .md file to img-dir.

			string filename = string.Empty;
			string result = string.Empty;

			switch (tag.Tag.ToUpper())
			{
				case "CSS":
					filename = $"{pathrels}/{tag.Value}";
					result = $"<link rel='stylesheet' type='text/css' href='{filename}'>".Replace("'", "\"");
					break;
				case "IMG":
					filename = $"{pathrels}/{tag.Value}";
					result = $"![{tag.Value}]({filename})";
					break;
				case "DYNAMIC":
					// import rendered Markdown
					result = File.ReadAllText(foundfile);
					// regex find tags
					// recursive ParseTag() to resolve img and css and other tags...					
					string pattern = @"\[[^\]]*\]"; // find all [command] or [command:filename.extension]
					foreach (Match match in Regex.Matches(result, pattern))
					{
						result = ParseTag(files, outputfilename, result, match.Value);
						content = content.Replace(match.Value, result);
					}
					result = Markdown.ToHtml(result);
					break;
			}

			return result;
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