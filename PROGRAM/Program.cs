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
        private static readonly List<string> blockedApps = new List<string>
        {
            "cmd",          // 명령 프롬프트
            "notepad",      // 메모장
            "mspaint",      // 그림판
            "SnippingTool", // 캡처 도구
            "chess",        // 체스 (Win7 게임)
            "freecell",     // 프리셀
            "hearts",       // 하트
            "mahjong",      // 마작
            "minesweeper",  // 지뢰찾기
            "purbleplace",  // 퍼플 플레이스
            "solitaire",    // 솔리테어
            "spider",        // 스파이더 솔리테어
        };

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
            // 허용된 프로세스 ID 저장
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
                            // 허용된 앱은 종료하지 않음
                            if (proc.ProcessName.Equals(allowedAppName, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }


                            // 설정 앱(SystemSettings.exe) 차단
                            if (proc.ProcessName.Equals("SystemSettings", StringComparison.OrdinalIgnoreCase))
                            {
                                proc.Kill();
                                ShowWarningMessage(proc.ProcessName); // 경고 메시지 출력
                                continue;
                            }

                            // 차단된 앱 처리
                            if (blockedApps.Contains(proc.ProcessName, StringComparer.OrdinalIgnoreCase))
                            {
                                
                                proc.Kill();
                                ShowWarningMessage(proc.ProcessName); // 경고 메시지 출력
                                Thread.Sleep(400); // 1초 대기
                            }
                        }
                        catch
                        {
                            continue; // 접근할 수 없는 프로세스 무시
                        }
                    }
                }
                Thread.Sleep(800); // 모니터링 간격 (2초)
            }
        }




        private static void ShowWarningMessage(string processName)
        {
            MessageBox.Show($"'{processName}' 실행이 차단되었습니다.\n이거 더 하면 니 얼굴 짱뚱어!", "경고",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

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

        private static void ToggleMonitoring(object sender, EventArgs e)
        {
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
