using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using TUP.AsmResolver.NET;
namespace TUP.AsmResolver.PreviewApplication
{
    public partial class RowEditorDlg : Form
    {
        MetaDataRow mdrow;
        public RowEditorDlg(MetaDataRow mdrow)
        {
            this.mdrow = mdrow;
            InitializeComponent();

            for (int i = 0; i < mdrow.Parts.Length; i++)
            { 
                DataGridViewRow row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = i.ToString() });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = (Convert.ToUInt64(mdrow.Parts[i])).ToString("X" +(Marshal.SizeOf(mdrow.Parts[i]))*2)  });
                DataGridViewComboBoxCell cbox = new DataGridViewComboBoxCell();
                cbox.Items.Add("Byte");
                cbox.Items.Add("UInt16");
                cbox.Items.Add("UInt32");
                cbox.Items.Add("UInt64");
                cbox.Value = mdrow.Parts[i].GetType().Name;
                row.Cells.Add(cbox);
                dataGridView1.Rows.Add(row);
            }
        }

        public MetaDataRow Result
        {
            get { return mdrow; }
        }

        private void RowEditorDlg_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {


            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                object[] parts = new object[dataGridView1.Rows.Count];
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewRow row = dataGridView1.Rows[i];

                    switch (row.Cells[2].Value.ToString())
                    {
                        case "Byte": parts[i] = byte.Parse(row.Cells[1].Value.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier); break;
                        case "UInt16": parts[i] = ushort.Parse(row.Cells[1].Value.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier); break;
                        case "UInt32": parts[i] = uint.Parse(row.Cells[1].Value.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier); break;
                        case "UInt64": parts[i] = ulong.Parse(row.Cells[1].Value.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier); break;
                    }
                }

                MetaDataRow newrow = new MetaDataRow(mdrow.Offset, parts);
                mdrow = newrow;
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
