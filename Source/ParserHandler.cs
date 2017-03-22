using System;

namespace MilitiaDataParsing
{
    public class ParserHandler
    {
        private Parser parser;

        public ParserHandler(Parser parser)
        {
            this.parser = parser;
            this.parser.Initialize(this);
        }

        public ParserHandler() : this(new Parser()) { }

        /// <summary>
        /// Creates a new object from source data.
        /// </summary>
        /// <param name="data">Source data.</param>
        public T Import<T>(string data) where T : IParsable, new()
        {
            parser.Reset();
            return ImportProcess<T>(data);
        }

        /// <summary>
        /// Updates an existing object with source data.
        /// </summary>
        /// <param name="data">Source data.</param>
        /// <param name="obj">Existing object.</param>
        public void Update(string data, IParsable obj)
        {
            parser.Reset();
            parser.Process(data, Mode.Load, obj);
        }

        /// <summary>
        /// Creates source data for an existing object.
        /// </summary>
        /// <param name="obj">Object.</param>
        public string Export(IParsable obj)
        {
            parser.Reset();
            return ExportProcess(obj);
        }

        internal T ImportProcess<T>(string data)
        {
            T newObject = default(T);
            try
            {
                newObject = Activator.CreateInstance<T>();
            }
            catch (Exception exception)
            {
                parser.OnErrorOccured("Failed creating type of \"" + typeof(T).ToString() + "\". Make sure a public parameterless constructor exists.", exception);
                return newObject;
            }
            return (T)parser.Process(data, Mode.Load, newObject as IParsable);
        }

        internal string ExportProcess(IParsable obj)
        {
            parser.Process("", Mode.Save, obj);
            return parser.buffer.output;
        }
    }
}
