
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
                        DateTime now = DateTime.Now;
                        sw.WriteLine($"{now.ToString("hh:mm:ss.F")}: {line}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error writing to log: " + ex.Message);
                }
            }
        }

        public static void PrettyPrint()
        {
            if (File.Exists(logFilePath))
            {
                IEnumerable<string> logs = File.ReadLines(logFilePath);

                foreach (string log in logs)
                {
                    Console.WriteLine(log);
                }
            }
        }

        public static void DeleteLogs()
        {

            lock (logFileLock)
            {
                if (File.Exists(logFilePath))
                {
                    File.Delete(logFilePath);
                }
            }

        }
    }
}
