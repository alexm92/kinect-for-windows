//------------------------------------------------------------------------------
// <copyright file="Settings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Scope = "member", Target = "Microsoft.Samples.Kinect.AdaptiveUI.Settings.#.cctor()", Justification = "Complexity is caused by long list of static properties. This is not real complexity.")]

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;

    /// <summary>
    /// Class holding all the application's settings.
    /// </summary>
    public class Settings : DependencyObject
    {
        public static readonly DependencyProperty DisplayWidthInMetersProperty =
            DependencyProperty.Register("DisplayWidthInMeters", typeof(double), typeof(Settings), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()), checkValue => ValidateDouble(checkValue, MinDisplayDimensionInMeters, MaxDisplayDimensionInMeters));

        public static readonly DependencyProperty DisplayHeightInMetersProperty =
            DependencyProperty.Register("DisplayHeightInMeters", typeof(double), typeof(Settings), new FrameworkPropertyMetadata(0.56, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()), checkValue => ValidateDouble(checkValue, MinDisplayDimensionInMeters, MaxDisplayDimensionInMeters));

        public static readonly DependencyProperty DisplayWidthInPixelsProperty =
            DependencyProperty.Register("DisplayWidthInPixels", typeof(double), typeof(Settings), new FrameworkPropertyMetadata(1280.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()), checkValue => ValidateDouble(checkValue, MinDisplayDimensionInPixels, MaxDisplayDimensionInPixels));

        public static readonly DependencyProperty DisplayHeightInPixelsProperty =
            DependencyProperty.Register("DisplayHeightInPixels", typeof(double), typeof(Settings), new FrameworkPropertyMetadata(1024.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()), checkValue => ValidateDouble(checkValue, MinDisplayDimensionInPixels, MaxDisplayDimensionInPixels));

        public static readonly DependencyProperty SensorOffsetXProperty =
            DependencyProperty.Register("SensorOffsetX", typeof(double), typeof(Settings), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        public static readonly DependencyProperty SensorOffsetYProperty =
            DependencyProperty.Register("SensorOffsetY", typeof(double), typeof(Settings), new FrameworkPropertyMetadata(0.35, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        public static readonly DependencyProperty SensorOffsetZProperty =
            DependencyProperty.Register("SensorOffsetZ", typeof(double), typeof(Settings), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        public static readonly DependencyProperty NearBoundaryProperty =
            DependencyProperty.Register("NearBoundary", typeof(double), typeof(Settings), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        public static readonly DependencyProperty FarBoundaryProperty =
            DependencyProperty.Register("FarBoundary", typeof(double), typeof(Settings), new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        public static readonly DependencyProperty BoundaryHysteresisProperty =
            DependencyProperty.Register("BoundaryHysteresis", typeof(double), typeof(Settings), new FrameworkPropertyMetadata(0.2, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()), checkValue => ValidateDouble(checkValue, 0.0, MaxHysteresis));

        public static readonly DependencyProperty EngagementZoneEnabledProperty =
            DependencyProperty.Register("EngagementZoneEnabled", typeof(bool), typeof(Settings), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        public static readonly DependencyProperty EngagementZoneRadiusProperty =
            DependencyProperty.Register("EngagementZoneRadius", typeof(double), typeof(Settings), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        public static readonly DependencyProperty EngagementZoneHysteresisProperty =
            DependencyProperty.Register("EngagementZoneHysteresis", typeof(double), typeof(Settings), new FrameworkPropertyMetadata(0.2, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()), checkValue => ValidateDouble(checkValue, 0.0, MaxHysteresis));

        public static readonly DependencyProperty VerticalUiOffsetInPixelsProperty =
            DependencyProperty.Register("VerticalUiOffsetInPixels", typeof(double), typeof(Settings), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        public static readonly DependencyProperty ShowProjectedSkeletonProperty =
            DependencyProperty.Register("ShowProjectedSkeleton", typeof(bool), typeof(Settings), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        public static readonly DependencyProperty ShowUiPlacementPreviewProperty =
            DependencyProperty.Register("ShowUiPlacementPreview", typeof(bool), typeof(Settings), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "InFar is not a compound word")]
        public static readonly DependencyProperty ShowUserViewerInFarModeProperty =
            DependencyProperty.Register("ShowUserViewerInFarMode", typeof(bool), typeof(Settings), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        public static readonly DependencyProperty FullScreenProperty =
            DependencyProperty.Register("FullScreen", typeof(bool), typeof(Settings), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        public static readonly DependencyProperty UseFixedSensorElevationAngleProperty =
            DependencyProperty.Register("UseFixedSensorElevationAngle", typeof(bool), typeof(Settings), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        public static readonly DependencyProperty FixedSensorElevationAngleProperty =
            DependencyProperty.Register("FixedSensorElevationAngle", typeof(double), typeof(Settings), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        public static readonly DependencyProperty NoUserTimeoutProperty =
            DependencyProperty.Register("NoUserTimeout", typeof(TimeSpan), typeof(Settings), new FrameworkPropertyMetadata(TimeSpan.FromSeconds(10), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        public static readonly DependencyProperty NoUserWarningTimeoutProperty =
            DependencyProperty.Register("NoUserWarningTimeout", typeof(TimeSpan), typeof(Settings), new FrameworkPropertyMetadata(TimeSpan.FromSeconds(5), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((Settings)o).OnParameterChanged()));

        private const double MinDisplayDimensionInMeters = 0.001;
        private const double MaxDisplayDimensionInMeters = 20;

        private const double MinDisplayDimensionInPixels = 1;
        private const double MaxDisplayDimensionInPixels = 10000;

        private const double MaxHysteresis = 10.0;

        public event EventHandler<EventArgs> ParameterChanged;

        /// <summary>
        /// Width of the display in meters.
        /// </summary>
        public double DisplayWidthInMeters
        {
            get
            {
                return (double)this.GetValue(DisplayWidthInMetersProperty);
            }

            set
            {
                this.SetValue(DisplayWidthInMetersProperty, value);
            }
        }

        /// <summary>
        /// Height of the display in meters.
        /// </summary>
        public double DisplayHeightInMeters
        {
            get
            {
                return (double)this.GetValue(DisplayHeightInMetersProperty);
            }

            set
            {
                this.SetValue(DisplayHeightInMetersProperty, value);
            }
        }

        /// <summary>
        /// Width of the display in pixels.
        /// </summary>
        public double DisplayWidthInPixels
        {
            get
            {
                return (double)this.GetValue(DisplayWidthInPixelsProperty);
            }

            set
            {
                this.SetValue(DisplayWidthInPixelsProperty, value);
            }
        }

        /// <summary>
        /// Height of the display in pixels.
        /// </summary>
        public double DisplayHeightInPixels
        {
            get
            {
                return (double)this.GetValue(DisplayHeightInPixelsProperty);
            }

            set
            {
                this.SetValue(DisplayHeightInPixelsProperty, value);
            }
        }

        /// <summary>
        /// X component of a vector that is the physical location of the Kinect
        /// sensor relative to the center of the display.  In meters.
        /// </summary>
        public double SensorOffsetX
        {
            get
            {
                return (double)this.GetValue(SensorOffsetXProperty);
            }

            set
            {
                this.SetValue(SensorOffsetXProperty, value);
            }
        }

        /// <summary>
        /// Y component of a vector that is the physical location of the Kinect
        /// sensor relative to the center of the display.  In meters.
        /// </summary>
        public double SensorOffsetY
        {
            get
            {
                return (double)this.GetValue(SensorOffsetYProperty);
            }

            set
            {
                this.SetValue(SensorOffsetYProperty, value);
            }
        }

        /// <summary>
        /// Z component of a vector that is the physical location of the Kinect
        /// sensor relative to the center of the display.  In meters.
        /// </summary>
        public double SensorOffsetZ
        {
            get
            {
                return (double)this.GetValue(SensorOffsetZProperty);
            }

            set
            {
                this.SetValue(SensorOffsetZProperty, value);
            }
        }

        /// <summary>
        /// Near boundary in meters from the center of the screen.
        /// </summary>
        public double NearBoundary
        {
            get
            {
                return (double)this.GetValue(NearBoundaryProperty);
            }

            set
            {
                this.SetValue(NearBoundaryProperty, value);
            }
        }

        /// <summary>
        /// Far boundary in meters from the center of the screen.
        /// </summary>
        public double FarBoundary
        {
            get
            {
                return (double)this.GetValue(FarBoundaryProperty);
            }

            set
            {
                this.SetValue(FarBoundaryProperty, value);
            }
        }

        /// <summary>
        /// Hysteresis used for the near and far boundaries in meters.
        /// </summary>
        public double BoundaryHysteresis
        {
            get
            {
                return (double)this.GetValue(BoundaryHysteresisProperty);
            }

            set
            {
                this.SetValue(BoundaryHysteresisProperty, value);
            }
        }

        /// <summary>
        /// Distance of the left and right engagement boundaries from the
        /// center of the display in meters.
        /// </summary>
        public double EngagementZoneRadius
        {
            get
            {
                return (double)this.GetValue(EngagementZoneRadiusProperty);
            }

            set
            {
                this.SetValue(EngagementZoneRadiusProperty, value);
            }
        }

        /// <summary>
        /// Hysteresis of the engagement zone boundaries in meters.
        /// </summary>
        public double EngagementZoneHysteresis
        {
            get
            {
                return (double)this.GetValue(EngagementZoneHysteresisProperty);
            }

            set
            {
                this.SetValue(EngagementZoneHysteresisProperty, value);
            }
        }

        /// <summary>
        /// Set if we are using the engagement zone.
        /// </summary>
        public bool EngagementZoneEnabled
        {
            get
            {
                return (bool)this.GetValue(EngagementZoneEnabledProperty);
            }

            set
            {
                this.SetValue(EngagementZoneEnabledProperty, value);
            }
        }

        /// <summary>
        /// Set to true to show the diagnostic skeleton view.
        /// </summary>
        public bool ShowProjectedSkeleton
        {
            get
            {
                return (bool)this.GetValue(ShowProjectedSkeletonProperty);
            }

            set
            {
                this.SetValue(ShowProjectedSkeletonProperty, value);
            }
        }

        /// <summary>
        /// Set to true to show where UI should be placed for the user.
        /// </summary>
        public bool ShowUiPlacementPreview
        {
            get
            {
                return (bool)this.GetValue(ShowUiPlacementPreviewProperty);
            }

            set
            {
                this.SetValue(ShowUiPlacementPreviewProperty, value);
            }
        }

        /// <summary>
        /// True to show the user viewer when in far mode.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "InFar is not a compound word")]
        public bool ShowUserViewerInFarMode
        {
            get
            {
                return (bool)this.GetValue(ShowUserViewerInFarModeProperty);
            }

            set
            {
                this.SetValue(ShowUserViewerInFarModeProperty, value);
            }
        }

        /// <summary>
        /// Offset in pixels from the head location to where the
        /// UI should be placed.
        /// </summary>
        public double VerticalUiOffsetInPixels
        {
            get
            {
                return (double)this.GetValue(VerticalUiOffsetInPixelsProperty);
            }

            set
            {
                this.SetValue(VerticalUiOffsetInPixelsProperty, value);
            }
        }

        /// <summary>
        /// True to put the window into full screen mode.
        /// </summary>
        public bool FullScreen
        {
            get
            {
                return (bool)this.GetValue(FullScreenProperty);
            }

            set
            {
                this.SetValue(FullScreenProperty, value);
            }
        }

        /// <summary>
        /// Set this to true to use the FixedSensorElevationAngle setting
        /// for transform calculations.  If it is false we use the reading
        /// from the sensor.
        /// </summary>
        public bool UseFixedSensorElevationAngle
        {
            get
            {
                return (bool)this.GetValue(UseFixedSensorElevationAngleProperty);
            }

            set
            {
                this.SetValue(UseFixedSensorElevationAngleProperty, value);
            }
        }

        /// <summary>
        /// Override value of the sensor angle for transform calculations in
        /// degrees.
        /// </summary>
        public double FixedSensorElevationAngle
        {
            get
            {
                return (double)this.GetValue(FixedSensorElevationAngleProperty);
            }

            set
            {
                this.SetValue(FixedSensorElevationAngleProperty, value);
            }
        }

        /// <summary>
        /// How long we wait after not seeing a user until we timeout to far
        /// mode.
        /// </summary>
        public TimeSpan NoUserTimeout
        {
            get
            {
                return (TimeSpan)this.GetValue(NoUserTimeoutProperty);
            }

            set
            {
                this.SetValue(NoUserTimeoutProperty, value);
            }
        }

        /// <summary>
        /// How long we wait after not seeing a user until we show a timeout
        /// message.
        /// </summary>
        public TimeSpan NoUserWarningTimeout
        {
            get
            {
                return (TimeSpan)this.GetValue(NoUserWarningTimeoutProperty);
            }

            set
            {
                this.SetValue(NoUserWarningTimeoutProperty, value);
            }
        }

        private static bool ValidateDouble(object checkValue, double minValue, double maxValue)
        {
            var doubleValue = (double)checkValue;
            return doubleValue >= minValue && doubleValue <= maxValue;
        }

        private void OnParameterChanged()
        {
            if (this.ParameterChanged != null)
            {
                this.ParameterChanged(this, new EventArgs());
            }
        }
    }
}
