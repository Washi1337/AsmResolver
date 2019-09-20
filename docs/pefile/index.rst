The PE File layer
=================

The PE file layer is the lowest level of abstraction of the portable executable (PE) file format. It's main purpose is to read and write raw executable files from and to the disk. It is mainly represented by the **PEFile** class, and provides access to the raw top-level PE headers, including the DOS header, COFF file header and optional header. It also exposes for each section the section header and raw contents, which can be read by the means of an **IBinaryStreamReader** instance. 

It is important to note that this layer mainly leaves the interpretation of the data to the user. You will not find any methods or properties returning models of what is stored in these sections. Interpretations of e.g. the import directory can be found one layer up, using the **PEImage** class.

Because of this primitive nature, many will probably not find this layer all that useful. If, however, you need to change anything in the PE headers that other layers of abstraction do not provide (like adding extra PE sections), this layer of abstraction can be very useful as the final step in the pipeline.