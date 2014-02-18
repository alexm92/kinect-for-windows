//------------------------------------------------------------------------------
// <copyright file="SensorTransforms.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System;
    using System.ComponentModel;
    using System.Windows.Media.Media3D;
    using Microsoft.Kinect;

    /// <summary>
    /// Helper class that uses Settings and the sensor angle
    /// to produce a transform matrix.  The matrix transforms
    /// skeleton space coordinates to screen space coordinates.
    /// </summary>
    public class SensorTransforms : INotifyPropertyChanged
    {
        /// <summary>
        /// Minimum change in a double property for which we will report
        /// a change.
        /// </summary>
        private const double MinimumDoubleDifference = 0.00001;

        /// <summary>
        /// Number of degrees the sensor angle needs to change before we update the transform.
        /// </summary>
        private const double MinimumSensorAngleChange = 0.4;

        /// <summary>
        /// Alpha for how much we smooth the sensor elevation angle readings.
        /// The closer to 1, the more like the original series it is.
        /// </summary>
        private const double SensorAngleSmoothingAlpha = 0.1;

        private static readonly Vector3D DownVector = new Vector3D(0.0, -1.0, 0.0);

        private readonly Smoother smoother = new Smoother(SensorAngleSmoothingAlpha);

        private double smoothedElevationAngle;

        private bool useFixedSensorElevationAngle;

        private double fixedSensorElevationAngle;

        private Vector3D sensorOffsetFromScreenCenter;

        private double displayWidthInMeters = 1.0;

        private double displayHeightInMeters = 1.0;

        private double displayWidthInPixels = 1024;

        private double displayHeightInPixels = 768;

        private Matrix3D? sensorToScreenPositionTransform = Matrix3D.Identity;

        private Matrix3D? sensorToScreenCoordinatesTransform = Matrix3D.Identity;

        private KinectSensor kinectSensor;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool UseFixedSensorElevationAngle
        {
            get
            {
                return this.useFixedSensorElevationAngle;
            }

            set
            {
                if (this.useFixedSensorElevationAngle != value)
                {
                    this.useFixedSensorElevationAngle = value;
                    this.InvalidateTransforms();
                    this.OnPropertyChanged("UseFixedSensorElevationAngle");
                }
            }
        }

        public double FixedSensorElevationAngle
        {
            get
            {
                return this.fixedSensorElevationAngle;
            }

            set
            {
                if (!AreClose(this.fixedSensorElevationAngle, value))
                {
                    this.fixedSensorElevationAngle = value;
                    this.InvalidateTransforms();
                    this.OnPropertyChanged("FixedSensorElevationAngle");
                }
            }
        }

        public Vector3D SensorOffsetFromScreenCenter
        {
            get
            {
                return this.sensorOffsetFromScreenCenter;
            }

            set
            {
                if (!AreClose(this.sensorOffsetFromScreenCenter.X, value.X) || !AreClose(this.sensorOffsetFromScreenCenter.Y, value.Y) || !AreClose(this.sensorOffsetFromScreenCenter.Z, value.Z))
                {
                    this.sensorOffsetFromScreenCenter = value;
                    this.InvalidateTransforms();
                    this.OnPropertyChanged("SensorOffsetFromScreenCenter");
                }
            }
        }

        public double DisplayWidthInMeters
        {
            get
            {
                return this.displayWidthInMeters;
            }

            set
            {
                if (!AreClose(this.displayWidthInMeters, value))
                {
                    this.displayWidthInMeters = value;
                    this.InvalidateTransforms();
                    this.OnPropertyChanged("DisplayWidthInMeters");
                }
            }
        }

        public double DisplayHeightInMeters
        {
            get
            {
                return this.displayHeightInMeters;
            }

            set
            {
                if (!AreClose(this.displayHeightInMeters, value))
                {
                    this.displayHeightInMeters = value;
                    this.InvalidateTransforms();
                    this.OnPropertyChanged("DisplayHeightInMeters");
                }
            }
        }

        public double DisplayWidthInPixels
        {
            get
            {
                return this.displayWidthInPixels;
            }

            set
            {
                if (!AreClose(this.displayWidthInPixels, value))
                {
                    this.displayWidthInPixels = value;
                    this.InvalidateTransforms();
                    this.OnPropertyChanged("DisplayWidthInPixels");
                }
            }
        }

        public double DisplayHeightInPixels
        {
            get
            {
                return this.displayHeightInPixels;
            }

            set
            {
                if (!AreClose(this.displayHeightInPixels, value))
                {
                    this.displayHeightInPixels = value;
                    this.InvalidateTransforms();
                    this.OnPropertyChanged("DisplayHeightInPixels");
                }
            }
        }

        public Matrix3D SensorToScreenPositionTransform
        {
            get
            {
                if (!this.sensorToScreenPositionTransform.HasValue)
                {
                    this.sensorToScreenPositionTransform = Transforms.SensorToScreenPositionTransform(this.SensorOffsetFromScreenCenter, this.SensorElevationAngleToUse);
                }

                return this.sensorToScreenPositionTransform.Value;
            }
        }

        public Matrix3D SensorToScreenCoordinatesTransform
        {
            get
            {
                if (!this.sensorToScreenCoordinatesTransform.HasValue)
                {
                    this.sensorToScreenCoordinatesTransform =
                        Transforms.SensorToScreenCoordinatesTransform(
                            this.SensorOffsetFromScreenCenter,
                            this.SensorElevationAngleToUse,
                            this.DisplayWidthInMeters,
                            this.DisplayHeightInMeters,
                            this.DisplayWidthInPixels,
                            this.DisplayHeightInPixels);
                }

                return this.sensorToScreenCoordinatesTransform.Value;
            }
        }

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
                    if (this.kinectSensor != null)
                    {
                        this.kinectSensor.DepthFrameReady -= this.OnDepthFrameReady;
                    }

                    if (value != null)
                    {
                        value.DepthFrameReady += this.OnDepthFrameReady;
                    }

                    this.kinectSensor = value;
                    this.OnPropertyChanged("KinectSensor");
                }
            }
        }

        private double SensorElevationAngleToUse
        {
            get
            {
                return this.UseFixedSensorElevationAngle ? this.FixedSensorElevationAngle : this.smoothedElevationAngle;
            }
        }

        private static double GetSensorAngleInDegrees(Vector4 accelerometerReading)
        {
            // This code assumes that the sensor's X axis is always perpendicular to gravity
            var accelerometer = new Vector3D(0.0, accelerometerReading.Y, accelerometerReading.Z);
            double sensorAngleInDegrees = Vector3D.AngleBetween(DownVector, accelerometer);
            return (accelerometerReading.Z > 0) ? -sensorAngleInDegrees : sensorAngleInDegrees;
        }

        /// <summary>
        /// Helper to check if two doubles are basically equal.
        /// </summary>
        /// <param name="a">first double</param>
        /// <param name="b">second double</param>
        /// <returns>true if the numbers are close enough for our purposes</returns>
        private static bool AreClose(double a, double b)
        {
            return Math.Abs(a - b) < MinimumDoubleDifference;
        }

        private void OnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs depthImageFrameReadyEventArgs)
        {
            // try/catch because we sometimes get events after the sensor is closed.
            try
            {
                var sensor = (KinectSensor)sender;

                var elevationAngle = GetSensorAngleInDegrees(sensor.AccelerometerGetCurrentReading());
                var newSmoothedElevationAngle = this.smoother.GetSmoothedValue(elevationAngle);
                if (Math.Abs(newSmoothedElevationAngle - this.smoothedElevationAngle) > MinimumSensorAngleChange)
                {
                    this.smoothedElevationAngle = newSmoothedElevationAngle;
                    this.InvalidateTransforms();
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void InvalidateTransforms()
        {
            if (this.sensorToScreenPositionTransform.HasValue)
            {
                this.sensorToScreenPositionTransform = null;
                this.OnPropertyChanged("SensorToScreenPositionTransform");
            }
            
            if (this.sensorToScreenCoordinatesTransform.HasValue)
            {
                this.sensorToScreenCoordinatesTransform = null;
                this.OnPropertyChanged("SensorToScreenCoordinatesTransform");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Helper class to smooth a data series.  Adds latency.
        /// </summary>
        private class Smoother
        {
            private readonly double alpha;
            private bool previousSampleSet;
            private double previousSample;

            /// <summary>
            /// Initializes a new instance of the <see cref="SensorTransforms.Smoother"/> class. 
            /// </summary>
            /// <param name="alpha">
            /// Between 0.0 and 1.0.  The closer to 1, the more like the original series.
            /// </param>
            public Smoother(double alpha)
            {
                if (alpha < 0 || alpha > 1.0)
                {
                    throw new ArgumentOutOfRangeException("alpha");
                }

                this.alpha = alpha;
            }

            public double GetSmoothedValue(double s)
            {
                if (this.previousSampleSet)
                {
                    this.previousSample = (alpha * s) + ((1.0f - alpha) * this.previousSample);
                }
                else
                {
                    this.previousSampleSet = true;
                    this.previousSample = s;
                }

                return this.previousSample;
            }
        }
    }
}
