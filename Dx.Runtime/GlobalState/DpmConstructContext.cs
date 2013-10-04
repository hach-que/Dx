using System;

namespace Dx.Runtime
{
    public static class DpmConstructContext
    {
        /// <summary>
        /// This thread static variable holds the current local node that is being used
        /// to store objects.  We need this information when the constructor of a
        /// distributed object calls back into Dx so that we can store it in the LocalNode's
        /// data storage.  All methods in distributed objects are instrumented so that
        /// any calls to new are immediately preceded with a call to assign the local
        /// node context here.
        /// </summary>
        [ThreadStatic]
        public static ILocalNode LocalNodeContext;
        
        /// <summary>
        /// Used by instrumented code to assign the local node context before new calls.
        /// </summary>
        public static void AssignNodeContext(object obj)
        {
            var transparent = obj as ITransparent;
            if (transparent == null)
                throw new InvalidOperationException();
            DpmConstructContext.LocalNodeContext = transparent.Node;
        }
    }
}

