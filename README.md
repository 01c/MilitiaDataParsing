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
- Not optimized for real-time use.

See Example/Program.cs for implementation.
