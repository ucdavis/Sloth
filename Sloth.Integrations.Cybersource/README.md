Most of the classes in this library are generated either from a DTD file or XSD file. The CreateClasses.bat script is used to generate the files.

The Common.xsd file is modified from the standard provided by CyberSource to improve the usability of the generated classes. You can inspect this change via it's git history.
Specifically, we prefer xs:complexType>xs:all over xs:complexType>xs:choice because it generates specific fields instead of an array of objects.
