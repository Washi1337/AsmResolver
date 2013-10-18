using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TUP.AsmResolver.NET.Specialized;
using TUP.AsmResolver.NET.Specialized.MSIL;
using TUP.AsmResolver.PE;
namespace TUP.AsmResolver.NET
{
    /// <summary>
    /// Represents the blob heap stream containing various values of many metadata Members.
    /// </summary>
    public class BlobHeap : MetaDataStream
    {
        internal SortedDictionary<uint, byte[]> _readBlobs = new SortedDictionary<uint, byte[]>();
        
        internal BlobHeap(NETHeader netheader, int headeroffset, Structures.METADATA_STREAM_HEADER rawHeader, string name)
            : base(netheader, headeroffset, rawHeader, name)
        {
        }

        internal override void Initialize()
        {
        }

        internal void Reconstruct()
        {
            // will be removed once blobs are being serialized.

            MemoryStream newStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(newStream);
            writer.Write((byte)0);
            ReadAllBlobs();
        
            foreach (var blob in _readBlobs)
            {
                NETGlobals.WriteCompressedUInt32(writer, (uint)blob.Value.Length);
                writer.Write(blob.Value);
            }
        
            _mainStream.Dispose();
            _binReader.Dispose();
            _binWriter.Dispose();
            _mainStream = newStream;
            _binReader = new BinaryReader(newStream);
            _binWriter = new BinaryWriter(newStream);
            this._streamHeader.Size = (uint)newStream.Length;
        }

        internal void ReadAllBlobs()
        {
            _mainStream.Seek(1, SeekOrigin.Begin);
            while (_mainStream.Position < _mainStream.Length)
            {
                bool alreadyExisted = _readBlobs.ContainsKey((uint)_mainStream.Position);
                byte[] value = GetBlob((uint)_mainStream.Position);

                int length = value.Length;
                if (length == 0)
                    break;
                if (alreadyExisted)
                    _mainStream.Seek(length + NETGlobals.GetCompressedUInt32Size((uint)length), SeekOrigin.Current);

            }
        }

        public override void Dispose()
        {
            ClearCache();
            base.Dispose();
        }

        public override void ClearCache()
        {
            _readBlobs.Clear();
        }

        /// <summary>
        /// Gets the blob value by it's signature/index.
        /// </summary>
        /// <param name="index">The index or signature to get the blob value from.</param>
        /// <returns></returns>
        public byte[] GetBlob(uint index)
        {
            byte[] bytes = null;
            if (_readBlobs.TryGetValue(index, out bytes))
                return bytes;

            _mainStream.Seek(index, SeekOrigin.Begin);
            int length = (int)NETGlobals.ReadCompressedUInt32(_binReader);

            bytes = _binReader.ReadBytes(length);
            _readBlobs.Add(index, bytes);
            return bytes;
            
        }

        /// <summary>
        /// Gets the blob value by it's signature/index and creates a binary reader.
        /// </summary>
        /// <param name="index">The index or signature to get the blob value from.</param>
        /// <returns></returns>
        public BlobSignatureReader GetBlobReader(uint index)
        {
            byte[] bytes = GetBlob(index);
            MemoryStream newStream = new MemoryStream(bytes);
            newStream.Seek(0, SeekOrigin.Begin);
            BlobSignatureReader reader = new BlobSignatureReader(newStream, _netheader);
            return reader;
        }

        /// <summary>
        /// Gets the blob value by it's signature/index and creates a binary reader using a generic instance.
        /// </summary>
        /// <param name="index">The index or signature to get the blob value from.</param>
        /// <param name="instance">The generic instance that is being used as a context.</param>
        /// <returns></returns>
        public BlobSignatureReader GetBlobReader(uint index, IGenericContext instance)
        {
            BlobSignatureReader reader = GetBlobReader(index);
            reader.GenericContext = instance;
            return reader;
        }

        public bool TryGetBlobReader(uint index, out BlobSignatureReader reader)
        {
            reader = null;
            if (index == 0 || index > StreamSize)
                return false;

            reader = GetBlobReader(index);
            return true;
        }

