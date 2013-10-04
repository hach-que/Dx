using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Linq;

namespace Dx.Runtime
{
    /// <summary>
    /// Provides entry points into the library when methods or property getters / setters
    /// are invoked.
    /// </summary>
    public static class DpmEntrypoint
    {
        public static object InvokeDynamic(Delegate d, object[] args)
        {
            return DpmEntrypoint.InvokeDynamicBase(d.GetType().DeclaringType, d.Method, d.Target, d.Method.GetGenericArguments(), args);
        }

        public static object InvokeDynamic(Type dt, MethodInfo mi, object target, Type[] targs, object[] args)
        {
            if (targs == null || targs.Any(x => x == null))
                throw new ArgumentNullException("targs");
        
            foreach (Type nt in dt.GetNestedTypes(BindingFlags.NonPublic))
            {
                if (mi.Name.IndexOf("__") == -1)
                    continue;
                if (nt.FullName.Contains("+" + mi.Name.Substring(0, mi.Name.IndexOf("__")) + "__InvokeDirect"))
                    return DpmEntrypoint.InvokeDynamicBase(nt, mi, target, targs, args);
            }

            // Fall back to slow invocation.
            if (mi.IsGenericMethod)
                mi = mi.MakeGenericMethod(targs);
            return mi.Invoke(target, args);
        }

        private static object InvokeDynamicBase(Type dt, MethodInfo mi, object target, Type[] targs, object[] args)
        {
            Type[] tparams = mi.DeclaringType.GetGenericArguments()
                                .Concat(targs)
                                .ToArray();
            foreach (var t in tparams)
                if (t.IsGenericParameter)
                    throw new InvalidOperationException("Generic parameter not resolved before invocation of method.");
            if (dt.ContainsGenericParameters)
                dt = dt.MakeGenericType(tparams);
            IDirectInvoke di = dt.GetConstructor(Type.EmptyTypes).Invoke(null) as IDirectInvoke;
            object o = di.Invoke(mi, target, args);
            return o;
        }


        public static object SetProperty(Delegate d, object[] args)
        {
            // Invoke directly if not networked.
            var node = (d.Target as ITransparent).Node;
            if (node == null)
                return DpmEntrypoint.InvokeDynamic(d, args);

            // Get the network name of the object.
            string objectName = (d.Target as ITransparent).NetworkName;

            // We need to get rid of the set_ prefix and Distributed suffix.
            string propertyName = d.Method.Name.Substring(4, d.Method.Name.LastIndexOf("__Distributed") - 4);

            // Get our local node and invoke the set property.
            node.SetProperty(objectName, propertyName, args[0]);

            return null;
        }

        public static object GetProperty(Delegate d, object[] args)
        {
            // Invoke directly if not networked.
            var node = (d.Target as ITransparent).Node;
            if (node == null)
                return DpmEntrypoint.InvokeDynamic(d, args);

            // Get the network name of the object.
            string objectName = (d.Target as ITransparent).NetworkName;

            // We need to get rid of the get_ prefix and Distributed suffix.
            string propertyName = d.Method.Name.Substring(4, d.Method.Name.LastIndexOf("__Distributed") - 4);

            // Get our local node and invoke the get property.
            return node.GetProperty(objectName, propertyName);
        }

        public static object AddEvent(Delegate d, object[] args)
        {
            // Invoke directly if not networked.
            var node = (d.Target as ITransparent).Node;
            if (node == null)
                return DpmEntrypoint.InvokeDynamic(d, args);

            // Get the network name of the object.
            string objectName = (d.Target as ITransparent).NetworkName;

            // We need to get rid of the add_ prefix and Distributed suffix.
            string eventName = d.Method.Name.Substring(4, d.Method.Name.LastIndexOf("__Distributed") - 4);
            Delegate handler = args[0] as Delegate;
            ID agreedref = null;
            if (handler.Target != null)
                agreedref = EventTransport.GetAgreedReference(node.Processor.AgreedReferences, handler.Target);

            // Construct the event transport information.
            EventTransport ev = new EventTransport
            {
                SourceObjectNetworkName = objectName,
                SourceEventName = eventName,
                SourceEventType = handler.Method.GetParameters()[1].ParameterType.FullName,
                ListenerAgreedReference = agreedref,
                ListenerNodeID = node.ID,
                ListenerType = handler.Method.DeclaringType.FullName,
                ListenerMethod = handler.Method.Name
            };

