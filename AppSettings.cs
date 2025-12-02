using System;
using System.IO;
using Newtonsoft.Json;

namespace RDR2ModeSwitcher
{
    public class AppSettings
    {
        public string GameDirectory { get; set; } = "";
        public string ModDirectory { get; set; } = "";
        public string SettingsXmlPath { get; set; } = "";
        public string SoundPath { get; set; } = "";
        public bool IsConfigured { get; set; } = false;

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RDR2ModeSwitcher",
            "settings.json"
        );

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Failed to load settings: {ex.Message}\nUsing default settings.",
                    "Settings Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning
                );
            }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string json = JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Failed to save settings: {ex.Message}",
                    "Settings Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );
            }
        }

        public bool Validate(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(GameDirectory))
            {
                errorMessage = "Game directory is not set.";
                return false;
            }
            if (!Directory.Exists(GameDirectory))
            {
                errorMessage = $"Game directory not found: {GameDirectory}";
                return false;
            }
            if (string.IsNullOrWhiteSpace(ModDirectory))
            {
                errorMessage = "Mod directory is not set.";
                return false;
            }
            if (!Directory.Exists(ModDirectory))
            {
                errorMessage = $"Mod directory not found: {ModDirectory}";
                return false;
            }
            if (string.IsNullOrWhiteSpace(SettingsXmlPath))
            {
                errorMessage = "Settings XML path is not set.";
                return false;
            }
            if (!File.Exists(SettingsXmlPath))
            {
                errorMessage = $"Settings XML file not found: {SettingsXmlPath}";
                return false;
            }

            errorMessage = "";
            return true;
        }
    }
}
