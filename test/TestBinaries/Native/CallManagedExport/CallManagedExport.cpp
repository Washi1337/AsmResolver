#include <iostream>
#include <Windows.h>

int main(int argc, char** argv)
{
    if (argc <= 2)
    {
        std::cout << "Usage: " << argv[0] << " <library_name> <export_name>";
        return 0;
    }

    auto library = LoadLibraryA(argv[1]);
    if (library == NULL)
    {
        std::cerr << "LoadLibraryA failed." << std::endl;
        return GetLastError();
    }

    auto function = GetProcAddress(library, argv[2]);
    if (function == NULL)
    {
        std::cerr << "GetProcAddress failed." << std::endl;
        return GetLastError();
    }

    function();

    return 0;
}
