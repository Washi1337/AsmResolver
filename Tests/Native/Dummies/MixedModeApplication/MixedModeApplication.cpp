// MixedModeApplication.cpp : main project file.

#include "stdafx.h"

using namespace System;

void test()
{
	Console::WriteLine("WOOF");
}

#pragma unmanaged

int add(int a, int b)
{
	test();
	return a + b;
}

#pragma managed
int main(array<System::String ^> ^args)
{
	Console::WriteLine(L"Hello World");
	Console::WriteLine(add(1, 2));
	return 0;
}
