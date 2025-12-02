using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace RDR2ModeSwitcher
{
    public static class BackupManager
    {
        private static readonly string BackupDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RDR2ModeSwitcher",
            "Backups"
        );

        public static void CreateBackup(string settingsXmlPath)
        {
            try
            {
                if (!Directory.Exists(BackupDirectory))
                    Directory.CreateDirectory(BackupDirectory);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFile = Path.Combine(BackupDirectory, $"system_backup_{timestamp}.xml");

                File.Copy(settingsXmlPath, backupFile, true);

                MessageBox.Show($"Backup created successfully!\n\nLocation: {backupFile}",
                    "Backup Created",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create backup:\n\n{ex.Message}",
                    "Backup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public static void RestoreBackup(string settingsXmlPath)
        {
            try
            {
                if (!Directory.Exists(BackupDirectory))
                {
                    MessageBox.Show("No backups found.",
                        "Restore Backup",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string[] backups = Directory.GetFiles(BackupDirectory, "system_backup_*.xml");

                if (backups.Length == 0)
                {
                    MessageBox.Show("No backups found.",
                        "Restore Backup",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.InitialDirectory = BackupDirectory;
                    ofd.Filter = "XML Backup Files (*.xml)|*.xml";
                    ofd.Title = "Select Backup to Restore";

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        var result = MessageBox.Show(
                            "This will overwrite your current settings XML file.\n\nContinue?",
                            "Confirm Restore",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                        {
                            File.Copy(ofd.FileName, settingsXmlPath, true);
                            MessageBox.Show("Backup restored successfully!",
                                "Restore Complete",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to restore backup:\n\n{ex.Message}",
                    "Restore Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
