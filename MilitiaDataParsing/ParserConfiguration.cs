namespace MilitiaDataParsing
{
    public class ParserConfiguration
    {
        /// <summary>
        /// Symbol to indicate a string value.
        /// </summary>
        public string StringReference { get; set; }

        /// <summary>
        /// Symbol to surround string values.
        /// </summary>
        public string StringBlock { get; set; }

        /// <summary>
        /// Indicates the start of a new container.
        /// </summary>
        public string ContainerHeader { get; set; }

        /// <summary>
        /// Indicates the end of a container.
        /// </summary>
        public string ContainerFooter { get; set; }

        public ParserConfiguration()
        {
            StringReference = "@";
            StringBlock = "\"";
            ContainerHeader = "{";
            ContainerFooter = "}";
        }
    }
}
