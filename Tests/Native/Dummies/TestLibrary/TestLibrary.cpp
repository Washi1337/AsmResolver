// TestLibrary.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "TestLibrary.h"


// This is an example of an exported variable
TESTLIBRARY_API int nTestLibrary=0;

// This is an example of an exported function.
TESTLIBRARY_API int fnTestLibrary(void)
{
	return 42;
}

// This is the constructor of a class that has been exported.
// see TestLibrary.h for the class definition
CTestLibrary::CTestLibrary()
{
	return;
}
