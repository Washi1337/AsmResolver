using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Be.Windows.Forms;
using TUP.AsmResolver;
using TUP.AsmResolver.NET;

namespace AsmResolver
{
    public static class TreeBuilder
    {
        public static TreeNode ConstructAssemblyNode(Win32Assembly assembly)
        {
            TreeNode assemblyNode = CreateNode(assembly.Path.Substring(assembly.Path.LastIndexOf('\\') + 1), assembly);
            
            assemblyNode.Nodes.AddRange(new TreeNode[] {
                CreateNode("Dos Header", assembly.MZHeader),
                CreateNode("NT Header", assembly.NTHeader).AddSubNodes(new TreeNode[] {
                    CreateNode("File Header", assembly.NTHeader.FileHeader),
                    CreateNode("Optional Header", assembly.NTHeader.OptionalHeader).AddSubNodes(new TreeNode[] {
                        CreateArrayNode("Data Directories", assembly.NTHeader.OptionalHeader.DataDirectories, (obj) => { return ((DataDirectory)obj).Name.ToString(); }),
                    })
                    
                }),
                CreateArrayNode("Sections", assembly.NTHeader.Sections.ToArray(), (obj) => { return ((Section)obj).Name; }),

                CreateArrayNode("Export Directory", assembly.LibraryExports.ToArray()),
                CreateArrayNode("Import Directory", assembly.LibraryImports.ToArray(), (obj) => { return ((LibraryReference)obj).LibraryName; }).AddForEachNode((tag) => { return ((LibraryReference)tag.Object).ImportMethods; }, (obj) => { return ((ImportMethod)obj).Name;}),
                CreateNode("Resource Directory", assembly.RootResourceDirectory, TreeNodeType.ResourcesTree),

                CreateNode(".NET Directory", assembly.NETHeader, (netHeader)=> { return assembly.NTHeader.IsManagedAssembly; }).AddSubNodes(new TreeNode[] {
                    CreateNode("MetaData Header", assembly.NETHeader.MetaDataHeader), 
                    CreateArrayNode("MetaData Streams", (assembly.NETHeader.MetaDataStreams != null ? assembly.NETHeader.MetaDataStreams.ToArray() : null), (obj) => { return ((MetaDataStream)obj).Name; }).AddForEachNode((tag) => { return (tag.Object is TablesHeap ? tag.Object : null); } , (obj) => { return "Tables";}, TreeNodeType.TablesTree)

                }), 
                CreateNode("Hex Editor", new DynamicFileByteProvider(assembly.Image.Stream), TreeNodeType.HexBox),
                CreateNode("Disassembler", assembly.Disassembler, TreeNodeType.Disassembler),
            });

            return assemblyNode;
        }


        private static TreeNode CreateNode(string name, object value)
        {
            return CreateNode(name, value, TreeNodeType.PropertyGrid);
        }
        private static TreeNode CreateNode(string name, object value, Func<object, bool> condition)
        {
            return CreateNode(name, value, TreeNodeType.PropertyGrid, condition);
        }
        private static TreeNode CreateNode(string name, object value, TreeNodeType type)
        {
            return new TreeNode(name) { Tag = new TreeNodeTag() { Object = value, Type = type } };
        }
        private static TreeNode CreateNode(string name, object value, TreeNodeType type, Func<object, bool> condition)
        {
            TreeNode node = new TreeNode(name);
            if (condition(value))
                node.Tag = new TreeNodeTag() { Object = value, Type = type };
            return node;
        }

        private static TreeNode CreateArrayNode(string name, object[] array)
        {
            return CreateArrayNode(name, array, (obj) => { return obj.ToString(); });
        }

        private static TreeNode CreateArrayNode(string name, object[] array, Func<object, string> toStringFunc)
        {
            TreeNode node = new TreeNode(name);
            if (array != null)
            {
                node.Tag = new TreeNodeTag() { Type = TreeNodeType.PropertyGrid, Object = array };
                node.AddSubNodes(array, toStringFunc);
            }
            return node;
        }

        public static TreeNode AddSubNodes(this TreeNode parent, TreeNode[] array)
        {
            if (parent.Tag != null && array != null)
                parent.Nodes.AddRange(array);
            return parent;
        }

        public static TreeNode AddSubNodes(this TreeNode parent, object[] array)
        {
            return AddSubNodes(parent, array, () => { return true; }, (obj) => { return obj.ToString(); });
        }

        public static TreeNode AddSubNodes(this TreeNode parent, object[] array, Func<object,string> toStringFunc)
        {
            return AddSubNodes(parent, array, () => { return true; }, toStringFunc);
        }

        public static TreeNode AddSubNodes(this TreeNode parent, object[] array, Func<bool> condition)
        {
            return AddSubNodes(parent,array, condition, (obj) => { return obj.ToString(); });
        }

        public static TreeNode AddSubNodes(this TreeNode parent, object[] array, Func<bool> condition,  Func<object,string> toStringFunc)
        {
            if (parent.Tag != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    string strValue = i.ToString() + " - ";
                    try
                    {
                        strValue += toStringFunc(array[i]);
                    }
                    catch (Exception ex) { strValue += ex.Message; }

                    parent.Nodes.Add(CreateNode(strValue, array[i]));
                }
            }
            return parent;
        }

        public static TreeNode AddForEachNode(this TreeNode parent, Func<TreeNodeTag, object> valuesToAdd)
        {
            return AddForEachNode(parent, valuesToAdd, (obj) => { return obj.ToString(); });
        }

        public static TreeNode AddForEachNode(this TreeNode parent, Func<TreeNodeTag, object> valuesToAdd, Func<object, string> toStringFunc)
        {
            return AddForEachNode(parent, valuesToAdd, toStringFunc, TreeNodeType.PropertyGrid);
        }

        public static TreeNode AddForEachNode(this TreeNode parent, Func<TreeNodeTag, object> valuesToAdd, Func<object, string> toStringFunc, TreeNodeType type)
        {
            foreach (TreeNode node in parent.Nodes)
            {
                object rawValue = valuesToAdd((TreeNodeTag)node.Tag);
                if (rawValue == null)
                    continue;
                if (rawValue is Array)
                    node.AddSubNodes((object[])rawValue, toStringFunc).MakeOtherType((o) => { return true; } , type);
                else
                    node.AddSubNodes(new object[] { rawValue }, toStringFunc).MakeOtherType((o) => { return true; }, type);

            }
            return parent;
        }

        public static TreeNode MakeOtherType(this TreeNode parent, Func<object,bool> condition, TreeNodeType newType)
        {
            foreach (TreeNode node in parent.Nodes)
            {
                TreeNodeTag tag = node.Tag as TreeNodeTag;
                if (tag != null)
                {
                    if (condition(tag.Object))
                        tag.Type = newType;
                    
                }
            }
            return parent;
        }
    }
}
