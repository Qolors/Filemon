using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Filemon
{
    public static class ProcessManager
    {
        public static void Status()
        {
            if (File.Exists(Constants.FilemonFileName))
            {
                Console.WriteLine("Filemon is Running");
            }
            else
            {
                Console.WriteLine("Filemon is Not Running");
            }
        }

        public static void Kill()
        {
            try
            {
                if (File.Exists(Constants.FilemonFileName))
                {
                    var lines = File.ReadLines(Constants.FilemonFileName).ToList();
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

                    File.Delete(Constants.FilemonFileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during kill operation: " + ex.Message);
            }
        }

        public static void Start()
        {
            try
            {
                if (File.Exists(Constants.FilemonFileName))
                {
                    var lines = File.ReadLines(Constants.FilemonFileName).ToList();
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

                        File.WriteAllText(Constants.FilemonFileName, process.Id.ToString());
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
