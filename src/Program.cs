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
        private readonly Button launchButton = new Button { Text = "通过代理启动 ChatGPT", Height = 44 };
        private readonly Label statusLabel = new Label { AutoSize = false, Text = "准备就绪", TextAlign = ContentAlignment.MiddleLeft };
        private readonly Label statusMark = new Label { AutoSize = false, Text = "●", TextAlign = ContentAlignment.MiddleCenter };
        private readonly Panel statusPanel = new Panel();

        public LauncherForm()
        {
            Text = "ChatGPT Proxy Launcher";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(480, 372);
            MinimumSize = new Size(496, 411);
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.FromArgb(244, 246, 248);
            ForeColor = Color.FromArgb(23, 23, 23);

            var header = new Panel { Dock = DockStyle.Top, Height = 118, BackColor = Color.FromArgb(23, 23, 23) };
            var badge = new Label
            {
                Text = "LOCAL PROXY",
                Font = new Font("Segoe UI Semibold", 8F),
                ForeColor = Color.FromArgb(212, 175, 55),
                Location = new Point(28, 19),
                AutoSize = true
            };
            var title = new Label
            {
                Text = "ChatGPT Proxy Launcher",
                Font = new Font("Segoe UI Semibold", 20F),
                ForeColor = Color.White,
                Location = new Point(26, 38),
                AutoSize = true
            };
            var description = new Label
            {
                Text = "无需开启全局代理，只代理本次启动的 ChatGPT / Codex",
                ForeColor = Color.FromArgb(190, 190, 190),
                Location = new Point(29, 82),
                AutoSize = true
            };
            header.Controls.Add(badge);
            header.Controls.Add(title);
            header.Controls.Add(description);

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(28, 24, 28, 22) };
            var fields = new TableLayoutPanel { ColumnCount = 2, RowCount = 2, Location = new Point(28, 24), Size = new Size(424, 62) };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            fields.Controls.Add(new Label { Text = "代理主机", AutoSize = true, ForeColor = Color.FromArgb(64, 64, 64) }, 0, 0);
            fields.Controls.Add(new Label { Text = "HTTP / Mixed 端口", AutoSize = true, ForeColor = Color.FromArgb(64, 64, 64) }, 1, 0);
            hostBox.Dock = DockStyle.Fill;
            hostBox.Margin = new Padding(0, 6, 10, 0);
            portBox.Dock = DockStyle.Fill;
            portBox.Margin = new Padding(0, 6, 0, 0);
            fields.Controls.Add(hostBox, 0, 1);
            fields.Controls.Add(portBox, 1, 1);

            launchButton.Location = new Point(28, 108);
            launchButton.Size = new Size(424, 44);
            launchButton.BackColor = Color.FromArgb(23, 23, 23);
            launchButton.ForeColor = Color.White;
            launchButton.FlatStyle = FlatStyle.Flat;
            launchButton.FlatAppearance.BorderSize = 0;
            launchButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 55, 55);
            launchButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 0, 0);
            launchButton.Cursor = Cursors.Hand;
            launchButton.Click += LaunchButtonClick;

            statusPanel.Location = new Point(28, 166);
            statusPanel.Size = new Size(424, 42);
            statusPanel.BackColor = Color.White;
            statusPanel.BorderStyle = BorderStyle.FixedSingle;
            statusMark.Location = new Point(8, 7);
            statusMark.Size = new Size(24, 26);
            statusMark.ForeColor = Color.FromArgb(100, 116, 139);
            statusLabel.Location = new Point(35, 6);
            statusLabel.Size = new Size(378, 28);
            statusLabel.ForeColor = Color.FromArgb(71, 85, 105);
            statusPanel.Controls.Add(statusMark);
            statusPanel.Controls.Add(statusLabel);

            var footnote = new Label
            {
                Text = "仅设置子进程代理环境，不修改 Windows 系统代理。",
                ForeColor = Color.FromArgb(100, 116, 139),
                Font = new Font("Segoe UI", 9F),
                Location = new Point(29, 222),
                AutoSize = true
            };
            body.Controls.Add(fields);
            body.Controls.Add(launchButton);
            body.Controls.Add(statusPanel);
            body.Controls.Add(footnote);
            Controls.Add(body);
            Controls.Add(header);
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
            statusMark.ForeColor = statusLabel.ForeColor;
            statusPanel.BackColor = success ? Color.FromArgb(240, 253, 244) : Color.FromArgb(254, 242, 242);
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
