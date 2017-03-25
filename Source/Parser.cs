using System;
using System.Collections.Generic;

namespace MilitiaDataParsing
{
    /// <summary>
    /// Current task of a parser.
    /// </summary>
    public enum Task
    {
        /// <summary>
        /// Indicates that the parser is currently not parsing any data.
        /// </summary>
        Sleeping,
        /// <summary>
        /// Indicates that the parser is currently importing data.
        /// </summary>
        Importing,
        /// <summary>
        /// Indicates that the parser is currently exporting data.
        /// </summary>
        Exporting
    }

    /// <summary>
    /// Represents a parser and its logic used to export and import data.
    /// </summary>
    public class Parser
    {
        #region Properties
        /// <summary>
        /// States the current task of the parser.
        /// </summary>
        public Task Task { get; private set; }
        #endregion Properties

        #region Fields
        internal ParseBuffer buffer;
        private string mainContainer;
        private ParserHandler handler;

        private string[] stringValues;
        private const string stringValueReferenceSymbol = "@";

        /// <summary>
        /// Symbol to surround string values.
        /// </summary>
        protected string stringSymbol = "\"";

        /// <summary>
        /// Indicates the start of a new container.
        /// </summary>
        protected string containerHeaderSymbol = "{";

        /// <summary>
        /// Indicates the end of a container.
        /// </summary>
        protected string containerFooterSymbol = "}";
        #endregion Fields

        /// <summary>
        /// Called when importing values. Override to extend parsing capabilities.
        /// </summary>
        /// <param name="data">The value loaded from source data.</param>
        protected virtual T TryParse<T>(string data)
        {
            return default(T);
        }

        internal void Initialize(ParserHandler handler)
        {
            this.handler = handler;
            Reset();
        }

        private void Output(string message, Exception exception)
        {
            handler.OnOutput("Error occured loading \"" + mainContainer + "\". " + message, exception);
        }

        #region Syntax
        protected virtual string ContainerHeaderSyntax(string key, string symbol)
        {
            return key + " " + symbol + Environment.NewLine;
        }

        protected virtual string ContainerFooterSyntax(string key, string symbol)
        {
            return symbol + Environment.NewLine;
        }

        protected virtual string KeyHeaderSyntax(string key)
        {
            return key + " = ";
        }

        protected virtual string KeyFooterSyntax(string key)
        {
            return Environment.NewLine;
        }

        protected virtual string EmbeddedObjectKeywordSyntax()
        {
            return "new ";
        }

        private string ContainerHeader(string key)
        {
            return Case(ContainerHeaderSyntax(key, containerHeaderSymbol));
        }

        private string ContainerFooter(string key)
        {
            return Case(ContainerFooterSyntax(key, containerFooterSymbol));
        }

        private string KeyHeader(string key)
        {
            return Case(KeyHeaderSyntax(key));
        }

        private string KeyFooter(string key)
        {
            return Case(KeyFooterSyntax(key));
        }

        private string EmbeddedObjectKeyword()
        {
            return Case(EmbeddedObjectKeywordSyntax());
        }

        private string Escaped(string value)
        {
            return @"\" + value;
        }

        private string Case(string text)
        {
            // Ignore case when loading.
            if (Task == Task.Importing && true)
                return text.ToLower();
            return text;
        }
        #endregion Syntax

        #region Logic
        internal void Reset()
        {
            stringValues = null;
            this.Task = Task.Sleeping;
        }

