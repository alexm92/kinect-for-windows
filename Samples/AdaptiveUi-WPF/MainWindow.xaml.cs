//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "UI-WPF", Justification = "Sample is correctly named")]

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;

    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "There is no immutable RoutedUICommand variant, and readonly provides some measure of protection")]
        public static readonly RoutedUICommand ShowSettings = new RoutedUICommand(Properties.Resources.ShowSettingsCaption, "ShowSettings", typeof(MainWindow));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "There is no immutable RoutedUICommand variant, and readonly provides some measure of protection")]
        public static readonly RoutedUICommand HideSettings = new RoutedUICommand(Properties.Resources.HideSettingsCaption, "HideSettings", typeof(MainWindow));

        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(Settings), typeof(MainWindow), new FrameworkPropertyMetadata(null, (o, args) => ((MainWindow)o).OnSettingsChanged((Settings)args.OldValue, (Settings)args.NewValue)));

        public static readonly DependencyProperty SensorTransformsProperty =
            DependencyProperty.Register("SensorTransforms", typeof(SensorTransforms), typeof(MainWindow), new PropertyMetadata(null));

        public static readonly DependencyProperty SomethingNearSensorProperty =
            DependencyProperty.Register("SomethingNearSensor", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty TimeoutWarningProperty =
            DependencyProperty.Register("TimeoutWarning", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty KinectSensorProperty = DependencyProperty.Register(
            "KinectSensor", typeof(KinectSensor), typeof(MainWindow), new PropertyMetadata(null));

        public static readonly DependencyProperty UserDistanceProperty =
            DependencyProperty.Register("UserDistance", typeof(UserDistance), typeof(MainWindow), new PropertyMetadata(UserDistance.Unknown));

        private readonly KinectSensorChooser sensorChooser;

        private readonly TrackingPolicy trackingPolicy = new TrackingPolicy();

        private readonly AdaptiveZoneLogic adaptiveZoneLogic = new AdaptiveZoneLogic();

        private readonly string settingsFileName = SettingsManager.DefaultSettingsFileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class. 
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.SensorTransforms = new SensorTransforms();
            this.LayoutRoot.DataContext = this;

            Settings newSettings;
            if (SettingsManager.TryLoadSettingsNoUi(this.settingsFileName, out newSettings))
            {
                this.Settings = newSettings;
            }
            else
            {
                 this.Settings = new Settings();
            }

            // initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += this.SensorChooserOnKinectChanged;
            this.SensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            // Code to make sure transform changes are reflected in the TrackingPolicy
            // and AdaptiveZoneLogic since they cannot bind.
            this.SensorTransforms.PropertyChanged += (o, changedArgs) =>
                {
                    if (changedArgs.PropertyName == "SensorToScreenPositionTransform")
                    {
                        this.trackingPolicy.SensorToScreenPositionTransform = this.SensorTransforms.SensorToScreenPositionTransform;
                        this.adaptiveZoneLogic.SensorToScreenPositionTransform = this.SensorTransforms.SensorToScreenPositionTransform;
                    }
                };

            // Bind our KinectSensor property to the one from the sensor chooser
            var sensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            this.SetBinding(KinectSensorProperty, sensorBinding);
            
            this.adaptiveZoneLogic.PropertyChanged += this.AdaptiveZoneLogicPropertyChanged;

            this.SizeChanged += OnSizeChanged;

            this.MouseDown += (sender, args) => this.ResetTimeout();

            // Put the UI into a default state.
            this.AdaptiveZoneLogicPropertyChanged(null, null);
        }

        /// <summary>
        /// Settings for the application.
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

        /// <summary>
        /// Object that transforms sensor skeleton space coordinates
        /// to display-relative coordinates.
        /// </summary>
        public SensorTransforms SensorTransforms
        {
            get
            {
                return (SensorTransforms)this.GetValue(SensorTransformsProperty);
            }

            set
            {
                this.SetValue(SensorTransformsProperty, value);
            }
        }

        /// <summary>
        /// Whether something is detected near the sensor.  Used when
        /// user is too close for skeletal tracking to work.
        /// </summary>
        public bool SomethingNearSensor
        {
            get
            {
                return (bool)this.GetValue(SomethingNearSensorProperty);
            }

            set
            {
                this.SetValue(SomethingNearSensorProperty, value);
            }
        }

        /// <summary>
        /// True if we should be showing the timeout warning UI.
        /// When the timeout fires, we go back to far mode.
        /// </summary>
        public bool TimeoutWarning
        {
            get
            {
                return (bool)this.GetValue(TimeoutWarningProperty);
            }

            set
            {
                this.SetValue(TimeoutWarningProperty, value);
            }
        }

        /// <summary>
        /// The sensor we are using.
        /// </summary>
        public KinectSensor KinectSensor
        {
            get
            {
                return (KinectSensor)this.GetValue(KinectSensorProperty);
            }

            set
            {
                this.SetValue(KinectSensorProperty, value);
            }
        }

        /// <summary>
        /// The current interaction zone of the user.
        /// </summary>
        public UserDistance UserDistance
        {
            get
            {
                return (UserDistance)this.GetValue(UserDistanceProperty);
            }

            set
            {
                this.SetValue(UserDistanceProperty, value);
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            this.Settings.DisplayWidthInPixels = sizeChangedEventArgs.NewSize.Width;
            this.Settings.DisplayHeightInPixels = sizeChangedEventArgs.NewSize.Height;
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
                this.OnSettingsParameterChanged(null, null);
            }

            this.trackingPolicy.Settings = newValue;
        }

        /// <summary>
        /// Gets called when any property in Settings changes.
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="eventArgs">event arguments</param>
        /// <remarks>
        /// Note that this code could be more efficient by only copying things that
        /// actually changed.  Settings don't change often enough for this to be a problem
        /// in this application.
        /// </remarks>
        private void OnSettingsParameterChanged(object sender, EventArgs eventArgs)
        {
            if (this.Settings.FullScreen
                && (this.WindowState != WindowState.Maximized || this.WindowStyle != WindowStyle.None))
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
            }

            if (!this.Settings.FullScreen && this.WindowStyle == WindowStyle.None)
            {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }

            this.adaptiveZoneLogic.NearBoundary = this.Settings.NearBoundary;
            this.adaptiveZoneLogic.NearBoundaryHysteresis = this.Settings.BoundaryHysteresis;
            this.adaptiveZoneLogic.FarBoundary = this.Settings.FarBoundary;
            this.adaptiveZoneLogic.FarBoundaryHysteresis = this.Settings.BoundaryHysteresis;
            this.adaptiveZoneLogic.NoUserTimeout = this.Settings.NoUserTimeout;
            this.adaptiveZoneLogic.NoUserWarningTimeout = this.Settings.NoUserWarningTimeout;

            this.SensorTransforms.UseFixedSensorElevationAngle = this.Settings.UseFixedSensorElevationAngle;
            this.SensorTransforms.FixedSensorElevationAngle = this.Settings.FixedSensorElevationAngle;
            this.SensorTransforms.SensorOffsetFromScreenCenter = new Vector3D(this.Settings.SensorOffsetX, this.Settings.SensorOffsetY, this.Settings.SensorOffsetZ);
            this.SensorTransforms.DisplayWidthInMeters = this.Settings.DisplayWidthInMeters;
            this.SensorTransforms.DisplayHeightInMeters = this.Settings.DisplayHeightInMeters;
            this.SensorTransforms.DisplayWidthInPixels = this.Settings.DisplayWidthInPixels;
            this.SensorTransforms.DisplayHeightInPixels = this.Settings.DisplayHeightInPixels;

            UpdateUserViewerVisibility();
        }

        private void UpdateUserViewerVisibility()
        {
            this.FullScreenKinectUserViewer.Visibility = (this.adaptiveZoneLogic.UserDistance == UserDistance.Far && this.Settings.ShowUserViewerInFarMode) ? Visibility.Visible : Visibility.Hidden;
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            this.trackingPolicy.KinectSensor = args.NewSensor;
            this.adaptiveZoneLogic.KinectSensor = args.NewSensor;
            this.SensorTransforms.KinectSensor = args.NewSensor;
        }


        private void AdaptiveZoneLogicPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            this.KinectRegion.IsCursorVisible = (this.adaptiveZoneLogic.UserDistance == UserDistance.Far) || (this.adaptiveZoneLogic.UserDistance == UserDistance.Medium);
            this.AttractTextBlock.Visibility = (this.adaptiveZoneLogic.UserDistance == UserDistance.Far) ? Visibility.Visible : Visibility.Hidden;
            UpdateUserViewerVisibility();

            this.UserDistance = this.adaptiveZoneLogic.UserDistance;
            this.SomethingNearSensor = this.adaptiveZoneLogic.SomethingNearSensor;
            this.TimeoutWarning = this.adaptiveZoneLogic.TimeoutWarning;
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            this.sensorChooser.Stop();
        }

        private void OnSave(object sender, ExecutedRoutedEventArgs e)
        {
            SettingsManager.SaveSettings(this.settingsFileName, this.Settings);
        }

        private void OnLoad(object sender, ExecutedRoutedEventArgs e)
        {
            Settings newSettings;
            if (!SettingsManager.TryLoadSettingsWithOpenFileDialog(this.settingsFileName, out newSettings))
            {
                return;
            }

            this.Settings = newSettings;
        }

        private void OnShowSettings(object sender, ExecutedRoutedEventArgs e)
        {
            this.SettingsControl.Visibility = Visibility.Visible;
            this.SettingsButton.Visibility = Visibility.Hidden;
        }

        private void OnHideSettings(object sender, ExecutedRoutedEventArgs e)
        {
            this.SettingsControl.Visibility = Visibility.Hidden;
            this.SettingsButton.Visibility = Visibility.Visible;
        }

        private void ResetTimeout()
        {
            if (this.TimeoutWarning)
            {
                this.adaptiveZoneLogic.ResetTimeout();
            }
        }
    }
}
