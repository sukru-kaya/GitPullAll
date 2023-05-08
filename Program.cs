using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace GitPullAll
{
    class Program
    {
        private static string gitPath;
        private static string[] excludedFolders;
        
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();
            gitPath = configuration["GitPath"];
            excludedFolders = configuration.GetSection("ExcludedFolders").GetChildren().Select(x => x.Value).ToArray();

            if (args != null && args.Length > 0)
            {
                bool recursive = args.Contains("-r") || args.Contains("-recursive");
                args = args.Where(x => x != "-r" && x != "-recursive").ToArray();

                foreach (var arg in args)
                {
                    PullAll(arg, recursive);
                }
            }
            else
            {
                ShowUsage();
            }            
        }

        private static void PullAll(string path, bool recursive)
        {
            if (excludedFolders != null && excludedFolders.Contains(path))
                return;

            try
            {
                string[] folders = System.IO.Directory.GetDirectories(path);

                bool isGitRepository = false;
                foreach (var folder in folders)
                {
                    if (folder.EndsWith(".git"))
                    {
                        isGitRepository = true;
                        break;
                    }

                    if (recursive)
                        PullAll(folder, recursive);
                }

                if (isGitRepository)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();
                    Console.WriteLine($"Pulling: {path}");
                    Console.ForegroundColor = ConsoleColor.Green;
                    string processInfo = gitPath;
                    var process = Process.Start(processInfo, $"-C {path} pull");
                    process.WaitForExit();
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine($"An error occured while checking path: {path}");
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Git Pull All v1.0, 2023");
            Console.WriteLine();
            Console.WriteLine("Usage: " + Process.GetCurrentProcess().MainModule?.FileName + " path1 [path2...] [-r|-recursive]");
            Console.WriteLine();
        }
    }
}