        internal IParsable Process(string mainContext, Task mode, IParsable obj)
        {
            buffer = new ParseBuffer();
            Task = mode;

            // Index string values.
            if (mode == Task.Importing && stringValues == null)
            {
                List<string> strings = new List<string>();
                int stringIndex = 0;
                // Search for strings.
                int index = -1;
                do
                {
                    // Find start of string.
                    index = FindStringSymbol(0, mainContext);
                    if (index != -1)
                    {
                        // Find end of string.
                        int endIndex = FindStringSymbol(index + 1, mainContext);
                        if (endIndex != -1)
                        {
                            // Restore escaped string symbols if any and store string.
                            strings.Add(mainContext.Substring(index + 1, endIndex - index - 1).Replace(Escaped(stringSymbol), stringSymbol));

                            // Replace string with a reference key.
                            mainContext = mainContext.Substring(0, index) + stringValueReferenceSymbol + stringIndex + mainContext.Substring(endIndex + 1);

                            stringIndex++;
                        }
                    }
                } while (index != -1);

                stringValues = new string[strings.Count];
                strings.CopyTo(stringValues);
            }

            buffer.contexts.Add(mainContext);

            // Set appropriate context before parsing.
            mainContainer = handler.GetIParsableHeader(obj);
            if (SetContext(mainContainer))
            {
                obj.Parsing(this);
                EndContext(mainContainer);
            }
            return obj;
        }

        private int FindStringSymbol(int index, string context)
        {
            do
            {
                // Find start of string.
                index = context.IndexOf(stringSymbol, index);

                if (index != -1)
                {
                    // Isn't escaped string symbol.
                    if (!context.Substring(index - 1).StartsWith(Escaped(stringSymbol)))
                        return index;
                    else
                        index++;
                }
            } while (index != -1);

            return -1;
        }

        private void Write(string text)
        {
            buffer.output += Indentation() + text;
        }

        private string Indentation()
        {
            string indents = "";
            for (int i = 0; i < buffer.indentIndex; i++)
                indents += "\t";
            return indents;
        }

        private bool SetContext(string key)
        {
            switch (Task)
            {
                case Task.Importing:
                    string newContext = GetContainerContents(key, buffer.currentContext);
                    if (newContext != null)
                    {
                        buffer.contexts.Add(newContext);
                        // Move to the newly created context.
                        buffer.contextIndex++;
                        return true;
                    }
                    return false;
                case Task.Exporting:
                    Write(ContainerHeader(key));
                    buffer.indentIndex++;
                    break;
            }
            return true;
        }

        private void EndContext(string key)
        {
            switch (Task)
            {
                case Task.Importing:
                    buffer.contextIndex--;
                    buffer.contexts.RemoveAt(buffer.contexts.Count - 1);
                    break;
                case Task.Exporting:
                    buffer.indentIndex--;
                    Write(ContainerFooter(key));
                    break;
            }
        }

        private int GetContainerLength(string key, string context)
        {
            string header = ContainerHeader("");
            string footer = ContainerFooter(key);

            int totalLength = 0;
            int currentIndention = 0;

            int iHeader = -1, iFooter = -1;
            do
            {
                iHeader = context.IndexOf(header);
                iFooter = context.IndexOf(footer);

                int newIndex = 0;
                // Header found AND (footer NOT found OR header found before footer).
                if (iHeader != -1 && (iFooter == -1 || iHeader < iFooter))
                {
                    newIndex = iHeader + 1;
                    currentIndention++;
                }
                // Footer found AND (header NOT found OR footer found before start).
                else if (iFooter != -1 && (iHeader == -1 || iFooter < iHeader))
                {
                    newIndex = iFooter + 1;
                    currentIndention--;
                    // Reached container end, return length.
                    if (currentIndention <= 0)
                        return totalLength + newIndex - 1;
                }
                else
                    break;

                // More context forward.
                context = context.Substring(newIndex);

                // Increase total length.
                totalLength += newIndex;
            }
            while (iHeader != -1 || iFooter != -1);

            Output("Failed retrieving length of \"" + key + "\" container.", null);
            return -1;
        }

        private string GetContainerContents(string key, string context)
        {
            int headerLength = ContainerHeader(key).Length;
            int headerIndex = Case(context).IndexOf(ContainerHeader(key));
            if (headerIndex == -1)
            {
                Output("Couldn't find header for \"" + key + "\" key.", null);
                return null;
            }

            context = context.Substring(headerIndex);
            int containerLength = GetContainerLength(key, context);

            if (containerLength == -1)
                return null;

            return context.Substring(headerLength, containerLength - headerLength);
        }

        private string GetContainerFull(string key, string context)
        {
            return context.Substring(0, GetContainerLength(key, context) + ContainerFooter("").Length);
        }

