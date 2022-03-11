using System;
using System.Collections.Generic;
using System.Reflection;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Provides an implementation of <see cref="ICilOperandResolver"/> that resolves operands based on
    /// runtime information.
    /// </summary>
    public class DynamicCilOperandResolver : PhysicalCilOperandResolver
    {
        private readonly ModuleReaderContext _readerContext;
        private readonly IList<object?> _tokens;
        private readonly ReferenceImporter _importer;

        /// <inheritdoc />
        public DynamicCilOperandResolver(SerializedModuleDefinition contextModule, CilMethodBody methodBody, IList<object?> tokens)
            : base(contextModule, methodBody)
        {
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            _readerContext = contextModule.ReaderContext;
            _importer = new ReferenceImporter(contextModule);
        }

        /// <inheritdoc />
        public override object? ResolveMember(MetadataToken token)
        {
            switch (token.Table)
            {
                case TableIndex.TypeDef:
                    object? type = _tokens[(int) token.Rid];
                    if (type is RuntimeTypeHandle runtimeTypeHandle)
                        return _importer.ImportType(Type.GetTypeFromHandle(runtimeTypeHandle));
                    break;

                case TableIndex.Field:
                    object? field = _tokens[(int) token.Rid];

                    if (field is null)
                        return null;

                    if (field is RuntimeFieldHandle runtimeFieldHandle)
                        return _importer.ImportField(FieldInfo.GetFieldFromHandle(runtimeFieldHandle));

                    if (field.GetType().FullName == "System.Reflection.Emit.GenericFieldInfo")
                    {
                        bool result = FieldReader.TryReadField<RuntimeFieldHandle>(field, "m_field", out var mField);
                        var ctx = FieldReader.ReadField<RuntimeTypeHandle>(field, "m_context");
                        return _importer.ImportField(FieldInfo.GetFieldFromHandle(result
                            ? mField
                            : FieldReader.ReadField<RuntimeFieldHandle>(field, "m_fieldHandle"), ctx));
                    }

                    break;

                case TableIndex.Method:
                case TableIndex.MemberRef:
                    object? obj = _tokens[(int) token.Rid];

                    if (obj is null)
                        return null;

                    if (obj is RuntimeMethodHandle methodHandle)
                    {
                        var method = MethodBase.GetMethodFromHandle(methodHandle);
                        return method is not null
                            ? _importer.ImportMethod(method)
                            : null;
                    }

                    if (obj.GetType().FullName == "System.Reflection.Emit.GenericMethodInfo")
                    {
                        var context = FieldReader.ReadField<RuntimeTypeHandle>(obj, "m_context");
                        bool hasHandle = FieldReader.TryReadField<RuntimeMethodHandle>(obj, "m_method", out var mMethod);
                        var mHandle = FieldReader.ReadField<RuntimeMethodHandle>(obj, "m_methodHandle");
                        var method = MethodBase.GetMethodFromHandle(
                            hasHandle ? mMethod : mHandle,
                            context);

                        return method is not null
                            ? _importer.ImportMethod(method)
                            : null;
                    }

                    if (obj.GetType().FullName == "System.Reflection.Emit.VarArgMethod")
                        return _importer.ImportMethod(FieldReader.ReadField<MethodInfo>(obj, "m_method")!);

                    break;

                case TableIndex.StandAloneSig:
                    var reader = ByteArrayDataSource.CreateReader((byte[]) _tokens[(int) token.Rid]!);
                    return CallingConventionSignature.FromReader(new BlobReadContext(_readerContext), ref reader);
            }

            return token;
        }

        /// <inheritdoc />
        public override object? ResolveString(MetadataToken token)
        {
            return _tokens[(int) token.Rid] as string;
        }
    }
}
