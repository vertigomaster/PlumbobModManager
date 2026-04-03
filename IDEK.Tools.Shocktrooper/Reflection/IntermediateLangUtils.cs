using System;
using System.Reflection;

namespace IDEK.Tools.ShocktroopUtils.CILAnalysis
{
    public static class CILUtils
    {
        public enum CILOpCodes
        {
            //more here: https://en.wikipedia.org/wiki/List_of_CIL_instructions

            /// <summary>
            /// Do nothing (No operation) | base instruction
            /// </summary>
            NoOperation = 0x00,

            #region Debugging

            /// <summary>
            /// Inform a debugger that a breakpoint has been reached | Base instruction
            /// </summary>
            Breakpoint = 0x01,

            /// <summary>
            /// Throw an exception | Object model instruction
            /// </summary>
            Throw = 0x7A,

            /// <summary>
            /// ckfinite | Base instruction
            /// <br/>Throw an ArithmeticException if value is not a finite number.	
            /// </summary>
            ThrowArithmeticIfNotFinite = 0xC3,

            #endregion

            #region Arithmetic

            /// <summary>
            /// add, Add two values, returning a new value | base instruction
            /// </summary>
            Add = 0x58,

            /// <summary>
            /// Add signed integer values with overflow check. | base instruction
            /// </summary>
            Add_OverflowCheck = 0xD6,

            /// <summary>
            /// add.ovf.un | Base instruction
            /// <br/> Add unsigned integer values with overflow check.
            /// </summary>
            UnsignedAdd_OverflowCheck = 0xD7,

            /// <summary>
            /// sub | Base instruction
            /// <br/>Subtract value2 from value1, returning a new value
            /// </summary>
            Subtract = 0x59,

            /// <summary>
            /// sub.ovf	| Base instruction
            /// <br/>Subtract native int from a native int. Signed result shall fit in same size
            /// </summary>
            Subtract_OverflowCheck = 0xDA,

            /// <summary>
            /// sub.ovf.un | Base instruction
            /// <br/>Subtract native unsigned int from a native unsigned int. Unsigned result shall fit in same size.	
            /// </summary>
            UnsignedSubtract_OverflowCheck = 0xDB,

            /// <summary>
            /// mul | Base instruction
            /// <br/>Multiply values 
            /// </summary>
            Multiply = 0x5A,

            /// <summary>
            /// mul.ovf	| Base instruction
            /// <br/>Multiply signed integer values. Signed result shall fit in same size.	
            /// </summary>
            Multiply_OverflowCheck = 0xD8,

            /// <summary>
            /// mul.ovf.un | Base instruction
            /// <br/>Multiply unsigned integer values. Unsigned result shall fit in same size.	
            /// </summary>
            UnsignedMultiply_OverflowCheck = 0xD9,

            /// <summary>
            /// div | Base instruction
            /// <br/>Divide two (signed) values to return a quotient or floating-point result 
            /// </summary>
            Divide = 0x5B,

            /// <summary>
            /// div.un | Base instruction
            /// <br/>Divide two values, unsigned, returning a quotient 
            /// </summary>
            UnsignedDivide = 0x5C,

            /// <summary>
            /// rem | Base instruction
            /// <br/>Remainder when dividing one value by another
            /// </summary>
            Modulo = 0x5D,

            /// <summary>
            /// rem.un, Remainder when dividing one unsigned value by another | Base instruction
            /// </summary>
            UnsignedModulo = 0x5E,

            /// <summary>
            /// neg, Sign negation of the given value | Base instruction
            /// </summary>
            Negate = 0x65,

            #endregion

            #region Bitwise Ops
            /// <summary>
            /// and, Bitwise AND of two integral (integer?) values, returns an integral (integer?) value | Base instruction
            /// </summary>
            BitwiseAnd = 0x5F,

            /// <summary>
            /// or, Bitwise OR of two integer values, returns an integer | Base instruction
            /// </summary>
            BitwiseOr = 0x60,

            /// <summary>
            /// xor, Bitwise XOR of integer values, returns an integer | Base instruction
            /// </summary>
            BitwiseXor = 0x61,

            /// <summary>
            /// shl, Shift an integer left (shifting in zeros), return an integer | Base instruction
            /// </summary>
            BitwiseLeftShift = 0x62,

            /// <summary>
            /// shr, Shift an integer right (shift in sign), return an integer | Base instruction
            /// </summary>
            BitwiseRightShift = 0x63,

            /// <summary>
            /// shr.un, Shift an integer right (shift in zero), return an integer | Base instruction
            /// </summary>
            /// <remarks>
            /// Right shifts are the only shift that requires separate handling for signed values</remarks>
            UnsignedBitwiseRightShift = 0x64,

