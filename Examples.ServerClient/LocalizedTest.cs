using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Process4.Attributes;

namespace Examples.ServerClient
{
    [Distributed]
    public class LocalizedTest
    {
        [Local]
        public static Random m_Random;
        [Local]
        public static Random p_Random { get; set; }
        [Local]
        public Random mi_Random;
        [Local]
        public Random pi_Random;
        [Local]
        public IInterfaceTest m_Interface;
        [Local]
        public IInterfaceTest p_Interface { get; set; }
    }
}
