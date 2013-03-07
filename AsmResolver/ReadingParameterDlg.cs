using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TUP.AsmResolver;

namespace AsmResolver
{
    public partial class ReadingParameterDlg : Form
    {
        public ReadingParameterDlg()
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = new ReadingParameters();
        }

        public ReadingParameters Parameters
        {
            get { return propertyGrid1.SelectedObject as ReadingParameters; }
        }

        private void ReadingParameterDlg_Load(object sender, EventArgs e)
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
