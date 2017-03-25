using System;

namespace MilitiaDataParsing
{
    /// <summary>
    /// Defines information about a parsing warning or error.
    /// </summary>
    public class OutputEventArgs : EventArgs
    {
        /// <summary>
        /// Message detailing the cause of warning or error.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The exception thrown causing the warning or error.
        /// </summary>
        public Exception Exception { get; private set; }

        internal OutputEventArgs(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }
    }
}