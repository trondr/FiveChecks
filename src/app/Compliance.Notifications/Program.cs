using System;
using System.Diagnostics;
using System.Reflection;

namespace Compliance_Notifications
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Compliance-Notifications {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}");
        }
    }
}
