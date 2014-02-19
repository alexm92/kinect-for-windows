using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PongGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal readonly KinectSensorChooser sensorChooser;
        Skeleton[] skeletons;

        private byte[] colorPixels;
        private WriteableBitmap colorBitmap;
        private double ballX, ballY, incX, incY;

        public MainWindow()
        {
            InitializeComponent();

            sensorChooser = new KinectSensorChooser();
            SensorChooserUi.KinectSensorChooser = sensorChooser;
            SensorChooserUi.KinectSensorChooser.KinectChanged += SensorChooserKinectChanged;
            sensorChooser.Start();
        }

        private void SensorChooserKinectChanged(object sender, KinectChangedEventArgs e)
        {
            KinectSensor oldSensor = e.OldSensor;
            KinectSensor newSensor = e.NewSensor;

            if (oldSensor != null)
            {
                try
                {
                    oldSensor.ColorStream.Disable();
                    oldSensor.DepthStream.Disable();
                    oldSensor.AllFramesReady -= AllFramesReady;
                    oldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (newSensor != null)
            {
                try
                {
                    // Smoothed with some latency.
                    // Filters out medium jitters.
                    // Good for a menu system that needs to be smooth but
                    // doesn't need the reduced latency as much as gesture recognition does.
                    //TransformSmoothParameters smoothingParam = new TransformSmoothParameters();
                    //{
                    //    smoothingParam.Smoothing = 0.5f;
                    //    smoothingParam.Correction = 0.1f;
                    //    smoothingParam.Prediction = 0.5f;
                    //    smoothingParam.JitterRadius = 0.1f;
                    //    smoothingParam.MaxDeviationRadius = 0.1f;
                    //};

                    TransformSmoothParameters smoothingParam = new TransformSmoothParameters();
                    {
                        smoothingParam.Smoothing = 0.7f;
                        smoothingParam.Correction = 0.3f;
                        smoothingParam.Prediction = 1.0f;
                        smoothingParam.JitterRadius = 1.0f;
                        smoothingParam.MaxDeviationRadius = 1.0f;
                    };

                    newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    newSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    newSensor.SkeletonStream.Enable(smoothingParam);
                    newSensor.AllFramesReady += AllFramesReady;
                    ballX = ballY = 0;
                    incX = incY = 1;

                    this.colorPixels = new byte[sensorChooser.Kinect.ColorStream.FramePixelDataLength];
                    this.colorBitmap = new WriteableBitmap(sensorChooser.Kinect.ColorStream.FrameWidth, sensorChooser.Kinect.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                    this.ColorImage.Source = this.colorBitmap;

                    if (skeletons == null)
                    {
                        skeletons = new Skeleton[newSensor.SkeletonStream.FrameSkeletonArrayLength];
                    }

                    try
                    {
                        newSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        newSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromMilliseconds(10);
                    timer.Tick += timer_Tick;
                    timer.Start();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Canvas.SetLeft(this.Ball, ballX + incX);
            Canvas.SetTop(this.Ball, ballY + incY);
        }

        private void AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (var colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null) {
                    colorFrame.CopyPixelDataTo(colorPixels);

                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
            using (var skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    this.info.Text = "Skeleton Ready";
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    var first = (from s in skeletons where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                    Update(first);
                }
            }
        }

        private void Update(Skeleton skeleton)
        {
            if (skeleton != null)
            {
                this.info.Text = "Update";

                var handRight = skeleton.Joints[JointType.HandRight];

                if (handRight.TrackingState == JointTrackingState.Tracked)
                {
                    Move(handRight);
                }
                else
                {
                    Debug.Print("No hand is traked");
                }
            }
        }

        private void Move(Joint hand)
        {
            this.info.Text = "Move";
            CoordinateMapper map = new CoordinateMapper(sensorChooser.Kinect);
            ColorImagePoint p = map.MapSkeletonPointToColorPoint(hand.Position, ColorImageFormat.RgbResolution640x480Fps30);

            double y = 1.0 * this.GameWindow.ActualHeight * p.Y / 480;
            Debug.WriteLine(this.GameWindow.ActualHeight + " ~ " + p.Y + " => " + y);

            Canvas.SetTop(this.Player, y);

            Canvas.SetLeft(this.MyCursor, p.X);
            Canvas.SetTop(this.MyCursor, p.Y);
        }
    }
}
