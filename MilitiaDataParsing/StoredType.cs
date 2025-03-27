using System;

namespace MilitiaDataParsing
{
    internal class StoredType
    {
        internal Type Type { get; set; }
        internal string Header { get; set; }

        internal StoredType(Type type, string header)
        {
            Type = type;
            Header = header;
        }
    }
}
