using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;

namespace DojoRequireParser
{
    class DojoRequireParser
    {
        static void Main(string[] args)
        {
            Regex exp = new Regex(@"dojo\.require\([""'][\w\.]+['""]\);?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var dictionary = new Dictionary<string, string>();

            Console.WriteLine("dojo.require parser for MVC 2.0");
            Console.WriteLine("Created by Steve Gourley : AGRC" + Environment.NewLine + Environment.NewLine);

#if DEBUG
            var path = GetProjectPath();
            var savePath = GetSavePath();
#endif

#if !DEBUG
            var path = @"some path";
            var savePath = @"some path";

            Stopwatch sw = Stopwatch.StartNew();
#endif

            while (!Directory.Exists(path))
            {
                Console.WriteLine(path + " does not exists.  Please Re-enter.");

                path = GetProjectPath();
            }

            var dir = new DirectoryInfo(path);

            BuildDictionaryOfRequires(exp, dictionary, dir);

#if DEBUG
            sw.Stop();
            Console.WriteLine("Time used (float): {0} ms", sw.Elapsed.TotalMilliseconds);
#endif

            Console.WriteLine("Number of Requires: {0}", dictionary.Keys.Count);
            SaveRequiresDoc(savePath, dictionary);

            Console.WriteLine("Process Complete. Press enter to exit.");
            Console.ReadLine();
        }

        private static void SaveRequiresDoc(string savePath, Dictionary<string, string> dictionary)
        {
            Directory.CreateDirectory(savePath);

            using (var file = File.CreateText(Path.Combine(savePath, "requires.js")))
            {
                file.WriteLine("dojo.provide('core.requires');" + Environment.NewLine);

                foreach (var item in dictionary.OrderBy(x => x.Key))
                {
                    file.WriteLine(item.Key);
                }
            }
        }

        private static string GetSavePath()
        {
            Console.WriteLine("Enter the location to save requires:");

            return Console.ReadLine().TrimEnd('\\');
        }

        private static void BuildDictionaryOfRequires(Regex exp, Dictionary<string, string> dictionary, DirectoryInfo dir)
        {
            foreach (var item in dir.EnumerateFiles("*.*", SearchOption.AllDirectories).
                                     Where(x => !x.DirectoryName.ToLower().Contains("obj")).
                                     Where(x => x.Extension == ".aspx" || x.Extension == ".ascx"))
            {
                var text = File.ReadAllText(item.FullName);

                MatchCollection matches = exp.Matches(text);
                foreach (Match match in matches)
                {
                    var matchValue = match.Groups[0].Value;

                    if (dictionary.ContainsKey(matchValue))
                    {
                        dictionary[matchValue] = string.Format("{0},{1}", dictionary[matchValue], item.Name);
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Adding: {0} from {1}", matchValue, item.FullName));
                        Console.ReadKey();
                        dictionary.Add(matchValue, item.Name);
                    }
                }
            }
        }

        private static string GetProjectPath()
        {
            Console.WriteLine("Enter the parent folder path to the project to parse:");

            return Console.ReadLine().TrimEnd('\\');
        }
    }
}
