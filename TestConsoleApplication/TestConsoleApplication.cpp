// TestConsoleApplication.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>
#include <string>

using namespace std;

int _tmain(int argc, _TCHAR* argv[])
{
	char username[80];
	char password[80];
	
	printf("Username:");
	scanf_s("%s", username , 80);
	printf("Password:");
	scanf_s("%s", password, 80);

	if (strcmp(username, "TUP") == 0 && strcmp(password, "awesome") == 0)
		printf("AWESOME");
	else
		printf("NOPE");

	int x;
	scanf_s("%d", &x, 4);
}

