using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Process4.Task
{
    internal interface IWrapper
    {
        StreamWriter Log { get; set; }
        void Wrap();
    }
}
