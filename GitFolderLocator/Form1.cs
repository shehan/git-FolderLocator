using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace GitFolderLocator
{
    public partial class Form1 : Form
    {
        String rootDirectory;
        StreamWriter outpuFile;

        public Form1()
        {
            InitializeComponent();
        }

        private void DoWork()
        {
            UpdateWorkStatusLog($"Started!");
            outpuFile.WriteLine($"Project,DirectoryPath,SourceURL,DefaultBranch");
            outpuFile.Flush();
            Regex regex_url = new Regex(@"url(.*)");
            Regex regex_branch = new Regex(@"\[branch(.*)");

            var directories = Directory.GetDirectories(rootDirectory);
            using (outpuFile)
            {
                foreach (var dir in directories)
                {
                    var directoyList = Directory.EnumerateDirectories(dir, ".git", SearchOption.AllDirectories);

                    foreach (var item in directoyList)
                    {                        
                        string sourceURL = string.Empty;
                        string branch = string.Empty;
                        using (StreamReader sr= new StreamReader($"{item}\\config"))
                        {
                            while (!sr.EndOfStream)
                            {
                                var text = sr.ReadLine();
                                if (regex_url.IsMatch(text))
                                {
                                    sourceURL = text.Substring(6);
                                    continue;
                                }

                                if (regex_branch.IsMatch(text))
                                {
                                    branch = text.Substring(9);
                                    branch = branch.Substring(0, branch.Length - 2);
                                    continue;
                                }
                            }
                        }

                        UpdateWorkStatusLog($"Processing: {item}");
                        outpuFile.WriteLine($"\"{Path.GetFileName(dir)}\",\"{item}\",\"{sourceURL}\",\"{branch}\"");
                        outpuFile.Flush();
                    }

                    if (directoyList.Count() == 0)
                    {
                        outpuFile.WriteLine($"\"{Path.GetFileName(dir)}\",\"{string.Empty}\",\"{string.Empty}\",\"{string.Empty}\"");
                        outpuFile.Flush();
                    }
                }
            }

            outpuFile.Close();
            UpdateWorkStatusLog($"Completed!");
            MessageBox.Show("Done!");
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBoxPath.Text))
            {
                MessageBox.Show("Direcorty does not exist!");
                return;
            }
            rootDirectory = textBoxPath.Text;
            var outpuFileName = $"Output_{DateTime.Now.Ticks}.csv";
            outpuFile = File.CreateText(outpuFileName);
            UpdateWorkStatusLog($"Output File: {new FileInfo(outpuFileName).FullName}");

            textBoxPath.Enabled = false;
            buttonStart.Enabled = false;
            Thread t = new Thread(DoWork);
            t.Start();
        }

        private void textBoxPath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(sender, new EventArgs());
            }
        }

        private void UpdateWorkStatusLog(string text)
        {
            if (this.textBoxLog.InvokeRequired)
            {
                UpdateLogCallback callback = new UpdateLogCallback(UpdateWorkStatusLog);
                this.Invoke(callback, new object[] { text });
            }
            else
            {
                textBoxLog.Focus();
                textBoxLog.AppendText($"{Environment.NewLine}{DateTime.Now.ToString()} - {text}");
            }
        }

        delegate void UpdateLogCallback(string text);
    }
}
