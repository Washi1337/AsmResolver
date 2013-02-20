using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Be.Windows.Forms;
namespace TUP.AsmResolver.PreviewApplication
{
    public partial class HexDlg : Form
    {
        int maxbytelength;
        public HexDlg(byte[] bytes)
        {
            InitializeComponent();

            hexBox1.ByteProvider = new DynamicByteProvider(bytes);
            maxbytelength = bytes.Length;
        }
        public byte[] Bytes
        {
            get
            {
                return ((DynamicByteProvider)hexBox1.ByteProvider).Bytes.ToArray();
            }
        }
        private void HexDlg_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void hexBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (hexBox1.ByteProvider.Length != maxbytelength | hexBox1.SelectionStart == maxbytelength & e.KeyCode != Keys.Left | hexBox1.SelectionStart == maxbytelength & e.KeyCode != Keys.Up)
                e.SuppressKeyPress = true;
            
        }


    }
}
