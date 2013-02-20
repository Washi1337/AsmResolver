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
    public partial class DisassembleDlg : Form
    {
        public DisassembleDlg(Win32Assembly assembly)
        {
            InitializeComponent();

            textBox1.Text = assembly.NTHeader.OptionalHeader.FileOffset.ToString("X8");
            textBox2.Text = "00000500";
        }

        public int StartOffset()
        {
            return int.Parse(textBox1.Text, System.Globalization.NumberStyles.AllowHexSpecifier);
        }
        public int CodeSize()
        {
            return int.Parse(textBox2.Text, System.Globalization.NumberStyles.AllowHexSpecifier);
        }
        public bool VirtualRepresentation()
        {
            return checkBox1.Checked;
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

        private void DisassembleDlg_Load(object sender, EventArgs e)
        {

        }

    }
}
