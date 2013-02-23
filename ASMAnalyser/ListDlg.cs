using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TUP.AsmResolver.PreviewApplication
{
    public partial class ListDlg : Form
    {
        bool c1;
        public ListDlg(DataGridViewColumn[] columns, DataGridViewRow[] rows, string Title, bool column1editable)
        {
            InitializeComponent();
            Text = Title;
            dataGridView1.Columns.AddRange(columns);
            dataGridView1.Rows.AddRange(rows);
            c1 = column1editable;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (c1)
            {
                Inputbox input = new Inputbox();
                input.Text = "Edit name " + dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                input.textBox1.MaxLength = Section.MaxSectionNameLength;
                input.textBox1.Text = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                if (input.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    object TAG = dataGridView1.Rows[e.RowIndex].Tag;
                    if (TAG.GetType().FullName == typeof(Section).FullName)
                    {
                        ((Section)TAG).Name = input.textBox1.Text;
                        dataGridView1.Rows[e.RowIndex].Cells[0].Value = input.textBox1.Text;
                    }
                }
            }
        }
    }
}
