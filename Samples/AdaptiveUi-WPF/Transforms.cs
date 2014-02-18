// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Transforms.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.AdaptiveUI
{
    using System;
    using System.Windows;
    using System.Windows.Media.Media3D;

    using Microsoft.Kinect;

    public static class Transforms
    {
        /// <summary>
        /// Create a look at matrix using U, V, and N vectors
        /// </summary>
        /// <param name="eye">Location of the eye</param>
        /// <param name="lookDirection">Direction to look</param>
        /// <param name="up">The up vector</param>
        /// <returns>Transform matrix from world space to camera space.</returns>
        public static Matrix3D LookAt(Vector3D eye, Vector3D lookDirection, Vector3D up)
        {
            var n = lookDirection;
            n.Normalize();

            var v = up - (Vector3D.DotProduct(up, n) * n);
            v.Normalize();

            // The "-" below makes this be a right-handed uvn space so we aren't
            // negating the x component in SensorToScreenCoordinatesTransform.
            // It also makes SensorToScreenPositionTransform give an immediately
            // usable transform without having to flip the X.
            var u = -Vector3D.CrossProduct(n, v);

            var lookAt = new Matrix3D(
                u.X,
                v.X,
                n.X,
                0,
                u.Y,
                v.Y,
                n.Y,
                0,
                u.Z,
                v.Z,
                n.Z,
                0,
                -Vector3D.DotProduct(u, eye),
                -Vector3D.DotProduct(v, eye),
                -Vector3D.DotProduct(n, eye),
                1);

            return lookAt;
        }

        /// <summary>
        /// Create a transform matrix that translates from skeleton space
        /// into a space whose origin is the center of the screen and has the
        /// same units as skeleton space.
        /// </summary>
        /// <param name="sensorOffset">vector, in meters, from the center of the display to the center of the Kinect sensor</param>
        /// <param name="sensorElevationAngle">elevation angle of the sensor in degrees</param>
        /// <returns>transform matrix</returns>
        public static Matrix3D SensorToScreenPositionTransform(Vector3D sensorOffset, double sensorElevationAngle)
        {
            var rotateIntoSensorSpace = new Matrix3D();
            rotateIntoSensorSpace.Rotate(new Quaternion(new Vector3D(1.0, 0.0, 0.0), sensorElevationAngle));

            var eye = -sensorOffset;
            eye = rotateIntoSensorSpace.Transform(eye);

            var normalFromEye = new Vector3D(0.0, 0.0, 1.0);
            normalFromEye = rotateIntoSensorSpace.Transform(normalFromEye);

            var up = new Vector3D(0.0, 1.0, 0.0);

            return LookAt(eye, normalFromEye, up);
        }

        /// <summary>
        /// Creates a transform matrix that transforms points from sensor skeleton space to screen coordinates
        /// </summary>
        /// <param name="sensorOffset">vector, in meters, from the center of the display to the center of the Kinect sensor</param>
        /// <param name="sensorElevationAngle">elevation angle of the sensor in degrees</param>
        /// <param name="displayWidthMeters">Width of the display area in meters</param>
        /// <param name="displayHeightMeters">Height of the display area in meters</param>
        /// <param name="displayWidthPixels">Pixel width of the display area</param>
        /// <param name="displayHeightPixels">Pixel height of the display area</param>
        /// <returns>transform matrix</returns>
        public static Matrix3D SensorToScreenCoordinatesTransform(Vector3D sensorOffset, double sensorElevationAngle, double displayWidthMeters, double displayHeightMeters, double displayWidthPixels, double displayHeightPixels)
        {
            Matrix3D transform = SensorToScreenPositionTransform(sensorOffset, sensorElevationAngle);

            // For devices with square pixels, horizontalPixelsPerMeter and verticalPixelsPerMeter
            // should be equal.
            var horizontalPixelsPerMeter = displayWidthPixels / displayWidthMeters;
            var verticalPixelsPerMeter = displayHeightPixels / displayHeightMeters;

            transform.Scale(new Vector3D(horizontalPixelsPerMeter, -verticalPixelsPerMeter, 1.0));
            transform.Translate(new Vector3D(displayWidthPixels / 2.0, displayHeightPixels / 2.0, 0.0));

            return transform;
        }

        /// <summary>
        /// Helper function to transform a Kinect SkeletonPoint using a
        /// transform matrix.
        /// </summary>
        /// <param name="transform">The transform to use</param>
        /// <param name="skeletonPoint">The skeleton point to be transformed</param>
        /// <returns>the transformed skeleton point</returns>
        public static SkeletonPoint TransformSkeletonPoint(Matrix3D transform, SkeletonPoint skeletonPoint)
        {
            var point = new Point4D(skeletonPoint.X, skeletonPoint.Y, skeletonPoint.Z, 1.0);
            point = transform.Transform(point);
            return new SkeletonPoint { X = (float)point.X, Y = (float)point.Y, Z = (float)point.Z };
        }

        /// <summary>
        /// Helper to get a rectangle on the display that is relative to the
        /// user.  The dimensions are expressed in terms of the user's field of view.
        /// </summary>
        /// <param name="displayWidthInMeters">Width of the display area in meters</param>
        /// <param name="displayHeightInMeters">Height of the display area in meters</param>
        /// <param name="displayWidthInPixels">Width of the display area in pixels</param>
        /// <param name="displayHeightInPixels">Height of the display area in pixels</param>
        /// <param name="userPositionCoordinates">Position of the user transformed by the SensorToScreenCoordinatesTransform (X and Y are in pixels, Z is in meters from screen)</param>
        /// <param name="horizontalDegrees">Width of the rectangle in the user's field of view</param>
        /// <param name="verticalDegrees">Height of the rectangle in the user's field of view</param>
        /// <param name="verticalUiOffsetInPixels">Offset of the rectangle from where it would normally be calculated</param>
        /// <returns>The calculated rectangle</returns>
        public static Rect CalculateUiPlacementRectangle(
            double displayWidthInMeters,
            double displayHeightInMeters,
            double displayWidthInPixels,
            double displayHeightInPixels,
            Vector3D userPositionCoordinates,
            double horizontalDegrees,
            double verticalDegrees,
            double verticalUiOffsetInPixels)
        {
            var uiTopCenter = new Point(userPositionCoordinates.X, userPositionCoordinates.Y);
            uiTopCenter.Y += verticalUiOffsetInPixels;

            var horizontalPixelsPerMeter = displayWidthInPixels / displayWidthInMeters;
            var verticalPixelsPerMeter = displayHeightInPixels / displayHeightInMeters;

            var halfWidth = userPositionCoordinates.Z * Math.Tan(DegreesToRadians(horizontalDegrees / 2.0)) * horizontalPixelsPerMeter;
            var height = userPositionCoordinates.Z * Math.Tan(DegreesToRadians(verticalDegrees)) * verticalPixelsPerMeter;

            var upperLeft = new Point(userPositionCoordinates.X - halfWidth, userPositionCoordinates.Y);
            var bottomRight = new Point(userPositionCoordinates.X + halfWidth, userPositionCoordinates.Y + height);

            upperLeft.Y += verticalUiOffsetInPixels;
            bottomRight.Y += verticalUiOffsetInPixels;

            return new Rect(upperLeft, bottomRight);
        }

        /// <summary>
        /// Helper to convert degrees to radians.
        /// </summary>
        /// <param name="degrees">value to convert</param>
        /// <returns>the input value in terms of radians</returns>
        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
