using System;

namespace XRepo.Core
{
    public class XRepoException : Exception
    {
        public XRepoException(string message) : base(message)
        {
        }

        public XRepoException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}