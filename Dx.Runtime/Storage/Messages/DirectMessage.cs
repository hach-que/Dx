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
using System.Runtime.Serialization;

namespace Data4
{
    [Serializable()]
    public class DirectMessage : Message, ISerializable
    {
        private Contact p_Target = null;

        public DirectMessage(Dht dht, Contact target, string data) : base(dht, data)
        {
            this.p_Target = target;
        }

        public DirectMessage(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Sends the direct message to it's recipient.
        /// </summary>
        public DirectMessage Send()
        {
            return base.Send(this.p_Target) as DirectMessage;
        }

        /// <summary>
        /// Clones the direct message.
        /// </summary>
        protected override Message Clone()
        {
            DirectMessage dm = new DirectMessage(Dht, this.p_Target, this.Data);
            return dm;
        }

        public Contact Target
        {
            get { return this.p_Target; }
        }
    }
}

