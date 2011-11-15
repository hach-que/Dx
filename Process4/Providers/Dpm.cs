using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Interfaces;
using System.Runtime.CompilerServices;
using System.Net;
using Data4;
using System.Reflection;
using System.Threading;
using System.Runtime.Serialization;
using Process4.Collections;
using Process4.Remoting;

namespace Process4.Providers
{
    internal class Dpm : IProcessorProvider
    {
        private LocalNode m_Node = null;

        /// <summary>
        /// The DHT in use by the associated node.
        /// </summary>
        private Dht Dht
        {
            get
            {
                if (this.m_Node.Storage is DhtWrapper)
                    return (this.m_Node.Storage as DhtWrapper).Dht;
                else
                    throw new InvalidOperationException("The default processing provider can only be used in conjunction with the default storage provider.");
            }
        }

        /// <summary>
        /// Creates a new Distributed Processing Module associated with the
        /// specified node.
        /// </summary>
        /// <param name="node">The local node to associate with.</param>
        public Dpm(LocalNode node)
        {            
            this.m_Node = node;
        }

        /// <summary>
        /// Starts the DPM.
        /// </summary>
        public void Start()
        {
        }

        /// <summary>
        /// Stops the DPM.
        /// </summary>
        public void Stop()
        {
        }

        #region IProcessorProvider Members

        public object Invoke(string id, string method, object[] args)
        {
            // TODO: Check to see whether this module should be invoked on this machine
            //       or whether we should pass it off to another machine.
            ITransparent obj = this.m_Node.Storage.Fetch(id) as ITransparent;
            if (obj == null) throw new ObjectVanishedException(id);

            MethodInfo mi = obj.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);
            if (mi == null)
                throw new MissingMethodException(obj.GetType().FullName, method);
            return mi.Invoke(obj, args);
        }

        public DTask<object> InvokeAsync(string id, string method, object[] args, Delegate callback)
        {
            // TODO: Check to see whether this module should be invoked on this machine
            //       or whether we should pass it off to another machine.
            DTask<object> task = new DTask<object>();
            ITransparent obj = this.m_Node.Storage.Fetch(id) as ITransparent;
            if (obj == null) throw new ObjectVanishedException(id);

            MethodInfo mi = obj.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);
            if (mi == null)
                throw new MissingMethodException(obj.GetType().FullName, method);
            new Thread(() =>
            {
                task.Value = mi.Invoke(obj, args);
                task.Completed = true;
                callback.DynamicInvoke(null);
            }).Start();
            return task;
        }

        #endregion
    }

    /// <summary>
    /// Provides entry points into the library when methods or property getters / setters
    /// are invoked.
    /// </summary>
    public static class DpmEntrypoint
    {
        public static object SetProperty(Delegate d, object[] args)
        {
            // Get the network name of the object and the name of the method.
            string objectName = (d.Target as ITransparent).NetworkName;

            // We need to get rid of the set_ prefix and Distributed suffix.
            string propertyName = d.Method.Name.Substring(4, d.Method.Name.LastIndexOf("__Distributed") - 4);

            // Get our local node and invoke the set property.
            LocalNode.Singleton.SetProperty(objectName, propertyName, args[0]);

            return null;
        }

        public static object GetProperty(Delegate d, object[] args)
        {
            // Get the network name of the object and the name of the method.
            string objectName = (d.Target as ITransparent).NetworkName;

            // We need to get rid of the get_ prefix and Distributed suffix.
            string propertyName = d.Method.Name.Substring(4, d.Method.Name.LastIndexOf("__Distributed") - 4);

            // Get our local node and invoke the get property.
            return LocalNode.Singleton.GetProperty(objectName, propertyName);
        }

        public static object Invoke(Delegate d, object[] args)
        {
            // Get the network name of the object and the name of the method.
            string objectName = (d.Target as ITransparent).NetworkName;
            string methodName = d.Method.Name;

            // Get our local node and invoke the method.
            return LocalNode.Singleton.Invoke(objectName, methodName, args);
        }

        public static void Construct(object obj)
        {
            // Check to see if we've already got a NetworkName; if we have
            // then we're a named distributed instance that doesn't need the
            // autoid assigned.
            if ((obj as ITransparent).NetworkName != null)
                return;

            // Allocate a randomly generated NetworkName.
            (obj as ITransparent).NetworkName = "autoid-" + ID.NewRandom().ToString();

            // Store the object in the Dht.
            LocalNode.Singleton.Storage.Store((obj as ITransparent).NetworkName, obj);
        }

        public static void Serialize(object obj, SerializationInfo info, StreamingContext context)
        {
            // Loop through all of the private, internal fields in the object and
            // serialize all of their values.
            foreach (FieldInfo fi in obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (fi.FieldType.GetInterface("ITransparent") != null)
                {
                    // Just serialize the NetworkName instead of the whole object.
                    if (fi.GetValue(obj) != null)
                        info.AddValue(fi.Name, (fi.GetValue(obj) as ITransparent).NetworkName);
                    else
                        info.AddValue(fi.Name, null);
                }
                else
                {
                    // Serialize the value itself.
                    info.AddValue(fi.Name, fi.GetValue(obj), fi.FieldType);
                }
            }
        }

        public static void Deserialize(object obj, SerializationInfo info, StreamingContext context)
        {
            // Loop through all of the private, internal fields in the object and
            // set their values based on the deserialized information.
            foreach (FieldInfo fi in obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (fi.FieldType.GetInterface("ITransparent") != null)
                {
                    // Deserialize the object based on the NetworkName.
                    if (info.GetValue(fi.Name, typeof(object)) != null)
                    {
                        object v = FormatterServices.GetUninitializedObject(fi.FieldType);
                        (v as ITransparent).NetworkName = info.GetString(fi.Name);

                        /*object v = typeof(Distributed<>)
                                        .MakeGenericType(new Type[] { fi.FieldType })
                                        .GetConstructor(new Type[] { typeof(string) })
                                        .Invoke(new object[] { info.GetString(fi.Name) });
                        object o = v.GetType()
                                        .GetMethod("ManualDistributedErasure", BindingFlags.NonPublic | BindingFlags.Instance)
                                        .Invoke(v, null);*/
                        fi.SetValue(obj, v);
                    }
                    else
                        fi.SetValue(obj, null);
                }
                else
                {
                    // Deserialize the value itself.
                    fi.SetValue(obj, info.GetValue(fi.Name, fi.FieldType));
                }
            }
        }
    }
}
