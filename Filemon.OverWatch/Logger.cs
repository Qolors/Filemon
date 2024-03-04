
namespace Filemon.OverWatch
{
    public static class Logger
    {
        private static readonly object logFileLock = new object();
        private static readonly string logFilePath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"),"AppData", "Local", "Filemon", "filemonlogs.txt");
        public static void InitializeLogs()
        {
            try
            {
                File.Create(logFilePath).Close();
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
                    using (StreamWriter sw = File.AppendText(logFilePath))
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
