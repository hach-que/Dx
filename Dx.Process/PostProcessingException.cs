using System;

namespace Dx.Process
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
