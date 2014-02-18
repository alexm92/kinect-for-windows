//------------------------------------------------------------------------------
// <copyright file="DialPadControl.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media.Media3D;

    using Microsoft.Kinect;

    /// <summary>
    /// Interaction logic for DialPadControl
    /// </summary>
    public partial class DialPadControl
    {
        public static readonly DependencyProperty UserDistanceProperty =
            DependencyProperty.Register("UserDistance", typeof(UserDistance), typeof(DialPadControl), new PropertyMetadata(UserDistance.Far, (o, args) => ((DialPadControl)o).OnUserDistanceChanged((UserDistance)args.NewValue)));

        public static readonly DependencyProperty KinectSensorProperty =
            DependencyProperty.Register("KinectSensor", typeof(KinectSensor), typeof(DialPadControl), new PropertyMetadata(null, (o, args) => ((DialPadControl)o).OnKinectSensorChanged((KinectSensor)args.NewValue)));

        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(Settings), typeof(DialPadControl), new PropertyMetadata(null, (o, args) => ((DialPadControl)o).OnSettingsChanged((Settings)args.NewValue)));

        public static readonly DependencyProperty SensorToScreenCoordinatesTransformProperty =
            DependencyProperty.Register("SensorToScreenCoordinatesTransform", typeof(Matrix3D), typeof(DialPadControl), new PropertyMetadata(Matrix3D.Identity, (o, args) => ((DialPadControl)o).OnTransformChanged((Matrix3D)args.NewValue)));

        public static readonly DependencyProperty TimeoutWarningProperty =
            DependencyProperty.Register("TimeoutWarning", typeof(bool), typeof(DialPadControl), new PropertyMetadata(false, (o, args) => ((DialPadControl)o).OnTimeoutWarningChanged((bool)args.NewValue)));

        /// <summary>
        /// The width of this control sized using degrees in the user's field of view.
        /// </summary>
        private const double DegreesWide = 20.0;

        /// <summary>
        /// The height of this control sized using degrees in the user's field of view.
        /// </summary>
        private const double DegreesHigh = 20.0;

        private readonly AdaptiveUIPlacementHelper adaptiveUIPlacementHelper = new AdaptiveUIPlacementHelper();

        /// <summary>
        /// Initializes a new instance of the <see cref="DialPadControl"/> class. 
        /// </summary>
        public DialPadControl()
        {
            this.InitializeComponent();
            this.Visibility = Visibility.Hidden;
            this.TimeoutWarningDisplay.Visibility = Visibility.Hidden;

            this.adaptiveUIPlacementHelper.DegreesHigh = DegreesHigh;
            this.adaptiveUIPlacementHelper.DegreesWide = DegreesWide;
            this.adaptiveUIPlacementHelper.Target = this;

            this.DigitsPanel.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(this.ButtonClickEventHandler));
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

        /// <summary>
        /// True when the app should show a warning that we are about to
        /// timeout to far mode.
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

        private void OnUserDistanceChanged(UserDistance userDistance)
        {
            this.adaptiveUIPlacementHelper.Parent = this.Parent as FrameworkElement;

            switch (userDistance)
            {
                case UserDistance.Unknown:
                case UserDistance.Far:
                case UserDistance.Medium:
                    this.Visibility = Visibility.Collapsed;
                    break;

                case UserDistance.Touch:
                    this.Visibility = Visibility.Visible;
                    this.adaptiveUIPlacementHelper.UpdatePlacement(this.Settings.NearBoundary - this.Settings.BoundaryHysteresis);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("userDistance");
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

        private void OnTimeoutWarningChanged(bool newValue)
        {
            this.TimeoutWarningDisplay.Visibility = newValue ? Visibility.Visible : Visibility.Hidden;
        }

        private void ButtonClickEventHandler(object sender, RoutedEventArgs args)
        {
            var button = args.OriginalSource as Button;

            if (button != null)
            {
                NumberDisplay.Text += button.Content as string;
                args.Handled = true;
            }
        }

        private void OnClearButtonClicked(object sender, RoutedEventArgs e)
        {
            NumberDisplay.Text = string.Empty;
        }
    }
}
