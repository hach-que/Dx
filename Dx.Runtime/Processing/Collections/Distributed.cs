using System;
using System.Runtime.Serialization;

namespace Dx.Runtime
{
    public class Distributed<T> : ISerializable
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
            if (node is LocalNode && (node as LocalNode).m_Fake)
                throw new InvalidOperationException("Object graph has not been deserialized correctly.");

            // Get the object from the DHT.
            this.m_Data = (T)node.Storage.Fetch(name);
            if (this.m_Data == null && typeof(T).GetInterface("ITransparent") != null && !preventCreate)
            {
                // Check to see if we are not the server and in a client-server network.
                if (node.Architecture == Architecture.ServerClient &&
                    !node.IsServer)
                {
                    throw new MemberAccessException("Clients are not permitted to create named objects in a server-client architecture.");
                }

                // Create the new object and register it.
                this.m_Data = (T)FormatterServices.GetUninitializedObject(typeof(T));
                (this.m_Data as ITransparent).NetworkName = this.m_Name;
                (this.m_Data as ITransparent).Node = node;
                node.Storage.Store(this.m_Name, this.m_Data);
                typeof(T).GetConstructor(Type.EmptyTypes).Invoke(this.m_Data, null);
            }
            else if (this.m_Data != null && typeof(T).GetInterface("ITransparent") != null)
                (this.m_Data as ITransparent).Node = node;
        }

        /// <summary>
        /// Constructs a Distributed&lt;&gt; from serialized data.
        /// </summary>
        public Distributed(SerializationInfo info, StreamingContext context)
        {
            this.m_Name = info.GetString("name");
            this.m_Data = (T)info.GetValue("data", typeof(T));
        }

        /// <summary>
        /// Returns the serialized data representation of this Distributed&lt;&gt;.
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", this.m_Name);
            info.AddValue("data", this.m_Data);
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
                ITransparent i = (t.m_Data as ITransparent);
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
