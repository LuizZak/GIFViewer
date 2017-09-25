using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;

namespace GIF_Viewer
{
    /// <summary>
    /// Manages a file association
    /// </summary>
    public class FileAssociation
    {
        public string Extension;

        public string CurrentProgram;

        public RegistryKey ExtensionKey;
        public RegistryKey OpenWithKey;

        public FileAssociation(string extension)
        {
            Extension = extension;

            if (!HasWriteAccess())
                return;
            
            ExtensionKey = Registry.ClassesRoot.OpenSubKey(extension);

            if (ExtensionKey == null)
                return;

            string originalKey = (string)ExtensionKey.GetValue(null);

            if (originalKey != null)
            {
                ExtensionKey = Registry.ClassesRoot.OpenSubKey(originalKey);
            }

            var shell = ExtensionKey?.OpenSubKey("shell");

            if (shell == null) return;

            OpenWithKey = shell.OpenSubKey("Open");

            var command = OpenWithKey?.OpenSubKey("command");

            if (command != null)
            {
                CurrentProgram = (string)command.GetValue(null);
            }
        }

        /// <summary>
        /// Returns whether this application can fully create the association with the current permission rights
        /// </summary>
        /// <returns>Whether this program can read and write to the required registry keys under ClassesRoot</returns>
        public bool HasWriteAccess()
        {
            try
            {
                var classes = Registry.ClassesRoot;

                if (!CanReadKey(classes))
                    return false;

                // Extension
                var extension = classes.OpenSubKey(Extension);

                // Key not present - test if we can write a new key
                if (extension == null)
                    return CanWriteKey(classes);
                if (!CanReadKey(extension))
                    return false;

                // Test if the extension is not mapped somewhere else
                var originalKey = (string)extension.GetValue(null);

                if (originalKey != null)
                {
                    extension = Registry.ClassesRoot.OpenSubKey(originalKey);
                }
                // Re-test key now
                if (extension == null)
                    return CanWriteKey(classes);
                if (!CanReadKey(extension))
                    return false;

                // Extension -> Shell
                var shell = extension.OpenSubKey("shell");

                // Key not present - test if we can write a new key
                if (shell == null)
                    return CanWriteKey(extension);
                if (!CanReadKey(shell))
                    return false;

                // Extension -> Shell -> Command
                var command = extension.OpenSubKey("command");

                // Test if we can write to command, and if command's not present, if we can write to 'shell' to create one
                return CanWriteKey(command ?? shell);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the program associated with this file extension
        /// </summary>
        /// <returns>The program associated with this file extension</returns>
        public string GetProgram()
        {
            return CurrentProgram;
        }

        /// <summary>
        /// Gets the program associated with this file extension with the given path
        /// </summary>
        /// <param name="programPath">The path of the program to match</param>
        /// <returns>The program associated with this file extension</returns>
        public string GetProgramExt(string programPath)
        {
            RegistryKey applications = Registry.ClassesRoot.OpenSubKey("Applications");

            if (applications != null && programPath != null)
            {
                string fileName = Path.GetFileName(programPath);
                RegistryKey app = applications.OpenSubKey(fileName);
                
                return (string)app?.OpenSubKey("shell")?.OpenSubKey("Open")?.OpenSubKey("command")?.GetValue(null);
            }

            return null;
        }

        /// <summary>
        /// Set the current file association to the given application
        /// </summary>
        /// <param name="programPath">The application to set the current file association</param>
        public void SetProgram(string programPath)
        {
            // Direct association:
            if (OpenWithKey != null)
            {
                RegistryKey command = OpenWithKey.OpenSubKey("command", true);

                if (command != null)
                {
                    command.SetValue(null, programPath);
                }
                else
                {
                    command = OpenWithKey.CreateSubKey("command");

                    command?.SetValue(null, programPath);
                }
            }
            else
            {
                RegistryKey shell = ExtensionKey.OpenSubKey("shell", true);

                if (shell != null) return;
                shell = ExtensionKey.CreateSubKey("shell", RegistryKeyPermissionCheck.ReadWriteSubTree);

                if (shell != null)
                    OpenWithKey = shell.CreateSubKey("Open", RegistryKeyPermissionCheck.ReadWriteSubTree);

                RegistryKey command = OpenWithKey?.CreateSubKey("command", RegistryKeyPermissionCheck.ReadWriteSubTree);

                command?.SetValue(null, programPath);
            }
        }

        /// <summary>
        /// Set the current file association to the given application
        /// </summary>
        /// <param name="programPath">The application to set the current file association</param>
        /// <param name="programName">The name for the program to use on the registry key</param>
        public void SetProgram(string programPath, string programName)
        {
            SetProgram(programPath);

            // 'Applications' key:
            RegistryKey applications = Registry.ClassesRoot.OpenSubKey("Applications", true);

            RegistryKey app = applications?.CreateSubKey(programName);

            RegistryKey shellSubKey = app?.CreateSubKey("shell");

            shellSubKey?.CreateSubKey("Open")?.CreateSubKey("command")?.SetValue(null, programPath);
        }

        /// <summary>
        /// Returns whether the current user has the given access rights to a registry key
        /// </summary>
        /// <param name="accessRights">An enumeration containing the access rights requested</param>
        /// <param name="key">The registry key to test the access rights on</param>
        /// <returns>Whether the user has the required access; true if all access rights are available, false otherwise.</returns>
        public static bool HavePermissionsOnKey(RegistryRights accessRights, RegistryKey key)
        {
            try
            {
                var security = Registry.ClassesRoot.GetAccessControl();
                var rights = security.GetAccessRules(true, true, typeof(NTAccount)).OfType<RegistryAccessRule>();

                if (rights.Select(rule => rule.RegistryRights).Any(rule => (rule & accessRights) == accessRights))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the user has RegistryRights.WriteKey access rights to a given registry key
        /// </summary>
        public static bool CanWriteKey(RegistryKey key)
        {
            return HavePermissionsOnKey(RegistryRights.WriteKey, key);
        }

        /// <summary>
        /// Returns whether the user has RegistryRights.ReadKey access rights to a given registry key
        /// </summary>
        public static bool CanReadKey(RegistryKey key)
        {
            return HavePermissionsOnKey(RegistryRights.ReadKey, key);
        }
    }
}