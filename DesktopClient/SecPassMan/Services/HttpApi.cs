using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SecPassMan.Services;

public static class HttpApi
{
    private const int ERROR_SUCCESS = 0;

    [DllImport("httpapi.dll", SetLastError = true)]
    private static extern uint HttpRemoveUrl(
        IntPtr pReserved,
        string pUrl,
        uint dwFlags,
        out uint pResult
    );

    public static bool ClearUrlReservation(string url)
    {
        uint result;
        uint errorCode = HttpRemoveUrl(IntPtr.Zero, url, 0, out result);
        if (errorCode == ERROR_SUCCESS)
        {
            Console.WriteLine($"Successfully removed URL reservation: {url}");
            return true;
        }
        else
        {
            Console.WriteLine($"Failed to remove URL reservation: {url}. Error code: {errorCode}");
            return false;
        }
    }
}
