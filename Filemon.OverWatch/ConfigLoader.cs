using System.Collections.Concurrent;

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

            string configPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "AppData", "Local", "Filemon", Constants.ConfigFileName);

            if (File.Exists(configPath))
            {
                using StreamReader sr = new StreamReader(configPath);
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
            string rulesetPath = Path.Combine(Environment
                .GetEnvironmentVariable("USERPROFILE"), "AppData", "Local", "Filemon", "scripts");

            if (Directory.Exists(rulesetPath))
            {
                foreach (string file in Directory.EnumerateFiles(rulesetPath, "*.bat"))
                {
                    string key = Path.GetFileNameWithoutExtension(file).Trim().Replace(" ", "_");
                    ruleSets.Add(key, Path.GetFullPath(file));
                    Logger.WriteToLog(Path.GetFullPath(file) + " added to rulesets.");
                }
            }
        }
    }
}
