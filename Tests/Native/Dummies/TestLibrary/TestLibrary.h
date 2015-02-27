// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the TESTLIBRARY_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// TESTLIBRARY_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef TESTLIBRARY_EXPORTS
#define TESTLIBRARY_API __declspec(dllexport)
#else
#define TESTLIBRARY_API __declspec(dllimport)
#endif

// This class is exported from the TestLibrary.dll
class TESTLIBRARY_API CTestLibrary {
public:
	CTestLibrary(void);
	// TODO: add your methods here.
};

extern TESTLIBRARY_API int nTestLibrary;

TESTLIBRARY_API int fnTestLibrary(void);
