namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Describes the operation that a single CIL instruction performs.  
    /// </summary>
    public readonly struct CilOpCode
    {
        internal const int ValueBitLength = 8;
        internal const int ValueOffset = 0;

        internal const int TwoBytesBitLength = 1;
        internal const int TwoBytesOffset = ValueOffset + ValueBitLength;
        
        internal const int StackBehaviourBitLength = 5;
        internal const int StackBehaviourPushOffset = TwoBytesOffset + TwoBytesBitLength;
        internal const int StackBehaviourPopOffset = StackBehaviourPushOffset + StackBehaviourBitLength;

        internal const int OpCodeTypeBitLength = 3;
        internal const int OpCodeTypeOffset = StackBehaviourPopOffset + StackBehaviourBitLength;

        internal const int OperandTypeBitLength = 5;
        internal const int OperandTypeOffset = OpCodeTypeOffset + OpCodeTypeBitLength;

        internal const int FlowControlBitLength = 4;
        internal const int FlowControlOffset = OperandTypeOffset + OperandTypeBitLength;

        /// <summary>
        /// Determines whether two operation codes encode the same operation.
        /// </summary>
        /// <param name="a">The first operation code.</param>
        /// <param name="b">The second operation code.</param>
        /// <returns><c>true</c> if the same operation code is encoded, <c>false</c> otherwise.</returns>
        public static bool operator ==(CilOpCode a, CilOpCode b) => a.Equals(b);

        /// <summary>
        /// Determines whether two operation codes do not encode the same operation.
        /// </summary>
        /// <param name="a">The first operation code.</param>
        /// <param name="b">The second operation code.</param>
        /// <returns><c>true</c> if a different operation code is encoded, <c>false</c> otherwise.</returns>
        public static bool operator !=(CilOpCode a, CilOpCode b) => !(a == b);


        // To reduce the memory footprint of a single CIL operation code, we put every property into a single 32 bit
        // number, using the following layout:
        //
        //  |31.. ..27|26.. ..22|21.19|18.. ..14|13..  ..9|8|7..         ..0|
        //  +---------+---------+-----+---------+---------+-+---------------+
        //  | flwctrl | operand | opc | stckpop | stckpsh |L| opcode byte   |
        //  +---------+---------+-----+---------+---------+-+---------------+
        //

        private readonly uint _value;

        internal CilOpCode(uint value)
        {
            _value = value;
            
            if (IsLarge)
                CilOpCodes.MultiByteOpCodes[Byte2] = this;
            else
                CilOpCodes.SingleByteOpCodes[Byte1] = this;
        }
        
        /// <summary>
        /// Gets the mnemonic of the operation code.
        /// </summary>
        public string Mnemonic => CilOpCodeNames.Names[IsLarge ? Byte2 + 256 : Byte1];

        /// <summary>
        /// Gets the value of the operation code.
        /// </summary>
        public CilCode Code => (CilCode) ((ushort) ((_value >> ValueOffset) & 0xFF) | (IsLarge ? 0xFE00 : 0));

        /// <summary>
        /// Gets a value indicating whether the operation code is large or not. If this value is <c>true</c>, the code
        /// needs two bytes to be encoded, otherwise it only needs one.
        /// </summary>
        public bool IsLarge => ((_value >> TwoBytesOffset) & 1) == 1;

        /// <summary>
        /// Gets the size in bytes of the operation code.
        /// </summary>
        /// <remarks>
        /// This does not include the operand of the instruction.
        /// </remarks>
        public int Size => IsLarge ? 2 : 1;

        /// <summary>
        /// Gets the first byte that appears in the instruction stream encoding this operation.
        /// </summary>
        public byte Byte1 => (byte) (IsLarge ? 0xFE : (_value >> ValueOffset) & 0xFF);

        /// <summary>
        /// Gets the second byte that appears in the instruction stream encoding this operation.
        /// </summary>
        /// <remarks>
        /// This property only has meaning if <see cref="IsLarge"/> is <c>true</c>.
        /// </remarks>
        public byte Byte2 => (byte) (IsLarge ? (_value >> ValueOffset) & 0xFF : 0);

        /// <summary>
        /// Gets a value indicating the stack push behaviour of the instruction.
        /// </summary>
        public CilStackBehaviour StackBehaviourPush => (CilStackBehaviour) ((_value >> StackBehaviourPushOffset) & 0b11111);

        /// <summary>
        /// Gets a value indicating the stack pop behaviour of the instruction.
        /// </summary>
        public CilStackBehaviour StackBehaviourPop => (CilStackBehaviour) ((_value >> StackBehaviourPopOffset) & 0b11111);

        /// <summary>
        /// Gets a value indicating the category of the operation code.
        /// </summary>
        public CilOpCodeType OpCodeType => (CilOpCodeType) ((_value >> OpCodeTypeOffset) & 0b111);

        /// <summary>
        /// Gets a value indicating the category of the operand.
        /// </summary>
        public CilOperandType OperandType => (CilOperandType) ((_value >> OperandTypeOffset) & 0b11111);

        /// <summary>
        /// Gets a value indicating the flow control behaviour of the operation.
        /// </summary>
        public CilFlowControl FlowControl => (CilFlowControl) ((_value >> FlowControlOffset) & 0b1111);

        /// <inheritdoc />
        public override string ToString() => Mnemonic;

        /// <summary>
        /// Determines whether the provided operation code is encoding the same operation.
        /// </summary>
        /// <param name="other">The other operation code.</param>
        /// <returns><c>true</c> if the same operation code is encoded, <c>false</c> otherwise.</returns>
        public bool Equals(CilOpCode other) => Code == other.Code;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is CilOpCode other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => (int) Code;
    }
}