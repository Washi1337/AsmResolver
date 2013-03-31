using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TUP.AsmResolver;
using TUP.AsmResolver.ASM;
using TUP.AsmResolver.NET.Specialized.MSIL;

namespace AsmResolver
{
    public class DisassemblerControl : Control
    {
        List<ListViewItem> items = new List<ListViewItem>();
        uint startOffset = 0;
        uint endOffset = 0;
        Win32Assembly assembly;
        x86Disassembler x86disassembler;
        TextBox offsetBox;
        TextBox sizeBox;
        ListView disassemblyView;
        Button disassembleButton;
        Button analyseButton;
        ProgressBar progressBar;

        public DisassemblerControl()
        {
            Width = 100;
            Height = 100;
            Label alphaLabel = new Label()
            {
                Text = "Disassembler is in pre-pre-alpha stage. Don't expect perfect results.",
                Location = new Point(3,5),
                Height = 20,
                Width = 400,
            };

            Label offsetLabel = new Label()
            {
                Location = new Point(3, 30),
                Width = 50,
                Text = "Offset:",
            };
            offsetBox = new TextBox()
            {
                Width = 100,
                Location = new Point(offsetLabel.Left + offsetLabel.Width + 5, offsetLabel.Top - 3),
            };
            Label sizeLabel = new Label()
            {
                Location = new Point(offsetBox.Right + 10, 30),
                Width = 50,
                Text = "Size:",
            };
            sizeBox = new TextBox()
            {
                Width = 100,
                Location = new Point(sizeLabel.Right + 5, sizeLabel.Top - 3),
            };
            disassembleButton = new Button()
            {
                Text = "Disassemble",
                Location = new Point(sizeBox.Right + 10, sizeBox.Top - 1),
            };
            analyseButton = new Button()
            {
                Text = "Analyse",
                Location = new Point(disassembleButton.Right + 5, disassembleButton.Top),
            };
            progressBar = new ProgressBar()
            {
                Width = 10,
                Height = 15,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Visible = false,
            };
            progressBar.Location = new Point(this.Width / 2 - progressBar.Width / 2, this.Height / 2 - progressBar.Height / 2);

            disassemblyView = new ListView()
            {
                Location = new Point(0, sizeBox.Bottom + 7),
                Width = this.Width,
                Height = this.Height - (sizeBox.Bottom + 7),
                View = View.Details,
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom,
                FullRowSelect = true,
                UseCompatibleStateImageBehavior = false,
            };

            disassemblyView.Columns.AddRange(new ColumnHeader[] {
                new ColumnHeader() { Text = "Offset", Width = 75,},
                new ColumnHeader() { Text = "Bytes" , Width = 100,},
                new ColumnHeader() { Text = "Disassembly", Width = 200,},
                new ColumnHeader() { Text = "Comment", Width = 100},
            });

            disassembleButton.Click += disassembleButton_Click;
            analyseButton.Click += analyseButton_Click;
            this.Controls.AddRange(new Control[] {
                alphaLabel,
                offsetLabel,
                offsetBox,
                sizeLabel,
                sizeBox,
                disassembleButton,
                analyseButton,
                disassemblyView,
                progressBar,

            });
            progressBar.BringToFront();
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

        void disassembleButton_Click(object sender, EventArgs e)
        {
            try
            {
                startOffset = uint.Parse(offsetBox.Text, NumberStyles.HexNumber);
                endOffset = startOffset + uint.Parse(sizeBox.Text, NumberStyles.HexNumber);
            }
            catch
            {
                MessageBox.Show("Please enter a valid offset and size");
                return;
            }
            progressBar.Value = 0;
            progressBar.Show();
            new Action(Disassemble).BeginInvoke(DisassembleCallBack, null);
        }

        void analyseButton_Click(object sender, EventArgs e)
        {
            Analyse(disassemblyView.Items);
        }

        void Disassemble()
        {
            items.Clear();
            x86disassembler.CurrentOffset = startOffset;
            try
            {
                //uint lastOffset = 0;
                while (x86disassembler.CurrentOffset < endOffset)
                {
                    // Debugging purposes:
                    //if (lastOffset == x86disassembler.CurrentOffset)
                    //    System.Diagnostics.Debugger.Break();
                    //lastOffset = x86disassembler.CurrentOffset;

                    x86Instruction instruction = x86disassembler.DisassembleNextInstruction();
               
                    
                    ListViewItem item = new ListViewItem(new string[] { 
                        instruction.Offset.FileOffset.ToString("X8"),
                        BytesToString(instruction.OpCode.OpCodeBytes) + " " + BytesToString(instruction.OperandBytes),
                        instruction.ToAsmString().ToLower(),
                        string.Empty,
                    }) { Tag = instruction };

                    item.UseItemStyleForSubItems = false;
                    if (instruction.OpCode.Name.StartsWith("CALL"))
                        item.SubItems[2].BackColor = Color.Cyan;
                    if (instruction.OpCode.Name.StartsWith("J"))
                        item.SubItems[2].BackColor = Color.Yellow;

                    items.Add(item);

                    Invoke(new Action(() =>
                    {
                        double currentValue = x86disassembler.CurrentOffset - startOffset;
                        double max = endOffset - startOffset;
                    
                        progressBar.Value = (int)(currentValue / max * 100);
                    
                    }));
                }
            }
            catch (Exception ex)
            {
                items.Add(new ListViewItem(new string[] {
                    x86disassembler.CurrentOffset.ToString("X8"),
                    "Error",
                    ex.GetType().FullName,
                    ex.Message,
                    ex.ToString(),
                }));
            }

        }

        void DisassembleCallBack(IAsyncResult ar)
        {
            Invoke(new Action(() => {
                progressBar.Hide();
                disassemblyView.Items.Clear();
                disassemblyView.Items.AddRange(items.ToArray());
            }));
        }

        void Analyse(ListView.ListViewItemCollection items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                x86Instruction instruction = items[i].Tag as x86Instruction;
                if (instruction != null)
                {
                    try
                    {
                        if (instruction.OpCode.IsBasedOn(x86OpCodes.Call_DwordPtr))
                        {
                            items[i].SubItems[disassemblyView.Columns.Count - 1].Text = ((Offset)instruction.Operand1.Value).ToMethod(assembly).FullName;
                        }
                    }
                    catch
                    {

                    }
                }
            }

        }

        public void SetAssembly(Win32Assembly assembly)
        {
            this.assembly = assembly;
            x86disassembler = assembly.Disassembler;
        }

        public void DisassembleSection(Section section)
        {
            SetAssembly(section.ParentAssembly);
            offsetBox.Text = section.RawOffset.ToString("X8");
            sizeBox.Text = (section.RawSize > 0x500) ? 0x500.ToString("X8") : section.RawSize.ToString("X8");
            
        }
    }
}
