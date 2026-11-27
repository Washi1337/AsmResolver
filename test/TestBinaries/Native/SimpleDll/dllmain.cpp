// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"

class VTableBase {
public:
    virtual void vfunc1() {}
    virtual void vfunc2() {}
    virtual ~VTableBase() {}
};

class VTableDerived : public VTableBase {
public:
    void vfunc1() override {}
    virtual void vfunc3() {}
};

__declspec(dllexport) VTableBase* CreateDerived() {
    return new VTableDerived();
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}
