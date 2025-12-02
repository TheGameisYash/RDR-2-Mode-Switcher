using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RDR2ModeSwitcher
{
    public class SettingsForm : Form
    {
        private Panel topBar;
        private Label lblTitle;
        private Button btnClose;

        private Panel contentPanel;
        private TextBox txtGameDir;
        private TextBox txtModDir;
        private TextBox txtSettingsXml;
        private TextBox txtSound;
        private Button btnBrowseGame;
        private Button btnBrowseMod;
        private Button btnBrowseXml;
        private Button btnBrowseSound;
        private Button btnSave;
        private Button btnCancel;

        private Point lastPoint;

        public AppSettings Settings { get; private set; }

        public SettingsForm(AppSettings currentSettings)
        {
            Settings = currentSettings;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "RDR2 Mode Switcher - Settings";
            this.Size = new Size(700, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.DoubleBuffered = true;

            CreateTopBar();
            CreateContentPanel();

            this.Load += SettingsForm_Load;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            // Apply ultra-transparent glassmorphism
            AcrylicHelper.EnableBlur(this.Handle, Color.FromArgb(120, 15, 15, 20));
        }

        private void CreateTopBar()
        {
            topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(15, 255, 255, 255)
            };
            topBar.Paint += TopBar_Paint;
            topBar.MouseDown += TopBar_MouseDown;
            topBar.MouseMove += TopBar_MouseMove;
            this.Controls.Add(topBar);

            lblTitle = new Label
            {
                Text = "⚙️  SETTINGS",
                Location = new Point(20, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            topBar.Controls.Add(lblTitle);

            btnClose = new Button
            {
                Text = "✕",
                Location = new Point(645, 10),
                Size = new Size(40, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(232, 17, 35),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            topBar.Controls.Add(btnClose);
        }

        private void TopBar_Paint(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                topBar.ClientRectangle,
                Color.FromArgb(20, 255, 255, 255),
                Color.FromArgb(10, 255, 255, 255),
                90f))
            {
                e.Graphics.FillRectangle(brush, topBar.ClientRectangle);
            }
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

        private void CreateContentPanel()
        {
            contentPanel = new Panel
            {
                Location = new Point(30, 90),
                Size = new Size(640, 320),
                BackColor = Color.FromArgb(15, 255, 255, 255)
            };
            contentPanel.Paint += ContentPanel_Paint;
            this.Controls.Add(contentPanel);

            // Game Directory
            AddPathRow("Game Directory:", ref txtGameDir, ref btnBrowseGame, 20, BrowseGameDir);

            // Mod Directory
            AddPathRow("Mod Directory:", ref txtModDir, ref btnBrowseMod, 80, BrowseModDir);

            // Settings XML
            AddPathRow("Settings XML:", ref txtSettingsXml, ref btnBrowseXml, 140, BrowseSettingsXml);

            // Sound File
            AddPathRow("Sound File (Optional):", ref txtSound, ref btnBrowseSound, 200, BrowseSound);

            // Save button
            btnSave = new Button
            {
                Text = "✅  Save Settings",
                Location = new Point(340, 260),
                Size = new Size(140, 45),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 102, 204);
            btnSave.Click += BtnSave_Click;
            contentPanel.Controls.Add(btnSave);

            // Cancel button
            btnCancel = new Button
            {
                Text = "❌  Cancel",
                Location = new Point(490, 260),
                Size = new Size(140, 45),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(15, 255, 255, 255),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(40, 255, 255, 255);
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 255, 255, 255);
            contentPanel.Controls.Add(btnCancel);

            this.CancelButton = btnCancel;
        }

        private void ContentPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (LinearGradientBrush brush = new LinearGradientBrush(
                contentPanel.ClientRectangle,
                Color.FromArgb(25, 255, 255, 255),
                Color.FromArgb(12, 255, 255, 255),
                90f))
            {
                e.Graphics.FillRectangle(brush, contentPanel.ClientRectangle);
            }

            using (Pen pen = new Pen(Color.FromArgb(40, 255, 255, 255), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, contentPanel.Width - 1, contentPanel.Height - 1);
            }
        }

        private void AddPathRow(string labelText, ref TextBox textBox, ref Button button, int yPos, EventHandler clickHandler)
        {
            Label lbl = new Label
            {
                Text = labelText,
                Location = new Point(20, yPos + 5),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            contentPanel.Controls.Add(lbl);

            textBox = new TextBox
            {
                Width = 370,
                Location = new Point(180, yPos),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(40, 40, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            contentPanel.Controls.Add(textBox);

            button = new Button
            {
                Text = "Browse...",
                Location = new Point(560, yPos),
                Size = new Size(70, 26),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8),
                BackColor = Color.FromArgb(15, 255, 255, 255),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(40, 255, 255, 255);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 255, 255, 255);
            button.Click += clickHandler;
            contentPanel.Controls.Add(button);
        }

        private void LoadSettings()
        {
            txtGameDir.Text = Settings.GameDirectory;
            txtModDir.Text = Settings.ModDirectory;
            txtSettingsXml.Text = Settings.SettingsXmlPath;
            txtSound.Text = Settings.SoundPath;
        }

        private void BrowseGameDir(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select RDR2 Game Directory (containing PlayRDR2.exe)";
                fbd.SelectedPath = txtGameDir.Text;
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtGameDir.Text = fbd.SelectedPath;
            }
        }

        private void BrowseModDir(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select Mods Directory";
                fbd.SelectedPath = txtModDir.Text;
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtModDir.Text = fbd.SelectedPath;
            }
        }

        private void BrowseSettingsXml(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select system.xml from RDR2 Settings";
                ofd.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
                ofd.FileName = "system.xml";
                if (!string.IsNullOrEmpty(txtSettingsXml.Text))
                    ofd.InitialDirectory = System.IO.Path.GetDirectoryName(txtSettingsXml.Text);

                if (ofd.ShowDialog() == DialogResult.OK)
                    txtSettingsXml.Text = ofd.FileName;
            }
        }

        private void BrowseSound(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Sound File (Optional)";
                ofd.Filter = "WAV Files (*.wav)|*.wav|All Files (*.*)|*.*";
                if (!string.IsNullOrEmpty(txtSound.Text))
                    ofd.InitialDirectory = System.IO.Path.GetDirectoryName(txtSound.Text);

                if (ofd.ShowDialog() == DialogResult.OK)
                    txtSound.Text = ofd.FileName;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Settings.GameDirectory = txtGameDir.Text.Trim();
            Settings.ModDirectory = txtModDir.Text.Trim();
            Settings.SettingsXmlPath = txtSettingsXml.Text.Trim();
            Settings.SoundPath = txtSound.Text.Trim();

            if (!Settings.Validate(out string error))
            {
                MessageBox.Show(error, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Settings.IsConfigured = true;
            Settings.Save();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
