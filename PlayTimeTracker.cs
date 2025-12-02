using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using Newtonsoft.Json;

namespace RDR2ModeSwitcher
{
    public class PlaySession
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Mode { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public int DurationMinutes => (int)Duration.TotalMinutes;
    }

    public class PlayTimeStats
    {
        public List<PlaySession> Sessions { get; set; } = new List<PlaySession>();
        public DateTime FirstPlayed { get; set; }
        public DateTime LastPlayed { get; set; }

        public TimeSpan TotalPlayTime
        {
            get
            {
                return TimeSpan.FromMinutes(Sessions.Sum(s => s.DurationMinutes));
            }
        }

        public TimeSpan TodayPlayTime
        {
            get
            {
                var today = DateTime.Today;
                return TimeSpan.FromMinutes(
                    Sessions.Where(s => s.StartTime.Date == today)
                            .Sum(s => s.DurationMinutes));
            }
        }

        public TimeSpan ThisWeekPlayTime
        {
            get
            {
                var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                return TimeSpan.FromMinutes(
                    Sessions.Where(s => s.StartTime >= startOfWeek)
                            .Sum(s => s.DurationMinutes));
            }
        }

        public TimeSpan ThisMonthPlayTime
        {
            get
            {
                var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                return TimeSpan.FromMinutes(
                    Sessions.Where(s => s.StartTime >= startOfMonth)
                            .Sum(s => s.DurationMinutes));
            }
        }

        public int TotalSessions => Sessions.Count;

        public TimeSpan AverageSessionTime
        {
            get
            {
                if (Sessions.Count == 0) return TimeSpan.Zero;
                return TimeSpan.FromMinutes(Sessions.Average(s => s.DurationMinutes));
            }
        }
    }

    public static class PlayTimeTracker
    {
        private static string GetStatsFilePath()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RDR2ModeSwitcher");

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            return Path.Combine(appDataPath, "playtime_stats.json");
        }

        private static Timer gameCheckTimer;
        private static Process gameProcess;
        private static DateTime currentSessionStart;
        private static string currentMode;
        private static bool isTracking = false;

        public static void StartTracking(string mode, Process process)
        {
            if (isTracking) return;

            gameProcess = process;
            currentMode = mode;
            currentSessionStart = DateTime.Now;
            isTracking = true;

            // Check every 5 seconds if game is still running
            gameCheckTimer = new Timer(5000);
            gameCheckTimer.Elapsed += CheckGameStatus;
            gameCheckTimer.Start();
        }

        private static void CheckGameStatus(object sender, ElapsedEventArgs e)
        {
            if (gameProcess == null || gameProcess.HasExited)
            {
                StopTracking();
            }
        }

        public static void StopTracking()
        {
            if (!isTracking) return;

            gameCheckTimer?.Stop();
            gameCheckTimer?.Dispose();

            var session = new PlaySession
            {
                StartTime = currentSessionStart,
                EndTime = DateTime.Now,
                Mode = currentMode
            };

            SaveSession(session);
            isTracking = false;
        }

        private static void SaveSession(PlaySession session)
        {
            var stats = LoadStats();

            if (stats.FirstPlayed == DateTime.MinValue)
                stats.FirstPlayed = session.StartTime;

            stats.LastPlayed = session.EndTime;
            stats.Sessions.Add(session);

            // Keep only last 500 sessions to prevent file bloat
            if (stats.Sessions.Count > 500)
            {
                stats.Sessions = stats.Sessions.OrderByDescending(s => s.StartTime)
                                               .Take(500)
                                               .ToList();
            }

            SaveStats(stats);
        }

        public static PlayTimeStats LoadStats()
        {
            string filePath = GetStatsFilePath();

            if (!File.Exists(filePath))
                return new PlayTimeStats { FirstPlayed = DateTime.MinValue };

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<PlayTimeStats>(json) ?? new PlayTimeStats();
            }
            catch
            {
                return new PlayTimeStats();
            }
        }

        private static void SaveStats(PlayTimeStats stats)
        {
            string filePath = GetStatsFilePath();
            string json = JsonConvert.SerializeObject(stats, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static void ResetStats()
        {
            string filePath = GetStatsFilePath();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static bool IsCurrentlyTracking()
        {
            return isTracking;
        }

        public static TimeSpan GetCurrentSessionTime()
        {
            if (!isTracking) return TimeSpan.Zero;
            return DateTime.Now - currentSessionStart;
        }
    }
}
