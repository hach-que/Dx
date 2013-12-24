using System;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    public class Distributed<T>
    {
        private string m_Name;
        private T m_Data;

        /// <summary>
        /// Constructs a Distributed&lt;&gt; generic with the specified name.  If the name is the same
        /// as a Distributed&lt;&gt; already in the network, it will point to the existing
        /// instance (so all instances of Distributed&lt;&gt; with the same name point to the
        /// same data in the network).
        /// </summary>
        /// <param name="name">The unique identifier for this object in the network.</param>
        public Distributed(ILocalNode node, string name) : this(node, name, false)
        {
        }

        /// <summary>
        /// Constructs a Distributed&lt;&gt; generic with the specified name.  If the name is the same
        /// as a Distributed&lt;&gt; already in the network, it will point to the existing
        /// instance (so all instances of Distributed&lt;&gt; with the same name point to the
        /// same data in the network).
        /// </summary>
        /// <param name="name">The unique identifier for this object in the network.</param>
        /// <param name="preventCreate">If this is false, then null is returned if the object does not already exist.</param> 
        public Distributed(ILocalNode node, string name, bool preventCreate)
        {
            this.m_Name = name;

            var constructor = typeof(T).GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                throw new InvalidOperationException();
            }

            this.m_Data = (T)node.Fetch(name);
            var transparentData = (ITransparent)this.m_Data;

            if (this.m_Data == null && typeof(T).GetInterface("ITransparent") != null && !preventCreate)
            {
                if (node.Architecture == Architecture.ServerClient && !node.IsServer)
                {
                    throw new InvalidOperationException("Clients can not construct new objects.");
                }

                // Create the new object and register it.
                this.m_Data = (T)FormatterServices.GetUninitializedObject(typeof(T));
                transparentData = (ITransparent)this.m_Data;
                transparentData.NetworkName = this.m_Name;
                transparentData.Node = node;
                node.Store(this.m_Name, this.m_Data);
                constructor.Invoke(this.m_Data, null);
            }
            else if (this.m_Data != null && typeof(T).GetInterface("ITransparent") != null)
                transparentData.Node = node;
        }

        public static implicit operator T(Distributed<T> t)
        {
            if (typeof(T).IsValueType)
            {
                // This type will not have ITransparent implemented and is never
                // null.  Throw an exception indicating that a conversion to the
                // normal version of this type would disconnect it's value from
                // the network version.
                throw new NotSupportedException("Attempted to perform distributed erasure on value type.  Value types must be accessed via Distributed<"
                                                    + typeof(T).Name + "> and can't be implicitly cast to their own type.");
            }
            
            if (typeof(T).GetInterface("ITransparent") != null)
            {
                // Convert the instance to an ITransparent.
                if (t.m_Data == null)
                    return default(T);
                var i = (t.m_Data as ITransparent);
                i.NetworkName = t.m_Name;
                return (T)i;
            }
            
            // This class does not have ITransparent implement, likely because
            // it does not have the Distributed attribute or the post-processor
            // hasn't been run correctly.
            throw new NotSupportedException("Attempting to perform distributed erasure on a " + typeof(T).FullName + " that does not have ITransparent implemented.  "
                                            + "If this class does have the Distributed attribute set, ensure the post-processor is being executed during build.");
        }
    }
}
