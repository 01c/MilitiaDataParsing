using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MilitiaDataParsing
{
    public partial class Parser
    {
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
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public void Auto<T>(string key, ref T field, bool required = false)
        {
            field = ParseAuto<T>(key, field, required);
        }

        /// <summary>
        /// Parse any value. To be called within Parsing method.
        /// <para>Usage: Property = Auto("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Field to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public T Auto<T>(string key, T property, bool required = false)
        {
            return ParseAuto<T>(key, property, required);
        }

        /// <summary>
        /// Parses a value. To be called within Parsing method.
        /// <para>Usage: String("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public void Value(string key, ref object field, bool required = false)
        {
            field = Value(key, field, required);
        }

        /// <summary>
        /// Parses a value. To be called within Parsing method.
        /// <para>Usage: Property = String("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Property to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public string Value(string key, object property, bool required = false)
        {
            return ProcessValue(key, property, required);
        }

        /// <summary>
        /// Parses a bool value. To be called within Parsing method.
        /// <para>Usage: Bool("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public void Bool(string key, ref bool field, bool required = false)
        {
            field = Bool(key, field, required);
        }

        /// <summary>
        /// Parses a bool value. To be called within Parsing method.
        /// <para>Usage: Property = Bool("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Property to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public bool Bool(string key, bool property, bool required = false)
        {
            return ParseBool(ProcessValue(key, property, required));
        }

        /// <summary>
        /// Parses a enum value. To be called within Parsing method.
        /// <para>Usage: Enum("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public void Enum<T>(string key, ref T field, bool required = false) where T : struct, IComparable, IFormattable, IConvertible
        {
            field = Enum(key, field, required);
        }

        /// <summary>
        /// Parses a enum value. To be called within Parsing method.
        /// <para>Usage: Property = Enum("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Property to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public T Enum<T>(string key, T property, bool required = false)
        {
            return ParseEnum<T>(ProcessValue(key, property, required));
        }

        /// <summary>
        /// Parses a float value. To be called within Parsing method.
        /// <para>Usage: Float("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public void Float(string key, ref float field, bool required = false)
        {
            field = Float(key, field, required);
        }

        /// <summary>
        /// Parses a float value. To be called within Parsing method.
        /// <para>Usage: Property = Float("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Property to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public float Float(string key, float property, bool required = false)
        {
            return ParseFloat(ProcessValue(key, property, required));
        }

        /// <summary>
        /// Parses a generic value. To be called within Parsing method.
        /// <para>Usage: Generic&lt;DataType&gt;("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public void Generic<T>(string key, ref T field, bool required = false)
        {
            field = Generic<T>(key, field, required);
        }

        /// <summary>
        /// Parses a generic value. To be called within Parsing method.
        /// <para>Usage: Property = Generic&lt;DataType&gt;("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Field to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public T Generic<T>(string key, object property, bool required = false)
        {
            string processed = ProcessValue(key, property, required);
            if (Task == Task.Exporting)
                return (T)property;
            else
            {
                return ImportGeneric<T>(processed, property);
            }
        }

        /// <summary>
        /// Parses an int value. To be called within Parsing method.
        /// <para>Usage: Int("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public void Int(string key, ref int field, bool required = false)
        {
            field = Int(key, field, required);
        }

        /// <summary>
        /// Parses an int value. To be called within Parsing method.
        /// <para>Usage: Property = Int("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Property to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public int Int(string key, int property, bool required = false)
        {
            return (int)ParseInt(ProcessValue(key, property, required));
        }

        /// <summary>
        /// Parses a string value. To be called within Parsing method.
        /// <para>Usage: String("field_key", field);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="field">Field to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public void String(string key, ref string field, bool required = false)
        {
            field = String(key, field, required);
        }

        /// <summary>
        /// Parses a string value. To be called within Parsing method.
        /// <para>Usage: Property = String("property_key", Property);</para>
        /// </summary>
        /// <param name="key">Key in source data.</param>
        /// <param name="property">Property to be processed.</param>
        /// <param name="required">Will return warnings if required and value could not be processed.</param>
        public string String(string key, string property, bool required = false)
        {
            switch (Task)
            {
                case Task.Importing:
                    return ParseString(ProcessValue(key, property, required));
                case Task.Exporting:
                    ProcessValue(key, ParseString(property), required);
                    return property;
            }
            return null;
        }
    }
}
