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
    public partial class PropertyDlg : Form
    {
        public PropertyDlg(object obj)
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = obj;
        }

        private void PropertyDlg_Load(object sender, EventArgs e)
        {

        }
    }
}