        public bool TryGetBlobReader(uint index, IGenericContext instance, out BlobSignatureReader reader)
        {
            if (TryGetBlobReader(index, out reader))
            {
                reader.GenericContext = instance;
                return true;
            }
            return false;
        }

        public uint GetBlobIndex(byte[] blobValue)
        {
            ReadAllBlobs();

            if (_readBlobs.ContainsValue(blobValue))
                return _readBlobs.FirstOrDefault(b => b.Value == blobValue).Key;

            _mainStream.Seek(0, SeekOrigin.End);
            uint index = (uint)_mainStream.Position;
            NETGlobals.WriteCompressedUInt32(_binWriter, (uint)blobValue.Length);
            _binWriter.Write(blobValue);
            _readBlobs.Add(index, blobValue);
            return index;
        }

        public IMemberSignature ReadMemberRefSignature(uint sig, IGenericContext context)
        {
            IMemberSignature signature = null;
            BlobSignatureReader reader;
            if (TryGetBlobReader(sig, context, out reader))
            {
                using (reader)
                {
                    byte flag = reader.ReadByte();

                    if (flag == 0x6)
                    {
                        FieldSignature fieldsignature = new FieldSignature();
                        fieldsignature.ReturnType = reader.ReadTypeReference((ElementType)reader.ReadByte());
                        signature = fieldsignature;
                    }
                    else
                    {
                        MethodSignature methodsignature = new MethodSignature();

                        if ((flag & 0x20) != 0)
                        {
                            methodsignature.HasThis = true;
                            flag = (byte)(flag & -33);
                        }
                        if ((flag & 0x40) != 0)
                        {
                            methodsignature.ExplicitThis = true;
                            flag = (byte)(flag & -65);
                        }
                        if ((flag & 0x10) != 0)
                        {
                            int genericsig = NETGlobals.ReadCompressedInt32(reader);
                            if (!context.IsDefinition)
                            {
                                AddMissingGenericParameters(context.Method, genericsig - 1);
                            }
                        }
                        methodsignature.CallingConvention = (MethodCallingConvention)flag;

                        uint paramCount = NETGlobals.ReadCompressedUInt32(reader);
                        methodsignature.ReturnType = reader.ReadTypeReference();

                        ParameterReference[] parameters = new ParameterReference[paramCount];
                        for (int i = 0; i < paramCount; i++)
                        {
                            parameters[i] = new ParameterReference() { ParameterType = reader.ReadTypeReference((ElementType)reader.ReadByte()) };
                        }
                        methodsignature.Parameters = parameters;

                        signature = methodsignature;
                    }
                }
            }
            return signature;
        }
        
        public PropertySignature ReadPropertySignature(uint signature, PropertyDefinition parentProperty)
        {
            PropertySignature propertySig = null;
            BlobSignatureReader reader;
            if (TryGetBlobReader(signature, out reader))
            {
                using (reader)
                {
                    reader.GenericContext = parentProperty.DeclaringType;

                    byte flag = reader.ReadByte();

                    if ((flag & 8) == 0)
                        throw new ArgumentException("Signature doesn't refer to a valid property signature.");

                    propertySig = new PropertySignature();
                    propertySig.HasThis = (flag & 0x20) != 0;
                    NETGlobals.ReadCompressedUInt32(reader);
                    propertySig.ReturnType = reader.ReadTypeReference();
                }
            }
            return propertySig;
        }
        
        public VariableDefinition[] ReadVariableSignature(uint signature, MethodDefinition parentMethod)
        {
            VariableDefinition[] variables = null;
            BlobSignatureReader reader;
            if (TryGetBlobReader(signature, parentMethod, out reader))
            {
                using (reader)
                {
                    reader.GenericContext = parentMethod;

                    byte local_sig = reader.ReadByte();

                    if (local_sig != 0x7)
                        throw new ArgumentException("Signature doesn't refer to a valid local variable signature");

                    uint count = NETGlobals.ReadCompressedUInt32(reader);

                    if (count == 0)
                        return null;

                    variables = new VariableDefinition[count];

                    for (int i = 0; i < count; i++)
                        variables[i] = new VariableDefinition(i, reader.ReadTypeReference());
                }
            }
            return variables;
        }
              
