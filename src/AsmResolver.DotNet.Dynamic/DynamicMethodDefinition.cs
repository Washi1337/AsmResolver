using System;
using System.Collections.Generic;
using System.Reflection;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using MethodAttributes = AsmResolver.PE.DotNet.Metadata.Tables.MethodAttributes;

namespace AsmResolver.DotNet.Dynamic
{
    /// <summary>
    /// Represents a single method in a type definition of a .NET module.
    /// </summary>
    public class DynamicMethodDefinition : MethodDefinition
    {
        /// <summary>
        /// Create a Dynamic Method Definition
        /// </summary>
        /// <param name="module">Target Module</param>
        /// <param name="dynamicMethodObj">Dynamic Method / Delegate / DynamicResolver</param>
        public DynamicMethodDefinition(ModuleDefinition module, object dynamicMethodObj) :
            base(new MetadataToken(TableIndex.Method, 0))
        {
            object resolver = DynamicMethodHelper.ResolveDynamicResolver(dynamicMethodObj);
            var methodBase = FieldReader.ReadField<MethodBase>(resolver, "m_method");
            if (methodBase is null)
            {
                throw new ArgumentException(
                    "Could not get the underlying method base in the provided dynamic method object.");
            }

            Module = module;
            Name = methodBase.Name;
            Attributes = (MethodAttributes)methodBase.Attributes;
            Signature = module.DefaultImporter.ImportMethodSignature(ResolveSig(methodBase, module));
            CilMethodBody = CreateDynamicMethodBody(this, resolver);
        }

        /// <summary>
        /// Determines whether dynamic method reading is fully supported in the current host's .NET environment.
        /// </summary>
        public static bool IsSupported => DynamicTypeSignatureResolver.IsSupported;

        /// <inheritdoc />
        public override ModuleDefinition Module { get; }

        private MethodSignature ResolveSig(MethodBase methodBase, ModuleDefinition module)
        {
            var importer = module.DefaultImporter;
            var returnType = methodBase is MethodInfo info
                ? importer.ImportTypeSignature(info.ReturnType)
                : module.CorLibTypeFactory.Void;

            var parameters = methodBase.GetParameters();

            var parameterTypes = new TypeSignature[parameters.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
                parameterTypes[i] = importer.ImportTypeSignature(parameters[i].ParameterType);

            return new MethodSignature(
                methodBase.IsStatic ? 0 : CallingConventionAttributes.HasThis,
                returnType, parameterTypes);
        }

        /// <summary>
        /// Creates a CIL method body from a dynamic method.
        /// </summary>
        /// <param name="method">The method that owns the method body.</param>
        /// <param name="dynamicMethodObj">The Dynamic Method/Delegate/DynamicResolver.</param>
        /// <returns>The method body.</returns>
        private static CilMethodBody CreateDynamicMethodBody(MethodDefinition method, object dynamicMethodObj)
        {
            if (method.Module is not SerializedModuleDefinition module)
                throw new ArgumentException("Method body should reference a serialized module.");

            var result = new CilMethodBody(method);
            object resolver = DynamicMethodHelper.ResolveDynamicResolver(dynamicMethodObj);

            // We prefer to extract the information from DynamicILInfo if it is there, as it has more accurate info
            // if the DynamicMethod code is not flushed yet into the resolver (e.g., it hasn't been invoked yet).
            object? dynamicILInfo = null;
            if (FieldReader.TryReadField<MethodBase>(resolver, "m_method", out var m) && m is not null)
            {
                if (!FieldReader.TryReadField(m, "m_DynamicILInfo", out dynamicILInfo))
                    FieldReader.TryReadField(m, "_dynamicILInfo", out dynamicILInfo);
            }

            // Extract all required information to construct the body.
            byte[]? code;
            object scope;
            List<object?> tokenList;
            byte[]? localSig;
            byte[]? ehHeader;
            IList<object>? ehInfos;

            if (resolver.GetType().FullName != "System.Reflection.Emit.DynamicILInfo" && dynamicILInfo is not null)
            {
                code = FieldReader.ReadField<byte[]>(dynamicILInfo, "m_code");
                scope = FieldReader.ReadField<object>(dynamicILInfo, "m_scope")!;
                tokenList = FieldReader.ReadField<List<object?>>(scope, "m_tokens")!;
                localSig = FieldReader.ReadField<byte[]>(dynamicILInfo, "m_localSignature");
                ehHeader = FieldReader.ReadField<byte[]>(dynamicILInfo, "m_exceptions");

                // DynamicILInfo does not have EH info. Try recover it from the resolver.
                ehInfos = FieldReader.ReadField<IList<object>>(resolver, "m_exceptions");
            }
            else
            {
                code = FieldReader.ReadField<byte[]>(resolver, "m_code");
                scope = FieldReader.ReadField<object>(resolver, "m_scope")!;
                tokenList = FieldReader.ReadField<List<object?>>(scope, "m_tokens")!;
                localSig = FieldReader.ReadField<byte[]>(resolver, "m_localSignature");
                ehHeader = FieldReader.ReadField<byte[]>(resolver, "m_exceptionHeader");
                ehInfos = FieldReader.ReadField<IList<object>>(resolver, "m_exceptions");
            }

            // Interpret local variables signatures.
            if (localSig is not null)
                DynamicMethodHelper.ReadLocalVariables(result, method, localSig);

            // Read raw instructions.
            if (code is not null)
            {
                var reader = new BinaryStreamReader(code);
                var operandResolver = new DynamicCilOperandResolver(module, result, tokenList);
                var disassembler = new CilDisassembler(reader, operandResolver);
                result.Instructions.AddRange(disassembler.ReadInstructions());
            }

            // Interpret exception handler information or header.
            DynamicMethodHelper.ReadReflectionExceptionHandlers(result, ehHeader, ehInfos, module.DefaultImporter);

            return result;
        }
    }
}
