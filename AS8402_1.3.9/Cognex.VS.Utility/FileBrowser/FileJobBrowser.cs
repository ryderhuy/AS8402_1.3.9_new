using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using Cognex.VS.Utility;

namespace Cognex.VS.Utility
{
    public class FileJobBrowser : BaseDialog
    {
        private string mRemoteDir;
        private string mLocalDir;
        private string mMode; // Open - Save
        private string mNameFileSellected;
        private Label label2;
        private TextBox tbFileName;
        private bool mSensor = false;
        public FileJobBrowser(string RemoteDir, string mode)
        {
            InitializeComponent();
            mRemoteDir = RemoteDir;
            mMode = mode;
            this.SetTitle(mMode);
            mBtnOK.Text = mMode;
        }
        #region Winforms Form Designer generated code
        private Button btnInsightPath;
        private TreeView treeViewListFolder;
        private Label label1;
        private TextBox tbDirectory;
        private ImageList imageList1;
        private IContainer components;
        private Button btnLocal;
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileJobBrowser));
            this.btnLocal = new System.Windows.Forms.Button();
            this.btnInsightPath = new System.Windows.Forms.Button();
            this.treeViewListFolder = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tbDirectory = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbFileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.tbFileName);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.tbDirectory);
            this.panel1.Controls.Add(this.treeViewListFolder);
            this.panel1.Controls.Add(this.btnInsightPath);
            this.panel1.Controls.Add(this.btnLocal);
            this.panel1.Size = new System.Drawing.Size(719, 431);
            // 
            // mBtnReset
            // 
            this.mBtnReset.Location = new System.Drawing.Point(521, 0);
            this.mBtnReset.Size = new System.Drawing.Size(99, 33);
            // 
            // mBtnOK
            // 
            this.mBtnOK.Location = new System.Drawing.Point(422, 0);
            this.mBtnOK.Size = new System.Drawing.Size(99, 33);
            // 
            // mBtnCancel
            // 
            this.mBtnCancel.Location = new System.Drawing.Point(620, 0);
            this.mBtnCancel.Size = new System.Drawing.Size(99, 33);
            // 
            // btnLocal
            // 
            this.btnLocal.Location = new System.Drawing.Point(30, 50);
            this.btnLocal.Name = "btnLocal";
            this.btnLocal.Size = new System.Drawing.Size(94, 68);
            this.btnLocal.TabIndex = 0;
            this.btnLocal.Text = "This PC";
            this.btnLocal.UseVisualStyleBackColor = true;
            this.btnLocal.Click += new System.EventHandler(this.btnLocal_Click);
            // 
            // btnInsightPath
            // 
            this.btnInsightPath.Location = new System.Drawing.Point(30, 124);
            this.btnInsightPath.Name = "btnInsightPath";
            this.btnInsightPath.Size = new System.Drawing.Size(94, 68);
            this.btnInsightPath.TabIndex = 1;
            this.btnInsightPath.Text = "Insight";
            this.btnInsightPath.UseVisualStyleBackColor = true;
            this.btnInsightPath.Click += new System.EventHandler(this.btnInsightPath_Click);
            // 
            // treeViewListFolder
            // 
            this.treeViewListFolder.ImageIndex = 1;
            this.treeViewListFolder.ImageList = this.imageList1;
            this.treeViewListFolder.Location = new System.Drawing.Point(150, 50);
            this.treeViewListFolder.Name = "treeViewListFolder";
            this.treeViewListFolder.SelectedImageIndex = 0;
            this.treeViewListFolder.Size = new System.Drawing.Size(563, 341);
            this.treeViewListFolder.TabIndex = 2;
            this.treeViewListFolder.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewListFolder_BeforeExpand);
            this.treeViewListFolder.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewListFolder_AfterSelect);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "folder.png");
            this.imageList1.Images.SetKeyName(1, "file.png");
            this.imageList1.Images.SetKeyName(2, "iconfinder_Windows_Drive_272526.ico");
            // 
            // tbDirectory
            // 
            this.tbDirectory.Location = new System.Drawing.Point(150, 12);
            this.tbDirectory.Name = "tbDirectory";
            this.tbDirectory.Size = new System.Drawing.Size(448, 22);
            this.tbDirectory.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(86, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Look in:";
            // 
            // tbFileName
            // 
            this.tbFileName.Location = new System.Drawing.Point(224, 399);
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.Size = new System.Drawing.Size(181, 22);
            this.tbFileName.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(147, 402);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 17);
            this.label2.TabIndex = 6;
            this.label2.Text = "File Name";
            // 
            // FileJobBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.ClientSize = new System.Drawing.Size(719, 469);
            this.Name = "FileJobBrowser";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        #region Load directory test
        public void LoadDirectory(string dir)
        {
            DirectoryInfo di = new DirectoryInfo(dir);
            TreeNode tds = treeViewListFolder.Nodes.Add(di.Name);
            tds.Tag = di.FullName;
            tds.ImageIndex = 0;
            //LoadFiles(dir, tds);
            LoadSubDirectories(dir, tds);
        }
        public void LoadSubDirectories(string dir, TreeNode td)
        {
            string[] subdirectoryEntries = Directory.GetDirectories(dir);
            foreach (string subdirectory in subdirectoryEntries)
            {
                DirectoryInfo di = new DirectoryInfo(subdirectory);
                TreeNode tds = td.Nodes.Add(di.Name);
                tds.ImageIndex = 0;
                tds.Tag = di.Name;
                LoadFiles(subdirectory, tds);
                LoadSubDirectories(subdirectory, tds);
            }
        }
        private void LoadFiles(string dir, TreeNode td)
        {
            string[] Files = Directory.GetFiles(dir, "*.*");

            // Loop through them to see files  
            foreach (string file in Files)
            {
                FileInfo fi = new FileInfo(file);
                TreeNode tds = td.Nodes.Add(fi.Name);
                tds.Tag = fi.FullName;
                tds.ImageIndex = 1;
            }
        }
        #endregion

        private void btnLocal_Click(object sender, EventArgs e)
        {
            treeViewListFolder.Nodes.Clear();
            mSensor = false;
            tbDirectory.Text = "ThisPC";
            string[] drives = Environment.GetLogicalDrives();

            foreach (string drive in drives)
            {
                DriveInfo di = new DriveInfo(drive);
                int driveImage = 2;

                TreeNode node = new TreeNode(drive.Substring(0, 2), driveImage, driveImage);
                node.Tag = drive;

                if (di.IsReady == true)
                    node.Nodes.Add("...");

                treeViewListFolder.Nodes.Add(node);
            }
        }
        private void btnInsightPath_Click(object sender, EventArgs e)
        {
            if (mRemoteDir == "")
            {
                MessageBox.Show("No sensor connected!");
                return;
            }
            mSensor = true;
            tbDirectory.Text = mRemoteDir;
            treeViewListFolder.Nodes.Clear();

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(mRemoteDir);
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            request.Credentials = new NetworkCredential("admin", "");

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            try
            {
                while (!reader.EndOfStream)
                {
                    string nameFile = reader.ReadLine();
                    TreeNode node = new TreeNode(nameFile, 1, 1);
                    node.Tag = nameFile;
                    treeViewListFolder.Nodes.Add(node);
                }
            }
            catch (Exception ex)
            {

            }

            reader.Close();
            response.Close();

        }
        private void treeViewListFolder_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count > 0)
            {
                if (e.Node.Nodes[0].Text == "..." && e.Node.Nodes[0].Tag == null)
                {
                    e.Node.Nodes.Clear();
                    string[] dirs = Directory.GetDirectories(e.Node.Tag.ToString());

                    foreach (string dir in dirs)
                    {
                        DirectoryInfo di = new DirectoryInfo(dir);
                        TreeNode node = new TreeNode(di.Name, 0, 0);

                        try
                        {
                            node.Tag = dir;
                            if (di.GetDirectories().Count() > 0)
                                node.Nodes.Add(null, "...", 0, 0);

                            foreach (var file in di.GetFiles())
                            {
                                TreeNode n = new TreeNode(file.Name, 1, 1);

                                node.Nodes.Add(n);
                            }

                        }
                        catch (UnauthorizedAccessException)
                        {
                            node.ImageIndex = -1;
                            node.SelectedImageIndex = -1;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "DirectoryLister", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            e.Node.Nodes.Add(node);
                        }
                    }
                }
            }
        }
        private void treeViewListFolder_AfterSelect(object sender, TreeViewEventArgs e)
        {
            mNameFileSellected = e.Node.FullPath;
        }

        public string GetLocalPath()
        {
            // return mNameFileSellected; // Open job file 
            return mNameFileSellected + "\\" + tbFileName.Text;
        }
        
        public string GetRemotePath()
        {
            //return mRemoteDir + mNameFileSellected; // Open Job file
            return mRemoteDir + tbFileName.Text;
        }
        public bool IsSensor()
        {
            return mSensor;
        }

        protected override void BtnOk_Click()
        {
           
            this.Hide();
        }

        protected override void BtnCancel_Click()
        {
            this.Hide();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

    }
}
