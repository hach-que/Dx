using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Interfaces;
using Data4;
using Process4.Providers;
using System.Runtime.Serialization;

namespace Process4.Remoting
{
    [Serializable()]
    public class EventTransport : ISerializable
    {
        public string SourceObjectNetworkName { get; set; }
        public string SourceEventType { get; set; }
        public string SourceEventName { get; set; }

        public ID ListenerAgreedReference { get; set; }
        public string ListenerType { get; set; }
        public string ListenerMethod { get; set; }
        public ID ListenerNodeID { get; set; }
        
        public EventTransport()
        {
        }

        public EventTransport(SerializationInfo info, StreamingContext context)
        {
            this.SourceObjectNetworkName = info.GetValue("transport.srcobjname", typeof(string)) as string;
            this.SourceEventType = info.GetValue("transport.srcevtype", typeof(string)) as string;
            this.SourceEventName = info.GetValue("transport.srcevname", typeof(string)) as string;
            this.ListenerAgreedReference = info.GetValue("transport.listenagreedref", typeof(ID)) as ID;
            this.ListenerType = info.GetValue("transport.listentype", typeof(string)) as string;
            this.ListenerMethod = info.GetValue("transport.listenmethod", typeof(string)) as string;
            this.ListenerNodeID = info.GetValue("transport.listennodeid", typeof(ID)) as ID;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("transport.srcobjname", this.SourceObjectNetworkName, typeof(string));
            info.AddValue("transport.srcevtype", this.SourceEventType, typeof(string));
            info.AddValue("transport.srcevname", this.SourceEventName, typeof(string));
            info.AddValue("transport.listenagreedref", this.ListenerAgreedReference, typeof(ID));
            info.AddValue("transport.listentype", this.ListenerType, typeof(string));
            info.AddValue("transport.listenmethod", this.ListenerMethod, typeof(string));
            info.AddValue("transport.listennodeid", this.ListenerNodeID, typeof(ID));
        }

        /// <summary>
        /// Gets a new or existing agreed reference for the specified object using the
        /// specified distributed processing module.
        /// </summary>
        /// <param name="dpm">The distributed processing module,</param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ID GetAgreedReference(Dictionary<ID, object> agreedrefs, object obj)
        {
            if (agreedrefs.Values.Contains(obj))
                return agreedrefs.Where(value => value.Value == obj).First().Key;
            else
            {
                ID key = ID.NewRandom();
                agreedrefs.Add(key, obj);
                return key;
            }
        }

        /// <summary>
        /// Creates a delegate that will invoke this event transport when the delegate
        /// is called (by the event).
        /// </summary>
        public EventHandler CreateRemotedDelegate()
        {
            return (sender, e) =>
                {
                    // Check to see if we are the target that the event should invoke on.
                    if (LocalNode.Singleton.ID == this.ListenerNodeID)
                    {
                        // Invoke locally.
                        LocalNode.Singleton.Processor.InvokeEvent(this, sender, e);
                    }
                    else
                    {
                        // Locate the contact to invoke the event callbacks on.
                        Contact c = LocalNode.Singleton.Contacts.FirstOrDefault(value => value.Identifier == this.ListenerNodeID);
                        if (c == null) return;

                        // Get the remote node and invoke the event.
                        RemoteNode node = new RemoteNode(c);
                        node.InvokeEvent(this, sender, e);
                    }


                    /*Type t = Type.GetType(transport.ListenerMethod);
                    MethodInfo tt = t.GetMethod(methodname, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                    Delegate handler = Delegate.CreateDelegate(mi.GetParameters()[0].GetType(), null, tt);*/
                };
        }
    }
}
