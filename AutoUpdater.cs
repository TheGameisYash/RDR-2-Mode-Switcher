using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace RDR2ModeSwitcher
{
    public class UpdateInfo
    {
        public string Version { get; set; }
        public string DownloadUrl { get; set; }
        public string Changelog { get; set; }
        public bool ForceUpdate { get; set; }
        public long FileSize { get; set; } = 0;
    }

    public static class AutoUpdater
    {
        // Your CORRECT GitHub repository URL
        private const string UPDATE_CHECK_URL = "https://raw.githubusercontent.com/TheGameisYash/RDR-2-Mode-Switcher/main/update.json";
        private const string CURRENT_VERSION = "1.0.6";


        public static async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "RDR2ModeSwitcher");
                    string json = await client.DownloadStringTaskAsync(UPDATE_CHECK_URL);
                    UpdateInfo updateInfo = JsonConvert.DeserializeObject<UpdateInfo>(json);

                    // Compare versions
                    Version current = new Version(CURRENT_VERSION);
                    Version latest = new Version(updateInfo.Version);

                    if (latest > current)
                    {
                        return updateInfo;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");
            }

            return null;
        }

        public static async Task<bool> DownloadAndInstallUpdateAsync(UpdateInfo updateInfo, IProgress<int> progress)
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "RDR2ModeSwitcher_Update.exe");
                string batchPath = Path.Combine(Path.GetTempPath(), "RDR2_update.bat");

                // Download the new version
                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        progress?.Report(e.ProgressPercentage);
                    };

                    await client.DownloadFileTaskAsync(new Uri(updateInfo.DownloadUrl), tempPath);
                }

                // Create batch script to replace exe
                string currentExe = Application.ExecutablePath;
                string batchScript = $@"@echo off
timeout /t 2 /nobreak > nul
taskkill /F /IM RDR2ModeSwitcher.exe > nul 2>&1
timeout /t 1 /nobreak > nul
del ""{currentExe}.old"" > nul 2>&1
move /Y ""{currentExe}"" ""{currentExe}.old""
move /Y ""{tempPath}"" ""{currentExe}""
start """" ""{currentExe}""
timeout /t 2 /nobreak > nul
del ""{currentExe}.old"" > nul 2>&1
del ""%~f0"" & exit";

                File.WriteAllText(batchPath, batchScript);

                // Launch updater batch file
                Process.Start(new ProcessStartInfo
                {
                    FileName = batchPath,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Update Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static string GetCurrentVersion()
        {
            return CURRENT_VERSION;
        }
    }
}
