//------------------------------------------------------------------------------
// <copyright file="TrackingPolicy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System;
    using System.Linq;
    using System.Windows.Media.Media3D;

    using Microsoft.Kinect;

    /// <summary>
    /// Class that makes skeleton tracking only track one skeleton
    /// in front of the sensor.  Allows boundaries to be adjusted.
    /// It's used to reduce the possibility of other users off to
    /// the side inadvertently taking control of the UI.
    /// </summary>
    public class TrackingPolicy
    {
        private const int InvalidTrackingId = -1;

        private KinectSensor kinectSensor;

        private Skeleton[] skeletons;

        private int engagedTrackingId = InvalidTrackingId;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingPolicy"/> class. 
        /// </summary>
        public TrackingPolicy()
        {
            this.SensorToScreenPositionTransform = Matrix3D.Identity;
        }

        /// <summary>
        /// The sensor the application is using.
        /// </summary>
        public KinectSensor KinectSensor
        {
            get
            {
                return this.kinectSensor;
            }

            set
            {
                if (this.kinectSensor != value)
                {
                    var oldSensor = this.kinectSensor;
                    this.kinectSensor = value;
                    this.OnKinectSensorChanged(oldSensor, value);
                }
            }
        }

        /// <summary>
        /// Settings for the application.
        /// </summary>
        public Settings Settings { get; set; }

        /// <summary>
        /// Matrix that transforms from the sensor's skeleton space into a space
        /// whose origin is at the center of the display.
        /// </summary>
        public Matrix3D SensorToScreenPositionTransform { get; set; }

        private void OnKinectSensorChanged(KinectSensor oldSensor, KinectSensor newSensor)
        {
            if (oldSensor != null)
            {
                oldSensor.SkeletonFrameReady -= this.SkeletonFrameReady;
                oldSensor.SkeletonStream.AppChoosesSkeletons = false; // back to default
            }

            if (newSensor != null)
            {
                newSensor.SkeletonStream.AppChoosesSkeletons = true;
                newSensor.SkeletonFrameReady += this.SkeletonFrameReady;
            }
        }

        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs args)
        {
            int newEngagedTrackingId = InvalidTrackingId;

            using (SkeletonFrame skeletonFrame = args.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if (this.skeletons == null || this.skeletons.Length != skeletonFrame.SkeletonArrayLength)
                    {
                        this.skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this.skeletons);
                }
                else
                {
                    this.skeletons = null;
                }
            }

            if (this.skeletons != null)
            {
                // First see if the previously engaged skeleton is still valid
                if (this.engagedTrackingId != InvalidTrackingId)
                {
                    // Try to find the the currently engaged skeleton
                    var engagedSkeleton = this.skeletons.FirstOrDefault(skeleton => skeleton.TrackingId == this.engagedTrackingId);

                    // Make sure skeleton is still tracked
                    if (engagedSkeleton != null && engagedSkeleton.TrackingState != SkeletonTrackingState.Tracked)
                    {
                        engagedSkeleton = null;
                    }

                    // Make sure the engaged skeleton is still within the
                    // engagement zone.  The zone is centered at x==0.0.
                    if (engagedSkeleton != null && this.Settings.EngagementZoneEnabled)
                    {
                        var position = Transforms.TransformSkeletonPoint(this.SensorToScreenPositionTransform, engagedSkeleton.Position);
                        if (position.X < (-this.Settings.EngagementZoneRadius - this.Settings.EngagementZoneHysteresis)
                            || position.X > (this.Settings.EngagementZoneRadius + this.Settings.EngagementZoneHysteresis))
                        {
                            engagedSkeleton = null;
                        }
                    }

                    if (engagedSkeleton != null)
                    {
                        // engaged skeleton is still good
                        newEngagedTrackingId = engagedSkeleton.TrackingId;
                    }
                }

                // If the previously engaged skeleton is no good, try to find a new one
                if (newEngagedTrackingId == InvalidTrackingId)
                {
                    float newEngagedThingDistance = float.MaxValue;

                    foreach (var skeleton in this.skeletons)
                    {
                        if (skeleton.TrackingState == SkeletonTrackingState.NotTracked)
                        {
                            continue;
                        }

                        var position = Transforms.TransformSkeletonPoint(this.SensorToScreenPositionTransform, skeleton.Position);

                        if (this.Settings.EngagementZoneEnabled)
                        {
                            if (position.X < (-this.Settings.EngagementZoneRadius + this.Settings.EngagementZoneHysteresis)
                                || position.X > (this.Settings.EngagementZoneRadius - this.Settings.EngagementZoneHysteresis))
                            {
                                continue;
                            }
                        }

                        if (position.Z > newEngagedThingDistance)
                        {
                            continue;
                        }

                        newEngagedThingDistance = position.Z;
                        newEngagedTrackingId = skeleton.TrackingId;
                    }
                }
            }

            if (newEngagedTrackingId != this.engagedTrackingId)
            {
                try
                {
                    ((KinectSensor)sender).SkeletonStream.ChooseSkeletons(newEngagedTrackingId);
                    this.engagedTrackingId = newEngagedTrackingId;
                }
                catch (InvalidOperationException)
                {
                    // Ignore this.  When things are shutting down this call may fail
                    // because we got an old event for a sensor that is no longer
                    // available.
                }
            }
        }
    }
}
