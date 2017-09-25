using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using Microsoft.Win32;

// Based on https://stackoverflow.com/a/44816953/1463239

namespace GIF_Viewer.FileExts
{
    /// <summary>
    /// For dealing w/ file associations
    /// </summary>
    public class GifFileAssociation
    {
        private readonly Association _association;

        public GifFileAssociation()
        {
            var filePath = Process.GetCurrentProcess().MainModule.FileName;

            _association = new Association
            {
                Extension = ".gif",
                ProgId = "GIF_Viewer",
                FileTypeDescription = "GIF File",
                ExecutableFilePath = filePath
            };
        }

        public bool HasWriteAccess()
        {
            try
            {
                Registry.CurrentUser.OpenSubKey(@"Software\Classes\" + _association.Extension,
                    RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.WriteKey);

                Registry.CurrentUser.OpenSubKey(@"Software\Classes\" + _association.ProgId,
                    RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.WriteKey);

                Registry.CurrentUser.OpenSubKey($@"Software\Classes\{_association.ProgId}\shell\open\command",
                    RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.WriteKey);

                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
            catch
            {
                // Unknown exception type :(
                return false;
            }
        }

        /// <summary>
        /// Returns if this assembly is currently associated w/ the GIF file extension
        /// </summary>
        /// <returns>Full path to current .exe associated</returns>
        public bool IsAssociated()
        {
            return ValidateAssociation(_association);
        }

        /// <summary>
        /// Associates GIF Viewer w/ the .gif file extension
        /// </summary>
        public void Associate()
        {
            EnsureAssociationsSet(_association);
        }

        public static void EnsureAssociationsSet(params Association[] associations)
        {
            bool madeChanges = false;
            foreach (var association in associations)
            {
                madeChanges |= SetAssociation(association);
            }

            if (madeChanges)
                SHChangeNotify(ShcneAssocchanged, ShcnfFlush, IntPtr.Zero, IntPtr.Zero);
        }

        public static bool SetAssociation(Association association)
        {
            bool madeChanges = false;
            madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + association.Extension, association.ProgId);
            madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + association.ProgId, association.FileTypeDescription);
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{association.ProgId}\shell\open\command", "\"" + association.ExecutableFilePath + "\" \"%1\"");
            return madeChanges;
        }

        public static bool ValidateAssociation(Association association)
        {
            bool isSetup = false;
            isSetup |= VerifyKeyValue(@"Software\Classes\" + association.Extension, association.ProgId);
            isSetup |= VerifyKeyValue(@"Software\Classes\" + association.ProgId, association.FileTypeDescription);
            isSetup |= VerifyKeyValue($@"Software\Classes\{association.ProgId}\shell\open\command", "\"" + association.ExecutableFilePath + "\" \"%1\"");
            return isSetup;
        }

        private static bool SetKeyDefaultValue(string keyPath, string value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                if (key != null && key.GetValue(null) as string == value) return false;

                key?.SetValue(null, value);
                return true;
            }
        }

        private static bool VerifyKeyValue(string keyPath, string value)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(keyPath, RegistryKeyPermissionCheck.Default, RegistryRights.ReadKey))
            {
                return key?.GetValue(null) as string == value;
            }
        }

        // needed so that Explorer windows get refreshed after the registry is updated
        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private const int ShcneAssocchanged = 0x8000000;
        private const int ShcnfFlush = 0x1000;

        public struct Association
        {
            public string Extension { get; set; }
            public string ProgId { get; set; }
            public string FileTypeDescription { get; set; }
            public string ExecutableFilePath { get; set; }
        }
    }
}
