using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace HashUtil {
    public partial class MainForm : Form {
        private List<FileInfo> FileList;
        private List<FileInfo> VerifyList;
        private BackgroundWorker bw;
        private bool running;

        private Hashes.Type currentHash;

        public MainForm() {
            InitializeComponent();
            FileList = new List<FileInfo>();
            VerifyList = new List<FileInfo>();
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = false;
            bw.DoWork += Bw_DoWork;
            bw.ProgressChanged += Bw_ProgressChanged;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            
            ColumnHeader h1 = new ColumnHeader();
            h1.Text = "Filename";

            ColumnHeader h2 = new ColumnHeader();
            h2.Text = "Hash";

            ColumnHeader h3 = new ColumnHeader();
            h3.Text = "Expected";
            
            lvMain.Columns.AddRange(new ColumnHeader[] { h1, h2, h3 });
            lvMain.SmallImageList = imgList;

            foreach (ColumnHeader header in lvMain.Columns)
                if (header.Width < 150)
                    header.Width = 150;

            running = false;
            btnSave.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
            
            ToolStripMenuItem h_crc32 = new ToolStripMenuItem();
            h_crc32.Text = "CRC32";
            h_crc32.Checked = true;
            h_crc32.Click += Hash_Click;

            ToolStripMenuItem h_md5 = new ToolStripMenuItem();
            h_md5.Text = "MD5";
            h_md5.Checked = false;
            h_md5.Click += Hash_Click;

            ToolStripMenuItem h_sha1 = new ToolStripMenuItem();
            h_sha1.Text = "SHA1";
            h_sha1.Checked = false;
            h_sha1.Click += Hash_Click;

            ToolStripMenuItem h_sha256 = new ToolStripMenuItem();
            h_sha256.Text = "SHA256";
            h_sha256.Checked = false;
            h_sha256.Click += Hash_Click;

            hashTypeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { h_crc32, h_md5, h_sha1, h_sha256 });
            currentHash = Hashes.Type.CRC32;
        }

        // Uncheck all hash types in the menu
        private void uncheck_hashes() {
            foreach(ToolStripMenuItem item in hashTypeToolStripMenuItem.DropDownItems)
                item.Checked = false;
        }

        private void hashes_toggleEnable() {
            foreach(ToolStripMenuItem item in hashTypeToolStripMenuItem.DropDownItems)
                item.Enabled = !item.Enabled;
        }

        private void Hash_Click(object sender, EventArgs e) {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            uncheck_hashes();
            item.Checked = true;
            currentHash = Hashes.ParseType(item.Text);
            lvMain.Columns[1].Text = item.Text;
            VerifyList.Clear();
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e) {
            object sp = null;
            if (currentHash == Hashes.Type.CRC32)
                sp = new Crc32();
            else if (currentHash == Hashes.Type.MD5)
                sp = new  MD5CryptoServiceProvider();
            else if (currentHash == Hashes.Type.SHA1)
                sp = new SHA1CryptoServiceProvider();
            else if (currentHash == Hashes.Type.SHA256)
                sp = new SHA256CryptoServiceProvider();
            else {
                bw.ReportProgress(-2, "Invalid hash type: " + currentHash.ToString());
                return;
            }

            List<FileInfo> fileList = (List<FileInfo>)e.Argument;
            foreach(FileInfo file in fileList) {
                FileStream fs = null;
                try {
                    fs = new FileStream(file.Name, FileMode.Open);
                } catch (Exception ex) {
                    file.Error = ex.Message;
                    file.status = FileInfo.FileStatus.NOTOK;
                    bw.ReportProgress(-1, file);
                    continue;
                }
                
                //file.Hash = "Hashing...";
                byte[] hash = null;
                switch (currentHash) {
                    case Hashes.Type.CRC32:
                        Crc32 p = (Crc32)sp;
                        hash = p.ComputeHash(fs);
                        break;
                    case Hashes.Type.MD5:
                        MD5CryptoServiceProvider pm = (MD5CryptoServiceProvider)sp;
                        hash = pm.ComputeHash(fs);
                        break;
                    case Hashes.Type.SHA1:
                        SHA1CryptoServiceProvider ps = (SHA1CryptoServiceProvider)sp;
                        hash = ps.ComputeHash(fs);
                        break;
                    case Hashes.Type.SHA256:
                        SHA256CryptoServiceProvider ps2 = (SHA256CryptoServiceProvider)sp;
                        hash = ps2.ComputeHash(fs);
                        break;
                }
                file.status = FileInfo.FileStatus.WORKING;
                file.Hash = BitConverter.ToString(hash);
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

            if (e.ProgressPercentage == -1) {
                lvMain.Items[idx].SubItems[1].Text = f.Error;
            }

            lvMain.Items[idx].ImageIndex = (int)f.status;
            lvMain.Items[idx].SubItems[1].Text = f.Hash;
            
            if (VerifyList.Count > 0) {
                if (VerifyList[idx].Hash != f.Hash)
                    f.status = FileInfo.FileStatus.NOTOK;
                else
                    f.status = FileInfo.FileStatus.OK;
            } else
                f.status = FileInfo.FileStatus.OK;

            Console.WriteLine("Verify: " + VerifyList.Count + "; status: " + f.status);
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            btnAdd.Enabled = true;
            btnClear.Enabled = true;
            btnExit.Enabled = true;
            btnSave.Enabled = true;
            saveToolStripMenuItem.Enabled = true;
            hashes_toggleEnable();
            UpdateListView();
        }


        private void btnHash_Click(object sender, EventArgs e) {
            for(int i = 0; i < FileList.Count; i++) {
                FileList.ElementAt(i).Hash = null;
            }
            running = true;
            UpdateListView();
            btnAdd.Enabled = false;
            btnClear.Enabled = false;
            btnExit.Enabled = false;
            btnSave.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
            hashes_toggleEnable();
            bw.RunWorkerAsync(FileList);
        }

        
        private void OpenFiles() {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Multiselect = true;
            fd.CheckFileExists = true;
            DialogResult res = fd.ShowDialog();
            if (res == DialogResult.OK) {
                FileList.Clear();
                foreach(string file in fd.FileNames)
                    FileList.Add(new FileInfo(file));
                UpdateListView();
            }
        }

        private void SaveHashes() {
            SaveFileDialog fd = new SaveFileDialog();
            fd.Filter = "Hash File|*.txt";
            fd.CheckFileExists = false;
            DialogResult res = fd.ShowDialog();
            if (res == DialogResult.OK) {
                StreamWriter writer = new StreamWriter(fd.FileName);
                foreach (FileInfo f in FileList)
                    writer.WriteLine(f.FileString());
                writer.Close();
            }
        }

        private void UpdateListView() {
            lvMain.Items.Clear();
            lvMain.BeginUpdate();
            bool even = true;
            bool verifying = false;
            if (VerifyList.Count > 0)
                verifying = true;

            for( int idx = 0; idx < FileList.Count(); idx++) {
                FileInfo file = FileList[idx];
                ListViewItem i = new ListViewItem();
                i.Text = file.Basename;
                i.ImageIndex = (int)file.status;

                if (file.Hash == null)
                    if (running)
                        i.SubItems.Add("Waiting...");
                    else
                        i.SubItems.Add("");
                else
                    i.SubItems.Add(file.Hash);
                
                if (verifying)
                    i.SubItems.Add(VerifyList[idx].Hash);
                else
                    i.SubItems.Add("");

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

        private void ClearList() {
            FileList.Clear();
            VerifyList.Clear();
            UpdateListView();
            btnSave.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
        }

        private void VerifyFiles() {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Text files|*.txt|MD5 Hashes|*.md5|All Files|*.*";
            fd.CheckFileExists = true;
            DialogResult res = fd.ShowDialog();
            if (res == DialogResult.Cancel || res == DialogResult.Abort)
                return;

            string dirname = Path.GetDirectoryName(fd.FileName);

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = dirname;
            res = fbd.ShowDialog();
            if (res == DialogResult.Cancel || res == DialogResult.Abort)
                return;

            StreamReader reader = new StreamReader(fd.FileName);
            string[] lines = Regex.Split(reader.ReadToEnd(), "\r\n");
            VerifyList.Clear();
            FileList.Clear();
            foreach(string line in lines) {
                if (line.Length > 0) {
                    FileInfo fi = FileInfo.ParseInfoLine(line);
                    fi.SetDirectory(dirname);
                    VerifyList.Add(fi);
                    FileList.Add(new FileInfo(fi.Name));
                }
            }

            currentHash = VerifyList[0].HashType;
            lvMain.Columns[1].Text = Hashes.String(currentHash);

            UpdateListView();
            reader.Close();
        }

        private void btnSave_Click(object sender, EventArgs e) {
            SaveHashes();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Close();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e) {
            ClearList();
        }

        private void btnClear_Click(object sender, EventArgs e) {
            ClearList();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveHashes();
        }

        private void openFilesToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFiles();
        }

        private void btnAdd_Click(object sender, EventArgs e) {
            OpenFiles();
        }

        private void btnExit_Click(object sender, EventArgs e) {
            Close();
        }

        private void verifyFilesToolStripMenuItem_Click(object sender, EventArgs e) {
            VerifyFiles();
        }
    }
}
