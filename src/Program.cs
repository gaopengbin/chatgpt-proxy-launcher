using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatGPTProxyLauncherLite
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LauncherForm());
        }
    }

    internal sealed class LauncherForm : Form
    {
        private readonly TextBox hostBox = new TextBox { Text = "127.0.0.1" };
        private readonly NumericUpDown portBox = new NumericUpDown { Minimum = 1, Maximum = 65535, Value = 10808 };
        private readonly Button launchButton = new Button { Text = "启动 ChatGPT", Height = 42 };
        private readonly Label statusLabel = new Label { AutoSize = true, Text = "准备就绪" };

        public LauncherForm()
        {
            Text = "ChatGPT Proxy Launcher";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(440, 246);
            MinimumSize = new Size(456, 285);
            MaximizeBox = false;
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.FromArgb(250, 250, 250);
            ForeColor = Color.FromArgb(23, 23, 23);

            var title = new Label { Text = "通过代理启动 ChatGPT", Font = new Font("Segoe UI Semibold", 18F), AutoSize = true, Margin = new Padding(0, 0, 0, 4) };
            var description = new Label { Text = "代理仅应用于本次启动的 ChatGPT 及其子进程。", ForeColor = Color.FromArgb(82, 82, 82), AutoSize = true, Margin = new Padding(0, 0, 0, 18) };
            var fields = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, RowCount = 2, Dock = DockStyle.Top, Margin = new Padding(0) };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            fields.Controls.Add(new Label { Text = "代理主机", AutoSize = true }, 0, 0);
            fields.Controls.Add(new Label { Text = "端口", AutoSize = true }, 1, 0);
            hostBox.Dock = DockStyle.Fill;
            hostBox.Margin = new Padding(0, 6, 10, 0);
            portBox.Dock = DockStyle.Fill;
            portBox.Margin = new Padding(0, 6, 0, 0);
            fields.Controls.Add(hostBox, 0, 1);
            fields.Controls.Add(portBox, 1, 1);

            launchButton.Dock = DockStyle.Top;
            launchButton.Margin = new Padding(0, 20, 0, 12);
            launchButton.BackColor = Color.FromArgb(23, 23, 23);
            launchButton.ForeColor = Color.White;
            launchButton.FlatStyle = FlatStyle.Flat;
            launchButton.FlatAppearance.BorderSize = 0;
            launchButton.Cursor = Cursors.Hand;
            launchButton.Click += LaunchButtonClick;
            statusLabel.ForeColor = Color.FromArgb(82, 82, 82);

            var content = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, Dock = DockStyle.Fill, Padding = new Padding(28, 24, 28, 20) };
            content.Controls.Add(title);
            content.Controls.Add(description);
            content.Controls.Add(fields);
            content.Controls.Add(launchButton);
            content.Controls.Add(statusLabel);
            content.SizeChanged += delegate
            {
                int width = content.ClientSize.Width - content.Padding.Horizontal;
                description.MaximumSize = new Size(width, 0);
                statusLabel.MaximumSize = new Size(width, 0);
                fields.Width = width;
                launchButton.Width = width;
            };
            Controls.Add(content);
            AcceptButton = launchButton;
        }

        private async void LaunchButtonClick(object sender, EventArgs e)
        {
            launchButton.Enabled = false;
            UseWaitCursor = true;
            try
            {
                string host = hostBox.Text.Trim();
                int port = (int)portBox.Value;
                if (host.Length == 0) throw new Exception("请输入代理主机。");
                SetStatus("正在检查代理…", false);

                bool reachable = await Task.Run(() => CanConnect(host, port));
                if (!reachable) throw new Exception(String.Format("无法连接代理 {0}:{1}。", host, port));
                if (IsDesktopAppRunning()) throw new Exception("ChatGPT 已在运行，请完全退出后再试。");

                SetStatus("正在查找 ChatGPT…", false);
                string executable = await Task.Run(() => FindDesktopExecutable());
                if (executable == null) throw new Exception("未找到 ChatGPT/Codex Windows 桌面应用。");

                string proxyUrl = String.Format("http://{0}:{1}", host, port);
                var info = new ProcessStartInfo(executable) { UseShellExecute = false };
                foreach (string key in new[] { "HTTP_PROXY", "HTTPS_PROXY", "http_proxy", "https_proxy" }) info.EnvironmentVariables[key] = proxyUrl;
                info.EnvironmentVariables["NO_PROXY"] = "localhost,127.0.0.1,::1";
                info.EnvironmentVariables["no_proxy"] = "localhost,127.0.0.1,::1";
                Process.Start(info);
                SetStatus("已通过 " + proxyUrl + " 启动 ChatGPT", true);
            }
            catch (Exception ex) { SetStatus(ex.Message, false); }
            finally { UseWaitCursor = false; launchButton.Enabled = true; }
        }

        private void SetStatus(string text, bool success)
        {
            statusLabel.Text = text;
            statusLabel.ForeColor = success ? Color.FromArgb(21, 128, 61) : Color.FromArgb(185, 28, 28);
        }

        private static bool CanConnect(string host, int port)
        {
            using (var client = new TcpClient())
            {
                try
                {
                    var result = client.BeginConnect(host, port, null, null);
                    if (!result.AsyncWaitHandle.WaitOne(1800)) return false;
                    client.EndConnect(result);
                    return client.Connected;
                }
                catch { return false; }
            }
        }

        private static bool IsDesktopAppRunning()
        {
            return Process.GetProcessesByName("ChatGPT").Any(p => IsAppExecutable(p, "ChatGPT.exe")) ||
                   Process.GetProcessesByName("Codex").Any(p => IsAppExecutable(p, "Codex.exe"));
        }

        private static bool IsAppExecutable(Process process, string fileName)
        {
            try { return process.MainModule.FileName.EndsWith("\\app\\" + fileName, StringComparison.OrdinalIgnoreCase); }
            catch { return false; }
            finally { process.Dispose(); }
        }

        private static string FindDesktopExecutable()
        {
            const string command = "$p=@(Get-AppxPackage -Name 'OpenAI.Codex';Get-AppxPackage -Name 'OpenAI.ChatGPT-Desktop')|Sort-Object Version -Descending|Select-Object -First 1;if($p){$p.InstallLocation}";
            var info = new ProcessStartInfo("powershell.exe", "-NoLogo -NoProfile -NonInteractive -Command \"" + command + "\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using (var process = Process.Start(info))
            {
                string location = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                if (process.ExitCode != 0 || location.Length == 0) return null;
                foreach (string name in new[] { "ChatGPT.exe", "Codex.exe" })
                {
                    string path = Path.Combine(location, "app", name);
                    if (File.Exists(path)) return path;
                }
            }
            return null;
        }
    }
}
