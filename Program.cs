using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace GitPullAll
{
    class Program
    {
        private static IConfiguration configuration;
        
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            configuration = builder.Build();

            if (args != null && args.Length > 0)
            {
                foreach (var arg in args)
                {
                    PullAll(arg);
                }
            }
            else
            {
                ShowUsage();
            }            
        }

        private static void PullAll(string path)
        {
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

                    PullAll(folder);
                }

                if (isGitRepository)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();
                    Console.WriteLine($"Pulling: {path}");
                    Console.ForegroundColor = ConsoleColor.Green;
                    string processInfo = configuration["GitPath"];
                    var process = Process.Start(processInfo, $"-C {path} pull");
                    process.WaitForExit();
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
            Console.WriteLine("Usage: " + Process.GetCurrentProcess().MainModule?.FileName + " path1 [path2...]");
            Console.WriteLine();
        }
    }
}
