using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using TUP.AsmResolver;
using TUP.AsmResolver.ASM;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security;

namespace TUP.AsmResolver.PreviewApplication
{
    public partial class Form1 : Form
    {
        Win32Assembly loadedAssembly;
        bool virtualInstructions = false;
        ListViewItem selectedItem;


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64EnableWow64FsRedirection(bool Wow64FsEnableRedirection);


        public Form1()
        {
            InitializeComponent();
        }

        public static string ByteArrayToHexString(byte[] bytes)
        {
            if (bytes == null)
                return "";
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i <= bytes.Length - 1; i++)
            {
                builder.Append(bytes[i].ToString("X2"));
            }
            return builder.ToString();
        }

        private ListViewItem CreateListViewItem(x86Instruction instr)
        {
            ListViewItem itm = new ListViewItem();
            itm.UseItemStyleForSubItems = false;
            if (virtualInstructions)
                itm.Text = instr.Offset.Va.ToString("X8");
            else
                itm.Text = instr.Offset.FileOffset.ToString("X8");


            itm.SubItems.Add(ByteArrayToHexString(instr.OpCode.OpCodeBytes) + " " + ByteArrayToHexString(instr.OperandBytes));
            itm.SubItems.Add(instr.ToAsmString(virtualInstructions));
            itm.SubItems.Add("");

            if (instr.OpCode.OperandType == x86OperandType.InstructionAddress || instr.OpCode.OperandType == x86OperandType.ShortInstructionAddress || instr.OpCode.OperandType == x86OperandType.DwordPtr)
            {
                if (instr.OpCode.Name.StartsWith("CALL"))
                    itm.SubItems[2].BackColor = Color.Cyan;
                if (instr.OpCode.Name.StartsWith("J") || instr.OpCode.Name.StartsWith("LOOP"))
                    itm.SubItems[2].BackColor = Color.Yellow;
            }

            itm.Tag = instr;
            return itm;
        }

        private void AddInstructions(int offset, int size)
        {
            Invoke(new Action(listView1.BeginUpdate));
            
            loadedAssembly.CurrentByteOffset = offset;
            while (loadedAssembly.CurrentByteOffset <= offset + size - 1)
           {
              
               x86Instruction instr = loadedAssembly.Disassembler.DisassembleNextInstruction();
               //Debug.Print(reader.CurrentByteOffset.ToString()  + "/" + reader.FileLength.ToString());


               Invoke(new Action<ListViewItem>(AddRow), CreateListViewItem(instr));
               if (loadedAssembly.CurrentByteOffset <= offset+size)
                    Invoke(new Action<int, int>(SetProgressBarValue), loadedAssembly.CurrentByteOffset, offset + size);

                   
               
           }
            Invoke(new Action(listView1.EndUpdate));
            Invoke(new Action<Control, bool>(SetVisibility), listView1, true);
            Invoke(new Action<Control, bool>(SetVisibility), progressBar1, false);
            Invoke(new Action<Control, bool>(SetVisibility), label1, false);
        }
        
        void AddRow(ListViewItem itm)
        {
            listView1.Items.Add(itm);
        }

        void Analyse()
        {
            for (int i = 0; i <= listView1.Items.Count - 1; i++)
            {
                x86Instruction instr = (x86Instruction)Invoke(new Func<int, object>(GetItemInstructionTag), i);
                ListViewItem itm = (ListViewItem)Invoke(new Func<int, ListViewItem>(GetListViewItem), i);
                
                try
                {
                    if (instr.OpCode.IsBasedOn(x86OpCodes.Call))
                    {
                       // Invoke(new Action<ListViewItem, int, string>(SetListViewSubItemText), itm, columnHeader4.Index, ((Offset)instr.Operand1.Value).ToMethod(loadedAssembly, instr).FullName);
                    }
                    if (instr.OpCode.IsBasedOn(x86OpCodes.Call_DwordPtr))
                        Invoke(new Action<ListViewItem, int, string>(SetListViewSubItemText), itm, columnHeader4.Index, ((Offset)instr.Operand1.Value).ToMethod(loadedAssembly).FullName);
                   // else if (instr.OpCode == x86OpCodes.Mov_DwordPtr_Esp_Dword)
                   //     Invoke(new Action<ListViewItem, int, string>(SetListViewSubItemText), itm, columnHeader4.Index, "ASCII: \"" + Offset.FromVa(Convert.ToUInt64(instr.Operand1), loadedAssembly).ToAsciiString(loadedAssembly) + "\"");
                }
                catch { }
            
                Invoke(new Action<int, int>(SetProgressBarValue), i, listView1.Items.Count - 1);
            }
            Invoke(new Action<Control, bool>(SetVisibility), progressBar1, false);
            Invoke(new Action<Control, bool>(SetVisibility), label1, false);
        }

        ListViewItem GetListViewItem(int index)
        {
            return listView1.Items[index];
        }

        void SetListViewSubItemText(ListViewItem itm, int index, string text)
        {
            itm.SubItems[index].Text = text;
            
        }

