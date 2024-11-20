using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;

namespace AppRestrictor
{
    static class Program
    {
        private static readonly string allowedAppPath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
        private static readonly string allowedAppName = "chrome.exe";
        private static readonly string password = "haris1021";
        private static List<int> allowedProcesses = new List<int>();
        private static NotifyIcon notifyIcon;
        private static bool isMonitoring = true;

        [STAThread]
        static void Main()
        {
            notifyIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "리틀 앱 제한"
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("활성화/비활성화", null, ToggleMonitoring);
            contextMenu.Items.Add("종료", null, ExitApplication);
            notifyIcon.ContextMenuStrip = contextMenu;

            StartAllowedApp();

            Thread monitorThread = new Thread(new ThreadStart(MonitorProcesses));
            monitorThread.IsBackground = true;
            monitorThread.Start();

            Application.Run();
        }

        private static void StartAllowedApp()
        {
            Process chrome = new Process();
            chrome.StartInfo.FileName = allowedAppPath;
            chrome.Start();
            allowedProcesses.Add(chrome.Id);
        }

        private static void MonitorProcesses()
        {
            while (true)
            {
                if (isMonitoring)
                {
                    Process[] processes = Process.GetProcesses();
                    foreach (Process proc in processes)
                    {
                        try
                        {
                            // 크롬 프로세스는 종료하지 않음
                            if (proc.ProcessName.Equals(allowedAppName, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            // 시스템 프로세스인지 확인하는 로직
                            if (IsSystemProcess(proc))
                            {
                                continue; // 시스템 프로세스는 종료하지 않음
                            }

                            // 비밀번호 체크 없이 프로세스를 즉시 종료
                            proc.Kill();

                            // 1초 대기
                            Thread.Sleep(1000); // 1초 (1000 밀리초) 대기
                        }
                        catch
                        {
                            continue; // 예외 발생 시 무시하고 다음 프로세스 검사
                        }
                    }
                }
                Thread.Sleep(2000); // 모니터링 간격 (2초)
            }
        }

        // 시스템 프로세스인지 확인하는 메서드
        private static bool IsSystemProcess(Process proc)
        {
            try
            {
                return proc.StartInfo.UserName == "SYSTEM"; // SYSTEM 계정으로 실행 중인 프로세스만 유지
            }
            catch
            {
                return false; // 접근할 수 없는 프로세스는 무시
            }
        }


        // 비밀번호 체크 메서드
        private static bool CheckPassword()
        {
            bool isAuthenticated = false;
            DialogResult result = MessageBox.Show("애플리케이션을 실행하려면 비밀번호를 입력하세요:", "비밀번호 요청",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                string inputPassword = Prompt.ShowDialog("비밀번호:", "비밀번호 입력");
                if (inputPassword == password)
                {
                    isAuthenticated = true;
                }
            }
            return isAuthenticated;
        }

        // 모니터링 상태 토글 메서드
        private static void ToggleMonitoring(object sender, EventArgs e)
        {
            // 비밀번호 체크
            if (CheckPassword())
            {
                isMonitoring = !isMonitoring; // 모니터링 상태를 반전
                string status = isMonitoring ? "활성화" : "비활성화";
                MessageBox.Show($"모니터링이 {status}되었습니다.", "상태 변경", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("비밀번호가 틀렸습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private static void ExitApplication(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            string input = "";
            Form prompt = new Form()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = text, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 240, UseSystemPasswordChar = true };
            Button confirmation = new Button() { Text = "확인", Left = 180, Width = 80, Top = 80, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                input = textBox.Text;
            }
            return input;
        }
    }
}
