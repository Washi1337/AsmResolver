using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TUP.AsmResolver.NET.Specialized.MSIL;

namespace TUP.AsmResolver.PreviewApplication
{
    public partial class MethodBodyDlg : Form
    {
        MSILAssembler assembler;
        MSILDisassembler disassembler;
        public MethodBodyDlg(MethodBody body)
        {
            InitializeComponent();
            textBox1.Text = body.IsFat.ToString();
            textBox2.Text = body.HeaderSize.ToString("X8");
            textBox3.Text = body.CodeSize.ToString("X8");
            textBox4.Text = body.MaxStack.ToString("X4");
            textBox5.Text = body.InitLocals.ToString();
            textBox6.Text = body.LocalVarSig.ToString("X8");

            disassembler = new MSILDisassembler(body);
            assembler = new MSILAssembler(body);

            MSILInstruction[] instructions = disassembler.Disassemble();
            foreach (MSILInstruction instruction in instructions)
            {
                listView1.Items.Add(CreateItem(instruction));

            }

            if (body.Variables != null)
            {
                foreach (VariableDefinition variable in body.Variables)
                    listView2.Items.Add(new ListViewItem(new string[] { variable.Index.ToString(), variable.VariableType.ToString() }));
            }
            if (body.HasExtraSections)
            {
                foreach (MethodBodySection section in body.ExtraSections)
                {
                    foreach (ExceptionHandler handler in section.ExceptionHandlers)
                        listView3.Items.Add(new ListViewItem(new string[] { handler.Type.ToString(), handler.TryStart.ToString("X4"), handler.TryEnd.ToString("X4"), handler.HandlerStart.ToString("X4"), handler.HandlerEnd.ToString("X4") , handler.CatchType != null ? handler.CatchType.FullName : ""}));
                }
            }
        }

        private void MethodBodyDlg_Load(object sender, EventArgs e)
        {

        }

        private void nopInstructionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems != null && listView1.SelectedItems.Count > 0)
            {
                for (int i = 0; i < listView1.SelectedItems.Count; i++)
                {
                    MSILInstruction target = listView1.SelectedItems[i].Tag as MSILInstruction;
                    assembler.Replace(target, MSILInstruction.Create(MSILOpCodes.Nop, null));
                    UpdateInstructions(listView1.SelectedItems[i].Index, target);
                }
            }
        }
        private ListViewItem CreateItem(MSILInstruction instruction)
        {
            return new ListViewItem(new string[] { "IL_" + instruction.Offset.ToString("X4"), instruction.OpCode.Name, instruction.GetOperandString() }) { Tag = instruction };
        }
        private void UpdateInstructions(int itemIndex, MSILInstruction originalInstruction)
        {
            
            listView1.Items.RemoveAt(itemIndex);
            MSILInstruction[] instructions = disassembler.Disassemble(originalInstruction.Offset, originalInstruction.Size);
            for (int i = 0; i < instructions.Length; i++)
            {
                listView1.Items.Insert(itemIndex + i, CreateItem(instructions[i]));
            }
        }
    }
}