        object GetItemInstructionTag(int index)
        {
            return GetListViewItem(index).Tag;
        }



        void SetProgressBarValue(int val, int max)
        {
            progressBar1.Maximum = max;
            progressBar1.Value = val;
        }

        void SetVisibility(Control ctrl, bool value)
        {
            ctrl.Visible = value;
        }






        private void OpenFile(string file)
        {
            listView1.Items.Clear();

            Wow64EnableWow64FsRedirection(false);
            loadedAssembly = Win32Assembly.LoadFile(file, new ReadingArguments() { IgnoreDataDirectoryAmount = true });
            Wow64EnableWow64FsRedirection(true);
            
            Text = "TUP.AsmResolver Preview Application - [" + Path.GetFileName(loadedAssembly.Path) + "]";
            
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {

                    OpenFile(ofd.FileName);

               
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                loadedAssembly.Save(sfd.FileName);
            }
            
        }



        private void Form1_Load(object sender, EventArgs e)
        {

            InstructionCollection instructions = new InstructionCollection();


        }

        private void listView1_CellDoubleClick(object sender, EventArgs e)
        {
           // try 
           // {
           //     Inputbox input = new Inputbox();
           //    // input.checkBox1.Show();
           //    // input.checkBox1.Text = "Allow Overwrite";
           //     input.textBox1.Text = selectedItem.SubItems[2].Text;
           //     int RowIndex = selectedItem.Index;
           //     if (input.ShowDialog() == System.Windows.Forms.DialogResult.OK)
           //     {
           //
           //         Instruction instruction = Instruction.Recognize(input.textBox1.Text, reader, true);
           //
           //         
           //         int Size = selectedItem.SubItems[1].Text.Replace(" ", "").Length / 2;
           //
           //         Instruction currentinstruction = reader.LastReadInstructions.GetInstructionByVirtualOffset(long.Parse(listView1.SelectedItems[0].SubItems[0].Text, System.Globalization.NumberStyles.AllowHexSpecifier));
           //
           //         reader.ASMProcessor.Replace(currentinstruction, instruction, input.checkBox1.Checked);
           //
           //         
           //
           //         for (int i = 0; i < Size ; i++)
           //         {
           //             Instruction instr = reader.LastReadInstructions[RowIndex + i];
           //
           //             ListViewItem itm = new ListViewItem();
           //             itm.UseItemStyleForSubItems = false;
           //             itm.Text = (instr.VirtualOffset.ToString("X8"));
           //             
           //             string code2 = instr.OpCode.Code2.ToString("X2");
           //             string code3 = instr.OpCode.Code3.ToString("X2");
           //             switch (instr.OpCode.OpCodeLength)
           //             {
           //                 case 1:
           //                     code2 = "";
           //                     code3 = "";
           //                     break;
           //                 case 2:
           //                     code3 = "";
           //                     break;
           //                 case 3:
           //                     break;
           //             }
           //
           //             itm.SubItems.Add(instr.OpCode.Code1.ToString("X2") + code2 + code3+ " " + ByteArrayToHexString(instr.OperandBytes));
           //
           //
           //
           //             
           //
           //             itm.SubItems.Add(instr.ToVirtualString().Substring(instr.ToString().IndexOf(": ") + 2));
           //             switch (instr.OpCode.InstructionType)
           //             {
           //                 case InstructionType.Return:
           //                 case InstructionType.CallDwordAddress:
           //                 case InstructionType.CallAddress:
           //                     itm.SubItems[itm.SubItems.Count - 1].BackColor = Color.Cyan;
           //                     break;
           //                 case InstructionType.ShortJumpAddress:
           //                 case InstructionType.JumpAddress:
           //                     itm.SubItems[itm.SubItems.Count - 1].BackColor = Color.Yellow;
           //                     break;
           //                 default:
           //                     break;
           //             }
           //
           //             itm.SubItems.Add("");
           //             itm.Tag = instr;
           //
           //             
           //             listView1.Items.Insert(RowIndex + i, itm);
           //             i += instr.OperandBytes.Length + instr.OpCode.OpCodeLength - 1;
           //         }
           //
           //         selectedItem.Remove();
           //     }
           // }
           // 
           // catch (Exception ex)
           // {
           //     MessageBox.Show(ex.ToString(),"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
           // }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems != null)
                if (listView1.SelectedItems.Count != 0)
                    selectedItem = listView1.SelectedItems[0];
        }


