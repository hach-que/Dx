// 
//  Copyright 2010  Trust4 Developers
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace Data4
{
    [Serializable()]
    public class BroadcastMessage : Message, ISerializable
    {
        public BroadcastMessage(Dht dht, string data) : base(dht, data)
        {
        }

        public BroadcastMessage(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public BroadcastMessage Send()
        {
            // Prevent rebroadcasting.
                        /*if (this.SeenBy(this.Dht.Self))
            {
                this.Dht.Log(Dht.LogType.INFO, "Not rebroadcasting the same message.");
                return new BroadcastMessage(this.Dht, this.Data);
            }
            
            // Broadcast.
            BroadcastMessage bm = null;
            foreach (Contact c in this.p_Contacts)
            {
                UdpClient udp = new UdpClient();
                using (MemoryStream writer = new MemoryStream())
                {
                    this.Dht.Log(Dht.LogType.INFO, "Sending message '" + this.ToString() + "'.");
                    this.Dht.Formatter.Serialize(writer, this);
                    udp.Send(writer.GetBuffer(), writer.GetBuffer().Length, c.EndPoint);
                }
            }*/
            return null;
        }

        protected override Message Clone()
        {
            return null;
        }
    }
}

