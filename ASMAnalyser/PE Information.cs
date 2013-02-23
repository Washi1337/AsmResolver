using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TUP.AsmResolver;

namespace TUP.AsmResolver.PreviewApplication
{
    public partial class PE_Information : Form
    {
        Win32Assembly assembly;
        public PE_Information(Win32Assembly assembly)
        {
            this.assembly = assembly;
            InitializeComponent();
        }

        private void PE_Information_Load(object sender, EventArgs e)
        {

            txtEntry.Text = "0x" + assembly.NTHeader.OptionalHeader.Entrypoint.ToString("x8").ToUpper();
            txtFileOffset.Text = "0x" + assembly.NTHeader.OptionalHeader.FileOffset.ToString("x8").ToUpper();
            txtDate.Text = assembly.NTHeader.FileHeader.CompilingDate.ToShortDateString();
            txtMachine.Text = assembly.NTHeader.FileHeader.Machine.ToString();
            txtSections.Text = assembly.NTHeader.FileHeader.AmountOfSections.ToString();
            txtCharacteristics.Text = assembly.NTHeader.FileHeader.ExecutableFlags.ToString();

            if (assembly.NTHeader.OptionalHeader.Is32Bit)
                txtArch.Text = "32-bit";
            else
                txtArch.Text = "64-bit";

            txtSize.Text = "0x" + (assembly.NTHeader.OptionalHeader.HeaderSize).ToString("x8").ToUpper();
            txtDllChar.Text = assembly.NTHeader.OptionalHeader.LibraryFlags.ToString();
            txtImageBase.Text = "0x" + ((long)assembly.NTHeader.OptionalHeader.ImageBase).ToString("x8").ToUpper();
            txtBaseOfCode.Text = "0x" + ((long)assembly.NTHeader.OptionalHeader.BaseOfCode).ToString("x8").ToUpper();
            txtBaseOfData.Text = "0x" + ((long)assembly.NTHeader.OptionalHeader.BaseOfData).ToString("x8").ToUpper();

            txtSubsystem.Text = assembly.NTHeader.OptionalHeader.SubSystem.ToString().Replace("_", " ");
            txtSubsystemVersion.Text = assembly.NTHeader.OptionalHeader.MinimumSubSystemVersion.ToString();
            txtMinOSVer.Text = assembly.NTHeader.OptionalHeader.MinimumOSVersion.ToString();
            txtLinker.Text = assembly.NTHeader.OptionalHeader.LinkerVersion.ToString();

            txtStopMessage.Text = assembly.MZHeader.StopMessage;

            txtManaged.Text = assembly.NTHeader.IsManagedAssembly.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<DataGridViewColumn> columns = new List<DataGridViewColumn>();
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Library";
            columns.Add(column);
            column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Name";
            columns.Add(column);
            column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Ordinal";
            columns.Add(column);
            column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "RVA";
            columns.Add(column);
            column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Value";
            columns.Add(column);
            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            foreach (LibraryReference lib in assembly.LibraryImports)
            {
                foreach (ImportMethod meth in lib.ImportMethods)
                {
                    
                    DataGridViewRow row = new DataGridViewRow();
                    DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                    cell.Value = lib.LibraryName;
                    row.Cells.Add(cell);
                    cell = new DataGridViewTextBoxCell();

                    cell.Value = meth.Name;
                    if (meth.Name.StartsWith("?"))
                        cell.Value += " -> " + NameDemangler.DemangleVCpp(meth.Name);
              
                    row.Cells.Add(cell);
                    cell = new DataGridViewTextBoxCell();
                    cell.Value = "0x" + ((long)meth.Ordinal).ToString("x8").ToUpper();
                    row.Cells.Add(cell);
                    cell = new DataGridViewTextBoxCell();
                    cell.Value = "0x" + ((long)meth.RVA).ToString("x8").ToUpper();
                    row.Cells.Add(cell);
                    cell = new DataGridViewTextBoxCell();
                    cell.Value = "0x" + ((long)meth.OriginalThunkValue).ToString("x8").ToUpper();
                    row.Cells.Add(cell);
                    
                    rows.Add(row);
                }
            }

            ListDlg dlg = new ListDlg(columns.ToArray(), rows.ToArray(), "Imports", false);
            dlg.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<DataGridViewColumn> columns = new List<DataGridViewColumn>();
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Name";
            columns.Add(column);
            column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "RVA";
            columns.Add(column);
            List<DataGridViewRow> rows = new List<DataGridViewRow>();

            foreach (ExportMethod meth in assembly.LibraryExports)
            {
                DataGridViewRow row = new DataGridViewRow();
                DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                cell.Value = meth.Name;
                row.Cells.Add(cell);
                cell = new DataGridViewTextBoxCell();
                cell.Value = "0x" + ((long)meth.RVA).ToString("x8").ToUpper();
                row.Cells.Add(cell);
                rows.Add(row);
            }
            

            ListDlg dlg = new ListDlg(columns.ToArray(), rows.ToArray(), "Exports",false);
            dlg.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<DataGridViewColumn> columns = new List<DataGridViewColumn>();
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Name";
            columns.Add(column);
            column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Virtual TargetOffset";
            columns.Add(column);
            column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Virtual Size";
            columns.Add(column);
            column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Raw TargetOffset";
            columns.Add(column);
            column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Raw Size";
            columns.Add(column);
            column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Characteristics";
            columns.Add(column);
            
            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            foreach (Section section in assembly.NTHeader.Sections)
            {
                

                DataGridViewRow row = new DataGridViewRow();
                DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                cell.Value = section.Name;
                row.Cells.Add(cell);
                cell = new DataGridViewTextBoxCell();
                cell.Value = "0x" + section.RVA.ToString("x8").ToUpper();
                row.Cells.Add(cell);
                cell = new DataGridViewTextBoxCell();
                cell.Value = "0x" + section.VirtualSize.ToString("x8").ToUpper();
                row.Cells.Add(cell);
                cell = new DataGridViewTextBoxCell();
                cell.Value = "0x" + section.RawOffset.ToString("x8").ToUpper();
                row.Cells.Add(cell);
                cell = new DataGridViewTextBoxCell();
                cell.Value = "0x" + section.RawSize.ToString("x8").ToUpper();
                row.Cells.Add(cell);
                cell = new DataGridViewTextBoxCell();
                cell.Value = section.Flags.ToString();
                row.Cells.Add(cell);
                row.Tag = section;
                rows.Add(row);

            }
            ListDlg dlg = new ListDlg(columns.ToArray(), rows.ToArray(), "Sections", true);
            dlg.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Inputbox input = new Inputbox();
            input.Text = "Edit MS DOS Stop message";
            input.textBox1.MaxLength = MZHeader.MaxStopMessageLength;
            input.textBox1.Text = txtStopMessage.Text;
            if (input.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                assembly.MZHeader.StopMessage = input.textBox1.Text;
                txtStopMessage.Text = input.textBox1.Text;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FlagsDlg<LibraryFlags> dlg = new FlagsDlg<LibraryFlags>(assembly.NTHeader.OptionalHeader.LibraryFlags);
            dlg.Text = "Edit Library Flags";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LibraryFlags flags = (LibraryFlags)dlg.GetRawFlagValue();
                assembly.NTHeader.OptionalHeader.LibraryFlags = flags;
                txtDllChar.Text = flags.ToString();

            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            FlagsDlg<ExecutableFlags> dlg = new FlagsDlg<ExecutableFlags>(assembly.NTHeader.FileHeader.ExecutableFlags);
            dlg.Text = "Edit Executable Flags";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ExecutableFlags flags = (ExecutableFlags)dlg.GetRawFlagValue();
                assembly.NTHeader.FileHeader.ExecutableFlags = flags;
                txtCharacteristics.Text = flags.ToString();

            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ComboBoxDlg<SubSystem> dlg = new ComboBoxDlg<SubSystem>(assembly.NTHeader.OptionalHeader.SubSystem);
    
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SubSystem flags = (SubSystem)dlg.GetRawFlagValue();
                assembly.NTHeader.OptionalHeader.SubSystem = flags;
                txtSubsystem.Text = flags.ToString();

            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            NumberDlg dlg = new NumberDlg(long.Parse(txtEntry.Text.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier));
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                uint value = (uint)dlg.GetValue();
                assembly.NTHeader.OptionalHeader.Entrypoint = value;
                txtEntry.Text = "0x" + value.ToString("x8");
                txtFileOffset.Text = "0x" + assembly.NTHeader.OptionalHeader.FileOffset.ToString("x8");
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            new NetHeaderDlg(assembly.NETHeader).ShowDialog();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ComboBoxDlg<Machine> dlg = new ComboBoxDlg<Machine>(assembly.NTHeader.FileHeader.Machine);

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Machine flags = (Machine)dlg.GetRawFlagValue();
                assembly.NTHeader.FileHeader.Machine = flags;
                txtMachine.Text = flags.ToString();

            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            NumberDlg dlg = new NumberDlg(assembly.NTHeader.OptionalHeader.FileOffset);
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                uint fileoffset = (uint)dlg.GetValue();
                
                uint entrypoint = new OffsetConverter(Section.GetSectionByFileOffset(assembly, fileoffset)).FileOffsetToRva(fileoffset);
                assembly.NTHeader.OptionalHeader.Entrypoint = entrypoint;
                txtFileOffset.Text = fileoffset.ToString("X8");
                txtEntry.Text = entrypoint.ToString("X8");
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            List<DataGridViewColumn> columns = new List<DataGridViewColumn>();
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Name";
            columns.Add(column);
            column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Offset";
            columns.Add(column);
            column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Size";
            columns.Add(column);
            column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.HeaderText = "Section";
            columns.Add(column);

            
            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            foreach (DataDirectory dataDir in assembly.NTHeader.OptionalHeader.DataDirectories)
            {
                DataGridViewRow row = new DataGridViewRow();
                DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                cell.Value = dataDir.Name.ToString();
                row.Cells.Add(cell);
                cell = new DataGridViewTextBoxCell();
                cell.Value = dataDir.TargetOffset.FileOffset.ToString("X8");
                row.Cells.Add(cell);
                cell = new DataGridViewTextBoxCell();
                cell.Value = dataDir.Size.ToString("X8");
                row.Cells.Add(cell);
                cell = new DataGridViewTextBoxCell();
                cell.Value = dataDir.Section != null ? dataDir.Section.Name : "";
                row.Cells.Add(cell);
                rows.Add(row);
            }

            ListDlg dlg = new ListDlg(columns.ToArray(), rows.ToArray(), "Data Directories", false);
            dlg.Show();
        }


    }
}
