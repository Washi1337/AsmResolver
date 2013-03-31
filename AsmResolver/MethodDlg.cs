using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TUP.AsmResolver.NET.Specialized.MSIL;

namespace AsmResolver
{
    public partial class MethodDlg : Form
    {
        MethodBody body;
        MSILDisassembler disassembler;
        public MethodDlg(MethodBody body)
        {
            InitializeComponent();
            this.body = body;
            this.propertyGrid1.SelectedObject = body;
            this.disassembler = new MSILDisassembler(body);
            this.Text = "Method Body of " + body.Method.MetaDataToken.ToString("X8");

            try
            {
                this.Text += " (" + body.Method.ToString() + ")";
            }
            catch
            {
                this.Text += " (method.ToString() failed)";
            }

            MSILInstruction[] instructions = disassembler.Disassemble();
            foreach (MSILInstruction instruction in instructions)
            {
                ListViewItem item = new ListViewItem(new string[] {
                    "IL_" + instruction.Offset.ToString("X4"),
                    BytesToString(instruction.OpCode.Bytes) + " " + BytesToString(instruction.OperandBytes),
                    instruction.OpCode.Name,
                    instruction.GetOperandString(),});
                listView1.Items.Add(item);
            }

            if (body.Variables != null && body.Variables.Length > 0)
            {
                foreach (VariableDefinition variable in body.Variables)
                    listView2.Items.Add(new ListViewItem(new string[] { 
                    variable.Index.ToString(),
                    variable.VariableType.FullName,
                }));
            }
            if (body.HasExtraSections)
            {
                foreach (MethodBodySection section in body.ExtraSections)
                    if (section.IsExceptionHandler && section.ExceptionHandlers.Length > 0)
                        foreach (ExceptionHandler handler in section.ExceptionHandlers)
                            listView3.Items.Add(new ListViewItem(new string[] {
                                handler.Type == ExceptionHandlerType.Catch ? "Catch -> " + handler.CatchType.FullName : handler.Type.ToString(),
                                handler.TryStart.ToString("X4"),
                                handler.TryEnd.ToString("X4"), 
                                handler.HandlerStart.ToString("X4"),
                                handler.HandlerEnd.ToString("X4"),
                                handler.FilterStart.ToString("X4")
                            }));
            }
        }        
            
        string BytesToString(byte[] bytes)
        {
            if (bytes == null)
                return "";
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
                builder.Append(b.ToString("X2"));
            return builder.ToString();
        }

        private void MethodDlg_Load(object sender, EventArgs e)
        {

        }

    }
}
