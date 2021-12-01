using System;
using System.Security;


namespace ExtentionMethods

public static class IntMethods
{


    public static SecureString Secure(this string value)

    {

        SecureString securedStringValue = new SecureString();

        if (!(string.IsNullOrWhiteSpace(value)))

        {

            foreach (char c in value.ToCharArray())

            {

                securedStringValue.AppendChar(c);

            }

        }

        else

            return null;

        return securedStringValue;

    }




}
