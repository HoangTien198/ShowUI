﻿using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

public class ImpersonatedUser : IDisposable
{
    private IntPtr userHandle;
    private WindowsImpersonationContext impersonationContext;

    public ImpersonatedUser(string user, string domain, string password)
    {
        userHandle = IntPtr.Zero;
        bool loggedOn = LogonUser(
            user,
            domain,
            password,
            LogonType.NewCredentials,
            LogonProvider.Default,
            out userHandle);

        if (!loggedOn)
        { }
        // throw new Win32Exception(Marshal.GetLastWin32Error());

        // Begin impersonating the user
        impersonationContext = WindowsIdentity.Impersonate(userHandle);
    }

    public void Dispose()
    {
        if (userHandle != IntPtr.Zero)
        {
            CloseHandle(userHandle);
            userHandle = IntPtr.Zero;
            impersonationContext.Undo();
        }
    }

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool LogonUser(
        string lpszUsername,
        string lpszDomain,
        string lpszPassword,
        LogonType dwLogonType,
        LogonProvider dwLogonProvider,
        out IntPtr phToken
        );

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hHandle);

    private enum LogonType : int
    {
        Interactive = 2,
        Network = 3,
        Batch = 4,
        Service = 5,
        NetworkCleartext = 8,
        NewCredentials = 9,
    }

    private enum LogonProvider : int
    {
        Default = 0,
    }
}