        private int Occurences(string context, string key)
        {
            int occurences = -1, index = -1;
            do
            {
                occurences++;
                index = context.IndexOf(key, index + 1);
            } while (index != -1);

            return occurences;
        }

        private int IndexWithinContext(string context, string value)
        {
            int valueIndex = -1;
            do
            {
                valueIndex = context.IndexOf(value, valueIndex + 1);

                if (valueIndex == -1)
                    break;
                // Make sure value before key is tab.
                // To prevent picking up values from keys which end the same.
                else if (!context.Substring(valueIndex - 1).StartsWith("\t"))
                    continue;

                string between = context.Substring(0, valueIndex);

                // If amount of headers and footers between start of context and value are the same it means the value was found in the same context.
                if (Occurences(between, containerHeaderSymbol) == Occurences(between, containerFooterSymbol))
                    break;

            } while (valueIndex != -1);

            return valueIndex;
        }

        private string GetObject(string key, string context)
        {
            // Get object declaration.
            string newObjectDec = KeyHeader(key) + EmbeddedObjectKeyword();
            int newObjectIndex = context.IndexOf(newObjectDec);
            if (newObjectIndex == -1) return null;

            // Get container header.
            context = context.Substring(newObjectIndex + newObjectDec.Length);
            int headerIndex = context.IndexOf(ContainerHeader(""));
            if (headerIndex == -1) return null;

            // Get object key and contents.
            string objectKey = context.Substring(0, headerIndex);
            string objectContents = GetContainerFull(objectKey, context);

            return EmbeddedObjectKeyword() + objectContents;
        }

        private string WriteValue(string key, object value)
        {
            if (value == null)
                return null;

            string val = null;

            IParsable parsable = value as IParsable;
            // Writing a child object.
            if (parsable != null)
            {
                // Store current buffer.
                ParseBuffer mainBuffer = buffer;

                // Get source data for child object.
                val = EmbeddedObjectKeyword() + handler.ExportProcess(parsable);

                // Restore buffer.
                buffer = mainBuffer;

                // Properly indent child data.
                val = val.Replace(Environment.NewLine, Environment.NewLine + Indentation());
            }
            else
            {
                val = value.ToString();
            }

            Write(KeyHeader(key) + val + KeyFooter(key));
            return val;
        }

        private string ReadValue(string key)
        {
            string context = buffer.currentContext;

            // Search for start of key header.
            int iHeader = IndexWithinContext(Case(context), KeyHeader(key));

            // Not found, key doesn't exist in this context.
            if (iHeader == -1)
            {
                Output("Couldn't find key header for \"" + key + "\".", null);
                return null;
            }

            // Go to start of key header.
            context = context.Substring(iHeader);

            string firstLine = context.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)[0];
            // Is container.
            if (firstLine.Contains(containerHeaderSymbol))
            {
                // Using the commented code below seems to be working but it causes the parser to eject some errors.
                //if (SetContext(key))
                //{
                //    context = currentContext;
                //    EndContext(key);
                //}
                context = GetContainerFull(key, context);

                // Return object data if object.
                string objectData = GetObject(key, context);
                if (objectData != null)
                    return objectData;

                return context;
            }
            else
            {
                context = firstLine;

                // Go to end of key ender.
                context = context.Substring(KeyHeader(key).Length);

                // Remove key footer.
                context = context.Split(new string[] { KeyFooter(key) }, StringSplitOptions.RemoveEmptyEntries)[0];

                // Finally return value between end of key header and start of key footer.
                return context;
            }
        }

        private string ProcessValue(string key, object value)
        {
            switch (Task)
            {
                case Task.Importing:
                    return ReadValue(key);
                case Task.Exporting:
                    return WriteValue(key, value);
            }
            return null;
        }
        #endregion Logic

