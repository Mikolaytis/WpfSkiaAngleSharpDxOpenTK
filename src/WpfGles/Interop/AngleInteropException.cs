using System;
using System.Runtime.Serialization;

namespace WpfGles.Interop
{
    /// <summary>
    /// An exception that indicates that something went wrong with 
    /// Angle Interop.
    /// </summary>
    public class AngleInteropException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AngleInteropException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AngleInteropException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected AngleInteropException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}