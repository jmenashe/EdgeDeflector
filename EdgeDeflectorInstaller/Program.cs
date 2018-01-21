/*
 * Copyright © 2017 Daniel Aleksandersen
 * SPDX-License-Identifier: MIT
 * License-Filename: LICENSE
 */

using Microsoft.Win32;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;

namespace EdgeDeflectorInstaller
{
    class Program
    {
        static bool IsElevated()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void ElevatePermissions()
        {
            ProcessStartInfo rerun = new ProcessStartInfo()
            {
                FileName = System.Reflection.Assembly.GetExecutingAssembly().Location,
                UseShellExecute = true,
                Verb = "runas"
            };
            Process.Start(rerun);
        }

        private static void RegisterProtocolHandler()
        {
            RegistryKey uriclass_key = Registry.ClassesRoot.OpenSubKey("EdgeUriDeflector", true);
            if (uriclass_key == null)
            {
                uriclass_key = Registry.ClassesRoot.CreateSubKey("EdgeUriDeflector", true);
            }

            uriclass_key.SetValue(string.Empty, "URL: Microsoft Edge Protocol Deflector");

            RegistryKey icon_key = uriclass_key.OpenSubKey("DefaultIcon", true);
            if (icon_key == null)
            {
                icon_key = uriclass_key.CreateSubKey("DefaultIcon");
            }

            string exec_dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var exec_path = System.IO.Path.Combine(exec_dir, "EdgeDeflector.exe");

            icon_key.SetValue(string.Empty, exec_path + ",0");
            icon_key.Close();

            RegistryKey shellcmd_key = uriclass_key.OpenSubKey(@"shell\open\command", true);
            if (shellcmd_key == null)
            {
                shellcmd_key = uriclass_key.CreateSubKey(@"shell\open\command");
            }

            shellcmd_key.SetValue(string.Empty, exec_path + " \"%1\"");
            shellcmd_key.Close();

            uriclass_key.SetValue("URL Protocol", string.Empty);

            uriclass_key.Close();

            RegistryKey software_key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\EdgeUriDeflector", true);
            if (software_key == null)
            {
                software_key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Clients\EdgeUriDeflector", true);
            }

            RegistryKey capability_key = software_key.OpenSubKey("Capabilities", true);
            if (capability_key == null)
            {
                capability_key = software_key.CreateSubKey("Capabilities", true);
            }

            capability_key.SetValue("ApplicationDescription", "Open web links normally forced to open in Microsoft Edge in your default web browser.");
            capability_key.SetValue("ApplicationName", "EdgeDeflector");

            RegistryKey urlass_key = capability_key.OpenSubKey("UrlAssociations", true);
            if (urlass_key == null)
            {
                urlass_key = capability_key.CreateSubKey("UrlAssociations", true);
            }

            urlass_key.SetValue("microsoft-edge", "EdgeUriDeflector");
            urlass_key.Close();

            capability_key.Close();
            software_key.Close();

            RegistryKey registeredapps_key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\RegisteredApplications", true);
            registeredapps_key.SetValue("EdgeUriDeflector", @"SOFTWARE\Clients\EdgeUriDeflector\Capabilities");
            registeredapps_key.Close();
        }

        static bool IsUri(string uristring)
        {
            try
            {
                Uri uri = new Uri(uristring);
                return uri.IsWellFormedOriginalString();
            }
            catch (System.UriFormatException)
            {
                return false;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        static void Main(string[] args)
        {
            if (!IsElevated())
            {
                ElevatePermissions();
            }
            RegisterProtocolHandler();
        }
    }
}
