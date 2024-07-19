using CmlLib.Core;
using CmlLib.Core.Auth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace PL
{
    public partial class Main : Form
    {
        public MinecraftPath MinecraftPath = new MinecraftPath();
        public CMLauncher Launcher;
        private StringBuilder LogMessage = new StringBuilder();
        private const int MaxLines = 500;
        public List<Process> MinecraftInstances = new List<Process>();

        private readonly object LogLock = new object();
        private readonly object MinecraftInstancesLock = new object();

        private bool Dragging = false;
        private Point DragCursorPoint;
        private Point DragFormPoint;

        public Main()
        {
            InitializeComponent();
        }

        public void InitFolders()
        {
            if (Directory.Exists(MinecraftPath.WindowsDefaultPath))
            {
                if(!File.Exists(Path.Combine(MinecraftPath.WindowsDefaultPath, "launcher_profiles.json")))
                {
                    File.WriteAllText(Path.Combine(MinecraftPath.WindowsDefaultPath, "launcher_profiles.json"), "{\\r\\n  \\\"profiles\\\" : {\\r\\n    \\\"ca488ed6927db2ca2ab38bd4621687c9\\\" : {\\r\\n      \\\"created\\\" : \\\"1970-01-02T00:00:00.000Z\\\",\\r\\n      \\\"icon\\\" : \\\"Grass\\\",\\r\\n      \\\"lastUsed\\\" : \\\"1970-01-02T00:00:00.000Z\\\",\\r\\n      \\\"lastVersionId\\\" : \\\"latest-release\\\",\\r\\n      \\\"name\\\" : \\\"\\\",\\r\\n      \\\"type\\\" : \\\"latest-release\\\"\\r\\n    },\\r\\n    \\\"f7256080e37be359d103a44fea00a23d\\\" : {\\r\\n      \\\"created\\\" : \\\"1970-01-01T00:00:00.000Z\\\",\\r\\n      \\\"icon\\\" : \\\"Crafting_Table\\\",\\r\\n      \\\"lastUsed\\\" : \\\"1970-01-01T00:00:00.000Z\\\",\\r\\n      \\\"lastVersionId\\\" : \\\"latest-snapshot\\\",\\r\\n      \\\"name\\\" : \\\"\\\",\\r\\n      \\\"type\\\" : \\\"latest-snapshot\\\"\\r\\n    }\\r\\n  },\\r\\n  \\\"settings\\\" : {\\r\\n    \\\"crashAssistance\\\" : true,\\r\\n    \\\"enableAdvanced\\\" : false,\\r\\n    \\\"enableAnalytics\\\" : true,\\r\\n    \\\"enableHistorical\\\" : false,\\r\\n    \\\"enableReleases\\\" : true,\\r\\n    \\\"enableSnapshots\\\" : false,\\r\\n    \\\"keepLauncherOpen\\\" : false,\\r\\n    \\\"profileSorting\\\" : \\\"ByLastPlayed\\\",\\r\\n    \\\"showGameLog\\\" : false,\\r\\n    \\\"showMenu\\\" : false,\\r\\n    \\\"soundOn\\\" : false\\r\\n  },\\r\\n  \\\"version\\\" : 3\\r\\n}");
                }
                else
                {
                    try
                    {
                        JsonConvert.DeserializeObject<LauncherProfiles>(File.ReadAllText(Path.Combine(MinecraftPath.WindowsDefaultPath, "launcher_profiles.json")));
                    }
                    catch 
                    {
                        File.Delete(Path.Combine(MinecraftPath.WindowsDefaultPath, "launcher_profiles.json"));
                        File.WriteAllText(Path.Combine(MinecraftPath.WindowsDefaultPath, "launcher_profiles.json"), "{\\r\\n  \\\"profiles\\\" : {\\r\\n    \\\"ca488ed6927db2ca2ab38bd4621687c9\\\" : {\\r\\n      \\\"created\\\" : \\\"1970-01-02T00:00:00.000Z\\\",\\r\\n      \\\"icon\\\" : \\\"Grass\\\",\\r\\n      \\\"lastUsed\\\" : \\\"1970-01-02T00:00:00.000Z\\\",\\r\\n      \\\"lastVersionId\\\" : \\\"latest-release\\\",\\r\\n      \\\"name\\\" : \\\"\\\",\\r\\n      \\\"type\\\" : \\\"latest-release\\\"\\r\\n    },\\r\\n    \\\"f7256080e37be359d103a44fea00a23d\\\" : {\\r\\n      \\\"created\\\" : \\\"1970-01-01T00:00:00.000Z\\\",\\r\\n      \\\"icon\\\" : \\\"Crafting_Table\\\",\\r\\n      \\\"lastUsed\\\" : \\\"1970-01-01T00:00:00.000Z\\\",\\r\\n      \\\"lastVersionId\\\" : \\\"latest-snapshot\\\",\\r\\n      \\\"name\\\" : \\\"\\\",\\r\\n      \\\"type\\\" : \\\"latest-snapshot\\\"\\r\\n    }\\r\\n  },\\r\\n  \\\"settings\\\" : {\\r\\n    \\\"crashAssistance\\\" : true,\\r\\n    \\\"enableAdvanced\\\" : false,\\r\\n    \\\"enableAnalytics\\\" : true,\\r\\n    \\\"enableHistorical\\\" : false,\\r\\n    \\\"enableReleases\\\" : true,\\r\\n    \\\"enableSnapshots\\\" : false,\\r\\n    \\\"keepLauncherOpen\\\" : false,\\r\\n    \\\"profileSorting\\\" : \\\"ByLastPlayed\\\",\\r\\n    \\\"showGameLog\\\" : false,\\r\\n    \\\"showMenu\\\" : false,\\r\\n    \\\"soundOn\\\" : false\\r\\n  },\\r\\n  \\\"version\\\" : 3\\r\\n}");
                    }
                }
            }
        }

        private void Start(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            InitFolders();

            new Thread(() =>
            {
                System.Net.ServicePointManager.DefaultConnectionLimit = 256;

                Launcher = new CMLauncher(MinecraftPath);

                Launcher.FileChanged += (e_) =>
                {
                    Console.WriteLine("FileKind: " + e_.FileKind.ToString());
                    Console.WriteLine("FileName: " + e_.FileName);
                    Console.WriteLine("ProgressedFileCount: " + e_.ProgressedFileCount);
                    Console.WriteLine("TotalFileCount: " + e_.TotalFileCount);
                };

                Launcher.ProgressChanged += (s, e_) =>
                {
                    progressBar1.Value = e_.ProgressPercentage;
                    Console.WriteLine("{0}%", e_.ProgressPercentage);
                };

                foreach (var v in Launcher.GetAllVersions().ToList())
                {
                    comboBox1.Items.Add(v.Name);
                }

                LoadConfig();
            }).Start();
        }

        private void Play(object sender, EventArgs e)
        {
            if (!ValidName(textBox1.Text.Replace(" ", "")))
                return;

            lock (MinecraftInstancesLock)
            {
                if (IsMoreThanOneMinecraftInstance())
                {
                    MessageBoxManager.Yes = "Run Anyways";
                    MessageBoxManager.No = "Cancel";
                    MessageBoxManager.Register();

                    DialogResult result = MessageBox.Show("There is already an instance of the game running, want to proceed anyways?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    MessageBoxManager.Unregister(); 

                    if (result != DialogResult.Yes)
                    {
                        return;
                    }
                }

                new Thread(() =>
                {
                    Process process_ = null;

                    MinecraftInstances.Add(process_);

                    var process = Launcher.CreateProcess(comboBox1.Text, new MLaunchOption
                    {
                        MaximumRamMb = 2048,
                        Session = MSession.CreateOfflineSession(textBox1.Text.Replace(" ", "")),
                    });

                    if (process.StartInfo == null)
                    {
                        process.StartInfo = new ProcessStartInfo
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        };
                    }
                    else
                    {
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                    }

                    process.EnableRaisingEvents = true;

                    process.Exited += (sender_, e_) =>
                    {
                        MinecraftInstances.Remove(process);
                    };

                    process.OutputDataReceived += (sender_, line) =>
                    {
                        if (!string.IsNullOrEmpty(line.Data))
                        {
                            string log = GetLog(line.Data);
                            UpdateRichTextBox(log);
                        }
                    };

                    process.Start();

                    MinecraftInstances.Remove(process_);
                    MinecraftInstances.Add(process);

                    process.BeginOutputReadLine();
                }).Start();
            }
        }

        public bool ValidName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Invalid Username", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        public void LoadConfig()
        {
            Config config = null;
            string PoliLauncherFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Poli Launcher");
            string PoliLauncherConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Poli Launcher", "Config.json");

            if (File.Exists(PoliLauncherConfigFilePath))
            {
                try
                {
                    string json = File.ReadAllText(PoliLauncherConfigFilePath);

                    config = JsonConvert.DeserializeObject<Config>(json);
                }
                catch
                {
                    config = new Config { Username = "", Version = 0 };

                    File.WriteAllText(PoliLauncherConfigFilePath, JsonConvert.SerializeObject(config));
                }
            }
            else
            {
                if (!Directory.Exists(PoliLauncherFolderPath))
                {
                    Directory.CreateDirectory(PoliLauncherFolderPath);
                }

                config = new Config { Username = "", Version = 0 };

                File.WriteAllText(PoliLauncherConfigFilePath, JsonConvert.SerializeObject(config));
            }

            if (config != null)
            {
                comboBox1.SelectedIndex = config.Version;
                textBox1.Text = config.Username;
            }
        }

        public void SaveConfig()
        {
            Config config = null;
            string PoliLauncherFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Poli Launcher");
            string PoliLauncherConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Poli Launcher", "Config.json");

            if (!Directory.Exists(PoliLauncherFolderPath))
            {
                Directory.CreateDirectory(PoliLauncherFolderPath);
            }

            config = new Config { Username = textBox1.Text, Version = comboBox1.SelectedIndex };

            File.WriteAllText(PoliLauncherConfigFilePath, JsonConvert.SerializeObject(config));
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            SaveConfig();
            Environment.Exit(Environment.ExitCode);
        }

        private void Update(object sender, EventArgs e)
        {

        }

        public bool IsMoreThanOneMinecraftInstance()
        {
            lock (MinecraftInstancesLock)
            {
                return MinecraftInstances.Count > 1;
            }
        }

        public string GetLog(string log)
        {
            lock (LogLock)
            {
                if (log.Contains("log4j:Event") || log.Contains("log4j:Message"))
                {
                    try
                    {
                        XmlDocument xml = new XmlDocument();

                        using (XmlReader xr = new XmlTextReader(new StringReader(log)) { Namespaces = false })
                        {
                            xml.Load(xr);
                        }

                        XmlNodeList messageList = xml.GetElementsByTagName("log4j:Message");
                        string message = $"[{DateTime.Now:HH:mm:ss tt}]  {messageList[0].InnerText}\n".Replace("[[", "["); ;

                        LogMessage.AppendLine(message);

                        TrimExcessLines();

                        return message;
                    }
                    catch
                    {
                        LogMessage.AppendLine(log);
                        TrimExcessLines();
                    }
                }
                else
                {
                    string message = $"[{DateTime.Now.ToString("HH:mm:ss tt")}] {log}\n".Replace("[[", "[");

                    LogMessage.AppendLine(message);
                    TrimExcessLines();
                    return message;
                }

                return string.Empty;
            }
        }

        private void TrimExcessLines()
        {
            string[] lines = LogMessage.ToString().Split('\n');
            if (lines.Length > MaxLines)
            {
                LogMessage = new StringBuilder(string.Join("\n", lines.Skip(lines.Length - MaxLines)));
            }
        }

        public void UpdateRichTextBox(string text)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    richTextBox1.AppendText(text);
                });
            }
            else
            {
                richTextBox1.AppendText(text);
            }
        }

        private void OnClickClose(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnClickMinimize(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void OnSizeChange(object sender, EventArgs e)
        {

        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            Dragging = true;
            DragCursorPoint = Cursor.Position;
            DragFormPoint = Location;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(DragCursorPoint));
                Location = Point.Add(DragFormPoint, new Size(dif));
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            Dragging = false;
        }
    }
}
