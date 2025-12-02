using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RDR2ModeSwitcher
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check for updates FIRST (blocking)
            CheckForUpdatesSync();

            AppSettings settings = AppSettings.Load();

            if (!settings.IsConfigured || !settings.Validate(out _))
            {
                using (var settingsForm = new SettingsForm(settings))
                {
                    if (settingsForm.ShowDialog() != DialogResult.OK)
                    {
                        MessageBox.Show("Settings are required to run the application.",
                            "Configuration Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    settings = settingsForm.Settings;
                }
            }

            Application.Run(new MainForm(settings));
        }

        private static void CheckForUpdatesSync()
        {
            try
            {
                Task<UpdateInfo> checkTask = AutoUpdater.CheckForUpdatesAsync();
                checkTask.Wait(); // Block until check completes

                UpdateInfo updateInfo = checkTask.Result;

                if (updateInfo != null)
                {
                    // Show update form (forced if ForceUpdate is true)
                    using (var updateForm = new UpdateForm(updateInfo))
                    {
                        if (updateForm.ShowDialog() != DialogResult.OK && updateInfo.ForceUpdate)
                        {
                            // If forced update and user didn't update, exit app
                            Environment.Exit(0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Silently continue if update check fails
                System.Diagnostics.Debug.WriteLine($"Update check error: {ex.Message}");
            }
        }
    }
}
