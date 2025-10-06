using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace CyberManhuntLocalizer
{
    public class OwnerPanel : Form
    {
        private ListView lvUpdates;
        private TextBox txtNotes;
        private TextBox txtVersion;
        private TextBox txtUrl;
        private Button btnPublish;
        private Button btnCancel;
        private Button btnCheckGithub;

        public OwnerPanel()
        {
            InitializeComponent();
            LoadUpdates();
        }

        private void InitializeComponent()
        {
            this.Text = "ПАНЕЛЬ ВЛАДЕЛЬЦА // Strahoduy";
            this.Size = new Size(800, 600);
            this.BackColor = Color.FromArgb(10, 10, 15);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lbl = new Label
            {
                Text = "УПРАВЛЕНИЕ ОБНОВЛЕНИЯМИ",
                ForeColor = Color.MediumPurple,
                Font = new Font("Consolas", 12, FontStyle.Bold),
                Location = new Point(20, 10),
                AutoSize = true
            };

            lvUpdates = new ListView
            {
                Location = new Point(20, 50),
                Size = new Size(400, 400),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(20, 20, 25)
            };
            lvUpdates.Columns.Add("Версия", 100);
            lvUpdates.Columns.Add("Статус", 100);
            lvUpdates.SelectedIndexChanged += (s, e) => OnUpdateSelected();

            btnCheckGithub = new Button
            {
                Text = "Проверить GitHub",
                Location = new Point(20, 460),
                Width = 150,
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCheckGithub.FlatAppearance.BorderSize = 0;
            btnCheckGithub.Click += async (s, e) =>
            {
                await UpdateManager.CheckForNewReleases();
                LoadUpdates();
                MessageBox.Show("Проверка завершена!", "Готово");
            };

            // Правая панель
            var lblVer = new Label { Text = "Версия:", Location = new Point(440, 50), ForeColor = Color.White };
            txtVersion = new TextBox { Location = new Point(440, 70), Width = 200, BackColor = Color.FromArgb(30, 30, 35), ForeColor = Color.White };

            var lblNotes = new Label { Text = "Описание:", Location = new Point(440, 100), ForeColor = Color.White };
            txtNotes = new TextBox
            {
                Location = new Point(440, 120),
                Width = 320,
                Height = 200,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(30, 30, 35),
                ForeColor = Color.White
            };

            var lblUrl = new Label { Text = "URL загрузки:", Location = new Point(440, 330), ForeColor = Color.White };
            txtUrl = new TextBox { Location = new Point(440, 350), Width = 320, BackColor = Color.FromArgb(30, 30, 35), ForeColor = Color.White };

            btnPublish = new Button
            {
                Text = "ОПУБЛИКОВАТЬ",
                Location = new Point(440, 390),
                Width = 150,
                BackColor = Color.LimeGreen,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            btnPublish.FlatAppearance.BorderSize = 0;
            btnPublish.Click += PublishUpdate;

            btnCancel = new Button
            {
                Text = "ОТМЕНИТЬ",
                Location = new Point(610, 390),
                Width = 150,
                BackColor = Color.DarkRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += CancelUpdate;

            this.Controls.AddRange(new Control[] {
                lbl, lvUpdates, btnCheckGithub,
                lblVer, txtVersion, lblNotes, txtNotes, lblUrl, txtUrl,
                btnPublish, btnCancel
            });
        }

        private void LoadUpdates()
        {
            lvUpdates.Items.Clear();
            var updates = UpdateManager.LoadUpdates();
            foreach (var u in updates.Updates)
            {
                var item = new ListViewItem(u.Version);
                item.SubItems.Add(u.Published ? "Опубликовано" : "Черновик");
                item.Tag = u;
                lvUpdates.Items.Add(item);
            }
        }

        private void OnUpdateSelected()
        {
            if (lvUpdates.SelectedItems.Count == 0) return;
            var update = (UpdateManager.UpdateInfo)lvUpdates.SelectedItems[0].Tag;
            txtVersion.Text = update.Version;
            txtNotes.Text = update.ReleaseNotes;
            txtUrl.Text = update.DownloadUrl;
        }

        private void PublishUpdate(object sender, EventArgs e)
        {
            if (lvUpdates.SelectedItems.Count == 0) return;
            var update = (UpdateManager.UpdateInfo)lvUpdates.SelectedItems[0].Tag;
            update.Version = txtVersion.Text;
            update.ReleaseNotes = txtNotes.Text;
            update.DownloadUrl = txtUrl.Text;
            update.Published = true;

            var data = UpdateManager.LoadUpdates();
            data.PublishedVersion = update.Version;
            UpdateManager.SaveUpdates(data);

            LoadUpdates();
            MessageBox.Show("Обновление опубликовано!", "Готово");
        }

        private void CancelUpdate(object sender, EventArgs e)
        {
            if (lvUpdates.SelectedItems.Count == 0) return;
            var update = (UpdateManager.UpdateInfo)lvUpdates.SelectedItems[0].Tag;
            update.Published = false;
            UpdateManager.SaveUpdates(UpdateManager.LoadUpdates());
            LoadUpdates();
            MessageBox.Show("Публикация отменена.", "Готово");
        }
    }
}