        private void pEInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loadedAssembly != null)
            {
                PE_Information a = new PE_Information(loadedAssembly);
                a.ShowDialog();
            }
        }

        private void loadInstructionsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (loadedAssembly != null)
            {
                DisassembleDlg dlg = new DisassembleDlg(loadedAssembly);
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    virtualInstructions = dlg.VirtualRepresentation();
                    progressBar1.Value = 0;
                    progressBar1.Show();
                    progressBar1.Maximum = dlg.CodeSize();
                    listView1.Items.Clear();
                    label1.Text = "Reading Instructions..."; 
                    label1.Show();
                    new System.Threading.Thread(delegate() { AddInstructions(dlg.StartOffset(), dlg.CodeSize()); }).Start();

                }
            }
        }

        private void analyseInstructionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loadedAssembly != null & listView1.Items.Count != 0)
            {
                progressBar1.Show();
                progressBar1.Value = 0;
                progressBar1.Maximum = listView1.Items.Count - 1;
                label1.Text = "Analysing Instructions...";
                label1.Show();
                new System.Threading.Thread(Analyse).Start();
            }
        }

        

        private void followToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                x86Instruction selectedInstruction = (x86Instruction)listView1.SelectedItems[0].Tag;
                if (selectedInstruction.OpCode.OperandType == x86OperandType.InstructionAddress || selectedInstruction.OpCode.OperandType == x86OperandType.ShortInstructionAddress)
                {
                    Offset targetOffset = (Offset)selectedInstruction.Operand1.Value;
                    foreach (ListViewItem item in listView1.Items)
                        if ((virtualInstructions && item.Text == targetOffset.Va.ToString("X8")) || item.Text == targetOffset.FileOffset.ToString("X8"))
                        {
                            item.EnsureVisible();
                            item.Selected = true;
                            return;
                        }
                }
            }
            catch(Exception ex)
            {
            }
        }

        private void editToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listView1_CellDoubleClick(listView1, null);
        }

        private void editConstantToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
               // Instruction selectedInstruction = (Instruction)listView1.SelectedItems[0].Tag;
               // if (selectedInstruction.OpCode == OpCodes.Mov_DwordPtr_Esp_Dword)
               // {
               //     Offset targetOffset = new Offset((int)selectedInstruction.Operand1,(int)selectedInstruction.Operand1,ASM.ValueType.Normal);
               //     HexDlg dlg = new HexDlg(Encoding.ASCII.GetBytes(targetOffset.ToAsciiString(loadedAssembly)));
               //     if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
               //     {
               //         string newstring = Encoding.ASCII.GetString(dlg.Bytes);
               //         loadedAssembly.Assembler.ReplaceString(selectedInstruction, newstring);
               //         listView1.SelectedItems[0].SubItems[3].Text = "ASCII \"" + newstring + "\"";
               //     }
               //
               // }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void editToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            editToolStripMenuItem1.PerformClick();
        }

        private void editConstantToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            editConstantToolStripMenuItem.PerformClick();
        }

        private void resourcesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ResourcesDlg(loadedAssembly.RootResourceDirectory).ShowDialog();
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;


        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Effect == DragDropEffects.Copy)
            {
                OpenFile(((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString());
            }
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem.PerformClick();
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            saveToolStripMenuItem.PerformClick();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            editToolStripMenuItem2.PerformClick();

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            editConstantToolStripMenuItem.PerformClick();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            pEInformationToolStripMenuItem.PerformClick();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            resourcesToolStripMenuItem.PerformClick();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            loadInstructionsToolStripMenuItem.PerformClick();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            analyseInstructionsToolStripMenuItem.PerformClick();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutDlg().ShowDialog();
        }

        private void listView1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    followToolStripMenuItem.PerformClick();
                    break;
                case Keys.Space:
                    editToolStripMenuItem2.PerformClick();
                    break;
            }
        }

        private void dependencyWalkerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new DependencyWalkerDlg(loadedAssembly).ShowDialog();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            dependencyWalkerToolStripMenuItem.PerformClick();
        }

        private void analyseInstructionToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // Instruction instr = (Instruction)selectedItem.Tag;
           // try
           // {
           //     string analysedoperand = instr.GetAnalysedValue(true);
           //     if (analysedoperand != instr.Operand1)
           //     {
           //
           //         switch (instr.OpCode.InstructionType)
           //         {
           //             case InstructionType.PushDwordAddress:
           //             case InstructionType.MoveDwordAddress:
           //                 Invoke(new Action<ListViewItem, int, string>(SetListViewSubItemText), selectedItem, 3, "ASCII \"" + analysedoperand + "\"");
           //                 break;
           //
           //             default:
           //                 Invoke(new Action<ListViewItem, int, string>(SetListViewSubItemText), selectedItem, 3, analysedoperand);
           //                 break;
           //         }
           //
           //
           //     }
           // }
           //  catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void nopInstructionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            x86Instruction oldInstruction = (x86Instruction)listView1.SelectedItems[0].Tag;
            x86Instruction newInstruction = x86Instruction.Create(loadedAssembly, x86OpCodes.Nop);

            int size = loadedAssembly.Assembler.CalculateSpaceNeeded(oldInstruction, newInstruction);

            loadedAssembly.Assembler.Replace(oldInstruction, newInstruction);
            int itemIndex = listView1.SelectedItems[0].Index;
            
            listView1.SelectedItems[0].Remove();

            loadedAssembly.Disassembler.CurrentOffset = oldInstruction.Offset.FileOffset; 
            for (int i = 0; i < size; i++)
            {
                
                listView1.Items.Insert(itemIndex, CreateListViewItem(loadedAssembly.Disassembler.DisassembleNextInstruction()));
            }
        }







    }

}