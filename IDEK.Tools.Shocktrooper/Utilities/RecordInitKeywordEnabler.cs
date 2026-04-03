// This file is only needed until Unity ships a runtime that already
// defines IsExternalInit (e.g., when it moves to .NET 6+).

#if !NET5_0_OR_GREATER // a guard so we don't collide on future versions
namespace System.Runtime.CompilerServices
{
    // The compiler only checks that the type *exists* – the body can be empty!
    // That's so weird, lol
    public static class IsExternalInit { }
}
#endif