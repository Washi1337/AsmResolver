using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TUP.AsmResolver;

namespace AsmResolver
{
    public partial class OffsetEditorDlg : Form
    {
        Offset offset;
        Offset original;
        Win32Assembly assembly;
        public OffsetEditorDlg(Offset offset, Win32Assembly assembly)
        {
            InitializeComponent();
            this.offset = offset;
            this.original = offset;
            this.assembly = assembly;
            RefreshTextBoxes();
        }

        private void RefreshTextBoxes()
        {
            textBox1.Text = offset.FileOffset.ToString("X8");
            textBox2.Text = offset.Rva.ToString("X8");
            textBox3.Text = offset.Va.ToString("X16");
            Section targetSection = Section.GetSectionByFileOffset(assembly, offset.FileOffset);
            if (targetSection == null)
                textBox4.Text = "";
            else
                textBox4.Text = targetSection.Name;
        }

        public Offset Offset
        {
            get { return offset; }
        }

        private void OffsetEditorDlg_Load(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            offset = original;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();

        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            try
            {
                offset = Offset.FromFileOffset(uint.Parse(textBox1.Text, NumberStyles.HexNumber), assembly);
                RefreshTextBoxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox1.Text = "00000000";
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                textBox1_Leave(null, null);
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            try
            {
                offset = Offset.FromRva(uint.Parse(textBox2.Text, NumberStyles.HexNumber), assembly);
                RefreshTextBoxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox2.Text = "00000000";
            }
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                textBox2_Leave(null, null);
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            try
            {
                offset = Offset.FromVa(ulong.Parse(textBox3.Text, NumberStyles.HexNumber), assembly);
                RefreshTextBoxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox3.Text = "0000000000000000";
            }
        }

        private void textBox3_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                textBox3_Leave(null, null);
        }
    }

    public class OffsetUIEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            Win32Assembly assembly = null;
            if (context.Instance is IHeader)
                assembly = (context.Instance as IHeader).ParentAssembly;
            else if (context.Instance is DataDirectory)
                assembly = (context.Instance as DataDirectory).Section.ParentAssembly;
            
            OffsetEditorDlg dlg = new OffsetEditorDlg(value as Offset, assembly);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                value = dlg.Offset;
            }
            return value;
        }

    }
}
