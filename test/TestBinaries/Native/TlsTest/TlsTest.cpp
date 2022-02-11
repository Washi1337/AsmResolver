// TlsTest.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <Windows.h>
#include <time.h>

__declspec(thread) int _threadLocalInt = 0x12345678;
__declspec(thread) char _threadLocalArray[14] = "Hello World!\n";

VOID WINAPI tls_callback1(
    PVOID DllHandle,
    DWORD Reason,
    PVOID Reserved)
{
    if (Reason == DLL_PROCESS_ATTACH)
        std::cout << "TLS callback 1" << std::endl;
}

#ifdef _WIN64
#pragma comment (linker, "/INCLUDE:_tls_used")
#pragma comment (linker, "/INCLUDE:tls_callback_func1")
#else
#pragma comment (linker, "/INCLUDE:__tls_used")
#pragma comment (linker, "/INCLUDE:_tls_callback_func1")
#endif

#ifdef _WIN64
#pragma const_seg(".CRT$XLF")
EXTERN_C const
#else
#pragma data_seg(".CRT$XLF")
EXTERN_C
#endif

PIMAGE_TLS_CALLBACK tls_callback_func1 = tls_callback1;
PIMAGE_TLS_CALLBACK tls_callback_end = NULL;
#ifdef _WIN64
#pragma const_seg()
#else
#pragma data_seg()
#endif //_WIN64

DWORD WINAPI thread_main(LPVOID arg)
{
    std::cout << '[' << GetCurrentThreadId() << "]: int = " << _threadLocalInt << std::endl;
    _threadLocalInt++;
    std::cout << '[' << GetCurrentThreadId() << "]: str = " << _threadLocalArray << std::endl;
    return 0;
}

int main()
{
    for (int i = 0; i < 5; i++) {
        CreateThread(NULL, NULL, thread_main, NULL, 0, NULL);
    }

    system("pause");
}
