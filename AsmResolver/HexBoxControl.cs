using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Be.Windows.Forms;
using TUP.AsmResolver;

namespace AsmResolver
{
    public class HexBoxControl : Control
    {
        TextBox fileOffsetBox;
        TextBox rvaOffsetBox;
        TextBox vaOffsetBox;
        HexBox hexBox;
        Win32Assembly assembly;
        Label targetSectionLabel;
        public HexBoxControl()
        {
            Width = 200;
            Height = 200;

            
            Label fileOffsetLabel = new Label()
            {
                Location = new Point(3, 5),
                Width = 60,
                Text = "File Offset:",
            };
            fileOffsetBox = new TextBox()
            {
                Width = 100,
                Location = new Point(fileOffsetLabel.Left + fileOffsetLabel.Width + 5, 2),
            };
            Label rvaOffsetLabel = new Label()
            {
                Location = new Point(fileOffsetBox.Right + 10, 5),
                Width = 30,
                Text = "RVA:",
            };
            rvaOffsetBox = new TextBox()
            {
                Width = 100,
                Location = new Point(rvaOffsetLabel.Right + 5, 2),
            };
            Label vaOffsetLabel = new Label()
            {
                Location = new Point(rvaOffsetBox.Right + 10, 5),
                Width = 30,
                Text = "VA:",
            };
            vaOffsetBox = new TextBox()
            {
                Width = 100,
                Location = new Point(vaOffsetLabel.Right + 5, 2),
            };
            targetSectionLabel = new Label()
            {
                Text = "Target Section:",
                Location = new Point(vaOffsetBox.Right + 5, 5),
                AutoSize = true,
            };
            hexBox = new HexBox()
            {
                Location = new Point(0, fileOffsetBox.Bottom + 7),
                Width = this.Width,
                Height = this.Height - (fileOffsetBox.Bottom + 7),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                StringViewVisible = true,
                LineInfoVisible = true,
                LineInfoForeColor = Color.Blue,
                UseFixedBytesPerLine = true,
                BytesPerLine = 16,
                VScrollBarVisible = true,
            };
            
            this.Controls.AddRange(new Control[] {
                fileOffsetLabel,
                fileOffsetBox,
                rvaOffsetLabel,
                rvaOffsetBox,
                vaOffsetLabel,
                vaOffsetBox,
                targetSectionLabel,
                hexBox });

            fileOffsetBox.Leave += fileOffsetBox_Leave;
            fileOffsetBox.KeyUp += fileOffsetBox_KeyUp;
            rvaOffsetBox.Leave += rvaOffsetBox_Leave;
            rvaOffsetBox.KeyUp += rvaOffsetBox_KeyUp;
            vaOffsetBox.Leave += vaOffsetBox_Leave;
            vaOffsetBox.KeyUp += vaOffsetBox_KeyUp;
        }

        void vaOffsetBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                vaOffsetBox_Leave(null, null);
        }

        void vaOffsetBox_Leave(object sender, EventArgs e)
        {
            try
            {
                uint vaOffset = uint.Parse(vaOffsetBox.Text, NumberStyles.HexNumber);
                Section targetSection = Section.GetSectionByRva(assembly, (uint)(vaOffset - assembly.NTHeader.OptionalHeader.ImageBase));
                OffsetConverter converter;
                if (targetSection == null)
                {
                    converter = new OffsetConverter(assembly);
                    targetSectionLabel.Text = "Target Section: ";
                }
                else
                {
                    converter = new OffsetConverter(targetSection);
                    targetSectionLabel.Text = "Target Section: " + targetSection.Name;
                }
                uint fileOffset = converter.VaToFileOffset(vaOffset);
                fileOffsetBox.Text = fileOffset.ToString("X8");
                rvaOffsetBox.Text = converter.VaToRva(vaOffset).ToString("X8");
                hexBox.Select(fileOffset, 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void rvaOffsetBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                rvaOffsetBox_Leave(null, null);
        }

        void rvaOffsetBox_Leave(object sender, EventArgs e)
        {
            try
            {
                uint rvaOffset = uint.Parse(rvaOffsetBox.Text, NumberStyles.HexNumber);
                Section targetSection = Section.GetSectionByRva(assembly, rvaOffset);
                OffsetConverter converter;
                if (targetSection == null)
                {
                    converter = new OffsetConverter(assembly);
                    targetSectionLabel.Text = "Target Section: ";
                }
                else
                {
                    converter = new OffsetConverter(targetSection);
                    targetSectionLabel.Text = "Target Section: " + targetSection.Name;
                }
                uint fileOffset = converter.RvaToFileOffset(rvaOffset);
                fileOffsetBox.Text = fileOffset.ToString("X8");
                vaOffsetBox.Text = converter.RvaToVa(rvaOffset).ToString("X8");
                hexBox.Select(fileOffset, 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void fileOffsetBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                fileOffsetBox_Leave(null, null);
        }

        void fileOffsetBox_Leave(object sender, EventArgs e)
        {
            try
            {
                uint fileOffset = uint.Parse(fileOffsetBox.Text, NumberStyles.HexNumber);
                Section targetSection = Section.GetSectionByFileOffset(assembly, fileOffset);
                OffsetConverter converter;
                if (targetSection == null)
                {
                    converter = new OffsetConverter(assembly);
                    targetSectionLabel.Text = "Target Section: ";
                }
                else
                {
                    converter = new OffsetConverter(targetSection);
                    targetSectionLabel.Text = "Target Section: " + targetSection.Name;
                }
                rvaOffsetBox.Text = converter.FileOffsetToRva(fileOffset).ToString("X8");
                vaOffsetBox.Text = converter.FileOffsetToVa(fileOffset).ToString("X8");
                hexBox.Select(fileOffset, 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        
        public void SetByteProvider(Win32Assembly assembly, IByteProvider provider)
        {
            fileOffsetBox.Clear();
            rvaOffsetBox.Clear();
            vaOffsetBox.Clear();
            this.assembly = assembly;
            hexBox.ByteProvider = provider;
        }
    }
}
