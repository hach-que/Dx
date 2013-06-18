using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Attributes;

namespace Examples.ServerClient
{
    [Distributed]
    public class InterfaceTest
    {
        public IInterfaceTest[] m_Test0 { get; set; }
        public IInterfaceTest m_Test1 { get; set; }
    }
}
