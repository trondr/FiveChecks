using System;
using System.Runtime.InteropServices;

namespace Compliance.Notifications.Common
{
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1815 // Override equals and operator equals on value types
    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct NotificationUserInputData
    {
        [MarshalAs(UnmanagedType.LPWStr)]

        public string Key;
        
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Value;
    }
#pragma warning restore CA1815 // Override equals and operator equals on value types
#pragma warning restore CA1051 // Do not declare visible instance fields
}