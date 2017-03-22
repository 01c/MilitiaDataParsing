using System;

namespace MilitiaDataParsing
{
    public class ErrorOccuredEventArgs : EventArgs
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public ErrorOccuredEventArgs(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }
    }
}