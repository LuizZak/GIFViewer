using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace GIF_Viewer
{
    /// <summary>
    /// Manages a file association
    /// </summary>
    public class FileAssociation
    {
        public string Extension = "";

        public string CurrentProgram = "";

        public RegistryKey ExtensionKey;
        public RegistryKey OpenWithKey;

        public FileAssociation(string extension)
        {
            Extension = extension;

            ExtensionKey = Registry.ClassesRoot.CreateSubKey(extension);

            string originalKey = (string)ExtensionKey.GetValue(null);

            if (originalKey != null)
            {
                ExtensionKey = Registry.ClassesRoot.OpenSubKey(originalKey);
            }

            RegistryKey shell = ExtensionKey.OpenSubKey("shell");

            if (shell != null)
            {
                OpenWithKey = shell.OpenSubKey("Open");

                if (OpenWithKey != null)
                {
                    RegistryKey command = OpenWithKey.OpenSubKey("command");

                    if (command != null)
                    {
                        CurrentProgram = (string)command.GetValue(null);
                    }
                }
            }
        }

        /// <summary>
        /// Init the file association, creating the OpenWithKey, if needed
        /// </summary>
        public void Init()
        {

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

            RegistryKey app = applications.OpenSubKey(System.IO.Path.GetFileName(programPath));

            if (app != null)
            {
                RegistryKey temp = app.OpenSubKey("shell");

                if (temp != null)
                    temp = temp.OpenSubKey("Open");
                if (temp != null)
                    temp = temp.OpenSubKey("command");
                if (temp != null)
                    return (string)temp.GetValue(null);
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
                    command.SetValue(null, programPath);
                }
            }
            else
            {
                RegistryKey shell = ExtensionKey.OpenSubKey("shell", true);

                if (shell == null)
                {
                    shell = ExtensionKey.CreateSubKey("shell", RegistryKeyPermissionCheck.ReadWriteSubTree);

                    OpenWithKey = shell.CreateSubKey("Open", RegistryKeyPermissionCheck.ReadWriteSubTree);

                    RegistryKey command = OpenWithKey.CreateSubKey("command", RegistryKeyPermissionCheck.ReadWriteSubTree);

                    command.SetValue(null, programPath);
                }
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

            RegistryKey app = applications.CreateSubKey(programName);

            if (app != null)
            {
                RegistryKey temp = app.CreateSubKey("shell");
                temp = temp.CreateSubKey("Open");
                temp = temp.CreateSubKey("command");

                temp.SetValue(null, programPath);
            }
        }
    }
}