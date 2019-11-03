// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

using static AsmResolver.PE.DotNet.Cil.CilOpCode;
using static AsmResolver.PE.DotNet.Cil.CilStackBehaviour;
using static AsmResolver.PE.DotNet.Cil.CilOpCodeType;
using static AsmResolver.PE.DotNet.Cil.CilOperandType;
using static AsmResolver.PE.DotNet.Cil.CilFlowControl;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides members defining the entire CIL instruction set.
    /// </summary>
    public static partial class CilOpCodes
    {
        public static readonly CilOpCode[] SingleByteOpCodes = new CilOpCode[256];
        public static readonly CilOpCode[] MultiByteOpCodes = new CilOpCode[256];

        public static readonly CilOpCode Nop = new CilOpCode(
            (((ushort) CilCode.Nop & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Nop >> 14) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) Next << FlowControlOffset));

        public static readonly CilOpCode Break = new CilOpCode(
            (((ushort) CilCode.Break & 0xFF) << ValueOffset)
            | (((ushort) CilCode.Break >> 14) << TwoBytesOffset)
            | ((ushort) Push0 << StackBehaviourPushOffset)
            | ((ushort) Pop0 << StackBehaviourPopOffset)
            | ((byte) Primitive << OpCodeTypeOffset)
            | ((byte) InlineNone << OperandTypeOffset)
            | ((byte) CilFlowControl.Break << FlowControlOffset));
    }
}