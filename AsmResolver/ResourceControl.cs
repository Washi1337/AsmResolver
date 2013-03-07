using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Be.Windows.Forms;
using TUP.AsmResolver;
namespace AsmResolver
{
    public class ResourceControl : Control
    {
        TreeView resourcesTree;
        PropertyGrid propertyGrid;
        HexBox hexBox;
        Stream currentStream;

        public ResourceControl()
        {
            SplitContainer mainContainer = new SplitContainer()
            {
                Dock = DockStyle.Fill,
            };
            SplitContainer treeSplitter = new SplitContainer()
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 400,
            };
            resourcesTree = new TreeView()
            {
                Dock = DockStyle.Fill,
            };
            propertyGrid = new PropertyGrid()
            {
                Dock = DockStyle.Fill,
                HelpVisible = false,

            };
            hexBox = new HexBox()
            {
                Dock = DockStyle.Fill,
                StringViewVisible = true,
                LineInfoVisible = true,
                LineInfoForeColor = Color.Blue,
                UseFixedBytesPerLine = true,
                BytesPerLine = 16,
                VScrollBarVisible = true,
            };
            resourcesTree.AfterSelect += resourcesTree_AfterSelect;

            treeSplitter.Panel1.Controls.Add(resourcesTree);
            treeSplitter.Panel2.Controls.Add(propertyGrid);
            mainContainer.Panel1.Controls.Add(treeSplitter);
            mainContainer.Panel2.Controls.Add(hexBox);
            this.Controls.Add(mainContainer);
        }


        public void SetRootDirectory(ResourceDirectory rootDir)
        {
            resourcesTree.Nodes.Clear();
            TreeNode rootDirectoryNode = new TreeNode("Root Resource Directory");
            rootDirectoryNode.Tag = rootDir;
            PopulateDirectory(rootDirectoryNode, rootDir);
            resourcesTree.Nodes.Add(rootDirectoryNode);
        }
        private void PopulateDirectory(TreeNode parentNode, ResourceDirectory directory)
        {
            if (directory.ChildEntries != null)
            {
                for (int i = 0; i < directory.ChildEntries.Length; i++)
                {
                    TreeNode node = new TreeNode("Resource Directory Entry - ID:" + directory.ChildEntries[i].Name);
                    node.Tag = directory.ChildEntries[i];
                    parentNode.Nodes.Add(node);
                }
            }
        }

        private void PopulateDirectoryEntry(TreeNode parentNode, ResourceDirectoryEntry entry)
        {
            TreeNode node = new TreeNode();
            if (entry.IsEntryToData)
            {
                node.Text = "Resource Data Entry";
                node.Tag = entry.DataEntry;
            }
            else
            {
                node.Text = "Resource Directory";
                node.Tag = entry.Directory;
                PopulateDirectory(node, entry.Directory);
            }
            parentNode.Nodes.Add(node);
        }

        void resourcesTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            propertyGrid.SelectedObject = e.Node.Tag;
            if (e.Node.Nodes.Count == 0)
            {
                if (e.Node.Tag is ResourceDirectory)
                    PopulateDirectory(e.Node, e.Node.Tag as ResourceDirectory);
                if (e.Node.Tag is ResourceDirectoryEntry)
                    PopulateDirectoryEntry(e.Node, e.Node.Tag as ResourceDirectoryEntry);
            }
            if (e.Node.Tag is ResourceDataEntry)
            {
                try
                {
                    if (currentStream != null)
                        currentStream.Dispose();
                    currentStream = (e.Node.Tag as ResourceDataEntry).GetStream();
                    hexBox.ByteProvider = new DynamicFileByteProvider(currentStream);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                hexBox.ByteProvider = null;
        }
    }

}