        #region Parsing front end
        /// <summary>
        /// Parses a list. To be called within Parsing method.
        /// <para>Usage: Property = List("key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">List to be processed.</param>
        public List<T> List<T>(string key, List<T> property)
        {
            switch (Task)
            {
                case Task.Importing:
                    List<T> list = new List<T>();
                    foreach (T rawValue in ParseList<T>(key))
                    {
                        list.Add(rawValue);
                    }
                    return list;
                case Task.Exporting:
                    if (SetContext(key))
                    {
                        for (int i = 0; i < property.Count; i++)
                        {
                            Auto(i.ToString(), property[i]);
                        }
                        EndContext(key);
                    }
                    return property;
            }
            return null;
        }

        /// <summary>
        /// Parse any value. To be called within Parsing method.
        /// <para>Usage: Auto("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        public void Auto<T>(string key, ref T field)
        {
            field = ParseAuto<T>(key, field);
        }

        /// <summary>
        /// Parse any value. To be called within Parsing method.
        /// <para>Usage: Property = Auto("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Field to be processed.</param>
        public T Auto<T>(string key, T property)
        {
            return ParseAuto<T>(key, property);
        }

        /// <summary>
        /// Parses a value. To be called within Parsing method.
        /// <para>Usage: String("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        public void Value(string key, ref object field)
        {
            field = Value(key, field);
        }

        /// <summary>
        /// Parses a value. To be called within Parsing method.
        /// <para>Usage: Property = String("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Property to be processed.</param>
        public string Value(string key, object property)
        {
            return ProcessValue(key, property);
        }

        /// <summary>
        /// Parses a bool value. To be called within Parsing method.
        /// <para>Usage: Bool("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        public void Bool(string key, ref bool field)
        {
            field = Bool(key, field);
        }

        /// <summary>
        /// Parses a bool value. To be called within Parsing method.
        /// <para>Usage: Property = Bool("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Property to be processed.</param>
        public bool Bool(string key, bool property)
        {
            return ParseBool(ProcessValue(key, property));
        }

        /// <summary>
        /// Parses a enum value. To be called within Parsing method.
        /// <para>Usage: Enum("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        public void Enum<T>(string key, ref T field) where T : struct, IComparable, IFormattable, IConvertible
        {
            field = Enum(key, field);
        }

        /// <summary>
        /// Parses a enum value. To be called within Parsing method.
        /// <para>Usage: Property = Enum("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Property to be processed.</param>
        public T Enum<T>(string key, T property)
        {
            return ParseEnum<T>(ProcessValue(key, property));
        }

        /// <summary>
        /// Parses a float value. To be called within Parsing method.
        /// <para>Usage: Float("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        public void Float(string key, ref float field)
        {
            field = Float(key, field);
        }

        /// <summary>
        /// Parses a float value. To be called within Parsing method.
        /// <para>Usage: Property = Float("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Property to be processed.</param>
        public float Float(string key, float property)
        {
            return ParseFloat(ProcessValue(key, property));
        }

        /// <summary>
        /// Parses a generic value. To be called within Parsing method.
        /// <para>Usage: Generic&lt;DataType&gt;("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        public void Generic<T>(string key, ref T field)
        {
            field = Generic<T>(key, field);
        }

        /// <summary>
        /// Parses a generic value. To be called within Parsing method.
        /// <para>Usage: Property = Generic&lt;DataType&gt;("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Field to be processed.</param>
        public T Generic<T>(string key, object property)
        {
            string processed = ProcessValue(key, property);
            if (Task == Task.Exporting)
                return (T)property;
            else
            {
                return ParseGeneric<T>(processed);
            }
        }

        /// <summary>
        /// Parses an int value. To be called within Parsing method.
        /// <para>Usage: Int("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        public void Int(string key, ref int field)
        {
            field = Int(key, field);
        }

        /// <summary>
        /// Parses an int value. To be called within Parsing method.
        /// <para>Usage: Property = Int("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Property to be processed.</param>
        public int Int(string key, int property)
        {
            return (int)ParseInt(ProcessValue(key, property));
        }

        /// <summary>
        /// Parses a string value. To be called within Parsing method.
        /// <para>Usage: String("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        public void String(string key, ref string field)
        {
            field = String(key, field);
        }

