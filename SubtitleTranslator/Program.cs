using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SubtitleTranslator
{
    class Program
    {
        private static Regex digitsRegex = new(@"^\d+$");
        private static uint noOfRunningFiles = 0;

        static void Main(string[] args)
        {
            DisplayIntro();

            var exit = false;
            var numberOfErrors = 0;
            while (!exit)
            {
                try
                {
                    var location = ReadLocation();

                    if (string.IsNullOrWhiteSpace(location))
                    {
                        DisplayError();
                    }
                    else
                    {
                        StartTranslation(location);
                    }
                }
                catch (Exception ex)
                {
                    numberOfErrors++;
                    DisplayError("Daca eroarea persista contactati-l pe asa zisul programator!");
                    if (numberOfErrors == 3)
                    {
                        exit = true;
                    }
                }
            }
        }

        private static void StartTranslation(string location)
        {
            var directoryInfo = new DirectoryInfo(location);
            var directories = directoryInfo.GetDirectories().ToList();
            var files = directoryInfo.GetFiles().ToList();

            if (!directories.Any() && !files.Any())
            {
                DisplayError("Locatia nu contine niciun fisier cu extensia .srt");
                return;
            }

            var newPath = Path.Combine(location, "SubtitrariTraduse");
            if (Directory.Exists(newPath))
            {
                DeleteDirectory(newPath);
            }

            Directory.CreateDirectory(newPath);

            foreach (var directory in directories)
            {
                TranslateDirectory(newPath, directory);
            }
        }

        private static void TranslateDirectory(string newPath, DirectoryInfo directory)
        {
            var directoryPath = Path.Combine(newPath, directory.Name);
            if (Directory.Exists(directoryPath))
            {
                DeleteDirectory(directoryPath);
            }

            Directory.CreateDirectory(directoryPath);
            if (directory.GetDirectories().Any())
            {
                foreach (var childDirectory in directory.GetDirectories())
                {
                    TranslateDirectory(directoryPath, childDirectory);
                }
            }

            if (directory.GetFiles().Any())
            {
                foreach (var fileInfo in directory.GetFiles())
                {
                    TranslateFile(directoryPath, fileInfo);

                    /*while (noOfRunningFiles == 2)
                    {
                        Task.Delay(1000);
                    }

                    noOfRunningFiles++;
                    Task.Run(async () =>
                    {
                        await TranslateFileAsync(directoryPath, fileInfo);
                    }).ConfigureAwait(false);*/
                }
            }
        }

        private static void TranslateFile(string newPath, FileSystemInfo fileInfo)
        {
            var newFileNamePath = Path.Combine(newPath, fileInfo.Name);
            File.CreateText(newFileNamePath);
            var allLines = File.ReadLines(fileInfo.FullName);
            foreach (var line in allLines)
            {
                var shouldTranslate = ShouldTranslate(line);
                var translatedLine = line;
                if (shouldTranslate)
                {
                    var translatedLineTask = TranslateLineAsync(line);
                    Task.WaitAll(translatedLineTask);

                    translatedLine = translatedLineTask.Result;
                }

                WriteLineInFile(translatedLine, newFileNamePath);
            }

            noOfRunningFiles--;
        }

        private static async Task<string> TranslateLineAsync(string line)
        {
            var request = new TranslateRequest
            {
                Qs = new List<string> { line },
                Source = Language.English,
                Target = Language.Romanian,
                Key = "ec1cd056894eaea25555517aa2b0aafb61a194cf"
            };

            var response = await GoogleTranslate.Translate.QueryAsync(request);

            return response.Data.Translations?.ElementAtOrDefault(0)?.TranslatedText ?? line;
        }

        private static void WriteLineInFile(string translatedLine, string newFileNamePath)
        {
            using var streamWriter = File.AppendText(newFileNamePath);
            streamWriter.WriteLine(translatedLine);
        }

        private static bool ShouldTranslate(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            if (digitsRegex.IsMatch(line))
            {
                return false;
            }

            if (line.Contains("-->"))
            {
                return false;
            }

            return true;
        }

        private static void DisplayIntro()
        {
            Console.WriteLine("Bine ati venit la traducatorul de subtitrari!");
            Console.WriteLine("Cum lucrez eu?");
            Console.WriteLine(
                "Eu primesc locatia unui folder in care sunt toate subtitrarile,\nindiferent daca sunt in alte subfoldere sau nu,\nle traduc pe toate si creez aceeasi structura de foldere dar cu subtitrarile traduse!");
            Console.WriteLine("Acum as vrea sa stiu unde se afla subtitrarile alea care va pun probleme.");
        }

        private static string ReadLocation()
        {
            Console.WriteLine("Introduceti locatia folderului:");
            var location = Console.ReadLine();
            return location;
        }

        private static void DisplayError(string message = null)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("A aparut o eroare!");
            if (!string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine(message);
            }

            Console.ResetColor();
        }

        private static void DeleteDirectory(string path)
        {
            var files = Directory.GetFiles(path);
            var dirs = Directory.GetDirectories(path);

            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (var dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(path, false);
        }
    }
}