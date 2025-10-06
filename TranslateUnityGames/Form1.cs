using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace CyberManhuntLocalizer
{
    public partial class Form1 : Form
    {
        private TextBox txtGamePath;
        private Button btnBrowse;
        private Button btnInstall;
        private Button btnUninstall;
        private Button btnAbout;
        private TextBox txtLog;
        private ProgressBar progressBar;
        private Label lblProgress;

        private DateTime _lockoutEnd = DateTime.MinValue;
        private int _failedAttempts = 0;
        private const string OWNER_PASSWORD = "Duave3691215";

        public Form1()
        {
            InitializeComponent();
            SetupCyberpunkStyle();
            CheckForAppUpdateOnStartup();
        }

        private void InitializeComponent()
        {
            this.Text = "CYBER MANHUNT // РУСИФИКАТОР by Strahoduy";
            this.Size = new Size(720, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            var lblTitle = new Label
            {
                Text = "РУСИФИКАТОР ДЛЯ CYBER MANHUNT (by Strahoduy)",
                ForeColor = Color.MediumPurple,
                Font = new Font("Consolas", 12, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblPath = new Label
            {
                Text = "Путь к папке с игрой (где есть CyberManhunt.exe):",
                ForeColor = Color.LightGray,
                Location = new Point(20, 60),
                AutoSize = true
            };

            txtGamePath = new TextBox
            {
                Location = new Point(20, 90),
                Width = 500,
                BackColor = Color.FromArgb(20, 20, 25),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnBrowse = new Button
            {
                Text = "ОБЗОР",
                Location = new Point(530, 88),
                Width = 90,
                BackColor = Color.MediumPurple,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.Click += (s, e) =>
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        txtGamePath.Text = fbd.SelectedPath;
                    }
                }
            };

            btnInstall = new Button
            {
                Text = "УСТАНОВИТЬ РУСИФИКАТОР",
                Location = new Point(20, 140),
                Width = 240,
                BackColor = Color.LimeGreen,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += BtnInstall_Click;

            btnUninstall = new Button
            {
                Text = "УДАЛИТЬ РУСИФИКАТОР",
                Location = new Point(270, 140),
                Width = 240,
                BackColor = Color.DarkRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUninstall.FlatAppearance.BorderSize = 0;
            btnUninstall.Click += BtnUninstall_Click;

            btnAbout = new Button
            {
                Text = "О СОЗДАТЕЛЕ",
                Location = new Point(520, 140),
                Width = 140,
                BackColor = Color.Cyan,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            btnAbout.FlatAppearance.BorderSize = 0;
            btnAbout.Click += BtnAbout_Click;

            progressBar = new ProgressBar
            {
                Location = new Point(20, 185),
                Width = 660,
                Height = 20,
                Style = ProgressBarStyle.Continuous
            };
            progressBar = new CyberProgressBar
            {
                Location = new Point(20, 185),
                Width = 660,
                Height = 20,
                Style = ProgressBarStyle.Continuous
            };
            // Никаких SetStyle или Paint — всё внутри класса!

            lblProgress = new Label
            {
                Text = "ГОТОВ",
                ForeColor = Color.Cyan,
                Font = new Font("Consolas", 9, FontStyle.Bold),
                Location = new Point(20, 210),
                AutoSize = true
            };

            txtLog = new TextBox
            {
                Location = new Point(20, 240),
                Width = 660,
                Height = 200,
                BackColor = Color.FromArgb(5, 5, 10),
                ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle
            };

            this.Controls.AddRange(new Control[] {
                lblTitle, lblPath, txtGamePath, btnBrowse,
                btnInstall, btnUninstall, btnAbout,
                progressBar, lblProgress, txtLog
            });
        }

        private void SetupCyberpunkStyle()
        {
            this.BackColor = Color.FromArgb(8, 8, 12);
        }

        private void Log(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{time}] {message}\r\n");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
            Application.DoEvents();
        }

        private void SetProgress(int percent, string status)
        {
            progressBar.Value = Math.Min(100, Math.Max(0, percent));
            lblProgress.Text = $"[{percent}%] {status}";
            Application.DoEvents();
        }

        private async Task<string> GetDirectDownloadUrl(string yandexPublicLink)
        {
            SetProgress(10, "Получение прямой ссылки...");
            string apiUrl = $"https://cloud-api.yandex.net/v1/disk/public/resources/download?public_key={Uri.EscapeDataString(yandexPublicLink)}";
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync(apiUrl);
                int start = response.IndexOf("\"href\":\"") + 8;
                int end = response.IndexOf("\"", start);
                if (start < 8 || end <= start)
                    throw new Exception("Не удалось получить прямую ссылку");
                SetProgress(20, "Ссылка получена");
                return response.Substring(start, end - start).Replace("\\/", "/");
            }
        }

        private async void BtnInstall_Click(object sender, EventArgs e) => await RunProcess("https://disk.yandex.ru/d/0NzPGAiDrStk9g", true);
        private async void BtnUninstall_Click(object sender, EventArgs e) => await RunProcess("https://disk.yandex.ru/d/6WKaLoomf4blrA", false);

        private async Task RunProcess(string yandexLink, bool isInstall)
        {
            string action = isInstall ? "УСТАНОВКА РУСИФИКАТОР" : "УДАЛЕНИЕ РУСИФИКАТОРА";
            string successMsg = isInstall ? "Русификатор успешно установлен!" : "Оригинальная версия восстановлена!";

            string gameFolder = txtGamePath.Text.Trim();
            if (string.IsNullOrWhiteSpace(gameFolder) || !Directory.Exists(gameFolder))
            {
                MessageBox.Show("Укажите корректную папку с игрой!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string dataFolder = Path.Combine(gameFolder, "CyberManhunt_Data");
            if (!Directory.Exists(dataFolder))
            {
                MessageBox.Show("Папка 'CyberManhunt_Data' не найдена!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string targetFile = Path.Combine(dataFolder, "sharedassets0.assets");

            var btns = new[] { btnInstall, btnUninstall, btnBrowse };
            foreach (var btn in btns) btn.Enabled = false;
            progressBar.Visible = true;
            lblProgress.Visible = true;
            SetProgress(0, "Начало...");

            try
            {
                SetProgress(5, "Проверка целостности...");
                await Task.Delay(200);

                if (File.Exists(targetFile))
                {
                    SetProgress(15, "Удаление старого файла...");
                    File.Delete(targetFile);
                    Log(">>> СТАРЫЙ ФАЙЛ УДАЛЁН");
                }

                SetProgress(20, "Запрос к Яндекс.Диску...");
                string directUrl = await GetDirectDownloadUrl(yandexLink);

                SetProgress(30, "Скачивание файла...");
                byte[] fileBytes;
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(directUrl, HttpCompletionOption.ResponseHeadersRead);
                    var total = response.Content.Headers.ContentLength ?? -1L;
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var memoryStream = new MemoryStream();
                        var buffer = new byte[8192];
                        int bytesRead;
                        long totalRead = 0;

                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await memoryStream.WriteAsync(buffer, 0, bytesRead);
                            totalRead += bytesRead;
                            if (total > 0)
                            {
                                int progress = 30 + (int)(50 * (totalRead / (double)total));
                                SetProgress(progress, $"Скачивание... {totalRead / 1024} KB");
                            }
                        }
                        fileBytes = memoryStream.ToArray();
                    }
                }

                SetProgress(85, "Сохранение в папку игры...");
                await File.WriteAllBytesAsync(targetFile, fileBytes);
                SetProgress(100, "Завершение...");

                Log($">>> {successMsg}");
                MessageBox.Show($"✅ {successMsg}\n\nЗапустите игру.", "ГОТОВО", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                SetProgress(0, "Ошибка");
                Log($"!!! ОШИБКА: {ex.Message}");
                MessageBox.Show($"Ошибка:\n{ex.Message}", "СБОЙ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                foreach (var btn in btns) btn.Enabled = true;
                SetProgress(0, "ГОТОВ");
            }
        }

        private void BtnAbout_Click(object sender, EventArgs e)
        {
            var aboutForm = new Form
            {
                Text = "О СОЗДАТЕЛЕ",
                Size = new Size(500, 500),
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.FromArgb(10, 10, 15)
            };

            var pictureBox = new PictureBox
            {
                Location = new Point(20, 20),
                Size = new Size(200, 200),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };

            Task.Run(async () =>
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var imageBytes = await client.GetByteArrayAsync("https://i.postimg.cc/vBX3ZDkT/2025-10-05-233909.png");
                        using (var ms = new MemoryStream(imageBytes))
                        {
                            var img = Image.FromStream(ms);
                            pictureBox.Invoke((MethodInvoker)(() => pictureBox.Image = img));
                        }
                    }
                }
                catch { }
            });

            var lblText = new Label
            {
                Text = "Русификатор для Cyber Manhunt\nby Strahoduy",
                ForeColor = Color.White,
                Font = new Font("Consolas", 11, FontStyle.Bold),
                Location = new Point(240, 20),
                AutoSize = true
            };

            var linkGuide = new LinkLabel
            {
                Text = "📘 Руководство по установке",
                Location = new Point(240, 80),
                AutoSize = true,
                LinkColor = Color.Cyan
            };
            linkGuide.LinkClicked += (s, ev) => Process.Start(new ProcessStartInfo("https://steamcommunity.com/sharedfiles/filedetails/?id=3581340769") { UseShellExecute = true });

            var linkProfile = new LinkLabel
            {
                Text = "👤 Профиль в Steam",
                Location = new Point(240, 110),
                AutoSize = true,
                LinkColor = Color.Cyan
            };
            linkProfile.LinkClicked += (s, ev) => Process.Start(new ProcessStartInfo("https://steamcommunity.com/id/S000000/") { UseShellExecute = true });

            aboutForm.Controls.AddRange(new Control[] { pictureBox, lblText, linkGuide, linkProfile });
            aboutForm.Show();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F6)
            {
                if (DateTime.Now < _lockoutEnd)
                {
                    MessageBox.Show($"Доступ заблокирован до {Math.Ceiling((_lockoutEnd - DateTime.Now).TotalMinutes)} мин.", "Блокировка");
                    return true;
                }

                var input = new Form();
                var txt = new TextBox { PasswordChar = '*', Width = 200, Top = 40 };
                var btn = new Button { Text = "Войти", Top = 80, DialogResult = DialogResult.OK };
                input.Controls.AddRange(new Control[] {
                    new Label { Text = "Пароль владельца:", Location = new Point(10, 10) },
                    txt, btn
                });
                input.AcceptButton = btn;
                input.StartPosition = FormStartPosition.CenterParent;
                input.FormBorderStyle = FormBorderStyle.FixedDialog;

                if (input.ShowDialog(this) == DialogResult.OK)
                {
                    if (txt.Text == OWNER_PASSWORD)
                    {
                        _failedAttempts = 0;
                        new OwnerPanel().Show();
                    }
                    else
                    {
                        _failedAttempts++;
                        if (_failedAttempts >= 3)
                        {
                            _lockoutEnd = DateTime.Now.AddMinutes(3);
                            _failedAttempts = 0;
                            MessageBox.Show("Доступ заблокирован на 3 минуты!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show("Неверный пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void CheckForAppUpdateOnStartup()
        {
            try
            {
                var updates = UpdateManager.LoadUpdates();
                var current = UpdateManager.GetCurrentVersion();
                if (Version.TryParse(updates.PublishedVersion, out var published) &&
                    published > current)
                {
                    if (MessageBox.Show(
                        $"Доступна новая версия {updates.PublishedVersion}!\n\n{updates.GetPublishedUpdate()?.ReleaseNotes}\n\nОбновить сейчас?",
                        "Обновление", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        await UpdateManager.DownloadAndInstall(this, updates.GetPublishedUpdate().DownloadUrl);
                    }
                }
            }
            catch { /* игнор */ }
        }
    }
}