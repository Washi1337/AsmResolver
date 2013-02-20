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
    public partial class ComboBoxDlg<T> : Form
    {
        public ComboBoxDlg(T value)
        {
            InitializeComponent();

            foreach (FieldInfo nfo in typeof(T).GetFields())
                if (nfo.FieldType.FullName == typeof(T).FullName)
                {
                    comboBox1.Items.Add(new FlagItem<T>(nfo.Name, (T)nfo.GetValue(null)));
                    if (value.ToString() == nfo.GetValue(null).ToString())
                        comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
                }
            
            

        }

        public int GetRawFlagValue()
        {
            return Convert.ToInt32(((FlagItem<T>)comboBox1.SelectedItem).Value);
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
