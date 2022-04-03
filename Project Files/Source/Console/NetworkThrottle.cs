//=================================================================
// NetworkThrottle.cs - MW0LGE 2021
//=================================================================

using System;
using Microsoft.Win32;
using System.Windows.Forms;

namespace Thetis
{
    static class NetworkThrottle
    {
        public static bool GetNetworkThrottle(out int throttle, bool showErrors = true)
        {
            bool bRet = false;
            throttle = 0;

            RegistryKey hklm = null;
            try
            {
                hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
            }
            catch
            {
                if (showErrors)
                {
                    MessageBox.Show("Unable to open LocalMachine registry base key.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                }
            }
            if (hklm != null)
            {
                RegistryKey key = null;
                try
                {
                    key = hklm.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile", false);
                }
                catch
                {
                    if (showErrors)
                    {
                        MessageBox.Show("Unable to open SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile registry key.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                    }
                }

                if (key != null)
                {
                    try
                    {
                        object o = key.GetValue("NetworkThrottlingIndex");
                        if (o != null)
                        {
                            if (o is int)
                            {
                                throttle = (int)o;
                                bRet = true;
                            }
                            else
                            {
                                if (showErrors)
                                {
                                    MessageBox.Show("Unsuitable value in NetworkThrottlingIndex key.",
                                        "Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                                }
                            }
                        }
                    }
                    catch
                    {
                        if (showErrors)
                        {
                            MessageBox.Show("Unable to GetValue on NetworkThrottlingIndex registry entry.",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                        }
                    }
                    key.Close();
                }
                hklm.Close();
            }
            return bRet;
        }
        public static bool SetNetworkThrottle(int throttle)
        {
            bool bRet = false;

            if (Common.IsAdministrator())
            {
                RegistryKey hklm = null;
                try
                {
                    hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
                }
                catch
                {
                    MessageBox.Show("Unable to open LocalMachine registry base key.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                }

                if (hklm != null)
                {
                    RegistryKey key = null;
                    try
                    {
                        key = hklm.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile", true);
                    }
                    catch
                    {
                        MessageBox.Show("Unable to open SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile registry key.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                    }

                    if (key != null)
                    {
                        try
                        {
                            key.SetValue("NetworkThrottlingIndex", throttle, RegistryValueKind.DWord);             //unchecked((int)0xffffffffu)

                            bRet = true;
                        }
                        catch
                        {
                            MessageBox.Show("Unable to SetValue on NetworkThrottlingIndex registry entry.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                        }
                        key.Close();
                    }
                    hklm.Close();
                }
            }
            else
            {
                //msgbox need to be admin !
                MessageBox.Show("You need to be an Administrator. Please run Thetis 'As Administrator'.",
                    "No Administrator Rights",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
            }

            return bRet;
        }
    }
}
