using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace HashUtil {
    public partial class MainForm : Form {
        private List<FileInfo> FileList;
        private BackgroundWorker bw;
        private bool running;
        private long lastUpdate = 0;
        private bool firstUpdate = true;

        public MainForm() {
            InitializeComponent();
            FileList = new List<FileInfo>();
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = false;
            bw.DoWork += Bw_DoWork;
            bw.ProgressChanged += Bw_ProgressChanged;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            
            ColumnHeader h1 = new ColumnHeader();
            h1.Text = "Filename";

            ColumnHeader h2 = new ColumnHeader();
            h2.Text = "MD5";

            ColumnHeader h3 = new ColumnHeader();
            h3.Text = "CRC32";

            ColumnHeader h4 = new ColumnHeader();
            h4.Text = "SHA1";

            lvMain.Columns.Add(h1);
            lvMain.Columns.Add(h2);
            lvMain.Columns.Add(h3);
            lvMain.Columns.Add(h4);

            foreach (ColumnHeader header in lvMain.Columns)
                if (header.Width < 150)
                    header.Width = 150;

            running = false;
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e) {
            MD5CryptoServiceProvider sp = new MD5CryptoServiceProvider();
            Crc32 crc = new Crc32();
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();

            List<FileInfo> fileList = (List<FileInfo>)e.Argument;
            foreach(FileInfo file in fileList) {
                bool tooshort = false;
                FileStream fs = new FileStream(file.Name, FileMode.Open);
                if (fs.Length / 1024 / 1024 < 1)
                    tooshort = true;
                file.MD5 = "Hashing...";
                //Console.WriteLine("Length of stream: " + fs.Length);
                if (!tooshort)
                    bw.ReportProgress(0, file);
                
                string hex_hash = BitConverter.ToString(sp.ComputeHash(fs));
                file.MD5 = hex_hash;
                file.CRC32 = "Hashing...";
                if (!tooshort)
                    bw.ReportProgress(0, file);
                
                fs.Seek(0, SeekOrigin.Begin);
                string hex_crc = BitConverter.ToString(crc.ComputeHash(fs)).Replace("-", "");
                file.CRC32 = hex_crc;
                file.SHA1 = "Hashing...";
                if (!tooshort)
                    bw.ReportProgress(0, file);

                fs.Seek(0, SeekOrigin.Begin);
                string hex_sha = BitConverter.ToString(sha.ComputeHash(fs));
                file.SHA1 = hex_sha;
                bw.ReportProgress(0, file);
                fs.Close();
            }
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            FileInfo f = (FileInfo)e.UserState;
            int idx = 0;
            foreach(FileInfo file in FileList) {
                if (file.Name == f.Name)
                    break;
                idx++;
            }
            FileList.ElementAt(idx).MD5 = f.MD5;
            UpdateListView();
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            btnAdd.Enabled = true;
            btnClear.Enabled = true;
            btnExit.Enabled = true;
            firstUpdate = true;
            UpdateListView();
        }

        private void btnExit_Click(object sender, EventArgs e) {
            Close();
        }

        private void btnHash_Click(object sender, EventArgs e) {
            for(int i = 0; i < FileList.Count; i++) {
                FileList.ElementAt(i).MD5 = null;
                FileList.ElementAt(i).CRC32 = null;
            }
            running = true;
            UpdateListView();
            btnAdd.Enabled = false;
            btnClear.Enabled = false;
            btnExit.Enabled = false;
            bw.RunWorkerAsync(FileList);
        }

        private void btnAdd_Click(object sender, EventArgs e) {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Multiselect = true;
            fd.CheckFileExists = true;
            DialogResult res = fd.ShowDialog();
            if (res == DialogResult.OK) {
                foreach(string file in fd.FileNames)
                    FileList.Add(new FileInfo(file));
                firstUpdate = true;
                UpdateListView();
            }
        }

        private void btnClear_Click(object sender, EventArgs e) {
            firstUpdate = true;
            FileList.Clear();
            UpdateListView();
        }

        private void UpdateListView() {
            // TODO: make this milisecond based
            if (!firstUpdate && DateTime.Now.Ticks - lastUpdate < 10000000)
                return;

            firstUpdate = false;
            lastUpdate = DateTime.Now.Ticks;
            lvMain.Items.Clear();
            lvMain.BeginUpdate();
            bool even = true;
            foreach(FileInfo file in FileList) {
                ListViewItem i = new ListViewItem();
                i.Text = file.Name;

                if (file.MD5 == null)
                    if (running)
                        i.SubItems.Add("Waiting...");
                    else
                        i.SubItems.Add("");
                else
                    i.SubItems.Add(file.MD5);

                if (file.CRC32 == null)
                    if (running)
                        i.SubItems.Add("Waiting...");
                    else
                        i.SubItems.Add("");
                else
                    i.SubItems.Add(file.CRC32);

                if (file.SHA1 == null)
                    if (running)
                        i.SubItems.Add("Waiting...");
                    else
                        i.SubItems.Add("");
                else
                    i.SubItems.Add(file.SHA1);

                if (even)
                    i.BackColor = Color.LightBlue;
                even = !even;
                lvMain.Items.Add(i);
            }
            lvMain.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            foreach(ColumnHeader header in lvMain.Columns)
                if (header.Width < 150)
                    header.Width = 150;
            lvMain.EndUpdate();
            lblFileCount.Text = lvMain.Items.Count.ToString();
        }

        private void btnSave_Click(object sender, EventArgs e) {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Multiselect = false;
            fd.CheckFileExists = false;
            DialogResult res = fd.ShowDialog();
            if (res == DialogResult.OK) {
                StreamWriter writer = new StreamWriter(fd.FileName);
                foreach(FileInfo f in FileList)
                    writer.WriteLine(f.FileString());
                writer.Close();
            }
        }
    }
}
