//------------------------------------------------------------------------------
// <copyright file="SettingsControl.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for SettingsControl.
    /// </summary>
    public partial class SettingsControl
    {
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(Settings), typeof(SettingsControl), new PropertyMetadata(null));

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsControl"/> class. 
        /// </summary>
        public SettingsControl()
        {
            this.InitializeComponent();
            this.LayoutRoot.DataContext = this;
        }

        /// <summary>
        /// The settings object we read from and write to.
        /// </summary>
        public Settings Settings
        {
            get
            {
                return (Settings)this.GetValue(SettingsProperty);
            }

            set
            {
                this.SetValue(SettingsProperty, value);
            }
        }
    }
}
