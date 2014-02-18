﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.KinectFusionColorBasics
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit.Fusion;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        /// <summary>
        /// Max tracking error count, we will reset the reconstruction if tracking errors
        /// reach this number
        /// </summary>
        private const int MaxTrackingErrors = 100;

        /// <summary>
        /// If set true, will automatically reset the reconstruction when MaxTrackingErrors have occurred
        /// </summary>
        private const bool AutoResetReconstructionWhenLost = false;

        /// <summary>
        /// The resolution of the depth image to be processed.
        /// </summary>
        private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;

        /// <summary>
        /// Format of color frame to use - in this basic sample we are limited to use the standard 640x480 
        /// resolution Rgb at 30fps, identical to the depth resolution.
        /// </summary>
        private const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

        /// <summary>
        /// The seconds interval to calculate FPS
        /// </summary>
        private const int FpsInterval = 5;

        /// <summary>
        /// The reconstruction volume voxel density in voxels per meter (vpm)
        /// 1000mm / 256vpm = ~3.9mm/voxel
        /// </summary>
        private const int VoxelsPerMeter = 256;

        /// <summary>
        /// The reconstruction volume voxel resolution in the X axis
        /// At a setting of 256vpm the volume is 512 / 256 = 2m wide
        /// </summary>
        private const int VoxelResolutionX = 512;

        /// <summary>
        /// The reconstruction volume voxel resolution in the Y axis
        /// At a setting of 256vpm the volume is 384 / 256 = 1.5m high
        /// </summary>
        private const int VoxelResolutionY = 384;

        /// <summary>
        /// The reconstruction volume voxel resolution in the Z axis
        /// At a setting of 256vpm the volume is 512 / 256 = 2m deep
        /// </summary>
        private const int VoxelResolutionZ = 512;

        /// <summary>
        /// The reconstruction volume processor type. This parameter sets whether AMP or CPU processing
        /// is used. Note that CPU processing will likely be too slow for real-time processing.
        /// </summary>
        private const ReconstructionProcessor ProcessorType = ReconstructionProcessor.Amp;

        /// <summary>
        /// The zero-based device index to choose for reconstruction processing if the 
        /// ReconstructionProcessor AMP options are selected.
        /// Here we automatically choose a device to use for processing by passing -1, 
        /// </summary>
        private const int DeviceToUse = -1;

        /// <summary>
        /// The frame interval where we integrate color.
        /// Capturing color has an associated processing cost, so we do not capture every frame here.
        /// </summary>
        private const int ColorIntegrationInterval = 2;

        /// <summary>
        /// Parameter to translate the reconstruction based on the minimum depth setting. When set to
        /// false, the reconstruction volume +Z axis starts at the camera lens and extends into the scene.
        /// Setting this true in the constructor will move the volume forward along +Z away from the
        /// camera by the minimum depth threshold to enable capture of very small reconstruction volumes
        /// by setting a non-identity world-volume transformation in the ResetReconstruction call.
        /// Small volumes should be shifted, as the Kinect hardware has a minimum sensing limit of ~0.35m,
        /// inside which no valid depth is returned, hence it is difficult to initialize and track robustly  
        /// when the majority of a small volume is inside this distance.
        /// </summary>
        private bool translateResetPoseByMinDepthThreshold = true;

        /// <summary>
        /// Minimum depth distance threshold in meters. Depth pixels below this value will be
        /// returned as invalid (0). Min depth must be positive or 0.
        /// </summary>
        private float minDepthClip = FusionDepthProcessor.DefaultMinimumDepth;

        /// <summary>
        /// Maximum depth distance threshold in meters. Depth pixels above this value will be
        /// returned as invalid (0). Max depth must be greater than 0.
        /// </summary>
        private float maxDepthClip = FusionDepthProcessor.DefaultMaximumDepth;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the extended depth data received from the camera in the current frame
        /// </summary>
        private DepthImagePixel[] depthImagePixels;

        /// <summary>
        /// Intermediate storage for the color data received from the camera in 32bit color
        /// </summary>
        private byte[] colorImagePixels;

        /// <summary>
        /// Intermediate storage for the depth data converted to color
        /// </summary>
        private int[] colorPixels;

        /// <summary>
        /// Intermediate storage for the depth float data converted from depth image frame
        /// </summary>
        private FusionFloatImageFrame depthFloatBuffer;

        /// <summary>
        /// Intermediate storage for the point cloud data converted from depth float image frame
        /// </summary>
        private FusionPointCloudImageFrame pointCloudBuffer;

        /// <summary>
        /// Raycast shaded surface image
        /// </summary>
        private FusionColorImageFrame shadedSurfaceColorFrame;

        /// <summary>
        /// The transformation between the world and camera view coordinate system
        /// </summary>
        private Matrix4 worldToCameraTransform;

        /// <summary>
        /// The default transformation between the world and volume coordinate system
        /// </summary>
        private Matrix4 defaultWorldToVolumeTransform;

        /// <summary>
        /// The Kinect Fusion volume, enabling color reconstruction
        /// </summary>
        private ColorReconstruction volume;

        /// <summary>
        /// The timer to calculate FPS
        /// </summary>
        private DispatcherTimer fpsTimer;

        /// <summary>
        /// Timer stamp of last computation of FPS
        /// </summary>
        private DateTime lastFPSTimestamp;

        /// <summary>
        /// The count of the frames processed in the FPS interval
        /// </summary>
        private int processedFrameCount;

        /// <summary>
        /// The tracking error count
        /// </summary>
        private int trackingErrorCount;

        /// <summary>
        /// The sensor depth frame data length
        /// </summary>
        private int frameDataLength;

        /// <summary>
        /// The count of the depth frames to be processed
        /// </summary>
        private bool processingFrame;

        /// <summary>
        /// Track whether Dispose has been called
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Capture, integrate and display color when true
        /// </summary>
        private bool captureColor = true;

        /// <summary>
        /// Kinect color mapped into depth frame
        /// </summary>
        private FusionColorImageFrame mappedColorFrame;

        /// <summary>
        /// Mapping of depth pixels into color image
        /// </summary>
        private ColorImagePoint[] colorCoordinates;

        /// <summary>
        /// Mapped color pixels in depth frame of reference
        /// </summary>
        private int[] mappedColorPixels;

        /// <summary>
        /// The coordinate mapper to convert between depth and color frames of reference
        /// </summary>
        private CoordinateMapper mapper;

        /// <summary>
        /// Image Width of depth frame
        /// </summary>
        private int depthWidth = 0;

        /// <summary>
        /// Image height of depth frame
        /// </summary>
        private int depthHeight = 0;

        /// <summary>
        /// Image width of color frame
        /// </summary>
        private int colorWidth = 0;

        /// <summary>
        /// Image height of color frame
        /// </summary>
        private int colorHeight = 0;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Finalizes an instance of the MainWindow class.
        /// This destructor will run only if the Dispose method does not get called.
        /// </summary>
        ~MainWindow()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Dispose the allocated frame buffers and reconstruction.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees all memory associated with the FusionImageFrame.
        /// </summary>
        /// <param name="disposing">Whether the function was called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (null != this.depthFloatBuffer)
                {
                    this.depthFloatBuffer.Dispose();
                }

                if (null != this.mappedColorFrame)
                {
                    this.mappedColorFrame.Dispose();
                }

                if (null != this.pointCloudBuffer)
                {
                    this.pointCloudBuffer.Dispose();
                }

                if (null != this.shadedSurfaceColorFrame)
                {
                    this.shadedSurfaceColorFrame.Dispose();
                }

                if (null != this.volume)
                {
                    this.volume.Dispose();
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Get the depth image size from the input depth image format.
        /// </summary>
        /// <param name="imageFormat">The depth image format.</param>
        /// <returns>The widht and height of the input depth image format.</returns>
        private static Size GetImageSize(DepthImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case DepthImageFormat.Resolution320x240Fps30:
                    return new Size(320, 240);

                case DepthImageFormat.Resolution640x480Fps30:
                    return new Size(640, 480);

                case DepthImageFormat.Resolution80x60Fps30:
                    return new Size(80, 60);
            }

            throw new ArgumentOutOfRangeException("imageFormat");
        }

        /// <summary>
        /// Get the color image size from the input color image format.
        /// </summary>
        /// <param name="imageFormat">The color image format.</param>
        /// <returns>The width and height of the input color image format.</returns>
        private static Size GetImageSize(ColorImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case ColorImageFormat.RgbResolution640x480Fps30:
                    return new Size(640, 480);

                case ColorImageFormat.RgbResolution1280x960Fps12:
                    return new Size(1280, 960);

                case ColorImageFormat.InfraredResolution640x480Fps30:
                    return new Size(640, 480);

                case ColorImageFormat.RawBayerResolution1280x960Fps12:
                    return new Size(1280, 960);

                case ColorImageFormat.RawBayerResolution640x480Fps30:
                    return new Size(640, 480);

                case ColorImageFormat.RawYuvResolution640x480Fps15:
                    return new Size(640, 480);

                case ColorImageFormat.YuvResolution640x480Fps15:
                    return new Size(640, 480);
            }

            throw new ArgumentOutOfRangeException("imageFormat");
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Check to ensure suitable DirectX11 compatible hardware exists before initializing Kinect Fusion
            try
            {
                string deviceDescription = string.Empty;
                string deviceInstancePath = string.Empty;
                int deviceMemory = 0;

                FusionDepthProcessor.GetDeviceInfo(ProcessorType, DeviceToUse, out deviceDescription, out deviceInstancePath, out deviceMemory);
            }
            catch (IndexOutOfRangeException)
            {
                // Thrown when index is out of range for processor type or there is no DirectX11 capable device installed.
                // As we set -1 (auto-select default) for the DeviceToUse above, this indicates that there is no DirectX11 
                // capable device. The options for users in this case are to either install a DirectX11 capable device 
                // (see documentation for recommended GPUs) or to switch to non-real-time CPU based reconstruction by 
                // changing ProcessorType to ReconstructionProcessor.Cpu
                this.statusBarText.Text = Properties.Resources.NoDirectX11CompatibleDeviceOrInvalidDeviceIndex;
                return;
            }
            catch (DllNotFoundException)
            {
                this.statusBarText.Text = Properties.Resources.MissingPrerequisite;
                return;
            }
            catch (InvalidOperationException ex)
            {
                this.statusBarText.Text = ex.Message;
                return;
            }

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
                return;
            }

            Size depthImageSize = GetImageSize(DepthFormat);
            this.depthWidth = (int)depthImageSize.Width;
            this.depthHeight = (int)depthImageSize.Height;

            Size colorImageSize = GetImageSize(ColorFormat);
            this.colorWidth = (int)colorImageSize.Width;
            this.colorHeight = (int)colorImageSize.Height;

            // Turn on the depth and color streams to receive frames
            this.sensor.DepthStream.Enable(DepthFormat);
            this.sensor.ColorStream.Enable(ColorFormat);

            this.frameDataLength = this.sensor.DepthStream.FramePixelDataLength;

            // Allocate space to put the color pixels we'll create
            this.colorPixels = new int[this.frameDataLength];

            // This is the bitmap we'll display on-screen
            this.colorBitmap = new WriteableBitmap(
                this.depthWidth,
                this.depthHeight,
                96.0,
                96.0,
                PixelFormats.Bgr32,
                null);

            // Set the image we display to point to the bitmap where we'll put the image data
            this.Image.Source = this.colorBitmap;

            // Add an event handler to be called whenever depth and color both have new data
            this.sensor.AllFramesReady += this.SensorFramesReady;

            var volParam = new ReconstructionParameters(VoxelsPerMeter, VoxelResolutionX, VoxelResolutionY, VoxelResolutionZ);

            // Set the world-view transform to identity, so the world origin is the initial camera location.
            this.worldToCameraTransform = Matrix4.Identity;

            try
            {
                // This creates a volume cube with the Kinect at center of near plane, and volume directly
                // in front of Kinect.
                this.volume = ColorReconstruction.FusionCreateReconstruction(volParam, ProcessorType, DeviceToUse, this.worldToCameraTransform);

                this.defaultWorldToVolumeTransform = this.volume.GetCurrentWorldToVolumeTransform();

                if (this.translateResetPoseByMinDepthThreshold)
                {
                    // Reset the reconstruction if we need to add a custom world-volume transformation
                    this.ResetReconstruction();
                }
            }
            catch (InvalidOperationException ex)
            {
                this.statusBarText.Text = ex.Message;
                return;
            }
            catch (DllNotFoundException)
            {
                this.statusBarText.Text = this.statusBarText.Text = Properties.Resources.MissingPrerequisite;
                return;
            }

            // Depth frames generated from the depth input
            this.depthFloatBuffer = new FusionFloatImageFrame(this.depthWidth, this.depthHeight);

            // Allocate color frame for color data from Kinect mapped into depth frame
            this.mappedColorFrame = new FusionColorImageFrame(this.depthWidth, this.depthHeight);

            // Point cloud frames generated from the depth float input
            this.pointCloudBuffer = new FusionPointCloudImageFrame(this.depthWidth, this.depthHeight);

            // Create images to raycast the Reconstruction Volume
            this.shadedSurfaceColorFrame = new FusionColorImageFrame(this.depthWidth, this.depthHeight);

            int depthImageArraySize = this.depthWidth * this.depthHeight;
            int colorImageArraySize = this.colorWidth * this.colorHeight * sizeof(int);

            // Create local depth pixels buffer
            this.depthImagePixels = new DepthImagePixel[depthImageArraySize];

            // Create local color pixels buffer
            this.colorImagePixels = new byte[colorImageArraySize];

            // Allocate the depth-color mapping points
            this.colorCoordinates = new ColorImagePoint[depthImageArraySize];

            // Allocate mapped color points (i.e. color in depth frame of reference)
            this.mappedColorPixels = new int[depthImageArraySize];

            // Start the sensor!
            try
            {
                this.sensor.Start();
            }
            catch (IOException ex)
            {
                // Device is in use
                this.sensor = null;
                this.statusBarText.Text = ex.Message;

                return;
            }
            catch (InvalidOperationException ex)
            {
                // Device is not valid, not supported or hardware feature unavailable
                this.sensor = null;
                this.statusBarText.Text = ex.Message;

                return;
            }

            // Set Near Mode by default
            try
            {
                this.sensor.DepthStream.Range = DepthRange.Near;
                this.checkBoxNearMode.IsChecked = true;
            }
            catch (InvalidOperationException)
            {
                // Near mode not supported on device, silently fail during initialization
                this.checkBoxNearMode.IsEnabled = false;
            }

            this.checkBoxCaptureColor.IsChecked = this.captureColor;

            // Initialize and start the FPS timer
            this.fpsTimer = new DispatcherTimer();
            this.fpsTimer.Tick += new EventHandler(this.FpsTimerTick);
            this.fpsTimer.Interval = new TimeSpan(0, 0, FpsInterval);

            this.fpsTimer.Start();

            this.lastFPSTimestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.fpsTimer)
            {
                this.fpsTimer.Stop();
            }

            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Handler for FPS timer tick
        /// </summary>
        /// <param name="sender">Object sending the event</param>
        /// <param name="e">Event arguments</param>
        private void FpsTimerTick(object sender, EventArgs e)
        {
            // Calculate time span from last calculation of FPS
            double intervalSeconds = (DateTime.UtcNow - this.lastFPSTimestamp).TotalSeconds;

            // Calculate and show fps on status bar
            this.statusBarText.Text = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                Properties.Resources.Fps,
                (double)this.processedFrameCount / intervalSeconds);

            // Reset frame counter
            this.processedFrameCount = 0;
            this.lastFPSTimestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Reset FPS timer and counter
        /// </summary>
        private void ResetFps()
        {
            // Restart fps timer
            if (null != this.fpsTimer)
            {
                this.fpsTimer.Stop();
                this.fpsTimer.Start();
            }

            // Reset frame counter
            this.processedFrameCount = 0;
            this.lastFPSTimestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Event handler for Kinect sensor's AllFramesReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // Here we will drop a frame if we are still processing the last one
            if (!this.processingFrame)
            {
                using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                {
                    if (null != colorFrame)
                    {
                        // Copy color pixels from the image to a buffer
                        colorFrame.CopyPixelDataTo(this.colorImagePixels);
                    }
                }

                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    if (depthFrame != null)
                    {
                        // Copy the depth pixel data from the image to a buffer
                        depthFrame.CopyDepthImagePixelDataTo(this.depthImagePixels);

                        // Mark that one frame will be processed
                        this.processingFrame = true;

                        this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() => this.ProcessDepthData()));
                    }
                }
            }
        }

        /// <summary>
        /// Process the depth input
        /// </summary>
        private void ProcessDepthData()
        {
            Debug.Assert(null != this.volume, "volume should be initialized");
            Debug.Assert(null != this.shadedSurfaceColorFrame, "shaded surface should be initialized");
            Debug.Assert(null != this.colorBitmap, "color bitmap should be initialized");

            try
            {
                // Convert the depth image frame to depth float image frame
                this.volume.DepthToDepthFloatFrame(
                    this.depthImagePixels,
                    this.depthFloatBuffer,
                    FusionDepthProcessor.DefaultMinimumDepth,
                    FusionDepthProcessor.DefaultMaximumDepth,
                    false);

                // ProcessFrame will first calculate the camera pose and then integrate
                // if tracking is successful
                bool trackingSucceeded = false;
                bool integrateColor = this.processedFrameCount % ColorIntegrationInterval == 0;

                if (this.captureColor && integrateColor)
                {
                    this.MapColorToDepth();

                    trackingSucceeded = this.volume.ProcessFrame(
                        this.depthFloatBuffer,
                        this.mappedColorFrame,
                        FusionDepthProcessor.DefaultAlignIterationCount,
                        FusionDepthProcessor.DefaultIntegrationWeight,
                        FusionDepthProcessor.DefaultColorIntegrationOfAllAngles,
                        this.volume.GetCurrentWorldToCameraTransform());
                }
                else
                {
                    trackingSucceeded = this.volume.ProcessFrame(
                        this.depthFloatBuffer,
                        FusionDepthProcessor.DefaultAlignIterationCount,
                        FusionDepthProcessor.DefaultIntegrationWeight,
                        this.volume.GetCurrentWorldToCameraTransform());
                }

                // If camera tracking failed, no data integration or raycast for reference
                // point cloud will have taken place, and the internal camera pose
                // will be unchanged.
                if (!trackingSucceeded)
                {
                    this.trackingErrorCount++;

                    // Show tracking error on status bar
                    this.statusBarText.Text = Properties.Resources.CameraTrackingFailed;
                }
                else
                {
                    Matrix4 calculatedCameraPose = this.volume.GetCurrentWorldToCameraTransform();
                     
                    // Set the camera pose and reset tracking errors
                    this.worldToCameraTransform = calculatedCameraPose;
                    this.trackingErrorCount = 0;
                }

                if (AutoResetReconstructionWhenLost && !trackingSucceeded && this.trackingErrorCount == MaxTrackingErrors)
                {
                    // Auto Reset due to bad tracking
                    this.statusBarText.Text = Properties.Resources.ResetVolume;

                    // Automatically Clear Volume and reset tracking if tracking fails
                    this.ResetReconstruction();
                }

                if (this.captureColor)
                {
                    // Calculate the point cloud and get color surface image
                    this.volume.CalculatePointCloud(this.pointCloudBuffer, this.shadedSurfaceColorFrame, this.worldToCameraTransform);
                }
                else
                {
                    // Calculate the point cloud
                    this.volume.CalculatePointCloud(this.pointCloudBuffer, this.worldToCameraTransform);

                    // Shade point cloud and render
                    FusionDepthProcessor.ShadePointCloud(
                        this.pointCloudBuffer, this.worldToCameraTransform, this.shadedSurfaceColorFrame, null);
                }

                this.shadedSurfaceColorFrame.CopyPixelDataTo(this.colorPixels);

                // Write the pixel data into our bitmap
                this.colorBitmap.WritePixels(
                    new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                    this.colorPixels,
                    this.colorBitmap.PixelWidth * sizeof(int),
                    0);

                // The input frame was processed successfully, increase the processed frame count
                ++this.processedFrameCount;
            }
            catch (InvalidOperationException ex)
            {
                this.statusBarText.Text = ex.Message;
            }
            finally
            {
                this.processingFrame = false;
            }
        }

        /// <summary>
        /// Process the color and depth inputs, converting the color into the depth space
        /// </summary>
        private unsafe void MapColorToDepth()
        {
            if (null == this.mapper)
            {
                // Create a coordinate mapper
                this.mapper = new CoordinateMapper(this.sensor);
            }

            this.mapper.MapDepthFrameToColorFrame(DepthFormat, this.depthImagePixels, ColorFormat, this.colorCoordinates);

            // Here we make use of unsafe code to just copy the whole pixel as an int for performance reasons, as we do
            // not need access to the individual rgba components.
            fixed (byte* ptrColorPixels = this.colorImagePixels)
            {
                int* rawColorPixels = (int*)ptrColorPixels;

                // Horizontal flip the color image as the standard depth image is flipped internally in Kinect Fusion
                // to give a viewpoint as though from behind the Kinect looking forward by default.
                Parallel.For(
                    0,
                    this.depthHeight,
                    y =>
                        {
                            int destIndex = y * this.depthWidth;
                            int flippedDestIndex = destIndex + (this.depthWidth - 1); // horizontally mirrored

                            for (int x = 0; x < this.depthWidth; ++x, ++destIndex, --flippedDestIndex)
                            {
                                // calculate index into depth array
                                int colorInDepthX = colorCoordinates[destIndex].X;
                                int colorInDepthY = colorCoordinates[destIndex].Y;

                                // make sure the depth pixel maps to a valid point in color space
                                if (colorInDepthX >= 0 && colorInDepthX < this.colorWidth && colorInDepthY >= 0
                                    && colorInDepthY < this.colorHeight && depthImagePixels[destIndex].Depth != 0)
                                {
                                    // Calculate index into color array- this will perform a horizontal flip as well
                                    int sourceColorIndex = colorInDepthX + (colorInDepthY * this.colorWidth);

                                    // Copy color pixel
                                    this.mappedColorPixels[flippedDestIndex] = rawColorPixels[sourceColorIndex];
                                }
                                else
                                {
                                    this.mappedColorPixels[flippedDestIndex] = 0;
                                }
                            }
                        });
            }

            this.mappedColorFrame.CopyPixelDataFrom(this.mappedColorPixels);
        }

        /// <summary>
        /// Reset the reconstruction to initial value
        /// </summary>
        private void ResetReconstruction()
        {
            // Reset tracking error counter
            this.trackingErrorCount = 0;

            // Set the world-view transform to identity, so the world origin is the initial camera location.
            this.worldToCameraTransform = Matrix4.Identity;

            if (null != this.volume)
            {
                // Translate the reconstruction volume location away from the world origin by an amount equal
                // to the minimum depth threshold. This ensures that some depth signal falls inside the volume.
                // If set false, the default world origin is set to the center of the front face of the 
                // volume, which has the effect of locating the volume directly in front of the initial camera
                // position with the +Z axis into the volume along the initial camera direction of view.
                if (this.translateResetPoseByMinDepthThreshold)
                {
                    Matrix4 worldToVolumeTransform = this.defaultWorldToVolumeTransform;

                    // Translate the volume in the Z axis by the minDepthThreshold distance
                    float minDist = (this.minDepthClip < this.maxDepthClip) ? this.minDepthClip : this.maxDepthClip;
                    worldToVolumeTransform.M43 -= minDist * VoxelsPerMeter;

                    this.volume.ResetReconstruction(this.worldToCameraTransform, worldToVolumeTransform); 
                }
                else
                {
                    this.volume.ResetReconstruction(this.worldToCameraTransform);
                }

                this.ResetColorImage();
            }

            this.ResetFps();
        }

        /// <summary>
        /// Reset the mapped color image on reset or re-create of volume
        /// </summary>
        private void ResetColorImage()
        {
            if (null != this.mappedColorFrame && null != this.mappedColorPixels)
            {
                // Clear the mapped color image
                Array.Clear(this.mappedColorPixels, 0, this.mappedColorPixels.Length);
                this.mappedColorFrame.CopyPixelDataFrom(this.mappedColorPixels);
            }
        }

        /// <summary>
        /// Handles the user clicking on the reset reconstruction button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ButtonResetReconstructionClick(object sender, RoutedEventArgs e)
        {
            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
                return;
            }

            // reset the reconstruction and update the status text
            this.ResetReconstruction();
            this.statusBarText.Text = Properties.Resources.ResetReconstruction;
        }

        /// <summary>
        /// Handles the checking or un-checking of the near mode check box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxNearModeChanged(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null)
            {
                // will not function on non-Kinect for Windows devices
                try
                {
                    if (this.checkBoxNearMode.IsChecked.GetValueOrDefault())
                    {
                        this.sensor.DepthStream.Range = DepthRange.Near;
                    }
                    else
                    {
                        this.sensor.DepthStream.Range = DepthRange.Default;
                    }
                }
                catch (InvalidOperationException)
                {
                    // Near mode not supported on this device
                    this.statusBarText.Text = Properties.Resources.NearModeNotSupported;
                    this.checkBoxNearMode.IsChecked = false;
                }
            }
        }

        /// <summary>
        /// Handles the checking or un-checking of the capture color check box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxCaptureColor(object sender, RoutedEventArgs e)
        {
            if (this.checkBoxCaptureColor.IsChecked.GetValueOrDefault())
            {
                this.captureColor = true;
            }
            else
            {
                this.captureColor = false;
            }
        }
    }
}
