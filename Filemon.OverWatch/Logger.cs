using System;
using System.IO;

namespace Filemon.OverWatch
{
    public static class Logger
    {
        private static readonly object logFileLock = new object();

        public static void InitializeLogs()
        {
            try
            {
                File.Create(Constants.FilemonFileName).Close();
                File.Create("filemonlogs.txt").Close();
                WriteToLog("Filemon is Running");
                WriteToLog("filemon.txt file path: " + Path.GetFullPath(Constants.FilemonFileName));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating files: " + ex.Message);
            }
        }

        public static void WriteToLog(string line)
        {
            lock (logFileLock)
            {
                try
                {
                    using (StreamWriter sw = File.AppendText("filemonlogs.txt"))
                    {
                        sw.WriteLine(line);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error writing to log: " + ex.Message);
                }
            }
        }
    }
}
