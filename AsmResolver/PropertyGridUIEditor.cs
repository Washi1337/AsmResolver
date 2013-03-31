using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsmResolver
{
    public class PropertyGridUIEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {

            PropertyGridDlg dlg = new PropertyGridDlg(value);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                value = dlg.Object;
            }
            return value;
        }

    }
}
