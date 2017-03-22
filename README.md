# MilitiaDataParsing
A simple framework for exporting and importing objects with a customizable file format. Intended for data files which need to be readable and easily edited by hand.

Features:
- Data can be converted using one parser for importing and another one with different settings for exporting.
- Supports fields, properties, lists, generics and embedded objects.

Pros:
- Tags can be redefined at run-time.
- Tags in source data don't need to match names in runtime.
- Extra functionality can be called depending if object is being exported or imported.

Cons:
- One call per value to parse.
- Not thoroughly tested.

## Minimal implementation
```C#
using System;
using MilitiaDataParsing;

namespace Example
{    
    class Program
    {
        static void Main(string[] args)
        {
            ParserHandler parser = new ParserHandler();
            Parser.ErrorOccured += Parser_ErrorOccured;
            
            // Create new object and store its data.
            MyData dataObject = new MyData();
            dataObject.Text = "Parsing";
            string exportedData = parser.Export(dataObject);
            Console.WriteLine("Exported data (1):" + Environment.NewLine + exportedData);

            // Load new object from data of previous object.
            MyData loadedDataObject = parser.Import<MyData>(exportedData);
            exportedData = parser.Export(loadedDataObject);
            Console.WriteLine("Exported data (2):" + Environment.NewLine + exportedData);

            Console.ReadLine();
        }
        
        private static void Parser_ErrorOccured(object sender, ErrorOccuredEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
        
        public class MyData : IParsable
        {
            public string Text { get; set; }

            public string Header { get { return "my_data"; } }

            public void Parsing(Parser p)
            {
                Text = p.Auto("text", Text);
            }
        }
    }
}
