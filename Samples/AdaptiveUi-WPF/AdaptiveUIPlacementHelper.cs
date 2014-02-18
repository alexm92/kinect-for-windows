//------------------------------------------------------------------------------
// <copyright file="AdaptiveUIPlacementHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System.Windows;
    using System.Windows.Media.Media3D;

    using Microsoft.Kinect;

    /// <summary>
    /// Class that helps place UI components relative to a skeleton
    /// </summary>
    public class AdaptiveUIPlacementHelper
    {
        private Skeleton[] skeletons;

        private KinectSensor kinectSensor;

        private bool needPlacementUpdate;

        private double distanceOverride;

        /// <summary>
        /// KinectSensor in use
        /// </summary>
        public KinectSensor KinectSensor
        {
            get
            {
                return this.kinectSensor;
            }

            set
            {
                if (value != this.kinectSensor)
                {
                    if (this.kinectSensor != null)
                    {
                        this.kinectSensor.SkeletonFrameReady -= this.SensorOnSkeletonFrameReady;
                    }

                    if (value != null)
                    {
                        value.SkeletonFrameReady += this.SensorOnSkeletonFrameReady;
                    }

                    this.kinectSensor = value;
                }
            }
        }

        /// <summary>
        /// Transform from sensor skeleton space to screen coordinates
        /// </summary>
        public Matrix3D SensorToScreenCoordinatesTransform { get; set; }

        /// <summary>
        /// Settings object used for placement calculations
        /// </summary>
        public Settings Settings { get; set; }

        /// <summary>
        /// Parent container of the UI we are placing
        /// </summary>
        public FrameworkElement Parent { get; set; }

        /// <summary>
        /// UI that we are setting the position and size for.
        /// </summary>
        public FrameworkElement Target { get; set; }

        /// <summary>
        /// The width of this control sized using degrees in the user's field of view.
        /// </summary>
        public double DegreesWide { get; set; }

        /// <summary>
        /// The height of this control sized using degrees in the user's field of view.
        /// </summary>
        public double DegreesHigh { get; set; }

        /// <summary>
        /// Causes the placement to be updated and makes the target visible if
        /// it isn't already.  Placement will actually happen when the next
        /// skeleton frame comes from the sensor.
        /// </summary>
        /// <param name="distanceOverrideParameter">The z coordinate in meters from the display to use for placement calculations.</param>
        public void UpdatePlacement(double distanceOverrideParameter)
        {
            this.distanceOverride = distanceOverrideParameter;
            this.needPlacementUpdate = true;
        }

        private void SensorOnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs skeletonFrameReadyEventArgs)
        {
            if (!this.needPlacementUpdate)
            {
                return;
            }

            bool skeletonFrameReceived = false;

            using (var skeletonFrame = skeletonFrameReadyEventArgs.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if (this.skeletons == null || this.skeletons.Length != skeletonFrame.SkeletonArrayLength)
                    {
                        this.skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this.skeletons);
                    skeletonFrameReceived = true;
                }
            }

            if (skeletonFrameReceived)
            {
                this.UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            SkeletonPoint? headPoint = null;

            foreach (var skeleton in this.skeletons)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    // Assume we only have one tracked skeleton
                    if (skeleton.Joints[JointType.Head].TrackingState != JointTrackingState.NotTracked)
                    {
                        headPoint = skeleton.Joints[JointType.Head].Position;
                        break;
                    }
                }
            }

            if (!headPoint.HasValue)
            {
                return;
            }

            var transformedPosition = Transforms.TransformSkeletonPoint(this.SensorToScreenCoordinatesTransform, headPoint.Value);

            transformedPosition.Z = (float)this.distanceOverride;

            var placementRectangle =
                Transforms.CalculateUiPlacementRectangle(
                    this.Settings.DisplayWidthInMeters,
                    this.Settings.DisplayHeightInMeters,
                    this.Settings.DisplayWidthInPixels,
                    this.Settings.DisplayHeightInPixels,
                    new Vector3D(transformedPosition.X, transformedPosition.Y, transformedPosition.Z),
                    this.DegreesWide,
                    this.DegreesHigh,
                    this.Settings.VerticalUiOffsetInPixels);

            // Clamp the position to the Parent's space
            double parentWidth = this.Parent.ActualWidth;
            double parentHeight = this.Parent.ActualHeight;

            if (placementRectangle.X < 0.0)
            {
                placementRectangle.X = 0.0;
            }
            else if (placementRectangle.Right > parentWidth)
            {
                placementRectangle.X = parentWidth - placementRectangle.Width;

                // Handle the edge case where the caller is making the UI
                // larger then the screen.  Have it be in the upper-left
                // of the display.  However, UI should be designed so this
                // does not happen.
                if (placementRectangle.X < 0.0)
                {
                    placementRectangle.X = 0.0;
                }
            }

            if (placementRectangle.Y < 0.0)
            {
                placementRectangle.Y = 0.0;
            }
            else if (placementRectangle.Bottom > parentHeight)
            {
                placementRectangle.Y = parentHeight - placementRectangle.Height;

                // Handle the edge case where the caller is making the UI
                // larger then the screen.  Have it be in the upper-left
                // of the display.  However, UI should be designed so this
                // does not happen.
                if (placementRectangle.Y < 0.0)
                {
                    placementRectangle.Y = 0.0;
                }
            }

            // This placement scheme assumes the target is in a continer that positioning
            // it in the upper-left corner.
            this.Target.Margin = new Thickness(placementRectangle.X, placementRectangle.Y, 0.0, 0.0);
            this.Target.Width = placementRectangle.Width;
            this.Target.Height = placementRectangle.Height;

            // Ensure the target is visible.  It will be given to us hidden
            // so that it doesn't flash in it's old position while we are
            // waiting for a skeleton frame.
            this.Target.Visibility = Visibility.Visible;

            // We placed the target
            this.needPlacementUpdate = false;
        }
    }
}
