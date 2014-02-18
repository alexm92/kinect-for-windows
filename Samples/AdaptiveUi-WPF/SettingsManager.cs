// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsManager.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Xaml;
    using System.Xml;

    using Microsoft.Win32;

    /// <summary>
    /// Functions to save and load settings
    /// </summary>
    public static class SettingsManager
    {
        private const string DefaultFileName = "Settings";

        private const string DefaultFileExtension = ".xml";

        /// <summary>
        /// Default directory where we read and write settings.
        /// </summary>
        public static string DefaultSettingsDirectory
        {
            get
            {
                return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            }
        }

        /// <summary>
        /// Default filename for settings files.
        /// </summary>
        public static string DefaultSettingsFileName
        {
            get
            {
                return Path.Combine(DefaultSettingsDirectory, DefaultFileName + DefaultFileExtension);
            }
        }

        /// <summary>
        /// Use the common dialog to save the settings to a file.
        /// </summary>
        /// <param name="fileName">settings file name to start with</param>
        /// <param name="settings">the settings to save</param>
        public static void SaveSettings(string fileName, Settings settings)
        {
            var saveFileDialog = new SaveFileDialog
                                     {
                                         FileName = Path.GetFileName(fileName),
                                         InitialDirectory = Path.GetDirectoryName(fileName),
                                         DefaultExt = DefaultFileExtension,
                                         Filter = Properties.Resources.FileDialogFilter
                                     };

            if (saveFileDialog.ShowDialog() == false)
            {
                // User canceled the dialog.  Quietly return.
                return;
            }

            fileName = saveFileDialog.FileName;

            var settingsXmlString = XamlServices.Save(settings);

            try
            {
                using (var streamWriter = new StreamWriter(fileName))
                {
                    streamWriter.Write(settingsXmlString);
                }
            }
            catch (Exception e)
            {
                if (e is IOException || e is UnauthorizedAccessException)
                {
                    // Couldn't save for a reason we expect.  Show an error message.
                    var errorMessageText = string.Format(CultureInfo.CurrentCulture, Properties.Resources.SaveErrorMessage, fileName);
                    MessageBox.Show(
                        errorMessageText, Properties.Resources.ErrorMessageCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Use the common dialog to load settings from a file.
        /// </summary>
        /// <param name="fileName">File to start with.</param>
        /// <param name="loadedSettings">The settings that were loaded</param>
        /// <returns>true if we were able to load the settings, false otherwise</returns>
        public static bool TryLoadSettingsWithOpenFileDialog(string fileName, out Settings loadedSettings)
        {
            loadedSettings = null;

            var openFileDialog = new OpenFileDialog
                          {
                              FileName = Path.GetFileName(fileName),
                              InitialDirectory = Path.GetDirectoryName(fileName),
                              DefaultExt = DefaultFileExtension,
                              Filter = Properties.Resources.FileDialogFilter
                          };

            if (openFileDialog.ShowDialog() == false)
            {
                // User canceled the dialog.  Quietly return.
                return false;
            }

            fileName = openFileDialog.FileName;

            if (!TryLoadSettingsNoUi(fileName, out loadedSettings))
            {
                // Couldn't load the file.  Show an error message.
                var errorMessageText = string.Format(CultureInfo.CurrentCulture, Properties.Resources.OpenErrorMessage, fileName);
                MessageBox.Show(
                    errorMessageText, Properties.Resources.ErrorMessageCaption, MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Load the settings from a file with no UI.
        /// </summary>
        /// <param name="fileName">filename to use</param>
        /// <param name="loadedSettings">settings that were loaded</param>
        /// <returns>true if settings were loaded, false otherwise</returns>
        public static bool TryLoadSettingsNoUi(string fileName, out Settings loadedSettings)
        {
            loadedSettings = null;
            if (!File.Exists(fileName))
            {
                return false;
            }

            using (var streamReader = new StreamReader(fileName))
            {
                try
                {
                    var loadedObjects = XamlServices.Load(streamReader.BaseStream);
                    loadedSettings = loadedObjects as Settings;
                    if (loadedSettings == null)
                    {
                        return false;
                    }
                }
                catch (XmlException)
                {
                    return false;
                }
                catch (XamlParseException)
                {
                    return false;
                }
                catch (XamlObjectWriterException)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
