namespace AsmResolver.Net.Cil
{
    // ReSharper disable InconsistentNaming
    public enum CilStackBehaviour : ushort
    {
        /// <summary>
        /// No values are popped off the stack.
        ///</summary>
        Pop0,

        /// <summary>
        /// Pops one value off the stack.
        ///</summary>
        Pop1,

        /// <summary>
        /// Pops 1 value off the stack for the first operand, and 1 value of the stack for the second operand.
        ///</summary>
        Pop1_pop1,

        /// <summary>
        ///Pops a 32-bit integer off the stack.
        ///</summary>
        Popi,

        /// <summary>
        ///Pops a 32-bit integer off the stack for the first operand, and a value off the stack for the second operand.
        ///</summary>
        Popi_pop1,

        /// <summary>
        ///Pops a 32-bit integer off the stack for the first operand, and a 32-bit integer off the stack for the second operand.
        ///</summary>
        Popi_popi,

        /// <summary>
        ///Pops a 32-bit integer off the stack for the first operand, and a 64-bit integer off the stack for the second operand.
        ///</summary>
        Popi_popi8,

        /// <summary>
        ///Pops a 32-bit integer off the stack for the first operand, a 32-bit integer off the stack for the second operand, and a 32-bit integer off the stack for the third operand.
        ///</summary>
        Popi_popi_popi,

        /// <summary>
        ///Pops a 32-bit integer off the stack for the first operand, and a 32-bit floating point number off the stack for the second operand.
        ///</summary>
        Popi_popr4,

        /// <summary>
        ///Pops a 32-bit integer off the stack for the first operand, and a 64-bit floating point number off the stack for the second operand.
        ///</summary>
        Popi_popr8,

        /// <summary>
        ///Pops a reference off the stack.
        ///</summary>
        Popref,

        /// <summary>
        ///Pops a reference off the stack for the first operand, and a value off the stack for the second operand.
        ///</summary>
        Popref_pop1,

        /// <summary>
        ///Pops a reference off the stack for the first operand, and a 32-bit integer off the stack for the second operand.
        ///</summary>
        Popref_popi,

        /// <summary>
        ///Pops a reference off the stack for the first operand, a value off the stack for the second operand, and a value off the stack for the third operand.
        ///</summary>
        Popref_popi_popi,

        /// <summary>
        ///Pops a reference off the stack for the first operand, a value off the stack for the second operand, and a 64-bit integer off the stack for the third operand.
        ///</summary>
        Popref_popi_popi8,

        /// <summary>
        ///Pops a reference off the stack for the first operand, a value off the stack for the second operand, and a 32-bit integer off the stack for the third operand.
        ///</summary>
        Popref_popi_popr4,

        /// <summary>
        ///Pops a reference off the stack for the first operand, a value off the stack for the second operand, and a 64-bit floating point number off the stack for the third operand.
        ///</summary>
        Popref_popi_popr8,

        /// <summary>
        ///Pops a reference off the stack for the first operand, a value off the stack for the second operand, and a reference off the stack for the third operand.
        ///</summary>
        Popref_popi_popref,

        /// <summary>
        ///No values are pushed onto the stack.
        ///</summary>
        Push0,

        /// <summary>
        ///Pushes one value onto the stack.
        ///</summary>
        Push1,

        /// <summary>
        ///Pushes 1 value onto the stack for the first operand, and 1 value onto the stack for the second operand.
        ///</summary>
        Push1_push1,

        /// <summary>
        ///Pushes a 32-bit integer onto the stack.
        ///</summary>
        Pushi,

        /// <summary>
        ///Pushes a 64-bit integer onto the stack.
        ///</summary>
        Pushi8,

        /// <summary>
        ///Pushes a 32-bit floating point number onto the stack.
        ///</summary>
        Pushr4,

        /// <summary>
        ///Pushes a 64-bit floating point number onto the stack.
        ///</summary>
        Pushr8,

        /// <summary>
        ///Pushes a reference onto the stack.
        ///</summary>
        Pushref,

        /// <summary>
        ///Pops a variable off the stack.
        ///</summary>
        Varpop,

        /// <summary>
        ///Pushes a variable onto the stack.
        ///</summary>
        Varpush,

        /// <summary>
        ///Pops a reference off the stack for the first operand, a value off the stack for the second operand, and a 32-bit integer off the stack for the third operand.
        ///</summary>
        Popref_popi_pop1
    }

    // ReSharper restore InconsistentNaming
}