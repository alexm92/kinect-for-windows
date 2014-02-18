// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdaptiveZoneLogic.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Windows.Media.Media3D;

    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit.Interaction;

    /// <summary>
    /// The different zones the user can be in.
    /// </summary>
    public enum UserDistance
    {
        Unknown = 0,
        Touch,
        Medium,
        Far,
    }

    /// <summary>
    /// Logic that determines which zone the tracked skeleton or blob is in.
    /// </summary>
    public class AdaptiveZoneLogic : INotifyPropertyChanged
    {
        /// <summary>
        /// Number of pixels needed to say that something is
        /// near the sensor.
        /// </summary>
        private const int MinimumNearPixelsCount = 20;

        /// <summary>
        /// Threshold used to determine if something is near the sensor.
        /// </summary>
        private const int DefaultNearPixelThreshold = 1000;

        private KinectSensor kinectSensor;

        private Skeleton[] skeletons;

        /// <summary>
        /// Time when we determined nothing was near the sensor.  This
        /// gets checked each frame to see if we timed out.
        /// </summary>
        private DateTime nothingNearSensorTime = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveZoneLogic"/> class. 
        /// </summary>
        public AdaptiveZoneLogic()
        {
            this.SensorToScreenPositionTransform = Matrix3D.Identity;
            this.NoUserTimeout = TimeSpan.FromSeconds(10.0);
            this.NoUserWarningTimeout = TimeSpan.FromSeconds(5.0);
            this.UserDistance = UserDistance.Unknown;
        }

        /// <summary>
        /// INotifyPropertyChanged event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// The sensor we are using
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
        /// The near boundary in meters from the sensor.
        /// </summary>
        public double NearBoundary { get; set; }

        /// <summary>
        /// Hysteresis for the near boundary in meters.
        /// </summary>
        public double NearBoundaryHysteresis { get; set; }

        /// <summary>
        /// The far boundary in meters from the sensor.
        /// </summary>
        public double FarBoundary { get; set; }

        /// <summary>
        /// Hysteresis for the far boundary in meters.
        /// </summary>
        public double FarBoundaryHysteresis { get; set; }

        /// <summary>
        /// Matrix that transforms from the sensor's skeleton space into a space
        /// whose origin is at the center of the display.
        /// </summary>
        public Matrix3D SensorToScreenPositionTransform { get; set; }

        /// <summary>
        /// How long we wait before timing out when nothing is near the sensor
        /// and no skeleton is tracked.  When we timeout, we go to the far
        /// distance.
        /// </summary>
        public TimeSpan NoUserTimeout { get; set; }

        /// <summary>
        /// How long we wait before giving a warning when nothing is
        /// near the sensor and no skeleton is tracked.
        /// </summary>
        public TimeSpan NoUserWarningTimeout { get; set; }

        /// <summary>
        /// Where the user is in relation to the display.
        /// </summary>
        public UserDistance UserDistance { get; private set; }

        /// <summary>
        /// True if something is near the sensor.
        /// </summary>
        public bool SomethingNearSensor { get; private set; }

        /// <summary>
        /// True when the app should show a warning that we are about to
        /// timeout to far mode.
        /// </summary>
        public bool TimeoutWarning { get; private set; }

        /// <summary>
        /// Reset the timer that will take us back to Far mode when nothing is detected
        /// by the sensor.
        /// </summary>
        public void ResetTimeout()
        {
            this.nothingNearSensorTime = DateTime.UtcNow;
        }

        private static bool IsInZone(double zoneMin, double zoneMax, float value)
        {
            return (value >= zoneMin) && (value <= zoneMax);
        }

        private void OnKinectSensorChanged(KinectSensor oldSensor, KinectSensor newSensor)
        {
            if (oldSensor != null)
            {
                oldSensor.AllFramesReady -= this.OnAllFramesReady;
            }

            if (newSensor != null)
            {
                newSensor.AllFramesReady += OnAllFramesReady;
            }
        }

        private void OnAllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            var now = DateTime.UtcNow;
            var newTrackedSkeletonUserDistance = UserDistance.Unknown;

            // Find the first tracked skeleton.  Code assumes there will be
            // a maximum of one tracked skeleton.
            var trackedSkeleton = this.TryGetTrackedSkeleton(allFramesReadyEventArgs);
            if (trackedSkeleton != null)
            {
                // figure out which zone the tracked skeleton should be in
                var displayRelativePosition = Transforms.TransformSkeletonPoint(this.SensorToScreenPositionTransform, trackedSkeleton.Position);
                newTrackedSkeletonUserDistance = this.DetermineZone(this.UserDistance, displayRelativePosition.Z);
            }

            var newSomethingNearSensor = DetermineSomethingNearSensor(allFramesReadyEventArgs);
            if (newSomethingNearSensor || newTrackedSkeletonUserDistance == UserDistance.Medium || newTrackedSkeletonUserDistance == UserDistance.Touch)
            {
                // Remember when we last saw something near the sensor
                // or a tracked skeleton in medium or touch zones.
                this.ResetTimeout();
            }

            var timeSinceNothingNearSensor = now - this.nothingNearSensorTime;
            bool somethingNearSensorTimedOut = timeSinceNothingNearSensor > this.NoUserTimeout;

            var newUserDistance = this.UserDistance;
            switch (this.UserDistance)
            {
                case UserDistance.Unknown:
                    newUserDistance = UserDistance.Far;
                    break;

                case UserDistance.Far:
                    if (newTrackedSkeletonUserDistance == UserDistance.Medium || newTrackedSkeletonUserDistance == UserDistance.Touch)
                    {
                        newUserDistance = UserDistance.Medium;
                    }

                    break;

                case UserDistance.Medium:
                    if (newTrackedSkeletonUserDistance == UserDistance.Touch)
                    {
                        newUserDistance = UserDistance.Touch;
                    }
                    else if (newTrackedSkeletonUserDistance == UserDistance.Far || newTrackedSkeletonUserDistance == UserDistance.Unknown)
                    {
                        newUserDistance = UserDistance.Far;
                    }

                    break;

                case UserDistance.Touch:
                    if (newTrackedSkeletonUserDistance == UserDistance.Medium && !newSomethingNearSensor)
                    {
                        newUserDistance = UserDistance.Medium;
                    }
                    else if ((newTrackedSkeletonUserDistance == UserDistance.Unknown || newTrackedSkeletonUserDistance == UserDistance.Far) && somethingNearSensorTimedOut)
                    {
                        newUserDistance = UserDistance.Far;
                    }

                    break;

                default:
                    throw new InvalidDataException("Previous User Distance invalid");
            }

            bool newTimeoutWarning = !somethingNearSensorTimedOut && (newUserDistance == UserDistance.Touch) && (timeSinceNothingNearSensor > this.NoUserWarningTimeout);

            var userDistanceChanged = this.UserDistance != newUserDistance;
            var somethingNearSensorChanged = this.SomethingNearSensor != newSomethingNearSensor;
            var timeoutWarningChanged = this.TimeoutWarning != newTimeoutWarning;

            // Change all the members at once so that we are not showing
            // an inconsistent state
            this.UserDistance = newUserDistance;
            this.SomethingNearSensor = newSomethingNearSensor;
            this.TimeoutWarning = newTimeoutWarning;

            if (userDistanceChanged)
            {
                this.OnPropertyChanged("UserDistance");
            }

            if (somethingNearSensorChanged)
            {
                this.OnPropertyChanged("SomethingNearSensor");
            }

            if (timeoutWarningChanged)
            {
                this.OnPropertyChanged("TimeoutWarning");
            }
        }

        private Skeleton TryGetTrackedSkeleton(AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            Skeleton trackedSkeleton = null;

            using (var skeletonFrame = allFramesReadyEventArgs.OpenSkeletonFrame())
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
                    // Clear out the skeletons so we don't use old data.
                    this.skeletons = null;
                }
            }

            if (this.skeletons != null)
            {
                foreach (var skeleton in this.skeletons)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        trackedSkeleton = skeleton;
                        break;
                    }
                }
            }

            return trackedSkeleton;
        }

        private UserDistance DetermineZone(UserDistance oldUserDistance, float trackedSkeletonDistance)
        {
            UserDistance newUserDistance;

            if (IsInZone(0.0f, this.NearBoundary - this.NearBoundaryHysteresis, trackedSkeletonDistance))
            {
                newUserDistance = UserDistance.Touch;
            }
            else if (IsInZone(this.NearBoundary + this.NearBoundaryHysteresis, this.FarBoundary - this.FarBoundaryHysteresis, trackedSkeletonDistance))
            {
                newUserDistance = UserDistance.Medium;
            }
            else if (IsInZone(this.FarBoundary + this.FarBoundaryHysteresis, float.MaxValue, trackedSkeletonDistance))
            {
                newUserDistance = UserDistance.Far;
            }
            else if (oldUserDistance != UserDistance.Unknown)
            {
                // the skeleton is in one of the border areas.  Since there
                // was a previous state, leave things in the prevoius state
                newUserDistance = oldUserDistance;
            }
            else if (IsInZone(this.NearBoundary - this.NearBoundaryHysteresis, this.NearBoundary + this.NearBoundaryHysteresis, trackedSkeletonDistance))
            {
                // User appeared near the touch and medium border so say they are in the medium area
                newUserDistance = UserDistance.Medium;
            }
            else
            {
                // User appeared in the far zone
                newUserDistance = UserDistance.Far;
            }

            return newUserDistance;
        }

        /// <summary>
        /// Does a very simple scan of the depth buffer to see if something is
        /// near the sensor.
        /// </summary>
        /// <param name="allFramesReadyEventArgs">args from AllFramesReady event</param>
        /// <returns>true if something is near the sensor</returns>
        private bool DetermineSomethingNearSensor(AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            int nearPixelCount = 0;
            using (var depthFrame = allFramesReadyEventArgs.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    int nearPixelThreshold = Math.Min(
                        DefaultNearPixelThreshold, (int)((this.NearBoundary - this.NearBoundaryHysteresis) * 1000.0));

                    if (depthFrame.Format != DepthImageFormat.Resolution640x480Fps30)
                    {
                        throw new InvalidOperationException("Adaptive Zone Logic must have depth in Resolution640x480Fps30 format.");
                    }

                    DepthImagePixel[] pixels = KinectRuntimeExtensions.GetRawPixelData(depthFrame);

                    for (int y = 0; y < 480; ++y)
                    {
                        var pixel = pixels[(y * 480) + 319];
                        if (pixel.IsKnownDepth && pixel.Depth < nearPixelThreshold)
                        {
                            ++nearPixelCount;
                        }
                    }
                }
            }

            return nearPixelCount > MinimumNearPixelsCount;
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