        public TypeReference ReadTypeSignature(uint signature, IGenericContext paramProvider)
        {
            TypeReference typeRef = null;
            BlobSignatureReader reader;
            if (TryGetBlobReader(signature, paramProvider, out reader))
            {
                using (reader)
                {
                    reader.GenericContext = paramProvider;
                    typeRef = reader.ReadTypeReference();
                }
            }
            return typeRef;
        }
        
        public TypeReference[] ReadGenericArgumentsSignature(uint signature, IGenericContext context)
        {
            BlobSignatureReader reader;
            if (TryGetBlobReader(signature, context, out reader))
            {
                using (reader)
                {
                    if (reader.ReadByte() == 0xa)
                    {
                        uint count = NETGlobals.ReadCompressedUInt32(reader);
                        TypeReference[] types = new TypeReference[count];
                        for (int i = 0; i < count; i++)
                            types[i] = reader.ReadTypeReference();

                        return types;
                    }
                }
            }
            throw new ArgumentException("Signature doesn't point to a valid generic arguments signature");
        }
        
        public object ReadConstantValue(ElementType type, uint signature)
        {
            object value = null;
            BlobSignatureReader reader;
            if (TryGetBlobReader(signature, out reader))
            {
                using (reader)
                {
                    if (type == ElementType.String)
                        value = Encoding.Unicode.GetString(reader.ReadBytes((int)reader.BaseStream.Length), 0, (int)reader.BaseStream.Length);
                    else
                        value = reader.ReadPrimitiveValue(type);
                }
            }
            return value;
        }
       
        public CustomAttributeSignature ReadCustomAttributeSignature(CustomAttribute parent, uint signature)
        {
            CustomAttributeSignature customAttrSig = null;
            BlobSignatureReader reader;
            if (TryGetBlobReader(signature, out reader))
            {
                using (reader)
                {
                    ushort sign = reader.ReadUInt16();
                    if (sign != 0x0001)
                        throw new ArgumentException("Signature doesn't refer to a valid Custom Attribute signature");

                    int fixedArgCount = 0;

                    if (parent.Constructor.Signature != null && parent.Constructor.Signature.Parameters != null)
                        fixedArgCount = parent.Constructor.Signature.Parameters.Length;

                    CustomAttributeArgument[] fixedArgs = new CustomAttributeArgument[fixedArgCount];
                    bool canReadNamedArgs = true; // temporary solution for skipping named args when fixed args failed.
                    for (int i = 0; i < fixedArgCount; i++)
                    {
                        fixedArgs[i] = new CustomAttributeArgument(reader.ReadCustomAttributeArgumentValue(parent.Constructor.Signature.Parameters[i].ParameterType));
                        if (fixedArgs[i].Value == null)
                            canReadNamedArgs = false;
                    }

                    CustomAttributeArgument[] namedArgs = null;
                    if (!reader.EndOfStream && canReadNamedArgs)
                    {
                        int namedArgCount = reader.ReadUInt16();
                        namedArgs = new CustomAttributeArgument[namedArgCount];

                        for (int i = 0; i < namedArgCount; i++)
                        {
                            byte argSignature = reader.ReadByte();
                            TypeReference argType = reader.ReadCustomAttributeFieldOrPropType();
                            string name = reader.ReadUtf8String();
                            namedArgs[i] = new CustomAttributeArgument(reader.ReadCustomAttributeArgumentValue(argType), name, argSignature == 0x53 ? CustomAttributeArgumentType.NamedField : CustomAttributeArgumentType.NamedProperty);
                        }
                    }

                    customAttrSig = new CustomAttributeSignature(fixedArgs, namedArgs);
                }
            }
            return customAttrSig;
        }

        private void AddMissingGenericParameters(IGenericParamProvider provider, int index)
        {
            for (int i = provider.GenericParameters.Length; i <= index; i++)
                provider.AddGenericParameter(new GenericParameter(provider, i));
        }
    }
}
