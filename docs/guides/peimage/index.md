# Overview

The PE image layer is the second layer of abstraction of the portable
executable (PE) file format. It is mostly represented by `PEImage`, and
works on top of the `PEFile` layer. Its main purpose is to provide access
to mutable models that are easier to use than the raw data structures the
PE file layer exposes.
