using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Diagnostics;

namespace CyberManhuntLocalizer
{
    public static class UpdateManager
    {
        private static readonly string UpdatesFile = Path.Combine(Application.StartupPath, "updates.json");

        public class UpdateInfo
        {
            public string Version { get; set; }
            public string ReleaseNotes { get; set; }
            public string DownloadUrl { get; set; }
            public bool Published { get; set; }
        }

        public class UpdatesData
        {
            public string PublishedVersion { get; set; } = "1.0.0";
            public System.Collections.Generic.List<UpdateInfo> Updates { get; set; } = new();
        }

        public static Version GetCurrentVersion()
        {
            return Version.Parse(Application.ProductVersion);
        }

        public static UpdatesData LoadUpdates()
        {
            if (!File.Exists(UpdatesFile))
            {
                var data = new UpdatesData();
                SaveUpdates(data);
                return data;
            }
            var json = File.ReadAllText(UpdatesFile);
            return JsonSerializer.Deserialize<UpdatesData>(json) ?? new UpdatesData();
        }

        public static void SaveUpdates(UpdatesData data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(UpdatesFile, json);
        }

        public static UpdateInfo GetPublishedUpdate(this UpdatesData data)
        {
            return data.Updates.Find(u => u.Version == data.PublishedVersion && u.Published);
        }

        public static async Task CheckForNewReleases()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("CyberManhuntLocalizer");
                var json = await client.GetStringAsync("https://api.github.com/repos/Sashaimanuilov/TranslateUnityGames/releases");
                var doc = JsonDocument.Parse(json);
                var array = doc.RootElement;

                if (array.GetArrayLength() == 0) return;

                var latest = array[0];
                string version = latest.GetProperty("tag_name").GetString().Trim('v');
                string notes = latest.GetProperty("body").GetString() ?? "Новое обновление";
                string url = "";

                if (latest.TryGetProperty("assets", out var assets) && assets.GetArrayLength() > 0)
                {
                    url = assets[0].GetProperty("browser_download_url").GetString();
                }

                var data = LoadUpdates();
                if (!data.Updates.Exists(u => u.Version == version))
                {
                    data.Updates.Insert(0, new UpdateInfo
                    {
                        Version = version,
                        ReleaseNotes = notes,
                        DownloadUrl = url,
                        Published = false
                    });
                    SaveUpdates(data);
                }
            }
            catch { /* ignore */ }
        }

        public static async Task DownloadAndInstall(Form owner, string downloadUrl)
        {
            try
            {
                var tempFile = Path.Combine(Path.GetTempPath(), "CyberManhuntLocalizer_new.exe");
                using (var client = new HttpClient())
                using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = File.Create(tempFile))
                {
                    var buffer = new byte[8192];
                    long total = response.Content.Headers.ContentLength ?? -1L;
                    long totalRead = 0;

                    while (await stream.ReadAsync(buffer, 0, buffer.Length) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, buffer.Length);
                        totalRead += buffer.Length;
                        if (total > 0)
                        {
                            int percent = (int)(100 * totalRead / total);
                            owner.Invoke((MethodInvoker)(() => owner.Text = $"Обновление... {percent}%"));
                        }
                    }
                }

                // Создаём bat-файл для замены
                string bat = $@"
@echo off
timeout /t 2 /nobreak >nul
del ""{Application.ExecutablePath}""
move ""{tempFile}"" ""{Application.ExecutablePath}""
start """" ""{Application.ExecutablePath}""
del ""%~f0""";
                string batFile = Path.Combine(Path.GetTempPath(), "update.bat");
                File.WriteAllText(batFile, bat);

                Process.Start(new ProcessStartInfo("cmd.exe", $"/c \"{batFile}\"") { UseShellExecute = true });
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}