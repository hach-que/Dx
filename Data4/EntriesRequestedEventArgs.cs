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

namespace Data4
{
    public class EntriesRequestedEventArgs : EventArgs
    {
        private Dictionary<ID, object> p_Entries = new Dictionary<ID,object>();

        public EntriesRequestedEventArgs()
        {
        }

        public Dictionary<ID, object> Entries
        {
            get { return this.p_Entries; }
        }
    }
}

