using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Demangles imported or exported method names. Under construction
    /// </summary>
    public static class NameDemangler
    {
        static Dictionary<char, string> VCppSpecialNameCodes = new Dictionary<char, string>() { 
            {'0', "Constructor:operator /="},
            {'1', "Destructor:operator %="},
            {'2', "operator new:operator >>="},
            {'3', "operator delete:operator <<="},
            {'4', "operator =:operator &="},
            {'5', "operator >>:operator |="},
            {'6', "operator <<:operator ^="},
            {'7', "operator !:"}, // vftable
            {'8', "operator ==:"}, // vbtable
            {'9', "operator !=:"}, // vcall
            {'A', "operator []:"},
            {'B', "operator returntype:"},
            {'C', "operator ->:"},
            {'D', "operator :"},
            {'E', "operator ++:"},
            {'F', "operator --:"},
            {'G', "operator -:"},
            {'H', "operator +:"},
            {'I', "operator &:"},
            {'J', "operator ->*:"},
            {'K', "operator /:"},
            {'L', "operator %:"},
            {'M', "operator <:"},
            {'N', "operator <=:"},
            {'O', "operator >:"},
            {'P', "operator >=:"},
            {'Q', "operator ,:"},
            {'R', "operator ():"},
            {'S', "operator ~:"},
            {'T', "operator ^:"},
            {'U', "operator |:operator new[]"},
            {'V', "operator &&:operator delete[]"},
            {'W', "operator ||:"},
            {'X', "operator *=:"},
            {'Y', "operator +=:"},
            {'Z', "operator -=:"},
        };
        static Dictionary<char, string> VCppDataTypeCodes = new Dictionary<char, string>() { 
            {'A', "reference:"},
            {'B', "volatile:"},
            {'C', "signed char:"},
            {'D', "char:__int8"},
            {'E', "unsigned char:unsigned __int8"},
            {'F', "short:__int16"},
            {'G', "unsigned short:unsigned __int16"},
            {'H', "int:__int32"},
            {'I', "unsigned int:unsigned __int32"},
            {'J', "long:__int64"},
            {'K', "unsigned long:unsigned __int64"},
            {'L', ":__int128"},
            {'M', "float:unsigned __int128"},
            {'N', "double:bool"},
            {'O', "long double:Array"},
            {'P', "ptr:"},
            {'Q', "const ptr:"},
            {'R', "volatile ptr):"},
            {'S', "const volatile ptr:"},
            {'T', "union:"},
            {'U', "struct:"},
            {'V', "class:"},
            {'W', "enum:wchar_t"},
            {'X', "void:"},
            {'Y', "cointerface:"},
            {'Z', ":"},
        };

        public static string DemangleVCpp(string name)
        {
            // remove first "?" 
            name = name.Remove(0, 1);

            string[] parts = name.Split('@');

            StringBuilder builder = new StringBuilder();

            for (int i = Array.IndexOf(parts, ""); i >= 0; i--)
            {
                if (i == 0)
                {
                    //check if function has got special name
                    if (parts[i][0] == '?')
                    {
                        // check if function starts with _ char.
                        bool underline = parts[i][1] == '_';


                        if (underline)
                        {
                            char code = parts[i][2];
                            builder.Append(VCppSpecialNameCodes[code].Split(':')[1]);
                        }
                        else
                        {
                            char code = parts[i][1];
                            builder.Append(VCppSpecialNameCodes[code].Split(':')[0]);
                        }
                    }
                    else
                    {
                        builder.Append(parts[i]);
                    }
                }
                else
                {
                    builder.Append(parts[i] + "::");
                }
            }




            return builder.ToString();
        }


    }
}