            /// <summary>
            /// Bitwise complement
            /// </summary>
            BitwiseNot = 0x66,

            #endregion

            #region Branching

            //there are a ton of these, covering inequalities, signs, 0 false cases, 0 numeric cases, null cases, etc

            /// <summary>
            /// Jump to one of n values | Base instruction
            /// <br/>Followed by potential values
            /// </summary>
            Switch = 0x45,

            #endregion

            #region Invocations
            /// <summary>
            /// call [method] | base instruction
            /// <br/>Call method described by method 
            /// <br/>Followed by 4 bytes containg method id
            /// </summary>
            MethodCall = 0x28,

            /// <summary>
            /// calli [callsitedescr] | base instruction
            /// <br/>Call method indicated on the stack with arguments described by [callsitedescr].
            /// <br/>Followed by 4 bytes containg the call site description (callsitedescr)
            /// </summary>
            IndicatedMethodCall = 0x29,

            /// <summary>
            /// callvirt [method] 
            /// <br/>"Virtual" method call - Call a method associated with an object. | Object Model instruction
            /// <br/>Followed by 4 bytes containg method id
            /// </summary>
            VirtualMethodCall = 0x6f,

            /// <summary>
            /// 0xFE followed by 0x16 | constrained. [thisType] | prefix to instruction
            /// <br/>Call a virtual method on a type constrained to be type T.
            /// </summary>
            TypeConstrainedVirtualMethodCall = 0xFE16,

            #endregion

            /// <summary>
            /// Return from method, possibly with a value | Base instruction
            /// <br/> Presumably followed by said value. Currently uncertain.
            /// </summary>
            Return = 0x2A,

            /// <summary>
            /// newobj [ctor]
            /// <br/> Allocate an uninitialized object or value type and call ctor (constructor) | Object model instruction
            /// </summary>
            NewObject = 0x73,

            /// <summary>
            /// newarr [etype] | Object model instruction
            /// <br/>Create a new array with elements of type "etype".	
            /// </summary>
            NewArray = 0x8D,

            /// <summary>
            /// isinst [class] | Object model instruction
            /// <br/>Test if obj is an instance of class, returning null or an instance of that class or interface
            /// </summary>
            TestIfClassInstance = 0x75,

            /// <summary>
            /// refanyval [type] | Object model instruction
            /// <br/>Push the address stored in a typed reference of type "type".	
            /// </summary>
            /// <remarks>
            /// Basically retrieves the pointer? Unless I'm misunderstanding.</remarks>
            PushRef = 0xC2
        }

        /// <summary>
        /// Examines the IL bytecode to determine if the base method of the given method info is invoked.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool IsOverridingOrHidingBaseMethod(this MethodInfo method)
        {
            MethodInfo baseMethod = method.GetBaseDefinition();
            if(baseMethod == null) return false; //no base method to call in the first place

            //having a base method doesn't imply anything other than inheritance.
            //even non virtual methods have a base definition.

            //if method and base have different IDs, they are distinct methods, implying an override/hide
            return method.GetMethodBody() != null &&
                method?.GetBaseDefinition() != null && 
                !method.HasSameMetadataDefinitionAs(baseMethod);
        }

        public static bool ContainsCallToBaseMethod(this MethodInfo method)
        {
            MethodBody methodBody = method.GetMethodBody();
            if(methodBody == null) return false; //abstract

            byte[] ilBody = methodBody.GetILAsByteArray();
            if(ilBody == null) return false; //empty method

            MethodInfo baseMethod = method.GetBaseDefinition();
            if(baseMethod == null) return false; //no base method to call in the first place

            if(method.HasSameMetadataDefinitionAs(baseMethod)) return false;

            return ilBody.ILContainsCallToMethod(baseMethod);
        }

        public static bool ILContainsCallToMethod(this byte[] ilBytes, MethodInfo method)
        {
            int methodToken = method.MetadataToken;

            for(int i = 0; i < ilBytes.Length; i++)
            {
                //step through the bytes
                if(ilBytes[i] is not (byte)CILOpCodes.MethodCall and not (byte)CILOpCodes.VirtualMethodCall)
                {
                    continue;
                }
                //these are followed by 4 bytes encoding the token of the method being called

                //get token by shaving off an integer's worth starting at the next byte
                int invokedMethodToken = BitConverter.ToInt32(ilBytes, i + 1);


                //if the token matches your given method, it is being called by the given IL snippet.
                if(invokedMethodToken == methodToken) return true;

                //TODO: increment by 4 at this point since we know the next 4 are part of an address, no instructions.
                //Avoids bugs from potential misinterpretations
            }

            return false;
        }
    }
}