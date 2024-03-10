
namespace Filemon
{
    public static class FileOperations
    {
        public static void Monitor(string path)
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
                string filepath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "AppData", "Local", "Filemon", Constants.ConfigFileName);

                if (!File.Exists(filepath))
                {
                    File.Create(filepath).Close();
                }

                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to config.txt: " + ex.Message);
            }
        }
    }
}
