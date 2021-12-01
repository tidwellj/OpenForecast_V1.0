using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ExtentionMethods
{
    public static class IntExtentions
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
            {
                return null;
            }

            return securedStringValue;
        }

        public static string Unsecure(this SecureString value)

        {
            // Read-only field representing a pointer initialized to zero.

            var unsecuredString = IntPtr.Zero;

            try

            {
                // Copies the value of SecureString to unmanaged memory.

                unsecuredString = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(value);

                return Marshal.PtrToStringUni(unsecuredString);
            }
            finally

            {
                // Frees unmanaged string pointer.

                Marshal.ZeroFreeGlobalAllocUnicode(unsecuredString);
            }
        }
    }
}