//------------------------------------------------------------------------------
// <copyright file="ProjectedSkeletonVisualizer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using Microsoft.Kinect;

    /// <summary>
    /// Displays the tracked skeleton and UI placement preview using the
    /// skeleton-space to screen space transform.  Used for making sure
    /// the settings are correct.
    /// </summary>
    public class ProjectedSkeletonVisualizer : Control
    {
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(Settings), typeof(ProjectedSkeletonVisualizer), new PropertyMetadata(null));

        public static readonly DependencyProperty KinectSensorProperty =
            DependencyProperty.Register("KinectSensor", typeof(KinectSensor), typeof(ProjectedSkeletonVisualizer), new PropertyMetadata(null, (o, args) => ((ProjectedSkeletonVisualizer)o).OnKinectSensorChanged((KinectSensor)args.OldValue, (KinectSensor)args.NewValue)));

        public static readonly DependencyProperty SensorToScreenCoordinatesTransformProperty =
            DependencyProperty.Register("SensorToScreenCoordinatesTransform", typeof(Matrix3D), typeof(ProjectedSkeletonVisualizer), new PropertyMetadata(Matrix3D.Identity));

        /// <summary>
        /// Thickness of drawn joint lines.
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// The width of the placement preview sized using degrees in the user's field of view.
        /// </summary>
        private const double PlacementPreviewDegreesWide = 20.0;

        /// <summary>
        /// The height of the placement preview sized using degrees in the user's field of view.
        /// </summary>
        private const double PlacementPreviewDegreesHigh = 20.0;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked.
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked.
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred.
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Pen used for drawing the UI placement guide outline.
        /// </summary>        
        private readonly Pen uiPlacementGuidePen = new Pen(new SolidColorBrush(Color.FromRgb(0x51, 0x31, 0x8F)), 2) { DashStyle = new DashStyle(new double[] { 2, 2 }, 0) };

        /// <summary>
        /// Typeface used for drawing the UI placement guide text.
        /// </summary>
        private readonly Typeface uiPlacementGuideTypeface = new Typeface("SegoeUI");

        /// <summary>
        /// Brush used for drawing the UI placement guide text.
        /// </summary>
        private readonly Brush uiPlacementGuideTextBrush = new SolidColorBrush(Color.FromRgb(0x51, 0x31, 0x8F));

        /// <summary>
        /// Point used to offset the UI placement guide text from the outline.
        /// </summary>
        private readonly Point uiPlacementGuideTextOffset = new Point(20, 20);

        private Skeleton[] skeletons;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "OverrideMetadata can't be done inline")]
        static ProjectedSkeletonVisualizer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProjectedSkeletonVisualizer), new FrameworkPropertyMetadata(typeof(ProjectedSkeletonVisualizer)));
        }

        /// <summary>
        /// Sensor used by the application.
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
        /// Settings object used by the application.
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
        /// Transformation matrix that transforms points from sensor skeleton
        /// space to screen coordinates in pixels.
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
        /// Draw the skeleton and placement preview.
        /// </summary>
        /// <param name="drawingContext">DrawingContext to draw to</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }

            base.OnRender(drawingContext);

            if (this.skeletons == null || this.Settings == null)
            {
                return;
            }

            if (this.Settings == null)
            {
                return;
            }

            foreach (var skeleton in this.skeletons)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    if (this.Settings.ShowProjectedSkeleton)
                    {
                        this.DrawBonesAndJoints(skeleton, drawingContext);
                    }

                    if (this.Settings.ShowUiPlacementPreview)
                    {
                        this.DrawUiPlacementGuide(skeleton, drawingContext);
                    }
                }
            }
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
                    this.InvalidateVisual();
                }
            }
        }

        /// <summary>
        /// Draws a box showing the ideal placement for UI for a specific user.
        /// </summary>
        /// <param name="skeleton">Skeleton to draw the placement for</param>
        /// <param name="drawingContext">destination to draw to</param>
        private void DrawUiPlacementGuide(Skeleton skeleton, DrawingContext drawingContext)
        {
            var headPositionCoordinates = Transforms.TransformSkeletonPoint(
                this.SensorToScreenCoordinatesTransform, skeleton.Joints[JointType.Head].Position);

            var rect = Transforms.CalculateUiPlacementRectangle(
                this.Settings.DisplayWidthInMeters,
                this.Settings.DisplayHeightInMeters,
                this.Settings.DisplayWidthInPixels,
                this.Settings.DisplayHeightInPixels,
                new Vector3D(headPositionCoordinates.X, headPositionCoordinates.Y, headPositionCoordinates.Z),
                PlacementPreviewDegreesWide,
                PlacementPreviewDegreesHigh,
                this.Settings.VerticalUiOffsetInPixels);

            drawingContext.DrawRectangle(null, this.uiPlacementGuidePen, rect);

            FormattedText text = new FormattedText(
                    "Optimal UI placement preview",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    uiPlacementGuideTypeface,
                    25,
                    uiPlacementGuideTextBrush);

            Point textOrigin = new Point(rect.X + uiPlacementGuideTextOffset.X, rect.Y + uiPlacementGuideTextOffset.Y);
            drawingContext.DrawText(text, textOrigin);
        }

        /// <summary>
        /// Draws a skeleton's bones and joints.
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to display coordinates and converts to Point.
        /// </summary>
        /// <param name="skeletonPoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skeletonPoint)
        {
            // Convert point to screen space.  
            var displayPoint = Transforms.TransformSkeletonPoint(this.SensorToScreenCoordinatesTransform, skeletonPoint);
            return new Point(displayPoint.X, displayPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }
    }
}
