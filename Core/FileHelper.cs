using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace stasigen.Core
{
    public static class FileHelper
    {
        public static IEnumerable<string> GetFiles(string path, string searchPattern = "*.*")
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch (Exception ex)
                {
                    // Console.Error.WriteLine(ex);
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(path, searchPattern);
                }
                catch (Exception ex)
                {
                    // Console.Error.WriteLine(ex);
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        yield return files[i];
                    }
                }
            }
        }



        public static string[] GetLines(string filename)
        {
            string content = File.ReadAllText(filename);
            return content.Split(Environment.NewLine);
        }


    }
}
