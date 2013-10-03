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

namespace Data4
{
    public class MessageEventArgs : EventArgs
    {
        private Message p_Message = null;
        private bool p_SendConfirmation = true;

        public MessageEventArgs(Message message)
        {
            this.p_Message = message;
        }

        public Message Message
        {
            get { return this.p_Message; }
        }

        public bool SendConfirmation
        {
            get { return this.p_SendConfirmation; }
            set { this.p_SendConfirmation = value; }
        }
    }
}

