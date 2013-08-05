using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        ToolStripMenuItem disassembleItem;
        ToolStripMenuItem addTableItem;
        ToolStripMenuItem addMemberItem;
        ToolStripMenuItem removeMemberItem;
        TablesHeap currentTablesHeap;

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
            tablesTree.AfterExpand += tablesTree_AfterSelect;
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
            disassembleItem = new ToolStripMenuItem("Disassemble Method");
            disassembleItem.Click += disassembleItem_Click;
            addTableItem = new ToolStripMenuItem("Add Table");
            addTableItem.Click += addTableItem_Click;
            addMemberItem = new ToolStripMenuItem("Add Member");
            addMemberItem.Click += addMemberItem_Click;
            removeMemberItem = new ToolStripMenuItem("Remove Member");
            removeMemberItem.Click += removeMemberItem_Click;
            ContextMenuStrip menuStrip = new System.Windows.Forms.ContextMenuStrip();
            menuStrip.Items.AddRange(new ToolStripMenuItem[]
            {
                disassembleItem,
                addTableItem,
                addMemberItem,
                removeMemberItem,
            });

            tablesTree.ContextMenuStrip = menuStrip;

            dataGridView.CellEndEdit += dataGridView_CellEndEdit;
            mdRowTab.Controls.Add(dataGridView);
            propertyTab.Controls.Add(propertyGrid);
            mainSplitter.Panel1.Controls.Add(tablesTree);
            tabControl.TabPages.Add(mdRowTab);
            tabControl.TabPages.Add(propertyTab);
            mainSplitter.Panel2.Controls.Add(tabControl);
            this.Controls.Add(mainSplitter);
        }

        void removeMemberItem_Click(object sender, EventArgs e)
        {
            if (tablesTree.SelectedNode != null && tablesTree.SelectedNode.Tag is MetaDataMember)
            {
                MetaDataMember member = tablesTree.SelectedNode.Tag as MetaDataMember;
                currentTablesHeap.GetTable(member.TableType, false).RemoveMember(member);
                tablesTree.SelectedNode.Remove();
            }
        }

        private void addMemberItem_Click(object sender, EventArgs e)
        {
            MethodDefinition methodDef = new MethodDefinition("test", MethodAttributes.Public | MethodAttributes.Static, 0, 0, 0);
            //methodDef.MetaDataRow = new MetaDataRow(new object[] { (uint)0, (uint)0, (uint)0, (uint)currentTablesHeap.NETHeader.StringsHeap.GetStringOffset("test"), (uint)0, (uint)0 });
         
            methodDef.Attributes = MethodAttributes.Public | MethodAttributes.Static;
            methodDef.ImplementationAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            
            currentTablesHeap.GetTable(MetaDataTableType.Method, true).AddMember(methodDef);
            //List<object> tablesLeft = new List<object>();
            //for (int i = 0; i < 45; i++)
            //    tablesLeft.Add((MetaDataTableType)i);
            //ComboboxDlg dlg = new ComboboxDlg("Select table to add.", "Add table", tablesLeft.ToArray());
            //if (dlg.ShowDialog() == DialogResult.OK)
            //{
            //    currentTablesHeap.GetTable((MetaDataTableType)dlg.SelectedObject).AddMember((MetaDataMember)Activator.cr;
            //}
        }

        void addTableItem_Click(object sender, EventArgs e)
        {
            List<object> tablesLeft = new List<object>();
            for (int i =0;i < 45; i++)
                if (!currentTablesHeap.HasTable((MetaDataTableType)i))
                    tablesLeft.Add((MetaDataTableType)i);
            ComboBoxDlg dlg = new ComboBoxDlg("Select table to add.", "Add table", tablesLeft.ToArray());
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                currentTablesHeap.AddTable((MetaDataTableType)dlg.SelectedObject);
            }
        }

        void disassembleItem_Click(object sender, EventArgs e)
        {
            if (tablesTree.SelectedNode != null && tablesTree.SelectedNode.Tag != null)
            {
                MethodDefinition member = tablesTree.SelectedNode.Tag as MethodDefinition;
                if (member != null && member.HasBody)
                {
                    try
                    {
                        new MethodDlg(member.Body).ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                ValueType[] parts = new ValueType[dataGridView.Rows.Count];
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
            this.currentTablesHeap = tablesHeap;
            tablesTree.Nodes.Clear();
            foreach (MetaDataTable table in tablesHeap.Tables)
            {
                if (table != null)
                {
                    TreeNode node = new TreeNode(table.Type.ToString() + " (" + table.AmountOfRows.ToString() + ")");
                    node.Tag = table;
                    if (table.AmountOfRows > 0)
                    {
                        node.Nodes.Add(new TreeNode("Loading..."));
                    }
                    tablesTree.Nodes.Add(node);


                }
            }
        }

        private TreeNode CreateTreeNode(MetaDataMember member)
        {
            TreeNode node = new TreeNode();
            uint index = (member.MetaDataToken | ((uint)0xFF << 24)) - ((uint)0xFF << 24);
            try
            {
                node.Text = string.Format("{0} ({1})", index, (member is MemberReference ? (member as MemberReference).Name : member.ToString()));
            }
            catch { node.Text = index.ToString(); }
            index++;
            node.Tag = member;
            return node;
        }

        private void tablesTree_AfterSelect(object sender, TreeViewEventArgs e)
        {

            dataGridView.Rows.Clear();
            if (e.Node.Tag is MetaDataTable)
            {
                MetaDataTable table = e.Node.Tag as MetaDataTable;
                if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "Loading...")
                {
                    e.Node.Nodes.Clear();
                    TreeNode[] subNodes = new TreeNode[table.Members.Count];
                    for (int i = 0; i < subNodes.Length; i++)
                        subNodes[i] = CreateTreeNode(table.Members[i]);

                    tablesTree.BeginUpdate();
                    e.Node.Nodes.AddRange(subNodes);
                    tablesTree.EndUpdate();
                }
            }
            else if (e.Node.Tag is MetaDataMember)
            {
                currentMember = (MetaDataMember)e.Node.Tag;
                propertyGrid.SelectedObject = currentMember;
                SetMetaDataRow(currentMember.MetaDataRow);
            }

        }

        private void SetMetaDataRow(MetaDataRow row)
        {
            dataGridView.Rows.Clear();
            if (row.Parts == null)
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
