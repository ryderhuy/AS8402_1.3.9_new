using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Cognex.VS.Utility
{
    public partial class FileBrowser : UserControl
    {
        public FileBrowser()
        {

            InitializeComponent();
        }

        private void treeViewList_BeforeExpand(object sender, TreeViewCancelEventArgs e)
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
                        catch (UnauthorizedAccessException ex)
                        {
                            node.ImageIndex = -1;
                            node.SelectedImageIndex = -1;
                            MessageBox.Show(ex.Message, "AccessException", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
    }
}
