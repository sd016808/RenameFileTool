using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 批次改檔名
{
    public partial class Form1 : Form
    {
        private Config _config = new Config();
        private static readonly string CONFIG_PATH = "config.json";
        public Form1()
        {
            InitializeComponent();
            if(File.Exists(CONFIG_PATH))
            {
                string config = File.ReadAllText(CONFIG_PATH);
                try
                {
                    _config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(config);
                    this.textBox1.Text = _config.LastFolderPath;
                    this.textBox2.Text = _config.LastName;
                    this.numericUpDown1.Value = _config.LastEpisode;
                }
                catch
                {
                    _config = new Config();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                this.textBox1.Text = dialog.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.textBox1.Text))
                    return;

                if (string.IsNullOrWhiteSpace(this.textBox2.Text))
                    return;

                var files = Directory.GetFiles(this.textBox1.Text);
                if (!files.Any())
                    return;

                int episodeCount = 1;
                string season = ((int)numericUpDown1.Value).ToString("D2");
                var sortFiles = files.CustomSort();
                foreach (var file in sortFiles)
                {
                    string extensionName = Path.GetExtension(file);
                    string oldPath = Path.GetDirectoryName(file);
                    string newName = Path.Combine(oldPath, $"{this.textBox2.Text}.s{season}e{episodeCount.ToString("D2")}{extensionName}");
                    System.IO.File.Move(file, newName);
                    episodeCount++;
                }
                MessageBox.Show($"成功轉換{files.Length}筆檔案");
            }
            catch
            {

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _config.LastFolderPath = this.textBox1.Text;
                _config.LastName = this.textBox2.Text;
                _config.LastEpisode = numericUpDown1.Value;
                string config = Newtonsoft.Json.JsonConvert.SerializeObject(_config);
                File.WriteAllText(CONFIG_PATH, config);
            }
            catch
            {

            }
        }
    }
    public class Config
    {
        public string LastFolderPath { get; set; }
        public string LastName { get; set; }
        public decimal LastEpisode { get; set; }
    }
    public static class MyExtensions
    {
        public static IEnumerable<string> CustomSort(this IEnumerable<string> list)
        {
            int maxLen = list.Select(s => s.Length).Max();

            return list.Select(s => new
            {
                OrgStr = s,
                SortStr = Regex.Replace(s, @"(\d+)|(\D+)", m => m.Value.PadLeft(maxLen, char.IsDigit(m.Value[0]) ? ' ' : '\xffff'))
            })
            .OrderBy(x => x.SortStr)
            .Select(x => x.OrgStr);
        }

    }
}
