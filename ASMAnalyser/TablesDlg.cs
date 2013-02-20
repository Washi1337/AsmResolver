using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using TUP.AsmResolver;
using TUP.AsmResolver.NET;
using TUP.AsmResolver.NET.Specialized;
using Be.Windows.Forms;
namespace TUP.AsmResolver.PreviewApplication
{
    public partial class TablesDlg : Form
    {
        int index = 1;
        TablesHeap tableheap;
        public TablesDlg(MetaDataStream stream)
        {
            InitializeComponent();

            tableheap = (TablesHeap)stream.ToHeap();
            hexBox1.ByteProvider = new DynamicByteProvider(stream.Contents);
           // foreach (MetaDataMember member in tableheap.Tables.First(t => t.Type == MetaDataTableType.ManifestResource).Members)
           //     treeView1.Nodes.Add(CreateTreeNode(member));
            foreach (MetaDataTable table in tableheap.Tables)
            {
                if (table != null)
                {
                    TreeNode node = new TreeNode(table.Type.ToString() + " (" + table.AmountOfRows.ToString() + ")");
                    treeView1.Nodes.Add(node);
                    index = 1;
                    foreach (MetaDataMember member in table.Members)
                        node.Nodes.Add(CreateTreeNode(member));
                }
            }
        }
        private TreeNode CreateTreeNode(MetaDataMember member)
        {
            TreeNode node = new TreeNode();
            try
            {
                node.Text = index + (member is MemberReference ? " (" + (member as MemberReference).Name + ")" : "");
            }
            catch { node.Text = index.ToString(); }
            index++;


            node.Tag = member;
            return node;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            
            dataGridView1.Rows.Clear();
            if (treeView1.SelectedNode.Tag != null)
            {
                MetaDataMember member = (MetaDataMember)treeView1.SelectedNode.Tag;

                foreach (PropertyInfo property in member.GetType().GetProperties())
                    if (property.Name != "NETHeader" && !property.Name.Contains("MetaData"))
                    {
                        
                        DataGridViewRow row = new DataGridViewRow();

                        row.Cells.Add(new DataGridViewTextBoxCell() { Value = property.Name });
                        try
                        {
                            object value = property.GetValue(member, null);
                            row.Tag = value;
                            if (value == null)
                            {
                                row.Cells.Add(new DataGridViewTextBoxCell() { Value = property.GetGetMethod().ReturnType.Name });
                                row.Cells.Add(new DataGridViewTextBoxCell() { Value = "null" });
                            }
                            else
                            {
                                row.Cells.Add(new DataGridViewTextBoxCell() { Value = value.GetType().Name });
                                ulong number = 0;
                                if (ulong.TryParse(value.ToString(), out number))
                                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = number.ToString("X") });
                                else
                                    row.Cells.Add(new DataGridViewTextBoxCell() { Value = value.ToString() });
                            }

                        }
                        catch (Exception ex)
                        {
                            if (row.Cells.Count == 1)
                                row.Cells.Add(new DataGridViewTextBoxCell() { Value = property.GetGetMethod().ReturnType.Name });
                            row.Cells.Add(new DataGridViewTextBoxCell() { Value = "Invalid MetaData - " + ex.InnerException.ToString()});
                            row.Cells[row.Cells.Count - 1].Style.BackColor = Color.Red;
                            row.Tag = ex.InnerException;
                        }
                        dataGridView1.Rows.Add(row);
                    }
            }
        }
        
        private void TablesDlg_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            MetaDataMember member = ((MetaDataMember)treeView1.SelectedNode.Tag);
            RowEditorDlg dlg = new RowEditorDlg(member.MetaDataRow);
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                member.MetaDataRow = dlg.Result;
                member.ApplyChanges();
            }
        }

        private void disassembleMethodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MetaDataMember member = ((MetaDataMember)treeView1.SelectedNode.Tag);
            if (member is MethodDefinition)
            {
                MethodDefinition methodDef = (MethodDefinition)member;
                if (methodDef.HasBody)
                {
                    var body = methodDef.Body;
                    new MethodBodyDlg(body).ShowDialog();
                }
            }
            else
                MessageBox.Show("Select a method");

        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].Tag != null)
                if (dataGridView1.Rows[e.RowIndex].Tag is byte[])
                    new HexDlg(dataGridView1.Rows[e.RowIndex].Tag as byte[]).ShowDialog();
                else
                    new PropertyDlg(dataGridView1.Rows[e.RowIndex].Tag).ShowDialog();
            
        }

    }
}
