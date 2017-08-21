/*===================================================================== 
  This file is part of the Microsoft Unified Communications Code Samples. 

  Copyright (C) 2010 Microsoft Corporation.  All rights reserved. 

This source code is intended only as a supplement to Microsoft 
Development Tools and/or on-line documentation.  See these other 
materials for detailed information regarding Microsoft code samples. 

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
PARTICULAR PURPOSE. 
=====================================================================*/

using Microsoft.Win32;
using System;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;

namespace AudioVideoConversation
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            KillSkypeProcess();

            EnableUiSuppressionMode(true);
            DisableSkypeSavePassword();

            Application.ApplicationExit += Application_ApplicationExit;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            try
            {
                KillSkypeProcess();
                EnableUiSuppressionMode(false);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Killing skype process failed. " + exception.Message);
            }
        }

        private static void KillSkypeProcess()
        {
            var skypeProcess = Process.GetProcessesByName("lync").FirstOrDefault();
            skypeProcess?.Kill();
        }

        private static void EnableUiSuppressionMode(bool enabled)
        {
            /* Skype for Business 2015 support */
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\15.0\Lync", "UISuppressionMode", (enabled) ? 1 : 0);
            /* Skype for Business 2016 support */
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\16.0\Lync", "UISuppressionMode", (enabled) ? 1 : 0);
        }

        private static void DisableSkypeSavePassword()
        {
            /* Skype for Business 2015 support */
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\15.0\Lync", "SavePassword", 0);
            /* Skype for Business 2016 support */
            Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\16.0\Lync", "SavePassword", 0);
        }

        private static void DisableSkypeStartup()
        {
            // NOTE: we do this because skype can crash if it runs at startup in UI suppression mode
            //Close the registry handle here, in case there is an exception while deleting the value, so that we don't leak it.
            RegistryKey runKey = null;
            try
            {
                runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\", true);
                runKey?.DeleteValue("Lync");
            }
            catch
            {
                //silently fail here
            }
            finally
            {
                runKey?.Close();
            }
        }

        public static string UserName { get; set; }
        public static string Password { get; set; }
        public static string ExchangeControlPanelUrl { get; set; }
    }
}
