using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Process4.Task
{
    public class PostProcessingException : Exception
    {
        public string OffendingType { get; set; }
        public string OffendingMember { get; set; }

        public PostProcessingException(string offendingType, string offendingMember, string message)
            : base(message)
        {
            this.OffendingType = offendingType;
            this.OffendingMember = offendingMember;
        }
    }
}
