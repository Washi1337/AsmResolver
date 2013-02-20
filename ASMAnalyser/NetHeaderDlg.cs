using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using TUP.AsmResolver;
using TUP.AsmResolver.NET;
namespace TUP.AsmResolver.PreviewApplication
{
    public partial class NetHeaderDlg : Form
    {
        NETHeader header;
        public NetHeaderDlg(NETHeader header)
        {
            this.header = header;
            InitializeComponent();

            textBox1.Text = "0x" + header.RawOffset.ToString("X8");
            textBox2.Text = header.Flags.ToString();
            textBox3.Text = header.MetaDataHeader.VersionString;
            textBox4.Text = header.MetaDataStreams.Length.ToString();
            textBox5.Text = "0x" + header.EntryPointToken.ToString("X8");

            for (int i = 0; i <= header.MetaDataStreams.Length - 1; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = header.MetaDataStreams[i].HeaderOffset.ToString("X8") });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = header.MetaDataStreams[i].StreamSize.ToString("X8") });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = header.MetaDataStreams[i].Name });

                byte[] bytes = new byte[4];
                new MemoryStream(header.MetaDataStreams[i].Contents).Read(bytes,0,4);
                string firstbytestext = "";
                foreach (byte b in bytes)
                    firstbytestext += b.ToString("X2") + " ";
                firstbytestext += "...";
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = firstbytestext });
                row.Tag = header.MetaDataStreams[i];
                dataGridView1.Rows.Add(row);
            }

            dataGridView1.CellDoubleClick += new DataGridViewCellEventHandler(dataGridView1_CellDoubleClick);
        }

        void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string name = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();
            if (name.Contains("#~") || name.Contains("#-"))
                new TablesDlg(((MetaDataStream)dataGridView1.Rows[e.RowIndex].Tag)).ShowDialog();
            else
                new HexDlg(((MetaDataStream)dataGridView1.Rows[e.RowIndex].Tag).Contents).ShowDialog();
        }

        private void NetHeaderDlg_Load(object sender, EventArgs e)
        {
            
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var dlg = new FlagsDlg<NETHeaderFlags>(header.Flags);
             if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
             {
                 header.Flags = (NETHeaderFlags)dlg.GetRawFlagValue();
                 textBox2.Text = header.Flags.ToString();
             }
        }

    }
}
