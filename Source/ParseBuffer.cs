using System.Collections.Generic;

namespace MilitiaDataParsing
{
    internal class ParseBuffer
    {
        #region Fields
        internal int indentIndex;
        internal string output;

        internal List<string> contexts;
        internal int contextIndex;
        internal string currentContext
        {
            get { return contexts[contextIndex]; }
            set { contexts[contextIndex] = value; }
        }
        #endregion Fields

        internal ParseBuffer()
        {
            contexts = new List<string>();
            contextIndex = 0;
            output = "";
            indentIndex = 0;
        }
    }
}