            // Get our local node and invoke the add event.
            node.AddEvent(ev);

            return null;
        }

        public static object RemoveEvent(Delegate d, object[] args)
        {
            // Invoke directly if not networked.
            var node = (d.Target as ITransparent).Node;
            if (node == null)
                return DpmEntrypoint.InvokeDynamic(d, args);

            // Get the network name of the object and the name of the method.
            string objectName = (d.Target as ITransparent).NetworkName;

            // We need to get rid of the remove_ prefix and Distributed suffix.
            string eventName = d.Method.Name.Substring(7, d.Method.Name.LastIndexOf("__Distributed") - 7);
            Delegate handler = args[0] as Delegate;
            ID agreedref = null;
            if (handler.Target != null)
                agreedref = EventTransport.GetAgreedReference(node.Processor.AgreedReferences, handler);

            // Construct the event transport information.
            EventTransport ev = new EventTransport
            {
                SourceObjectNetworkName = objectName,
                SourceEventName = eventName,
                SourceEventType = handler.Method.GetParameters()[1].ParameterType.FullName,
                ListenerAgreedReference = agreedref,
                ListenerNodeID = node.ID,
                ListenerType = handler.Method.DeclaringType.FullName,
                ListenerMethod = handler.Method.Name
            };

            // Get our local node and invoke the remove event.
            node.RemoveEvent(ev);

            return null;
        }

        public static object Invoke(Delegate d, object[] args)
        {
            // Invoke directly if not networked.
            var node = (d.Target as ITransparent).Node;
            if (node == null)
                return DpmEntrypoint.InvokeDynamic(d, args);

            // Get the network name of the object and the name of the method.
            string objectName = (d.Target as ITransparent).NetworkName;
            string methodName = d.Method.Name;

            // Get our local node and invoke the method.
            object o = node.Invoke(objectName, methodName, d.Method.GetGenericArguments(), args);
            return o;
        }

        public static void Construct(object obj)
        {
            // We need to use some sort of thread static variable; when the post-processor
            // wraps methods, it also needs to update them so that calls to new are adjusted
            // so the thread static variable gets set to the object's current node.  Then we
            // pull the information out here to reassign it.
            //
            // If there's no node context in the thread static variable, then that means someone
            // is new'ing up a distributed object from outside a distributed scope, and they
            // haven't used the Distributed<> class to create it.
            if (DpmConstructContext.LocalNodeContext == null)
                throw new InvalidOperationException(
                    "Unable to determine current construction context for distributed object. " +
                    "You can only new distributed objects directly from inside other " +
                    "distributed objects.  For construction of a distributed object " +
                    "from a local context, use the Distributed<> class.");
        
            var node = DpmConstructContext.LocalNodeContext;

            // Check to see if we've already got a NetworkName; if we have
            // then we're a named distributed instance that doesn't need the
            // autoid assigned.
            if ((obj as ITransparent).NetworkName != null)
                return;

            // Allocate a randomly generated NetworkName.
            (obj as ITransparent).Node = DpmConstructContext.LocalNodeContext;
            (obj as ITransparent).NetworkName = "autoid-" + ID.NewRandom();

            // Store the object in the Dht.
            node.Storage.Store((obj as ITransparent).NetworkName, obj);
            
            // Reset the context automatically.
            DpmConstructContext.LocalNodeContext = null;
        }

        public static void Serialize(object obj, SerializationInfo info, StreamingContext context)
        {
            // Loop through all of the private, internal fields in the object and
            // serialize all of their values.
            foreach (FieldInfo fi in obj.GetType().GetFields(BindingFlagsCombined.All))
            {
                if (fi.GetCustomAttributes(typeof(LocalAttribute), true).Length > 0)
                    continue;
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
            foreach (FieldInfo fi in obj.GetType().GetFields(BindingFlagsCombined.All))
            {
                if (fi.GetCustomAttributes(typeof(LocalAttribute), true).Length > 0)
                    continue;
                if (fi.FieldType.GetInterface("ITransparent") != null)
                {
                    // Deserialize the object based on the NetworkName.
                    if (info.GetValue(fi.Name, typeof(object)) != null)
                    {
                        object v = FormatterServices.GetUninitializedObject(fi.FieldType);
                        (v as ITransparent).NetworkName = info.GetString(fi.Name);
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

