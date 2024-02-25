using System.Collections.Concurrent;

namespace Filemon.OverWatch
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            ConcurrentDictionary<string, FileSystemWatcher> config = new();
            Dictionary<string, string> ruleSets = [];

            ConfigLoader.LoadConfig(config, ruleSets);
            FileMonitor.StartMonitoring(config, ruleSets);
        }
    }
}
