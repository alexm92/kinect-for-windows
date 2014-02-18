//------------------------------------------------------------------------------
// <copyright file="AdaptiveZoneVisualizer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    using Microsoft.Kinect;

    /// <summary>
    /// Displays the adaptive zones, user positions, and sensor and display
    /// layout.
    /// </summary>
    public class AdaptiveZoneVisualizer : Control
    {
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(Settings), typeof(AdaptiveZoneVisualizer), new PropertyMetadata(null, (o, args) => ((AdaptiveZoneVisualizer)o).OnSettingsChanged((Settings)args.OldValue, (Settings)args.NewValue)));

        public static readonly DependencyProperty SensorToScreenPositionTransformProperty =
            DependencyProperty.Register("SensorToScreenPositionTransform", typeof(Matrix3D), typeof(AdaptiveZoneVisualizer), new PropertyMetadata(Matrix3D.Identity));

        public static readonly DependencyProperty SomethingNearSensorProperty =
            DependencyProperty.Register("SomethingNearSensor", typeof(bool), typeof(AdaptiveZoneVisualizer), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty KinectSensorProperty =
            DependencyProperty.Register("KinectSensor", typeof(KinectSensor), typeof(AdaptiveZoneVisualizer), new PropertyMetadata(null, (o, args) => ((AdaptiveZoneVisualizer)o).OnKinectSensorChanged((KinectSensor)args.OldValue, (KinectSensor)args.NewValue)));

        public static readonly DependencyProperty HorizontalBoundaryBrushProperty =
            DependencyProperty.Register("HorizontalBoundaryBrush", typeof(Brush), typeof(AdaptiveZoneVisualizer), new PropertyMetadata(new SolidColorBrush(ChangeAlpha(Colors.Purple, 0.5))));

        public static readonly DependencyProperty VerticalBoundaryBrushProperty =
            DependencyProperty.Register("VerticalBoundaryBrush", typeof(Brush), typeof(AdaptiveZoneVisualizer), new PropertyMetadata(new SolidColorBrush(ChangeAlpha(Colors.Aqua, 0.5))));

        public static readonly DependencyProperty BoundaryPenProperty =
            DependencyProperty.Register("BoundaryPen", typeof(Pen), typeof(AdaptiveZoneVisualizer), new PropertyMetadata(new Pen(Brushes.White, 1.0) { DashStyle = new DashStyle(new[] { 2.0, 2.0 }, 0.0) }));

        public static readonly DependencyProperty SomethingNearSensorBrushProperty =
            DependencyProperty.Register("SomethingNearSensorBrush", typeof(Brush), typeof(AdaptiveZoneVisualizer), new PropertyMetadata(new SolidColorBrush(ChangeAlpha(Colors.Aqua, 0.7))));

        public static readonly DependencyProperty TrackedUserBrushProperty =
            DependencyProperty.Register("TrackedUserBrush", typeof(Brush), typeof(AdaptiveZoneVisualizer), new PropertyMetadata(Brushes.Aqua));

        public static readonly DependencyProperty NonTrackedUserBrushProperty =
            DependencyProperty.Register("NonTrackedUserBrush", typeof(Brush), typeof(AdaptiveZoneVisualizer), new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty KinectDeviceBrushProperty =
            DependencyProperty.Register("KinectDeviceBrush", typeof(Brush), typeof(AdaptiveZoneVisualizer), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty DisplayDeviceBrushProperty =
            DependencyProperty.Register("DisplayDeviceBrush", typeof(Brush), typeof(AdaptiveZoneVisualizer), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty FieldOfViewBrushProperty =
            DependencyProperty.Register("FieldOfViewBrush", typeof(Brush), typeof(AdaptiveZoneVisualizer), new PropertyMetadata(Brushes.LightGray));

        private const double MeterWidth = 4.0;

        private const double MeterHeight = 4.0;

        private const double KinectWidthInMeters = 0.3;

        private const double KinectDepthInMeters = 0.05;

        private const double DisplayDepthInMeters = 0.1;

        private const double SkeletonIndicatorWidthInMeters = 0.2;

        private const double SomethingNearSensorIndicatorDiameterInMeters = 0.5;

        private readonly Point somethingNearSensorIndicatorCenterInMeters = new Point(MeterWidth / 2.0, 0.5);

        private readonly List<Boundary> boundaries = new List<Boundary>();

        private Geometry fieldOfViewGeometry;

        private Rect displayRectInMeters;

        private Rect kinectDeviceRectInMeters;
        
        private Skeleton[] skeletons;
        
        private double pixelWidth;

        private double pixelHeight;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "OverrideMetadata cannot be done inline.")]
        static AdaptiveZoneVisualizer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AdaptiveZoneVisualizer), new FrameworkPropertyMetadata(typeof(AdaptiveZoneVisualizer)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveZoneVisualizer"/> class. 
        /// </summary>
        public AdaptiveZoneVisualizer()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                // Add some place holder boundaries for design mode.
                const double PlaceHolderHysteresis = 0.2;
                const double HorizontalPlaceHolder1Position = 1.0;
                const double HorizontalPlaceHolder2Position = 2.5;
                const double VerticalPlaceHolder1Position = -1.0;
                const double VerticalPlaceHolder2Position = 1.0;

                this.AddBoundary(this.HorizontalBoundaryBrush, true, HorizontalPlaceHolder1Position, PlaceHolderHysteresis);
                this.AddBoundary(this.HorizontalBoundaryBrush, true, HorizontalPlaceHolder2Position, PlaceHolderHysteresis);
                this.AddBoundary(this.VerticalBoundaryBrush, false, VerticalPlaceHolder1Position, PlaceHolderHysteresis);
                this.AddBoundary(this.VerticalBoundaryBrush, false, VerticalPlaceHolder2Position, PlaceHolderHysteresis);
            }

            this.SizeChanged += (sender, args) => this.ClearVieldOfViewGeometry();
        }

        /// <summary>
        /// Object that contains all the settings of the application.
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
        /// Transforms from sensor skeleton space into a space whose origin is
        /// the center of the screen and has the same units as skeleton space.
        /// </summary>
        public Matrix3D SensorToScreenPositionTransform
        {
            get
            {
                return (Matrix3D)this.GetValue(SensorToScreenPositionTransformProperty);
            }

            set
            {
                this.SetValue(SensorToScreenPositionTransformProperty, value);
            }
        }

        /// <summary>
        /// Whether when something is detected near the sensor.
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
        /// The current Kinect sensor.
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
        /// Brush used to draw the horizontal zone boundaries.
        /// </summary>
        public Brush HorizontalBoundaryBrush
        {
            get
            {
                return (Brush)this.GetValue(HorizontalBoundaryBrushProperty);
            }

            set
            {
                this.SetValue(HorizontalBoundaryBrushProperty, value);
            }
        }

        /// <summary>
        /// Brush used to draw the vertical zone boundaries.
        /// </summary>
        public Brush VerticalBoundaryBrush
        {
            get
            {
                return (Brush)this.GetValue(VerticalBoundaryBrushProperty);
            }

            set
            {
                this.SetValue(VerticalBoundaryBrushProperty, value);
            }
        }

        /// <summary>
        /// Pen used to draw the center of the boundaries.
        /// </summary>
        public Pen BoundaryPen
        {
            get
            {
                return (Pen)this.GetValue(BoundaryPenProperty);
            }

            set
            {
                this.SetValue(BoundaryPenProperty, value);
            }
        }

        /// <summary>
        /// Brush used to draw the something near sensor indicator.
        /// </summary>
        public Brush SomethingNearSensorBrush
        {
            get
            {
                return (Brush)this.GetValue(SomethingNearSensorBrushProperty);
            }

            set
            {
                this.SetValue(SomethingNearSensorBrushProperty, value);
            }
        }

        /// <summary>
        /// Brush used to draw tracked skeleton markers.
        /// </summary>
        public Brush TrackedUserBrush
        {
            get
            {
                return (Brush)this.GetValue(TrackedUserBrushProperty);
            }

            set
            {
                this.SetValue(TrackedUserBrushProperty, value);
            }
        }

        /// <summary>
        /// Brush used to draw non-tracked skeleton markers
        /// (position only.)
        /// </summary>
        public Brush NonTrackedUserBrush
        {
            get
            {
                return (Brush)this.GetValue(NonTrackedUserBrushProperty);
            }

            set
            {
                this.SetValue(NonTrackedUserBrushProperty, value);
            }
        }

        /// <summary>
        /// Brush for the Kinect device indicator.
        /// </summary>
        public Brush KinectDeviceBrush
        {
            get
            {
                return (Brush)this.GetValue(KinectDeviceBrushProperty);
            }

            set
            {
                this.SetValue(KinectDeviceBrushProperty, value);
            }
        }

        /// <summary>
        /// Brush used to draw the display.
        /// </summary>
        public Brush DisplayDeviceBrush
        {
            get
            {
                return (Brush)this.GetValue(DisplayDeviceBrushProperty);
            }

            set
            {
                this.SetValue(DisplayDeviceBrushProperty, value);
            }
        }

        /// <summary>
        /// Brush used to draw the field of view indicator.
        /// </summary>
        public Brush FieldOfViewBrush
        {
            get
            {
                return (Brush)this.GetValue(FieldOfViewBrushProperty);
            }

            set
            {
                this.SetValue(FieldOfViewBrushProperty, value);
            }
        }

        /// <summary>
        /// Does all the drawing of the visualizer.
        /// </summary>
        /// <param name="drawingContext">DrawingContext to draw to</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }

            base.OnRender(drawingContext);

            // These are used by the MeterToPixel
            // conversion so they must be set right away.
            this.pixelWidth = this.ActualWidth;
            this.pixelHeight = this.ActualHeight;

            // Add the boundaries if we have none.
            if (this.boundaries.Count == 0 && this.Settings != null)
            {
                this.AddBoundary(this.HorizontalBoundaryBrush, true, this.Settings.NearBoundary, this.Settings.BoundaryHysteresis);
                this.AddBoundary(this.HorizontalBoundaryBrush, true, this.Settings.FarBoundary, this.Settings.BoundaryHysteresis);
                if (this.Settings.EngagementZoneEnabled)
                {
                    this.AddBoundary(this.VerticalBoundaryBrush, false, -this.Settings.EngagementZoneRadius, this.Settings.EngagementZoneHysteresis);
                    this.AddBoundary(this.VerticalBoundaryBrush, false, this.Settings.EngagementZoneRadius, this.Settings.EngagementZoneHysteresis);
                }
            }
            
            // Create the field of view indicator geometry if needed
            if (this.fieldOfViewGeometry == null)
            {
                this.fieldOfViewGeometry = this.CreateFieldOfViewGeometry();
            }

            // Draw the field of view indicator
            drawingContext.DrawGeometry(this.FieldOfViewBrush, null, this.fieldOfViewGeometry);

            // Draw the boundaries
            foreach (var boundary in this.boundaries)
            {
                if (boundary.Horizontal)
                {
                    var rect = new Rect(0.0, boundary.Position - boundary.Hysteresis, MeterWidth, boundary.Hysteresis * 2.0);
                    var boundaryPosition = boundary.Position;
                    rect = this.MeterToPixel(rect);
                    boundaryPosition = MeterToPixel(this.pixelHeight, MeterHeight, boundaryPosition);
                    drawingContext.DrawRectangle(boundary.Brush, null, rect);
                    drawingContext.DrawLine(this.BoundaryPen, new Point(0.0, boundaryPosition), new Point(this.pixelWidth, boundaryPosition));
                }
                else
                {
                    var rect = new Rect(boundary.Position - boundary.Hysteresis + (MeterWidth / 2.0), 0.0, boundary.Hysteresis * 2, MeterHeight);
                    var boundaryPosition = boundary.Position + (MeterWidth / 2.0);
                    rect = this.MeterToPixel(rect);
                    boundaryPosition = MeterToPixel(this.pixelWidth, MeterWidth, boundaryPosition);
                    drawingContext.DrawRectangle(boundary.Brush, null, rect);
                    drawingContext.DrawLine(this.BoundaryPen, new Point(boundaryPosition, 0.0), new Point(boundaryPosition, this.pixelHeight));
                }
            }

            // Draw the display device
            var displayRect = this.MeterToPixel(this.displayRectInMeters);
            drawingContext.DrawRectangle(this.DisplayDeviceBrush, null, displayRect);

            // Draw the Kinect device
            var kinectRect = this.MeterToPixel(this.kinectDeviceRectInMeters);
            drawingContext.DrawRectangle(this.KinectDeviceBrush, null, kinectRect);

            // If something is near the sensor, draw that
            if (this.SomethingNearSensor)
            {
                var center = this.MeterToPixel(this.somethingNearSensorIndicatorCenterInMeters);
                var diameters = this.MeterToPixel(new Vector(SomethingNearSensorIndicatorDiameterInMeters, SomethingNearSensorIndicatorDiameterInMeters));
                drawingContext.DrawEllipse(this.SomethingNearSensorBrush, null, center, diameters.X, diameters.Y);
            }

            // Draw the skeletons
            if (this.skeletons != null)
            {
                foreach (var skeleton in this.skeletons)
                {
                    if (skeleton.TrackingState != SkeletonTrackingState.NotTracked)
                    {
                        var displayRelativePosition =
                            Transforms.TransformSkeletonPoint(this.SensorToScreenPositionTransform, skeleton.Position);
                        var brush = (skeleton.TrackingState == SkeletonTrackingState.Tracked) ? this.TrackedUserBrush : this.NonTrackedUserBrush;
                        this.PlotSkeleton(drawingContext, brush, displayRelativePosition.X, displayRelativePosition.Z);
                    }
                }
            }
            else
            {
                if (DesignerProperties.GetIsInDesignMode(this))
                {
                    // Draw some place holders when we are in design mode
                    this.PlotSkeleton(drawingContext, this.TrackedUserBrush, -0.5f, 1.0f);
                    this.PlotSkeleton(drawingContext, this.NonTrackedUserBrush, 0.5f, 1.0f);
                }
            }
        }

        private static Color ChangeAlpha(Color color, double alpha)
        {
            var retval = color;
            retval.A = (byte)(alpha * 255);
            return retval;
        }

        private static double MeterToPixel(double pixelExtent, double meterExtent, double meters)
        {
            return meters * pixelExtent / meterExtent;
        }

        private void AddBoundary(Brush brush, bool isHorizontal, double position, double hysteresis)
        {
            this.boundaries.Add(new Boundary { Brush = brush, Horizontal = isHorizontal, Position = position, Hysteresis = hysteresis });
        }

        private void ClearVieldOfViewGeometry()
        {
            this.fieldOfViewGeometry = null;
        }

        private Geometry CreateFieldOfViewGeometry()
        {
            // Positioing of the start and end of the rays from
            // the sensor that bound the FOV shape.
            const double StartEdgeMeterOffset = 0.4;
            const double EndEdgeMeterLength = 3.0;

            // The depth sensor is documented to have a field of view
            // of 57 degrees.
            const double FovRadians = 57.0 * Math.PI / 180.0;

            // The angles of the left and right boundaries of the field of
            // view indicator.
            const double FovLeft = Math.PI + ((Math.PI - FovRadians) / 2.0);
            const double FovRight = FovLeft + FovRadians;

            // Radius of the circle for drawing the arc segment at the bottom
            // of the field of view indicator.
            const double ArcRadius = 120;

            var figure = new PathFigure();
            var segments = new PathSegmentCollection();

            var center = new Vector(MeterWidth / 2.0, 0.0);
            if (this.Settings != null)
            {
                center += new Vector(this.Settings.SensorOffsetX, this.Settings.SensorOffsetZ);
            }

            var rightEdgeVector = new Vector(Math.Cos(FovRight), -Math.Sin(FovRight));
            var leftEdgeVector = new Vector(Math.Cos(FovLeft), -Math.Sin(FovLeft));
            var rightEdgeStartPoint = this.MeterToPixel((Point)(center + (rightEdgeVector * StartEdgeMeterOffset)));
            var rightEdgeEndPoint = this.MeterToPixel((Point)(center + (rightEdgeVector * EndEdgeMeterLength)));
            var leftEdgeStartPoint = this.MeterToPixel((Point)(center + (leftEdgeVector * StartEdgeMeterOffset)));
            var leftEdgeEndPoint = this.MeterToPixel((Point)(center + (leftEdgeVector * EndEdgeMeterLength)));

            figure.IsFilled = true;
            figure.StartPoint = rightEdgeStartPoint;
            segments.Add(new LineSegment(rightEdgeEndPoint, true));
            segments.Add(new ArcSegment(leftEdgeEndPoint, new Size(ArcRadius, ArcRadius), 0.0, false, SweepDirection.Clockwise, true));
            segments.Add(new LineSegment(leftEdgeStartPoint, true));
            segments.Add(new LineSegment(rightEdgeStartPoint, true));
            figure.Segments = segments;

            var figures = new PathFigureCollection { figure };
            return new PathGeometry(figures);
        }

        private void PlotSkeleton(DrawingContext drawingContext, Brush fill, float x, float y)
        {
            drawingContext.DrawEllipse(
                fill,
                null,
                this.MeterToPixel(new Point(x + (MeterWidth / 2.0), y)),
                MeterToPixel(this.pixelWidth, MeterWidth, SkeletonIndicatorWidthInMeters),
                MeterToPixel(this.pixelHeight, MeterHeight, SkeletonIndicatorWidthInMeters));
        }

        private Rect MeterToPixel(Rect rect)
        {
            return new Rect(MeterToPixel(this.pixelWidth, MeterWidth, rect.Left), MeterToPixel(this.pixelHeight, MeterHeight, rect.Top), MeterToPixel(this.pixelWidth, MeterWidth, rect.Width), MeterToPixel(this.pixelHeight, MeterHeight, rect.Height));
        }

        private Point MeterToPixel(Point pt)
        {
            return new Point(MeterToPixel(this.pixelWidth, MeterWidth, pt.X), MeterToPixel(this.pixelHeight, MeterHeight, pt.Y));
        }

        private Vector MeterToPixel(Vector pt)
        {
            return new Vector(MeterToPixel(this.pixelWidth, MeterWidth, pt.X), MeterToPixel(this.pixelHeight, MeterHeight, pt.Y));
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

            this.OnSettingsParameterChanged(null, null);
        }

        private void OnSettingsParameterChanged(object sender, EventArgs eventArgs)
        {
            this.boundaries.Clear();
            this.ClearVieldOfViewGeometry();

            if (this.Settings != null)
            {
                this.displayRectInMeters = new Rect((MeterWidth / 2.0) - (this.Settings.DisplayWidthInMeters / 2.0), 0.0, this.Settings.DisplayWidthInMeters, DisplayDepthInMeters);
                this.kinectDeviceRectInMeters = new Rect((MeterWidth / 2.0) - (KinectWidthInMeters / 2.0) + this.Settings.SensorOffsetX, this.Settings.SensorOffsetZ, KinectWidthInMeters, KinectDepthInMeters);
            }

            this.InvalidateVisual();
        }

        private void OnKinectSensorChanged(KinectSensor oldSensor, KinectSensor newSensor)
        {
            if (oldSensor != null)
            {
                oldSensor.SkeletonFrameReady -= this.SensorOnSkeletonFrameReady;
            }

            if (newSensor != null)
            {
                newSensor.SkeletonFrameReady += this.SensorOnSkeletonFrameReady;
            }
        }

        private void SensorOnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs skeletonFrameReadyEventArgs)
        {
            using (var skeletonFrame = skeletonFrameReadyEventArgs.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if (this.skeletons == null || this.skeletons.Length != skeletonFrame.SkeletonArrayLength)
                    {
                        this.skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this.skeletons);
                }
            }

            this.InvalidateVisual();
        }

        private class Boundary
        {
            public Brush Brush { get; set; }

            public bool Horizontal { get; set; }

            public double Position { get; set; }

            public double Hysteresis { get; set; }
        }
    }
}