        /// <summary>
        /// Parses a string value. To be called within Parsing method.
        /// <para>Usage: Property = String("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Property to be processed.</param>
        public string String(string key, string property)
        {
            switch (Task)
            {
                case Task.Importing:
                    return ParseString(ProcessValue(key, property));
                case Task.Exporting:
                    ProcessValue(key, ParseString(property));
                    return property;
            }
            return null;
        }
        #endregion Parsing front end

        #region Parsing back end
        private List<T> ParseList<T>(string key)
        {
            List<T> list = new List<T>();
            if (SetContext(key))
            {
                int i = 0;
                int iHeader = -1;
                do
                {
                    // Find header for current element within context.
                    iHeader = IndexWithinContext(buffer.currentContext, KeyHeader(i.ToString()));
                    // Header exists.
                    if (iHeader != -1)
                    {
                        // Get value.
                        T value = ParseAuto<T>(i.ToString(), buffer.currentContext.Substring(iHeader));
                        if (value != null)
                            list.Add(value);
                        i++;
                    }
                } while (iHeader != -1);
                EndContext(key);
            }
            return list;
        }

        private T ParseAuto<T>(string key, object property)
        {
            Type type = typeof(T);

            // Bool.
            if (type == typeof(bool))
                return (T)(object)Bool(key, bool.Parse(property.ToString()));
            // Enum.
            else if (type.IsEnum)
                return Enum(key, (T)property);
            // Float.
            else if (type == typeof(float))
                return (T)(object)Float(key, float.Parse(property.ToString()));
            // Int.
            else if (type == typeof(int))
                return (T)(object)Int(key, int.Parse(property.ToString()));
            // String.
            else if (type == typeof(string))
                return (T)(object)String(key, property as string);
            // Generic.
            else
            {
                return Generic<T>(key, property);
            }
        }

        private bool ParseBool(string value)
        {
            if (value == null)
                return false;

            return bool.Parse(value);
        }

        private T ParseEnum<T>(string value)
        {
            return (T)System.Enum.Parse(typeof(T), value);
        }

        private float ParseFloat(string value)
        {
            if (value == null)
                return 0f;

            return float.Parse(value);
        }

        private T ParseGeneric<T>(string value)
        {
            if (value == "" || value == null)
                return default(T);

            // Is a new (parsable) object.
            if (value.StartsWith(EmbeddedObjectKeyword()) && typeof(IParsable).IsAssignableFrom(typeof(T)))
            {
                // Remove keyword before parsing.
                value = value.Substring(EmbeddedObjectKeyword().Length);

                Type type = typeof(T);
                // Check if object is inherited from type.
                foreach (StoredType storedType in handler.storedTypes)
                {
                    if (value.StartsWith(ContainerHeader(storedType.Header)) && type.IsAssignableFrom(storedType.Type))
                    {
                        type = storedType.Type;
                        break;
                    }
                }

                // Store current buffer.
                ParseBuffer mainBuffer = buffer;

                // Get object.
                IParsable obj = handler.ImportProcess(value, type);

                // Restore buffer.
                buffer = mainBuffer;

                return (T)obj;
            }
            else
            {
                T t = TryParse<T>(value);

                // Try normal conversion.
                if (t == null)
                {
                    try
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    catch (Exception exception)
                    {
                        Output("Failed converting " + value + " to " + typeof(T) + ". Try overriding TryParse() to extend parsing capabilities.", exception);
                    }
                }

                return t;
            }
        }

        private int? ParseInt(string value)
        {
            if (value == null)
                return null;

            return int.Parse(value);
        }

        private string ParseString(string value)
        {
            if (value == null)
                return null;

            switch (Task)
            {
                case Task.Importing:
                    if (value.StartsWith(stringValueReferenceSymbol))
                    {
                        int index = int.Parse(value.Substring(stringValueReferenceSymbol.Length));
                        return stringValues[index];
                    }
                    return value;
                case Task.Exporting:
                    value = value.Replace(stringSymbol, Escaped(stringSymbol));
                    value = "\"" + value + "\"";
                    return value;
            }
            return null;
        }
        #endregion Parsing back end
    }
}