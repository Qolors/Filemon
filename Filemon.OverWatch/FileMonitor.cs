using Filemon.OverWatch.Helpers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipes;

namespace Filemon.OverWatch
{
    public static class FileMonitor
    {
        private static Dictionary<string, string> _ruleSets = new();
        public static void StartMonitoring(ConcurrentDictionary<string, FileSystemWatcher> config, Dictionary<string, string> ruleSets)
        {
            _ruleSets = ruleSets;

            Logger.InitializeLogs();
            MonitoringLoop(config, ruleSets);
        }

        public static void InitializeWatcher(string directory, ConcurrentDictionary<string, FileSystemWatcher> config)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(directory)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            watcher.Created += (sender, e) => OnChanged(sender, e, _ruleSets);

            if (!config.TryAdd(directory, watcher))
            {
                Logger.WriteToLog($"Failed to add watcher for directory: {directory}");
            }
        }

        private static void MonitoringLoop(ConcurrentDictionary<string, FileSystemWatcher> config, Dictionary<string, string> ruleSets)
        {
            Logger.WriteToLog("Monitoring started.");

            while (true)
            {
                using (var serverClient = new NamedPipeServerStream("FilemonPipe"))
                {
                    serverClient.WaitForConnection();

                    try
                    {
                        byte[] buffer = new byte[1024];

                        int bytesRead = serverClient.Read(buffer, 0, buffer.Length);

                        string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        if (message == "status")
                        {
                            string response = "Running";

                            byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(response);

                            serverClient.Write(responseBytes, 0, responseBytes.Length);
                        }
                        else if (message == "kill")
                        {
                            string response = "Stopped";

                            byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(response);

                            serverClient.Write(responseBytes, 0, responseBytes.Length);

                            Logger.DeleteLogs();

                            break;
                        }
                        else if (message == "logs")
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToLog("Error processing client: " + ex.Message);
                    }
                }
            }
        }

        private static void OnChanged(object sender, FileSystemEventArgs e, Dictionary<string, string> ruleSets)
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
                    Logger.WriteToLog("Failed to start magika.exe for: " + fullPath);
                    return;
                }

                using StreamReader reader = process.StandardOutput;

                Thread.Sleep(1000); // Wait for the process to produce output

                string result = reader.ReadToEnd();

                var (docType, format) = StringParser.ParseString(result);

                if (string.IsNullOrEmpty(docType) || string.IsNullOrEmpty(format))
                {
                    Logger.WriteToLog("Failed to parse output for: " + fullPath);
                    return;
                }

                Logger.WriteToLog("Doc Type: " + docType);
                Logger.WriteToLog("Format: " + format);

                string docstrip = docType.Split(':')[1].Trim();
                string commandstrip = docstrip.Replace(" ", "_");

                if (ruleSets.TryGetValue(commandstrip, out string? value))
                {
                    Logger.WriteToLog(commandstrip + " found in rulesets - executing rule");

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
                            Logger.WriteToLog(commandresult);
                        }
                        else
                        {
                            Logger.WriteToLog("Failed to execute command.");
                        }
                    }
                }
                else
                {
                    Logger.WriteToLog(commandstrip + " not found in rulesets - no action taken");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("Error processing file: " + fullPath + " - " + ex.Message);
            }
        }
    }
}
