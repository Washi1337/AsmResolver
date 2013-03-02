using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Be.Windows.Forms;
using TUP.AsmResolver;
using TUP.AsmResolver.NET;

namespace AsmResolver
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void OpenFile(string file)
        {
            try
            {
                Win32Assembly assembly = Win32Assembly.LoadFile(file);
                treeView1.Nodes.Add(TreeBuilder.ConstructAssemblyNode(assembly));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private Win32Assembly GetCurrentAssembly()
        {
            TreeNode node = treeView1.SelectedNode;
            if (node == null)
                return null;
            while (node.Parent != null)
                node = node.Parent;

            if (node.Tag is TreeNodeTag)
                return ((TreeNodeTag)node.Tag).Object as Win32Assembly;
            return null;
        }

        private void SetTypeConverters()
        {
            Type[] types = new Type[] { 
                typeof(ulong), typeof(uint), typeof(ushort), typeof(byte),
                typeof(long), typeof(int), typeof(short), typeof(sbyte)};

            foreach (Type t in types)
            {
                TypeDescriptionProvider m_OriginalProvider = TypeDescriptor.GetProvider(t);
                TypeDescriptionProvider hexProvider = (TypeDescriptionProvider)Activator.CreateInstance(typeof(IntToHexTypeDescriptionProvider<>).MakeGenericType(new Type[] { t }), new object[] { m_OriginalProvider }); ;
                TypeDescriptor.AddProvider(hexProvider, t);
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            propertyGrid1.Dock = DockStyle.Fill;
            hexBoxControl1.Dock = DockStyle.Fill;
            resourceControl1.Dock = DockStyle.Fill;
            tablesControl1.Dock = DockStyle.Fill;
            disassemblerControl1.Dock = DockStyle.Fill;

            SetTypeConverters();
        }
        
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Assembly files (*.exe; *.dll)|*.exe;*.dll";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenFile(ofd.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Assembly files (*.exe; *.dll)|*.exe;*.dll";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    GetCurrentAssembly().QuickSave(sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void rebuildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Assembly files (*.exe; *.dll)|*.exe;*.dll";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    GetCurrentAssembly().Rebuild(sfd.FileName, new WritingParameters());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            propertyGrid1.Hide();
            hexBoxControl1.Hide();
            resourceControl1.Hide();
            tablesControl1.Hide();
            disassemblerControl1.Hide();
            object tag = e.Node.Tag;
            if (tag is TreeNodeTag)
            {
                TreeNodeTag treeNodeTag = tag as TreeNodeTag;
                if (treeNodeTag.Object != null)
                {
                    switch (treeNodeTag.Type)
                    {
                        case TreeNodeType.PropertyGrid:
                            propertyGrid1.SelectedObject = treeNodeTag.Object;
                            propertyGrid1.Show();
                            break;
                        case TreeNodeType.HexBox:
                            hexBoxControl1.SetByteProvider(GetCurrentAssembly(), ((IByteProvider)treeNodeTag.Object));
                            hexBoxControl1.Show();
                            break;
                        case TreeNodeType.ResourcesTree:
                            resourceControl1.SetRootDirectory(treeNodeTag.Object as ResourceDirectory);
                            resourceControl1.Show();
                            break;
                        case TreeNodeType.TablesTree:
                            tablesControl1.SetTablesHeap(treeNodeTag.Object as TablesHeap);
                            tablesControl1.Show();
                            break;
                        case TreeNodeType.Disassembler:
                            disassemblerControl1.SetAssembly(GetCurrentAssembly());
                            disassemblerControl1.Show();
                            break;
                    }
                }
            }
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Effect == DragDropEffects.Copy)
                foreach (string file in ((string[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop)))
                    OpenFile(file);
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem.PerformClick();
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            saveToolStripButton.PerformClick();
        }

        private void sourceCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.github.com/TheUnknownProgrammer/AsmResolver");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Program and library created by TheUnknownProgrammer", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



    }
}
