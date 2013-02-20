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
    public partial class NumberDlg : Form
    {
        public NumberDlg(long value)
        {
            InitializeComponent();
            textBox1.Text = value.ToString();
        }
        public long GetValue()
        {
            return long.Parse(textBox1.Text);
        }

        string keys = new string(new char[] {((char)Keys.ShiftKey), ((char)Keys.Back), ((char)Keys.Up), ((char)Keys.Down), ((char)Keys.Left), ((char)Keys.Right), ((char)Keys.Home), ((char)Keys.End)});
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            string supported = "1234567890" + keys;
            if (!supported.Contains((char)e.KeyCode))
                e.SuppressKeyPress = true;
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {

            string supported = "1234567890ABCDEF" + keys;
            if (!supported.Contains((char)e.KeyCode))
                e.SuppressKeyPress = true;
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {

            string supported = "12345670" + keys;
            if (!supported.Contains((char)e.KeyCode))
                e.SuppressKeyPress = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                long value = long.Parse(textBox1.Text);

                textBox2.Text = value.ToString("x");
                textBox3.Text = Convert.ToString(value, 8);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                long value = long.Parse(textBox2.Text, System.Globalization.NumberStyles.AllowHexSpecifier);

                textBox1.Text = value.ToString();
                textBox3.Text = Convert.ToString(value, 8);
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text != "")
            {
                long DecimalNumber = 0;
                long OctalNumber;
                int power = 1;

                OctalNumber = Convert.ToInt64(textBox3.Text);

                while (OctalNumber > 0)
                {
                    DecimalNumber += OctalNumber % 10 * power;
                    OctalNumber = OctalNumber / 10;
                    power = power * 8;
                }

                textBox1.Text = DecimalNumber.ToString();
                textBox2.Text = DecimalNumber.ToString("x");
            }

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
