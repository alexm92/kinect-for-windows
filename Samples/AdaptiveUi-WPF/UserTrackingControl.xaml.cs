//------------------------------------------------------------------------------
// <copyright file="UserTrackingControl.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System;
    using System.Windows;
    using System.Windows.Media.Media3D;
    using Microsoft.Kinect;

    /// <summary>
    /// Control that places itself where the user is.  Pays
    /// attention to the user's zone to change the experience.
    /// </summary>
    public partial class UserTrackingControl
    {
        public static readonly DependencyProperty UserDistanceProperty =
            DependencyProperty.Register("UserDistance", typeof(UserDistance), typeof(UserTrackingControl), new PropertyMetadata(UserDistance.Far, (o, args) => ((UserTrackingControl)o).OnUserDistanceChanged((UserDistance)args.OldValue, (UserDistance)args.NewValue)));

        public static readonly DependencyProperty KinectSensorProperty =
            DependencyProperty.Register("KinectSensor", typeof(KinectSensor), typeof(UserTrackingControl), new PropertyMetadata(null, (o, args) => ((UserTrackingControl)o).OnKinectSensorChanged((KinectSensor)args.NewValue)));

        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(Settings), typeof(UserTrackingControl), new PropertyMetadata(null, (o, args) => ((UserTrackingControl)o).OnSettingsChanged((Settings)args.NewValue)));

        public static readonly DependencyProperty SensorToScreenCoordinatesTransformProperty =
            DependencyProperty.Register("SensorToScreenCoordinatesTransform", typeof(Matrix3D), typeof(UserTrackingControl), new PropertyMetadata(Matrix3D.Identity, (o, args) => ((UserTrackingControl)o).OnTransformChanged((Matrix3D)args.NewValue)));

        /// <summary>
        /// The width of this control sized using degrees in the user's field of view.
        /// </summary>
        private const double DegreesWide = 10.0;

        /// <summary>
        /// The height of this control sized using degrees in the user's field of view.
        /// </summary>
        private const double DegreesHigh = 10.0;

        private readonly AdaptiveUIPlacementHelper adaptiveUIPlacementHelper = new AdaptiveUIPlacementHelper();

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTrackingControl"/> class. 
        /// </summary>
        public UserTrackingControl()
        {
            this.InitializeComponent();
            this.Visibility = Visibility.Hidden;

            this.adaptiveUIPlacementHelper.DegreesHigh = DegreesHigh;
            this.adaptiveUIPlacementHelper.DegreesWide = DegreesWide;
            this.adaptiveUIPlacementHelper.Target = this;
        }

        /// <summary>
        /// Where the user is in relation to the display
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

        /// <summary>
        /// The sensor we are using
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
        /// Settings object used for placement calculations
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
        /// Transform from sensor skeleton space to screen coordinates
        /// </summary>
        public Matrix3D SensorToScreenCoordinatesTransform
        {
            get
            {
                return (Matrix3D)this.GetValue(SensorToScreenCoordinatesTransformProperty);
            }

            set
            {
                this.SetValue(SensorToScreenCoordinatesTransformProperty, value);
            }
        }

        private void OnUserDistanceChanged(UserDistance oldValue, UserDistance newValue)
        {
            this.adaptiveUIPlacementHelper.Parent = this.Parent as FrameworkElement;

            switch (newValue)
            {
                case UserDistance.Unknown:
                case UserDistance.Far:
                    this.Visibility = Visibility.Collapsed;
                    break;

                case UserDistance.Medium:
                    this.Visibility = Visibility.Visible;
                    this.StartingButton.Visibility = Visibility.Visible;
                    this.InformationButton.Visibility = Visibility.Hidden;
                    this.FadeOutOverlay.Visibility = Visibility.Hidden;

                    if (oldValue != UserDistance.Touch)
                    {
                        this.adaptiveUIPlacementHelper.UpdatePlacement(this.Settings.FarBoundary - this.Settings.BoundaryHysteresis);
                    }

                    break;

                case UserDistance.Touch:
                    this.Visibility = Visibility.Visible;
                    this.FadeOutOverlay.Visibility = Visibility.Visible;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("newValue");
            }
        }

        private void OnKinectSensorChanged(KinectSensor newSensor)
        {
            this.adaptiveUIPlacementHelper.KinectSensor = newSensor;
        }

        private void OnSettingsChanged(Settings newValue)
        {
            this.adaptiveUIPlacementHelper.Settings = newValue;
        }

        private void OnTransformChanged(Matrix3D newValue)
        {
            this.adaptiveUIPlacementHelper.SensorToScreenCoordinatesTransform = newValue;
        }

        private void StartingButtonClicked(object sender, RoutedEventArgs e)
        {
            this.StartingButton.Visibility = Visibility.Hidden;
            this.InformationButton.Visibility = Visibility.Visible;
        }

        private void InformationButtonClicked(object sender, RoutedEventArgs e)
        {
            this.StartingButton.Visibility = Visibility.Visible;
            this.InformationButton.Visibility = Visibility.Hidden;
        }
    }
}
