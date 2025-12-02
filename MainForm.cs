using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace RDR2ModeSwitcher
{
    public partial class MainForm : Form
    {
        private AppSettings _settings;
        private string GameDir => _settings.GameDirectory;
        private string ModDir => _settings.ModDirectory;
        private string SettingsXmlPath => _settings.SettingsXmlPath;
        private string SoundPath => _settings.SoundPath;

        private const string Rdr2ProcessName = "RDR2";
        private NotifyIcon trayIcon;
        private int animPhase = 0;
        private bool isWorking = false;
        private Point lastPoint;

        // NEW: keep track of the launched game process
        private Process gameProcess;

        // NEW: stats button
        private Button btnStats;

        // OPTIONAL: tooltip for stats button
        private ToolTip toolTip;

        public MainForm(AppSettings settings)
        {
            _settings = settings;
            InitializeComponent();
            SetupTrayIcon();

            // NEW: create stats button after InitializeComponent so topBar exists
            CreateStatsButton();

            UpdateStats();
            this.FormClosing += MainForm_FormClosing;
        }

        // NEW: create the stats button and add to top bar
        private void CreateStatsButton()
        {
            // If you already have a ToolTip component from designer, you can skip this
            toolTip = new ToolTip();

            btnStats = new Button
            {
                Text = "📊",
                Location = new Point(645, 10), // Adjust as needed
                Size = new Size(40, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(15, 255, 255, 255),
                Cursor = Cursors.Hand
            };

            btnStats.FlatAppearance.BorderColor = Color.FromArgb(40, 255, 255, 255);
            btnStats.FlatAppearance.BorderSize = 1;
            btnStats.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 255, 255, 255);
            btnStats.Click += BtnStats_Click;

            // Correct way to set tooltip for a Button
            toolTip.SetToolTip(btnStats, "View Play Time Statistics");

            // Assumes you have a Panel or control named topBar
            topBar.Controls.Add(btnStats);
        }

        private void TopBar_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = new Point(e.X, e.Y);
        }

        private void TopBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            if (isWorking)
            {
                animPhase = (animPhase + 3) % 100;
                progressBar.Value = animPhase;
            }
        }

        private void UpdateStats()
        {
            DateTime lastPlayed = LaunchHistoryManager.GetLastPlayed();
            int totalLaunches = LaunchHistoryManager.GetTotalLaunches();

            lblLastPlayed.Text = $"📅 Last Played: {(lastPlayed == DateTime.MinValue ? "Never" : lastPlayed.ToString("MMM dd, yyyy HH:mm"))}";
            lblTotalLaunches.Text = $"🚀 Total Launches: {totalLaunches}";
        }

        private void SetWorkingState(bool working, string status, Color statusColor)
        {
            isWorking = working;
            btnLaunchStory.Enabled = !working;
            btnLaunchOnline.Enabled = !working;
            btnSettings.Enabled = !working;
            btnSync.Enabled = !working;
            btnBackup.Enabled = !working;
            btnHistory.Enabled = !working;
            lblStatus.Text = status;
            lblStatus.ForeColor = statusColor;
            progressBar.Visible = working;
            Application.DoEvents();
        }

        private async Task HandleStoryModeAsync()
        {
            try
            {
                SetWorkingState(true, "⚙️ Configuring Story Mode...", Color.Orange);

                await Task.Run(() =>
                {
                    EnableMods();
                    SetAPI("kSettingAPI_Vulkan");
                });

                PlayDoneSound();
                SetWorkingState(true, "🚀 Launching Story Mode...", Color.Cyan);
                await Task.Delay(500);

                // UPDATED: use LaunchGame so tracking + history are handled
                LaunchGame("Story Mode");

                MinimizeToTray();
                await MonitorGameProcessAsync();
                RestoreFromTray();

                SetWorkingState(false, "✅ Game closed. Ready.", Color.LightGreen);
            }
            catch (Exception ex)
            {
                ErrorBox(ex);
            }
        }

        private async Task HandleOnlineModeAsync()
        {
            try
            {
                SetWorkingState(true, "⚙️ Configuring Online Mode...", Color.Orange);

                await Task.Run(() =>
                {
                    DisableMods();
                    SetAPI("kSettingAPI_DX12");
                });

                PlayDoneSound();
                SetWorkingState(true, "🚀 Launching Online Mode...", Color.Cyan);
                await Task.Delay(500);

                // UPDATED: use LaunchGame so tracking + history are handled
                LaunchGame("Online Mode");

                MinimizeToTray();
                await MonitorGameProcessAsync();
                RestoreFromTray();

                SetWorkingState(false, "✅ Game closed. Ready.", Color.LightGreen);
            }
            catch (Exception ex)
            {
                ErrorBox(ex);
            }
        }

        private void EnableMods()
        {
            if (!Directory.Exists(ModDir))
                throw new DirectoryNotFoundException($"Mod directory not found: {ModDir}");
            if (!Directory.Exists(GameDir))
                throw new DirectoryNotFoundException($"Game directory not found: {GameDir}");
            CopyAll(new DirectoryInfo(ModDir), new DirectoryInfo(GameDir));
        }

        private void DisableMods()
        {
            if (!Directory.Exists(ModDir))
                throw new DirectoryNotFoundException($"Mod directory not found: {ModDir}");
            if (!Directory.Exists(GameDir))
                throw new DirectoryNotFoundException($"Game directory not found: {GameDir}");

            var source = new DirectoryInfo(ModDir);
            foreach (var entry in source.GetFileSystemInfos())
            {
                string target = Path.Combine(GameDir, entry.Name);
                try
                {
                    if (Directory.Exists(target))
                        Directory.Delete(target, true);
                    else if (File.Exists(target))
                        File.Delete(target);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to remove {entry.Name}: {ex.Message}", ex);
                }
            }
        }

        private void SetAPI(string apiValue)
        {
            if (!File.Exists(SettingsXmlPath))
                throw new FileNotFoundException($"Settings XML not found: {SettingsXmlPath}");

            var doc = new XmlDocument();
            doc.XmlResolver = null;
            doc.Load(SettingsXmlPath);

            XmlNode apiNode = doc.SelectSingleNode("//advancedGraphics/API");
            if (apiNode == null)
                throw new Exception("Could not find <API> node in system.xml");

            apiNode.InnerText = apiValue;
            doc.Save(SettingsXmlPath);
        }

        // NEW: replaces the old LaunchRDR2 logic for launching + tracking
        private void LaunchGame(string mode)
        {
            try
            {
                string exePath = Path.Combine(GameDir, "PlayRDR2.exe");

                if (!File.Exists(exePath))
                {
                    MessageBox.Show($"Executable not found: {exePath}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Launch game and keep the process
                gameProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    WorkingDirectory = GameDir
                });

                // Start tracking
                PlayTimeTracker.StartTracking(mode, gameProcess);

                // Add to history
                LaunchHistoryManager.AddEntry(mode, "Launched");
                UpdateStats();

                MessageBox.Show($"{mode} launched successfully!\nPlay time tracking started.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LaunchHistoryManager.AddEntry(mode, $"Failed: {ex.Message}");
                MessageBox.Show($"Failed to launch: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // If you no longer need this, you can remove it.
        // Keeping it UNUSED for now in case other code calls it.
        private void LaunchRDR2()
        {
            string exe = Path.Combine(GameDir, "PlayRDR2.exe");
            if (!File.Exists(exe))
                throw new FileNotFoundException($"PlayRDR2.exe not found: {exe}");

            Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = true,
                WorkingDirectory = GameDir
            });
        }

        private async Task MonitorGameProcessAsync()
        {
            await Task.Delay(5000);
            var timeout = DateTime.Now.AddHours(12);

            while (DateTime.Now < timeout)
            {
                var procs = Process.GetProcessesByName(Rdr2ProcessName);
                if (procs == null || procs.Length == 0)
                    break;
                await Task.Delay(3000);
            }

            // NEW: stop tracking when the game actually closes
            PlayTimeTracker.StopTracking();
        }

        private void PlayDoneSound()
        {
            try
            {
                if (!string.IsNullOrEmpty(SoundPath) && File.Exists(SoundPath))
                    new SoundPlayer(SoundPath).Play();
                else
                    SystemSounds.Asterisk.Play();
            }
            catch
            {
                SystemSounds.Asterisk.Play();
            }
        }

        private void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);
            foreach (FileInfo fi in source.GetFiles())
            {
                try
                {
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to copy {fi.Name}: {ex.Message}", ex);
                }
            }
            foreach (DirectoryInfo sub in source.GetDirectories())
                CopyAll(sub, target.CreateSubdirectory(sub.Name));
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm(_settings))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    _settings = settingsForm.Settings;
                    MessageBox.Show("Settings saved successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnSync_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "This will scan your game directory for new mods and sync them to your mod backup folder.\n\nContinue?",
                "Sync Mods", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SetWorkingState(true, "🔄 Scanning and syncing mods...", Color.Cyan);

                Task.Run(() =>
                {
                    var syncResult = ModSyncManager.ScanAndSync(GameDir, ModDir);

                    this.Invoke(new Action(() =>
                    {
                        SetWorkingState(false, "✨ Ready to launch", Color.LightGreen);
                        MessageBox.Show(syncResult.Message,
                            syncResult.Success ? "Sync Complete" : "Sync Failed",
                            MessageBoxButtons.OK,
                            syncResult.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                    }));
                });
            }
        }

        private void BtnHistory_Click(object sender, EventArgs e)
        {
            using (var historyForm = new HistoryForm())
            {
                historyForm.ShowDialog();
            }
        }

        // NEW: open play time stats window
        private void BtnStats_Click(object sender, EventArgs e)
        {
            using (var statsForm = new PlayTimeStatsForm())
            {
                statsForm.ShowDialog();
            }
        }

        private void SetupTrayIcon()
        {
            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show", null, (s, e) => RestoreFromTray());
            trayMenu.Items.Add("Exit", null, (s, e) => Application.Exit());

            trayIcon = new NotifyIcon
            {
                Text = "RDR2 Mode Switcher",
                Visible = false,
                ContextMenuStrip = trayMenu,
                Icon = SystemIcons.Application
            };
            trayIcon.DoubleClick += (s, e) => RestoreFromTray();
        }

        private void MinimizeToTray()
        {
            this.Hide();
            trayIcon.Visible = true;
            trayIcon.ShowBalloonTip(2000, "RDR2 Mode Switcher",
                "Game is running. Double-click to restore.", ToolTipIcon.Info);
        }

        private void RestoreFromTray()
        {
            trayIcon.Visible = false;
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // NEW: stop tracking on app exit as well
            PlayTimeTracker.StopTracking();
            trayIcon?.Dispose();
        }

        private void ErrorBox(Exception ex)
        {
            SetWorkingState(false, "❌ Error occurred", Color.Red);
            MessageBox.Show($"Error:\n\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
