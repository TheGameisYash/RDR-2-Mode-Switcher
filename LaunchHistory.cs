using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace RDR2ModeSwitcher
{
    public class LaunchHistoryEntry
    {
        public DateTime Timestamp { get; set; }
        public string Mode { get; set; }
        public string Status { get; set; }
    }

    public static class LaunchHistoryManager
    {
        private static string GetHistoryFilePath()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RDR2ModeSwitcher");

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            return Path.Combine(appDataPath, "launch_history.json");
        }

        public static void AddEntry(string mode, string status)
        {
            var history = LoadHistory();

            history.Insert(0, new LaunchHistoryEntry
            {
                Timestamp = DateTime.Now,
                Mode = mode,
                Status = status
            });

            // Keep only last 100 entries
            if (history.Count > 100)
                history = history.Take(100).ToList();

            SaveHistory(history);
        }

        public static List<LaunchHistoryEntry> LoadHistory()
        {
            string filePath = GetHistoryFilePath();

            if (!File.Exists(filePath))
                return new List<LaunchHistoryEntry>();

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<LaunchHistoryEntry>>(json)
                       ?? new List<LaunchHistoryEntry>();
            }
            catch
            {
                return new List<LaunchHistoryEntry>();
            }
        }

        private static void SaveHistory(List<LaunchHistoryEntry> history)
        {
            string filePath = GetHistoryFilePath();
            string json = JsonConvert.SerializeObject(history, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static DateTime GetLastPlayed()
        {
            var history = LoadHistory();
            return history.Count > 0 ? history[0].Timestamp : DateTime.MinValue;
        }

        public static int GetTotalLaunches()
        {
            var history = LoadHistory();
            return history.Count;
        }

        public static void ClearHistory()
        {
            string filePath = GetHistoryFilePath();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
