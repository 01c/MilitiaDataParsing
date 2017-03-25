using System;
using System.Reflection;
using System.Collections.Generic;

namespace MilitiaDataParsing
{
    /// <summary>
    /// Represents a handler for a parser.
    /// </summary>
    public class ParserHandler
    {
        #region Properties
        /// <summary>
        /// Occurs when the handler or its parser ejects information on warnings or errors. A EventHandler can be provided through the constructor to register possible initializing errors.
        /// </summary>
        public event EventHandler<OutputEventArgs> Output;
        #endregion Properties

        #region Fields
        private Parser parser;
        internal List<StoredType> storedTypes;
        #endregion Fields

        /// <summary>
        /// Initialize a parser handler using a specific parser and provided EventHandler for listening to Output.
        /// </summary>
        /// <param name="parser">Parser to be used.</param>
        /// <param name="onOutput">EventHandler to be registered to Output.</param>
        public ParserHandler(Parser parser, EventHandler<OutputEventArgs> onOutput)
        {
            if (onOutput != null)
                Output += onOutput;

            this.parser = parser;
            this.parser.Initialize(this);
            storedTypes = new List<StoredType>();

            Type iParsable = typeof(IParsable);

            // Find all types which are assignable from IParsable and store them.
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (iParsable.IsAssignableFrom(type) && !type.IsInterface)
                    {
                        IndexType(type);
                    }
                }
            }
        }

        /// <summary>
        /// Initialize a parser handler using the default parser with a provided EventHandler for listening to Output.
        /// </summary>
        /// <param name="onOutput">EventHandler to be registered to Output.</param>
        public ParserHandler(EventHandler<OutputEventArgs> onOutput) : this(new Parser(), onOutput) { }

        /// <summary>
        /// Initialize a parser handler using the default parser.
        /// </summary>
        public ParserHandler() : this(new Parser(), null) { }

        /// <summary>
        /// If you're having problems importing embedded objects which derive from the variable type, try indexing them with this method first.
        /// </summary>
        /// <param name="type">Type to store.</param>
        public void IndexType(Type type)
        {
            // Make sure type is not already stored.
            foreach (StoredType storedType in storedTypes)
            {
                if (storedType.Type == type)
                {
                    OnOutput("Type \"" + type.ToString() + "\" is already stored. No need to store it manually.", null);
                    return;
                }
            }

            IParsable parsable = (IParsable)(Activator.CreateInstance(type));
            string header = GetIParsableHeader(parsable);
            // Make sure no other type shares the same header.
            foreach (StoredType storedType in storedTypes)
            {
                if (storedType.Header == header)
                {
                    OnOutput("Type \"" + type.ToString() + "\" shares header with \"" + storedType.Type.ToString() + "\" and cannot be stored. Please provide another header.", null);
                    return;
                }
            }

            storedTypes.Add(new StoredType(type, header));
        }

        /// <summary>
        /// Creates and returns a new object from source data.
        /// </summary>
        /// <param name="data">Source data.</param>
        public T Import<T>(string data) where T : IParsable, new()
        {
            T obj = (T)ImportProcess(data, typeof(T));
            parser.Reset();
            return obj;
        }

        /// <summary>
        /// Updates a provided object with source data.
        /// </summary>
        /// <param name="data">Source data.</param>
        /// <param name="obj">Existing object.</param>
        public void Update(string data, IParsable obj)
        {
            parser.Process(data, Task.Importing, obj);
            parser.Reset();
        }

        /// <summary>
        /// Returns source data for the provided object.
        /// </summary>
        /// <param name="obj">Existing object.</param>
        public string Export(IParsable obj)
        {
            string data = ExportProcess(obj);
            parser.Reset();
            return data;
        }

        internal void OnOutput(string message, Exception exception)
        {
            if (Output != null)
                Output(this, new OutputEventArgs(message, exception));
        }

        internal virtual string GetIParsableHeader(IParsable parsable)
        {
            return parsable.Header != null ? parsable.Header : parsable.GetType().Name;
        }

        internal IParsable ImportProcess(string data, Type type)
        {
            IParsable newObject = null;
            try
            {
                newObject = (IParsable)Activator.CreateInstance(type);
            }
            catch (Exception exception)
            {
                OnOutput("Failed creating instance of \"" + type.ToString() + "\". Make sure a public parameterless constructor exists.", exception);
                return newObject;
            }
            return parser.Process(data, Task.Importing, newObject);
        }

        internal string ExportProcess(IParsable obj)
        {
            parser.Process("", Task.Exporting, obj);
            return parser.buffer.output;
        }
    }
}
