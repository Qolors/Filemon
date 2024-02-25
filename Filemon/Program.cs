using System.Diagnostics;
using System.IO;

namespace Filemon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                switch (args[0])
                {
                    case "monitor":
                        if (args.Length == 2)
                        {
                            Monitor(args[1]);
                        }
                        else
                        {
                            Console.WriteLine("Usage: Filemon monitor <path>");
                        }
                        break;
                    case "status":
                        Status();
                        break;
                    case "start":
                        Start();
                        break;
                    case "return":
                        Kill();
                        break;
                    default:
                        Console.WriteLine("Invalid command. Available commands: monitor, status, start, return");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Usage: Filemon <command> [options]");
            }
        }

        private static void Monitor(string path)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Path --> " + path + " - Does Not Exist");
                return;
            }

            WriteToConfig(path);
        }

        private static void WriteToConfig(string path)
        {
            try
            {
                if (!File.Exists("config.txt"))
                {
                    File.Create("config.txt").Close();
                }

                using (StreamWriter sw = File.AppendText("config.txt"))
                {
                    sw.WriteLine(path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to config.txt: " + ex.Message);
            }
        }

        private static void Status()
        {
            if (File.Exists("filemon.txt"))
            {
                Console.WriteLine("Filemon is Running");
            }
            else
            {
                Console.WriteLine("Filemon is Not Running");
            }
        }

        private static void Kill()
        {
            try
            {
                if (File.Exists("filemon.txt"))
                {
                    var lines = File.ReadLines("filemon.txt").ToList();
                    if (lines.Count > 0)
                    {
                        Console.WriteLine("Filemon is Running");
                        var readProcess = int.TryParse(lines.First(), out int id);

                        if (readProcess)
                        {
                            Console.WriteLine("Killing Process: " + id);
                            Process.GetProcessById(id)?.Kill();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Filemon is Not Running");
                    }

                    File.Delete("filemon.txt");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during kill operation: " + ex.Message);
            }
        }

        private static void Start()
        {
            try
            {
                if (File.Exists("filemon.txt"))
                {
                    var lines = File.ReadLines("filemon.txt").ToList();
                    if (lines.Count > 0)
                    {
                        Console.WriteLine("Filemon is Already Running");
                        var readProcess = int.TryParse(lines.First(), out int id);

                        if (readProcess)
                        {
                            Console.WriteLine("Finding/Killing Process: " + id);
                            Process.GetProcessById(id)?.Kill();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Filemon is Not Running");
                    }
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "Filemon.OverWatch.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        // Wait for a moment to ensure the process ID is available
                        System.Threading.Thread.Sleep(100);

                        File.WriteAllText("filemon.txt", process.Id.ToString());
                        Console.WriteLine(process.Id.ToString());
                        Console.WriteLine("Filemon Started Successfully");
                    }
                    else
                    {
                        Console.WriteLine("Filemon Failed to Start");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during start operation: " + ex.Message);
            }
        }
    }
}
