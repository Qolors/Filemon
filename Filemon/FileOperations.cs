using Microsoft.VisualBasic;
using System;
using System.IO;

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
                if (!File.Exists(Constants.ConfigFileName))
                {
                    File.Create(Constants.ConfigFileName).Close();
                }

                using (StreamWriter sw = File.AppendText(Constants.ConfigFileName))
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
