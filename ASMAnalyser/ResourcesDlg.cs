using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Be.Windows.Forms;
using System.IO;
using System.Reflection;
namespace TUP.AsmResolver.PreviewApplication
{
    public partial class ResourcesDlg : Form
    {
        Stream currentStream = null;
        public ResourcesDlg(ResourceDirectory rootDir)
        {
            InitializeComponent();
            TreeNode rootDirectoryNode = new TreeNode("Root Resource Directory");
            rootDirectoryNode.Tag = rootDir;
            PopulateDirectory(rootDirectoryNode, rootDir);
            treeView1.Nodes.Add(rootDirectoryNode);
        }

        private void PopulateDirectory(TreeNode parentNode, ResourceDirectory directory)
        {
            for (int i = 0; i < directory.ChildEntries.Length; i++)
            {
                TreeNode node = new TreeNode("Resource Directory Entry - ID:" + directory.ChildEntries[i].Name);
                node.Tag = directory.ChildEntries[i];
                PopulateDirectoryEntry(node, directory.ChildEntries[i]);
                parentNode.Nodes.Add(node);
            }
        }

        private void PopulateDirectoryEntry(TreeNode parentNode, ResourceDirectoryEntry entry)
        {
            TreeNode node = new TreeNode();
            if (entry.IsEntryToData)
            {
                node.Text = "Resource Data Entry";
                node.Tag = entry.DataEntry;
            }
            else
            {
                node.Text = "Resource Directory";
                node.Tag = entry.Directory;
                PopulateDirectory(node, entry.Directory);
            }
            parentNode.Nodes.Add(node);
        }

        private void ResourcesDlg_Load(object sender, EventArgs e)
        {
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            propertyGrid1.SelectedObject = e.Node.Tag;
            if (e.Node.Tag is ResourceDataEntry)
            {
                try
                {
                    if (currentStream != null)
                        currentStream.Dispose();
                    currentStream = (e.Node.Tag as ResourceDataEntry).GetStream();
                    hexBox1.ByteProvider = new DynamicFileByteProvider(currentStream);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                hexBox1.ByteProvider = null;
            //else
              //  hexBox1.Hide();


            //if (e.Node.Tag.GetType().FullName == typeof(ResourceData).FullName)
            //{
            //    ResourceData data = (ResourceData)e.Node.Tag;
            //    ResourceDirectory directory = (ResourceDirectory)e.Node.Parent.Tag;
            //
            //    hexBox1.Hide();
            //    pictureBox1.Hide();
            //    richTextBox1.Hide();
            //   // hexBox1.ByteProvider = new DynamicByteProvider(data.Contents);
            //   // hexBox1.Show()
            //        try
            //        {
            //            using (MemoryStream ms = new MemoryStream(data.Contents))
            //            {
            //                pictureBox1.Image = Image.FromStream(ms);
            //                pictureBox1.Show();
            //            }
            //        }
            //        catch (ArgumentException ex)
            //        {
            //   
            //            hexBox1.ByteProvider = new DynamicByteProvider(data.Contents);
            //            hexBox1.Show();
            //        }
            //    
            //    
            //}
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // if (treeView1.SelectedNode.Tag.GetType().FullName == typeof(ResourceData).FullName)
           // {
           //  ResourceData data = (ResourceData)treeView1.SelectedNode.Tag;
           // SaveFileDialog sfd = new SaveFileDialog();
           // if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
           //     System.IO.File.WriteAllBytes(sfd.FileName, data.Contents);
           // }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
