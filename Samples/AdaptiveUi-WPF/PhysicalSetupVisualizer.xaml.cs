//------------------------------------------------------------------------------
// <copyright file="PhysicalSetupVisualizer.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System;
    using System.Windows;
    using System.Windows.Media.Media3D;

    /// <summary>
    /// Displays a simple 3D visualization of the display device and the
    /// Kinect sensor.
    /// </summary>
    public partial class PhysicalSetupVisualizer
    {
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(Settings), typeof(PhysicalSetupVisualizer), new PropertyMetadata(null, (o, args) => ((PhysicalSetupVisualizer)o).OnSettingsChanged((Settings)args.OldValue, (Settings)args.NewValue)));

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalSetupVisualizer"/> class. 
        /// </summary>
        public PhysicalSetupVisualizer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Settings used for the app.
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

        private void OnSettingsChanged(Settings oldValue, Settings newValue)
        {
            if (oldValue != null)
            {
                oldValue.ParameterChanged -= this.OnSettingsParameterChanged;
            }

            if (newValue != null)
            {
                newValue.ParameterChanged += this.OnSettingsParameterChanged;
            }

            this.UpdateTransforms();
        }

        private void OnSettingsParameterChanged(object sender, EventArgs eventArgs)
        {
            this.UpdateTransforms();
        }

        private void UpdateTransforms()
        {
            if (this.Settings == null)
            {
                // Transforms for use in design mode or when Settings are not
                // available.
                this.DisplayModel.Transform = new ScaleTransform3D(3.0, 1.0, 0.1);
                this.SensorModel.Transform = new TranslateTransform3D(0.0, 0.6, 0.0);
            }
            else
            {
                this.DisplayModel.Transform = new ScaleTransform3D(this.Settings.DisplayWidthInMeters, this.Settings.DisplayHeightInMeters, 0.1);
                this.SensorModel.Transform = new TranslateTransform3D(this.Settings.SensorOffsetX, this.Settings.SensorOffsetY, this.Settings.SensorOffsetZ);
            }
        }
    }
}
