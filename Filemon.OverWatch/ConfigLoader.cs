using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Filemon.OverWatch
{
    public static class ConfigLoader
    {
        public static void LoadConfig(ConcurrentDictionary<string, FileSystemWatcher> config, Dictionary<string, string> ruleSets)
        {
            List<string> directories = LoadDirectories();

            foreach (string directory in directories)
            {
                FileMonitor.InitializeWatcher(directory, config);
            }

            LoadRuleSets(ruleSets);
        }

        private static List<string> LoadDirectories()
        {
            List<string> directories = new List<string>();

            if (File.Exists(Constants.ConfigFileName))
            {
                using StreamReader sr = new StreamReader(Constants.ConfigFileName);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    directories.Add(line);
                    Logger.WriteToLog("Added: " + line);
                }
            }

            return directories;
        }

        private static void LoadRuleSets(Dictionary<string, string> ruleSets)
        {
            if (Directory.Exists("rulesets"))
            {
                foreach (string file in Directory.EnumerateFiles("rulesets", "*.bat"))
                {
                    string key = Path.GetFileNameWithoutExtension(file).Trim().Replace(" ", "_");
                    ruleSets.Add(key, Path.GetFullPath(file));
                    Logger.WriteToLog(Path.GetFullPath(file) + " added to rulesets.");
                }
            }
        }
    }
}
