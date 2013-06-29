using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Be.Windows.Forms;
using TUP.AsmResolver;
using TUP.AsmResolver.NET;
using TUP.AsmResolver.NET.Specialized;

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
            OpenFile(file, new ReadingParameters());
        }

        private void OpenFile(string file, ReadingParameters parameters)
        {
            try
            {
                Win32Assembly assembly = Win32Assembly.LoadFile(file, parameters);
                treeView1.Nodes.Add(TreeBuilder.ConstructAssemblyNode(assembly));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private TreeNode GetRootNode()
        {
            TreeNode node = treeView1.SelectedNode;
            if (node == null)
                return null;
            while (true)
            {
                if (node.Parent == null)
                    return node;
                else
                    node = node.Parent;
            }
            

        }

        private Win32Assembly GetCurrentAssembly()
        {
            TreeNode node = GetRootNode();
            if (node.Tag is TreeNodeTag)
                return ((TreeNodeTag)node.Tag).Object as Win32Assembly;
            return null;
        }

        private void SetTypeConverters()
        {
            Type[] numberTypes = new Type[] { 
                typeof(ulong), typeof(uint), typeof(ushort), typeof(byte),
                typeof(long), typeof(int), typeof(short), typeof(sbyte)};

            foreach (Type t in numberTypes)
            {
                TypeDescriptionProvider m_OriginalProvider = TypeDescriptor.GetProvider(t);
                TypeDescriptionProvider hexProvider = (TypeDescriptionProvider)Activator.CreateInstance(typeof(IntToHexTypeDescriptionProvider<>).MakeGenericType(new Type[] { t }), new object[] { m_OriginalProvider }); ;
                TypeDescriptor.AddProvider(hexProvider, t);
            }

            TypeDescriptor.AddAttributes(typeof(Offset), new EditorAttribute(typeof(OffsetUIEditor), typeof(UITypeEditor)));
            TypeDescriptor.AddAttributes(typeof(IMemberSignature), new EditorAttribute(typeof(PropertyGridUIEditor), typeof(UITypeEditor)));
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
                if (((ToolStripMenuItem)sender).Name == openToolStripMenuItem.Name)
                    OpenFile(ofd.FileName);
                else
                {
                    PropertyGridDlg rdlg = new PropertyGridDlg(new ReadingParameters());
                    if (rdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        OpenFile(ofd.FileName, rdlg.Object as ReadingParameters);
                }
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
                    PropertyGridDlg dlg = new PropertyGridDlg(new WritingParameters());
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        GetCurrentAssembly().Rebuild(sfd.FileName, dlg.Object as WritingParameters);
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
            saveToolStripMenuItem.PerformClick();
        }

        private void sourceCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.github.com/TheUnknownProgrammer/AsmResolver");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Program and library created by TheUnknownProgrammer", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void disassembleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                TreeNodeTag tag = treeView1.SelectedNode.Tag as TreeNodeTag;
                if (tag.Object is Section)
                {
                    TreeNode root = GetRootNode();
                    foreach (TreeNode node in root.Nodes)
                        if (node.Tag != null && ((TreeNodeTag)node.Tag).Type == TreeNodeType.Disassembler)
                        {
                            node.EnsureVisible();
                            treeView1.SelectedNode = node;
                            disassemblerControl1.DisassembleSection(tag.Object as Section);
                            break;
                        }
                }
            }
        }

        private void addStreamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] bytes = new byte[0x100];
            byte[] pattern = new byte[] { 0x13, 0x37, 0xC0, 0xDE };
            int j = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = pattern[j];
                j++;
                if (j >= pattern.Length)
                    j = 0;
            }
           // GetCurrentAssembly().NETHeader.MetaDataStreams.Insert(0, new MetaDataStream("1337", bytes));
        }

        private void unloadApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                GetCurrentAssembly().Dispose();
                treeView1.SelectedNode.Remove();
            }
        }



    }

}
