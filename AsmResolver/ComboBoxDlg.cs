using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsmResolver
{
    public partial class ComboBoxDlg : Form
    {
        public ComboBoxDlg(string text, string caption, object[] items)
        {
            InitializeComponent();
            label1.Text = text;
            Text = caption;
            comboBox1.Items.AddRange(items);
        }

        public object SelectedObject
        {
            get { return comboBox1.SelectedItem; }
        }

        private void ComboboxDlg_Load(object sender, EventArgs e)
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
    }
}
