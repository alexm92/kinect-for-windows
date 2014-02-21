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

using Coding4Fun.Kinect.Wpf;

namespace PongGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal readonly KinectSensorChooser sensorChooser;
        Skeleton[] skeletons;

        public double ballLeft, ballTop, incLeft, incTop;
        public double speed;
        public int intervalMilisec;
        public DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            sensorChooser = new KinectSensorChooser();
            SensorChooserUi.KinectSensorChooser = sensorChooser;
            SensorChooserUi.KinectSensorChooser.KinectChanged += SensorChooserKinectChanged;
            sensorChooser.Start();

            intervalMilisec = 5;
            speed = 2;
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

                    incLeft = incTop = ballLeft = ballTop = -Int32.MaxValue;

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

                    timer = new DispatcherTimer();
                    timer.Interval = new TimeSpan(0, 0, 0, 0, intervalMilisec);
                    timer.Tick += timer_Tick;
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
            ballLeft = ballLeft + incLeft;
            ballTop = ballTop + incTop;
            Canvas.SetLeft(this.Ball, ballLeft);
            Canvas.SetTop(this.Ball, ballTop);

            CheckCollision();
        }

        private void AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (var skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if (ballLeft == -Int32.MaxValue)
                    {
                        var r = new Random();
                        incLeft = -speed;
                        incTop = -speed;
                        ballLeft = (this.GameWindow.ActualWidth - this.Ball.ActualWidth) / 2;
                        ballTop = (this.GameWindow.ActualHeight - this.Ball.ActualHeight) / 2;
                        timer.Start();
                    }

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
            //Debug.WriteLine(hand.Position.X + " <> " + hand.Position.Y);
            hand = hand.ScaleTo((int)this.GameWindow.ActualWidth, (int)this.GameWindow.ActualHeight);
            //Debug.WriteLine(hand.Position.X + " <> " + hand.Position.Y);

            Canvas.SetTop(this.Player, hand.Position.Y - this.Player.ActualHeight / 2);
            Canvas.SetTop(this.Computer, hand.Position.Y - this.Computer.ActualHeight / 2);
        }

        public void CheckCollision()
        {
            if (ballLeft <= 0)
            {
                incLeft = speed;
                this.ScoreComputer.Text = (Convert.ToInt32(this.ScoreComputer.Text) + 1) + "";
            }
            if (ballLeft + this.Ball.ActualWidth >= this.GameWindow.ActualWidth)
            {
                incLeft = -speed;
                this.ScorePlayer.Text = (Convert.ToInt32(this.ScorePlayer.Text) + 1) + "";
            }
            if (ballTop <= 0) incTop = speed;
            if (ballTop + this.Ball.ActualHeight >= this.GameWindow.ActualHeight) incTop = -speed;

            // check player and computer rect
            if (Intersect(this.Player, this.Ball)) incLeft = speed;
            if (Intersect(this.Computer, this.Ball)) incLeft = -speed;
        }

        public bool Intersect(FrameworkElement a, FrameworkElement b)
        {
            Rect ra = new Rect((double)a.GetValue(Canvas.LeftProperty), (double)a.GetValue(Canvas.TopProperty), a.Width, a.Height);
            Rect rb = new Rect((double)b.GetValue(Canvas.LeftProperty), (double)b.GetValue(Canvas.TopProperty), b.Width, b.Height);

            return ra.IntersectsWith(rb);
        }
    }
}
