using System.Collections.Generic;

namespace MilitiaDataParsing
{
    public class ParseBuffer
    {
        internal int indentIndex;
        internal string output;

        public Mode mode { get; set; }

        internal List<string> contexts;
        internal int contextIndex;
        internal string currentContext
        {
            get { return contexts[contextIndex]; }
            set { contexts[contextIndex] = value; }
        }

        public ParseBuffer()
        {
            contexts = new List<string>();
            contextIndex = 0;
            output = "";
            indentIndex = 0;
        }
    }
}