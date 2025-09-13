using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Code.Cil;

internal sealed class SerializedCilMethodBody : CilMethodBody
{
    private readonly ModuleReaderContext _context;
    private readonly MethodDefinition _originalOwner;

    public SerializedCilMethodBody(
        ModuleReaderContext context,
        MethodDefinition owner,
        CilRawMethodBody rawBody,
        ICilOperandResolver? operandResolver)
    {
        _context = context;
        _originalOwner = owner;
        OriginalRawBody = rawBody;

        // Interpret body header.
        if (rawBody is CilRawFatMethodBody fatBody)
        {
            MaxStack = fatBody.MaxStack;
            InitializeLocals = fatBody.InitLocals;
            ReadLocalVariables(fatBody);
        }
        else
        {
            MaxStack = 8;
            InitializeLocals = false;
        }

        OperandResolver = operandResolver
            ?? new PhysicalCilOperandResolver(context.ParentModule, _originalOwner, this);
    }

    public CilRawMethodBody OriginalRawBody { get; }

    public ICilOperandResolver OperandResolver
    {
        get;
    }

    public override bool IsFat
    {
        get
        {
            // If we're fully initialized, we cannot assume anything about the original body to be still the same.
            if (IsInitialized)
                return base.IsFat;

            // The local variables list can be modified without triggering full initialization, so we need to
            // check this explicitly.
            if (LocalVariables.Count > 0)
                return true;

            // Otherwise, we can safely assume the body hasn't been modified yet.
            return OriginalRawBody.IsFat;
        }
    }

    protected override void Initialize(CilInstructionCollection instructions, List<CilExceptionHandler> exceptionHandlers)
    {
        // Decode instructions.
        ReadInstructions(instructions);

        // Read exception handlers.
        if (OriginalRawBody is CilRawFatMethodBody fatBody)
            ReadExceptionHandlers(fatBody, instructions, exceptionHandlers);
    }

    private void ReadInstructions(CilInstructionCollection result)
    {
        try
        {
            var reader = OriginalRawBody.Code.CreateReader();
            var disassembler = new CilDisassembler(reader, OperandResolver);
            result.AddRange(disassembler.ReadInstructions());
        }
        catch (Exception ex)
        {
            _context.RegisterException(new BadImageFormatException(
                $"Method body of {_originalOwner.SafeToString()} contains an invalid CIL code stream.", ex));
        }
    }

    private void ReadLocalVariables(CilRawFatMethodBody fatBody)
    {
        // Method bodies can have 0 tokens if there are no locals defined.
        if (fatBody.LocalVarSigToken == MetadataToken.Zero)
            return;

        // If there is a non-zero token however, it needs to point to a stand-alone signature with a
        // local variable signature stored in it.
        if (!_context.ParentModule.TryLookupMember(fatBody.LocalVarSigToken, out var member)
            || member is not StandAloneSignature { Signature: LocalVariablesSignature localVariablesSignature })
        {
            _context.BadImage($"Method body of {_originalOwner.SafeToString()} contains an invalid local variable signature token.");
            return;
        }

        // Copy over the local variable types from the signature into the method body.
        var variableTypes = localVariablesSignature.VariableTypes;
        for (int i = 0; i < variableTypes.Count; i++)
            LocalVariables.Add(new CilLocalVariable(variableTypes[i]));
    }

    private void ReadExceptionHandlers(CilRawFatMethodBody fatBody, CilInstructionCollection instructions, List<CilExceptionHandler> result)
    {
        try
        {
            for (int i = 0; i < fatBody.ExtraSections.Count; i++)
            {
                var section = fatBody.ExtraSections[i];
                if (section.IsEHTable)
                {
                    var reader = new BinaryStreamReader(section.Data);
                    uint size = section.IsFat
                        ? CilExceptionHandler.FatExceptionHandlerSize
                        : CilExceptionHandler.TinyExceptionHandlerSize;

                    while (reader.CanRead(size))
                    {
                        result.Add(CilExceptionHandler.FromReader(
                            _originalOwner.DeclaringModule!, instructions, ref reader,
                            section.IsFat
                        ));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _context.RegisterException(new BadImageFormatException(
                $"Method body of {_originalOwner.SafeToString()} contains invalid extra sections.", ex));
        }
    }
}
