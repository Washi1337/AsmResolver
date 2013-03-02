using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TUP.AsmResolver.NET;
using TUP.AsmResolver.NET.Specialized;

namespace AsmResolver
{
    public class TablesControl : Control
    {
        int index = 1;
        TreeView tablesTree;
        PropertyGrid propertyGrid;
        DataGridView dataGridView;
        MetaDataMember currentMember;

        public TablesControl()
        {
            SplitContainer mainSplitter = new SplitContainer()
            {
                Dock = DockStyle.Fill,
            };
            tablesTree = new TreeView()
            {
                Dock = DockStyle.Fill,
            };
            TabControl tabControl = new TabControl()
            {
                Dock = DockStyle.Fill,
            };
            TabPage mdRowTab = new TabPage()
            {
                Text = "MetaData Row",
            };
            TabPage propertyTab = new TabPage()
            {
                Text = "Properties",
            };
            dataGridView = new DataGridView()
            {
                Dock = DockStyle.Fill,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
            };
            propertyGrid = new PropertyGrid()
            {
                Dock = DockStyle.Fill,
            };
            tablesTree.AfterSelect += tablesTree_AfterSelect;
            dataGridView.Columns.AddRange(new DataGridViewTextBoxColumn() 
            { 
                HeaderText = "#",
                Width = 30, 
                ReadOnly = true,
            },
            new DataGridViewTextBoxColumn()
            {
                HeaderText = "Value",
                Width = 300,
                ReadOnly = false,
            },
            new DataGridViewTextBoxColumn()
            {
                HeaderText = "Data Type",
                Width = 150,
                ReadOnly = true,
            }
            );
            dataGridView.CellEndEdit += dataGridView_CellEndEdit;
            mdRowTab.Controls.Add(dataGridView);
            propertyTab.Controls.Add(propertyGrid);
            mainSplitter.Panel1.Controls.Add(tablesTree);
            tabControl.TabPages.Add(mdRowTab);
            tabControl.TabPages.Add(propertyTab);
            mainSplitter.Panel2.Controls.Add(tabControl);
            this.Controls.Add(mainSplitter);
        }

        void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                object[] parts = new object[dataGridView.Rows.Count];
                for (int i = 0; i < dataGridView.Rows.Count; i++)
                {
                    DataGridViewRow row = dataGridView.Rows[i];

                    switch (row.Cells[2].Value.ToString())
                    {
                        case "Byte": parts[i] = byte.Parse(row.Cells[1].Value.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier); break;
                        case "UInt16": parts[i] = ushort.Parse(row.Cells[1].Value.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier); break;
                        case "UInt32": parts[i] = uint.Parse(row.Cells[1].Value.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier); break;
                        case "UInt64": parts[i] = ulong.Parse(row.Cells[1].Value.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier); break;
                    }
                }

                MetaDataRow newrow = new MetaDataRow(currentMember.MetaDataRow.Offset, parts);
                currentMember.MetaDataRow = newrow;
                currentMember.ClearCache();
                currentMember.ApplyChanges();
                propertyGrid.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured. " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SetTablesHeap(TablesHeap tablesHeap)
        {
            tablesTree.Nodes.Clear();
            foreach (MetaDataTable table in tablesHeap.Tables)
            {
                if (table != null)
                {
                    TreeNode node = new TreeNode(table.Type.ToString() + " (" + table.AmountOfRows.ToString() + ")");
                    tablesTree.Nodes.Add(node);
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

        private void tablesTree_AfterSelect(object sender, TreeViewEventArgs e)
        {

            dataGridView.Rows.Clear();
            if (tablesTree.SelectedNode.Tag != null)
            {
                currentMember = (MetaDataMember)tablesTree.SelectedNode.Tag;
                propertyGrid.SelectedObject = currentMember;
                SetMetaDataRow(currentMember.MetaDataRow);
            }

        }

        private void SetMetaDataRow(MetaDataRow row)
        {
            dataGridView.Rows.Clear();
            if (row == null)
                return;
            
            for (int i = 0; i < row.Parts.Length; i++)
            {
                DataGridViewRow drow = new DataGridViewRow();
                string value = string.Empty;
                switch (row.Parts[i].GetType().Name)
                {
                    case "Byte":
                        value = ((byte)row.Parts[i]).ToString("X2");
                        break;
                    case "UInt16":
                        value = ((ushort)row.Parts[i]).ToString("X4");
                        break;
                    case "UInt32":
                        value = ((uint)row.Parts[i]).ToString("X8");
                        break;
                    case "UInt64":
                        value = ((ulong)row.Parts[i]).ToString("X16");
                        break;
                }

                drow.Cells.Add(new DataGridViewTextBoxCell() { Value = i.ToString() });
                drow.Cells.Add(new DataGridViewTextBoxCell() { Value = value });
                drow.Cells.Add(new DataGridViewTextBoxCell() { Value = row.Parts[i].GetType().Name });
                dataGridView.Rows.Add(drow);
            }

            
        }
    }
}
