namespace MilitiaDataParsing
{
    /// <summary>
    /// Defines methods which are required to parse the object.
    /// </summary>
    public interface IParsable
    {
        /// <summary>
        /// Source data header for this object type. Will use type name if null.
        /// <para>Example: { get { return null; } }</para>
        /// <para>Example: { get { return "data_type_name"; } }</para>
        /// </summary>
        string Header { get; }

        /// <summary>
        /// Called when importing or exporting and is used connect all fields or properties with their specified keys. Not intended to be called manually.
        /// </summary>
        /// <param name="parse">Parser containing methods for parsing values.</param>
        void Parsing(Parser parse);
    }
}
