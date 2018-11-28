using System;

namespace NUnrar
{
#if SILVERLIGHT || PORTABLE
    public class RarException : Exception
#else
    public class RarException : ApplicationException
#endif
    {
        public RarException(string message)
            : base(message)
        {
        }

        public RarException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
