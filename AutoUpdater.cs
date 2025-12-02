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
        public long FileSize { get; set; }
    }

    public static class AutoUpdater
    {
        // Change this to your update server URL (GitHub, your website, etc.)
        private const string UPDATE_CHECK_URL = "https://raw.githubusercontent.com/YOUR-USERNAME/RDR2ModeSwitcher/main/update.json";
        private const string CURRENT_VERSION = "1.0.0"; // Update this with each release

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
                // Log error but don't block app launch
                System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");
            }

            return null;
        }

        public static async Task<bool> DownloadAndInstallUpdateAsync(UpdateInfo updateInfo, IProgress<int> progress)
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "RDR2ModeSwitcher_Update.exe");
                string updaterPath = Path.Combine(Application.StartupPath, "Updater.exe");

                // Download the new version
                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        progress?.Report(e.ProgressPercentage);
                    };

                    await client.DownloadFileTaskAsync(new Uri(updateInfo.DownloadUrl), tempPath);
                }

                // Create updater script
                CreateUpdaterScript(tempPath, updaterPath);

                // Launch updater and exit current app
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = updaterPath,
                    UseShellExecute = true,
                    Verb = "runas" // Request admin rights
                };

                Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Update Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static void CreateUpdaterScript(string newExePath, string updaterPath)
        {
            // Create a batch script that will replace the old exe with new one
            string currentExe = Application.ExecutablePath;
            string currentExeBackup = currentExe + ".old";

            string batchScript = $@"
@echo off
timeout /t 2 /nobreak > nul
taskkill /F /IM RDR2ModeSwitcher.exe > nul 2>&1
timeout /t 1 /nobreak > nul
move /Y ""{currentExe}"" ""{currentExeBackup}"" > nul 2>&1
move /Y ""{newExePath}"" ""{currentExe}""
start """" ""{currentExe}""
timeout /t 2 /nobreak > nul
del ""{currentExeBackup}"" > nul 2>&1
del ""%~f0"" & exit
";

            File.WriteAllText(updaterPath.Replace(".exe", ".bat"), batchScript);

            // Create updater.exe (simple launcher for the batch file)
            CreateUpdaterExe(updaterPath);
        }

        private static void CreateUpdaterExe(string updaterPath)
        {
            string batchPath = updaterPath.Replace(".exe", ".bat");

            // Simple VBS script to run batch silently
            string vbsScript = $@"
Set WshShell = CreateObject(""WScript.Shell"")
WshShell.Run chr(34) & ""{batchPath}"" & Chr(34), 0
Set WshShell = Nothing
";

            string vbsPath = updaterPath.Replace(".exe", ".vbs");
            File.WriteAllText(vbsPath, vbsScript);

            // Create a simple exe wrapper (you can use ILMerge or embed this)
            // For now, we'll just use the VBS approach
            File.Copy(vbsPath, updaterPath, true);
        }

        public static string GetCurrentVersion()
        {
            return CURRENT_VERSION;
        }
    }
}
