using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections;
using Process4.Interfaces;
using Data4;

namespace Process4.Collections
{
    public class Distributed<T> : ISerializable
    {
        private string m_Name = null;
        private T m_Data = default(T);

        /// <summary>
        /// Constructs a Distributed&lt;&gt; generic with the specified name.  If the name is the same
        /// as a Distributed&lt;&gt; already in the network, it will point to the existing
        /// instance (so all instances of Distributed&lt;&gt; with the same name point to the
        /// same data in the network).
        /// </summary>
        /// <param name="name">The unique identifier for this object in the network.</param>
        public Distributed(string name) : this(name, false)
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
        public Distributed(string name, bool preventCreate)
        {
            this.m_Name = name;

            // Get the object from the DHT.
            this.m_Data = (T)LocalNode.Singleton.Storage.Fetch(name);
            if (this.m_Data == null && typeof(T).GetInterface("ITransparent") != null && !preventCreate)
            {
                // Check to see if we are not the server and in a client-server network.
                if (LocalNode.Singleton.Architecture == Attributes.Architecture.ServerClient &&
                    !LocalNode.Singleton.IsServer)
                {
                    (LocalNode.Singleton.Storage as Process4.Providers.DhtWrapper).InspectorLog("Client attempted to create new named object '" + name + "' as it could not find one that exists on the network.", "object_vanished");
                    throw new MemberAccessException("Clients are not permitted to create named objects in a server-client architecture.");
                }

                // Create the new object and register it.
                this.m_Data = (T)FormatterServices.GetUninitializedObject(typeof(T));
                (this.m_Data as ITransparent).NetworkName = this.m_Name;
                LocalNode.Singleton.Storage.Store(this.m_Name, this.m_Data);
                typeof(T).GetConstructor(Type.EmptyTypes).Invoke(this.m_Data, null);
            }
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
            else if (typeof(T).GetInterface("ITransparent") != null)
            {
                // Convert the instance to an ITransparent.
                ITransparent i = (t.m_Data as ITransparent);
                i.NetworkName = t.m_Name;
                return (T)i;
            }
            else
                // This class does not have ITransparent implement, likely because
                // it does not have the Distributed attribute or the post-processor
                // hasn't been run correctly.
                throw new NotSupportedException("Attempting to perform distributed erasure on a reference type that does not have ITransparent implemented.  "
                                                    + "If this class does have the Distributed attribute set, ensure the post-processor is being executed during build.");
        }

        #region Helpers for Immutable Storage

        /// <summary>
        /// Pushes the current object out to all other nodes in the network, informing them that
        /// they should store the object data forever and never attempt to fetch it from the
        /// source again.  This method only works if the node caching is set to StoreOnDemand.
        /// </summary>
        public void PushImmutable()
        {
            if (typeof(T).GetInterface("Process4.Interfaces.IImmutable") == null)
                throw new NotSupportedException("Unable to push non-immutable type using PushImmutable.");
            if ((this.m_Data as ITransparent).IsImmutablyPushed)
                throw new NotSupportedException("Unable to repush immutable type using PushImmutable.");
            (this.m_Data as ITransparent).IsImmutablyPushed = true;
            LocalNode.Singleton.Storage.Store(this.m_Name, this.m_Data);

            // TODO: Push to other nodes in the network.
        }

        /// <summary>
        /// Pulls an immutable object from the network, making sure that it does not accidently
        /// create a new immutable object in the network.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T PullImmutable(string key)
        {
            if (typeof(T).GetInterface("Process4.Interfaces.IImmutable") == null)
                throw new NotSupportedException("Unable to pull non-immutable type using PullImmutable.");
            Distributed<T> d = new Distributed<T>(key, true);
            if (d.m_Data == null)
                return default(T);
            else
                return d;
        }

        #endregion

        #region Manual Cast Method

        internal T ManualDistributedErasure()
        {
            return (T)this;
        }

        #endregion
    }
}
