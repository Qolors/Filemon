using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Filemon.OverWatch
{
    public static class Program
    {
        private static readonly object logFileLock = new object();
        private static ConcurrentQueue<string> results = new ConcurrentQueue<string>();
        private static Dictionary<string, string> ruleSets = new Dictionary<string, string>();

        public static void Main(string[] args)
        {
            ConcurrentDictionary<string, FileSystemWatcher> config = new ConcurrentDictionary<string, FileSystemWatcher>();
            LoadConfig(config, ruleSets);

            // Ensure file creation is done safely
            try
            {
                File.Create("filemon.txt").Close();
                File.Create("filemonlogs.txt").Close();
                WriteToLog("Filemon is Running");
                WriteToLog("filemon.txt file path: " + Path.GetFullPath("filemon.txt"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating files: " + ex.Message);
                return;
            }

            while (true)
            {
                while (results.TryDequeue(out string result))
                {
                    WriteToLog(result);
                }

                Thread.Sleep(1000); // Reduce CPU usage
            }
        }

        private static void LoadConfig(ConcurrentDictionary<string, FileSystemWatcher> config, Dictionary<string, string> rulesets)
        {
            List<string> directories = new List<string>();

            try
            {
                if (File.Exists("config.txt"))
                {
                    using StreamReader sr = new StreamReader("config.txt");
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        WriteToLog("Added: " + line);
                        directories.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Error reading config.txt: " + ex.Message);
            }

            foreach (string directory in directories)
            {
                if (Directory.Exists(directory))
                {
                    FileSystemWatcher watcher = new FileSystemWatcher(directory)
                    {
                        EnableRaisingEvents = true,
                        IncludeSubdirectories = false
                    };

                    watcher.Created += new FileSystemEventHandler(OnChanged);

                    Console.WriteLine("Watcher: " + watcher + " Directory: " + directory);
                    if (config.TryAdd(directory, watcher))
                    {
                        WriteToLog("ADDED: " + directory);
                    }
                }
                else
                {
                    WriteToLog("Directory does not exist: " + directory);
                }
            }

            try
            {
                if (Directory.Exists("rulesets"))
                {
                    //for each file in that path
                    foreach (string file in Directory.EnumerateFiles("rulesets", "*.bat"))
                    {
                        //read the file
                        string[] lines = File.ReadAllLines(file);

                        if (lines.Length > 2)
                        {
                            //trim the string and replace " " with "_"
                            string key = Path.GetFileNameWithoutExtension(file).Trim().Replace(" ", "_");
                            ruleSets.Add(key, Path.GetFullPath(file));
                            WriteToLog(Path.GetFullPath(file) + " added to rulesets.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Error reading rulesets: " + ex.Message);
            }
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            string fullPath = e.FullPath;

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "magika.exe",
                    Arguments = fullPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using Process process = Process.Start(startInfo);

                if (process == null)
                {
                    WriteToLog("Failed to start magika.exe for: " + fullPath);
                    return;
                }

                using StreamReader reader = process.StandardOutput;

                Thread.Sleep(1000); // Wait for the process to produce output

                string result = reader.ReadToEnd();

                var (docType, format) = ParseString(result);

                if (string.IsNullOrEmpty(docType) || string.IsNullOrEmpty(format))
                {
                    WriteToLog("Failed to parse output for: " + fullPath);
                    return;
                }

                WriteToLog("Doc Type: " + docType);
                WriteToLog("Format: " + format);

                string docstrip = docType.Split(':')[1].Trim();
                string commandstrip = docstrip.Replace(" ", "_");

                if (ruleSets.TryGetValue(commandstrip, out string? value))
                {
                    WriteToLog(commandstrip + " found in rulesets - executing rule");

                    ProcessStartInfo bashInfo = new ProcessStartInfo
                    {
                        FileName = value, // Use "/bin/bash" for Linux or macOS, "cmd.exe" for Windows
                        Arguments = $"\"{e.FullPath}\"", // -c argument is used to pass the command to the shell
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    using (Process commandprocess = Process.Start(bashInfo))
                    {
                        if (process != null)
                        {
                            var commandstream = commandprocess.StandardOutput;
                            Thread.Sleep(1000); // Wait for the process to produce output
                            string commandresult = commandstream.ReadToEnd();
                            process.WaitForExit();
                            WriteToLog(commandresult);
                        }
                        else
                        {
                            WriteToLog("Failed to execute command.");
                        }
                    }
                }
                else
                {
                    if (!Directory.Exists(Path.Combine(Path.GetDirectoryName(fullPath), docstrip)))
                    {
                        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(fullPath), docstrip));
                    }

                    string newFullPath = Path.Combine(Path.Combine(Path.GetDirectoryName(fullPath), docstrip), Path.GetFileName(fullPath));

                    File.Move(fullPath, newFullPath);

                    Thread.Sleep(1000); // Wait before enqueuing the result

                    results.Enqueue(docType + " " + format);
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Error processing file: " + fullPath + " - " + ex.Message);
            }
        }

        private static (string, string) ParseString(string input)
        {
            // Remove ANSI escape codes
            string cleanInput = Regex.Replace(input, @"\u001B\[\d+(;\d+)?m", "");

            // Extract the document type and format
            string docType = Regex.Match(cleanInput, @"[^:]+:\s*(.*?)\s*\(").Groups[1].Value;
            string format = Regex.Match(cleanInput, @"\((.*?)\)").Groups[1].Value;

            return (docType, format);
        }

        private static void WriteToLog(string line)
        {
            lock (logFileLock)
            {
                try
                {
                    using StreamWriter sw = File.AppendText("filemonlogs.txt");
                    sw.WriteLine(line);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error writing to log: " + ex.Message);
                }
            }
        }
    }
}
