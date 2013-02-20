using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace TUP.AsmResolver.PreviewApplication
{
    public partial class FlagsDlg<T> : Form
    {
        public FlagsDlg(T value)
        {
            InitializeComponent();
            int val = Convert.ToInt32(value);
            foreach (FieldInfo nfo in typeof(T).GetFields())
                if (nfo.FieldType.FullName == typeof(T).FullName)
                {
                    bool ischecked = (val & Convert.ToInt32(nfo.GetValue(null))) == Convert.ToInt32(nfo.GetValue(null));
                    checkedListBox1.Items.Add(new FlagItem<T>(nfo.Name, (T)nfo.GetValue(null)), ischecked);
                }
        }


        public int GetRawFlagValue()
        {
            int val = 0;
            foreach (FlagItem<T> flag in checkedListBox1.CheckedItems)
            {
                val = (val | Convert.ToInt32(flag.Value));
            }
            return val;
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
