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

namespace PongGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal readonly KinectSensorChooser sensorChooser;
        Skeleton[] skeletons;


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
                    oldSensor.SkeletonFrameReady -= SkeletonFrameReady;
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
                    newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    newSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    newSensor.SkeletonStream.Enable();
                    newSensor.SkeletonFrameReady += SkeletonFrameReady;

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
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }

        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
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

                var elbowLeft = skeleton.Joints[JointType.ElbowLeft];
                var handLeft = skeleton.Joints[JointType.WristLeft];
                var elbowRight = skeleton.Joints[JointType.ElbowRight];
                var handRight= skeleton.Joints[JointType.WristRight];

                if (CheckHand(elbowRight, handRight))
                {
                    Move(handRight);
                }
                else if (CheckHand(elbowLeft, handLeft))
                {
                    Move(handLeft);
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
        }

        bool CheckHand(Joint elbow, Joint hand)
        {
            // if hand and elbow joint are traked
            // and hand is above the elbow, return true
            return elbow.TrackingState == JointTrackingState.Tracked
                && hand.TrackingState == JointTrackingState.Tracked 
                && hand.Position.Y > elbow.Position.Y;
        }
    }
}
