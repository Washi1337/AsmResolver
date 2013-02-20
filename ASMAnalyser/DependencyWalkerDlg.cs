using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace TUP.AsmResolver.PreviewApplication
{
    public partial class DependencyWalkerDlg : Form
    {
        Win32Assembly assembly;
        public DependencyWalkerDlg(Win32Assembly assembly)
        {
            InitializeComponent();
            this.assembly = assembly;
            TreeNode node = new TreeNode(Path.GetFileName(assembly.Path));
            foreach (LibraryReference reference in assembly.LibraryImports)
            {
                treeView1.Nodes.Add(new TreeNode(reference.LibraryName) { Tag = new AssemblyReference(reference), ImageIndex = 0, SelectedImageIndex = 0 });

            }

        }

        private void DependencyWalkerDlg_Load(object sender, EventArgs e)
        {

        }

        private void AddRowToDataGridView(string property, string value)
        {
            DataGridViewRow row = new DataGridViewRow();

            row.Cells.Add(new DataGridViewTextBoxCell() { Value = property });
            row.Cells.Add(new DataGridViewTextBoxCell() { Value = value });

            dataGridView1.Rows.Add(row);

        }

        private string FileSize(long bytes)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB" };
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            string readable = num.ToString() + suf[place];
            return readable;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                dataGridView1.Rows.Clear();


                if (e.Node.Tag.GetType() == typeof(AssemblyReference))
                {
                    AssemblyReference tag = (AssemblyReference)e.Node.Tag;
                    LibraryReference reference = tag.reference;

                    
                    try
                    {
                        Win32Assembly resolvedassembly ;
                        if (tag.assembly == null)
                        {
                            resolvedassembly = reference.Resolve(assembly);

                            e.Node.Tag = new AssemblyReference() { assembly = resolvedassembly };
                        }
                        else
                        {
                            resolvedassembly = tag.assembly;
                        }

                        FileVersionInfo nfo = FileVersionInfo.GetVersionInfo(resolvedassembly.Path);
                        AddRowToDataGridView("File Path:", resolvedassembly.Path);
                        AddRowToDataGridView("Description:", nfo.FileDescription);
                        AddRowToDataGridView("Product:", nfo.ProductName);
                        AddRowToDataGridView("Version:", nfo.ProductVersion);
                        AddRowToDataGridView("Company:", nfo.CompanyName);
                        
                        if (e.Node.Nodes.Count == 0)
                        {
                            foreach (LibraryReference loadedref in resolvedassembly.LibraryImports)
                            {
                                e.Node.Nodes.Add(new TreeNode(loadedref.LibraryName) { Tag = new AssemblyReference(loadedref), ImageIndex = 0, SelectedImageIndex = 0 });

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (e.Node.Nodes.Count == 0)
                        e.Node.Nodes.Add(new TreeNode(ex.Message) { Tag = ex, ImageIndex = 1, SelectedImageIndex = 1 });
                    }
                }
                else
                {
                   
                   
                   Exception ex = (Exception)e.Node.Tag;
                   AddRowToDataGridView("Error Message: ", ex.Message);
                   AddRowToDataGridView("Error Type: ", ex.GetType().FullName);
                   Exception inner = ex.InnerException;
                   while (inner != null)
                   {
                       AddRowToDataGridView("Inner Exception:", inner.Message);
                       inner = inner.InnerException;
                   }
                    

                }

    

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {  
            Win32Assembly assembly = ((AssemblyReference)treeView1.SelectedNode.Tag).assembly;

            if (assembly != null)
            {
                List<DataGridViewColumn> columns = new List<DataGridViewColumn>();
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.HeaderText = "Name";
                columns.Add(column);
                column = new DataGridViewTextBoxColumn();
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.HeaderText = "RVA";
                columns.Add(column);
                List<DataGridViewRow> rows = new List<DataGridViewRow>();

                foreach (ExportMethod meth in assembly.LibraryExports)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                    cell.Value = meth.Name;
                    row.Cells.Add(cell);
                    cell = new DataGridViewTextBoxCell();
                    cell.Value = "0x" + ((long)meth.RVA).ToString("x8").ToUpper();
                    row.Cells.Add(cell);
                    rows.Add(row);
                }


                ListDlg dlg = new ListDlg(columns.ToArray(), rows.ToArray(), "Exports of " + Path.GetFileName(assembly.Path), false);
                dlg.Show();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {    

             Win32Assembly assembly;
            if (treeView1.SelectedNode.Parent == null)
                assembly = this.assembly;
            else
                assembly = ((AssemblyReference)treeView1.SelectedNode.Parent.Tag).assembly;

            if (assembly != null)
            {
                List<DataGridViewColumn> columns = new List<DataGridViewColumn>();
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column = new DataGridViewTextBoxColumn();
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.HeaderText = "Name";
                columns.Add(column);
                column = new DataGridViewTextBoxColumn();
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.HeaderText = "Ordinal";
                columns.Add(column);
                column = new DataGridViewTextBoxColumn();
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.HeaderText = "RVA";
                columns.Add(column);
                column = new DataGridViewTextBoxColumn();
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.HeaderText = "Value";
                columns.Add(column);
                List<DataGridViewRow> rows = new List<DataGridViewRow>();


                foreach (LibraryReference lib in assembly.LibraryImports)
                {
                    if (lib.LibraryName == treeView1.SelectedNode.Text)
                    {
                        foreach (ImportMethod meth in lib.ImportMethods)
                        {

                            DataGridViewRow row = new DataGridViewRow();
                            DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                            cell.Value = meth.Name;
                            row.Cells.Add(cell);
                            cell = new DataGridViewTextBoxCell();
                            cell.Value = "0x" + ((long)meth.Ordinal).ToString("x8").ToUpper();
                            row.Cells.Add(cell);
                            cell = new DataGridViewTextBoxCell();
                            cell.Value = "0x" + ((long)meth.RVA).ToString("x8").ToUpper();
                            row.Cells.Add(cell);
                            cell = new DataGridViewTextBoxCell();
                            cell.Value = "0x" + ((long)meth.Value).ToString("x8").ToUpper();
                            row.Cells.Add(cell);

                            rows.Add(row);
                        }
                    }
                }

                ListDlg dlg = new ListDlg(columns.ToArray(), rows.ToArray(), "Methods of current dll used by " + Path.GetFileName(assembly.Path), false);
                dlg.Show();
            }
        }

        private struct AssemblyReference
        {
            public Win32Assembly assembly;
            public LibraryReference reference;

            public AssemblyReference(LibraryReference reference)
            {
                this.reference = reference;
                this.assembly = null;
            }
        }
    }
}
