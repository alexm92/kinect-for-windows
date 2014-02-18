using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System;
using System.Collections.Generic;
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

namespace UserHeight
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

            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooseUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.KinectChanged += SensorChooserKinectChanged;

            this.sensorChooser.Start();

            //var bind = new Binding("Kinect") { Source = this.sensorChooseUi.KinectSensorChooser.Kinect; };
            //BindingOperations.SetBinding(this.depthViewr, DependencyProperty, bind);
        }

        void SensorChooserKinectChanged(object sender, KinectChangedEventArgs e)
        {
            if (e.OldSensor != null)
            {
                try
                {
                    e.OldSensor.ColorStream.Disable();
                    e.OldSensor.DepthStream.Disable();
                    e.OldSensor.SkeletonFrameReady -= this.SkeletonFrameReady;
                    e.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (e.NewSensor != null)
            {
                try
                {
                    e.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    e.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    e.NewSensor.SkeletonStream.Enable();

                    // Allocate space to put the depth, color, and skeleton data we'll receive
                    if (null == this.skeletons)
                    {
                        this.skeletons = new Skeleton[e.NewSensor.SkeletonStream.FrameSkeletonArrayLength];
                    }

                    // Add an event handler to be called whenever there is new depth frame data
                    e.NewSensor.SkeletonFrameReady += this.SkeletonFrameReady;

                    try
                    {
                        e.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        e.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
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
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);

                    var user = (from s in skeletons where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                    if (user != null)
                    {
                        GetUserHeight(user);
                    }
                }
            }
        }


        public static double Length(Joint p1, Joint p2)
        {
            return Math.Sqrt(
                Math.Pow(p1.Position.X - p2.Position.X, 2) +
                Math.Pow(p1.Position.Y - p2.Position.Y, 2) +
                Math.Pow(p1.Position.Z - p2.Position.Z, 2));
        }

        public static double Length(params Joint[] joints)
        {
            double length = 0;

            for (int index = 0; index < joints.Length - 1; index++)
            {
                length += Length(joints[index], joints[index + 1]);
            }

            return length;
        }


        public static int NumberOfTrackedJoints(params Joint[] joints)
        {
            int trackedJoints = 0;

            foreach (var joint in joints)
            {
                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    trackedJoints++;
                }
            }

            return trackedJoints;
        }

        private void GetUserHeight(Skeleton skeleton)
        {
            const double HEAD_DIVERGENCE = 0.1;

            var head = skeleton.Joints[JointType.Head];
            var neck = skeleton.Joints[JointType.ShoulderCenter];
            var spine = skeleton.Joints[JointType.Spine];
            var waist = skeleton.Joints[JointType.HipCenter];
            var hipLeft = skeleton.Joints[JointType.HipLeft];
            var hipRight = skeleton.Joints[JointType.HipRight];
            var kneeLeft = skeleton.Joints[JointType.KneeLeft];
            var kneeRight = skeleton.Joints[JointType.KneeRight];
            var ankleLeft = skeleton.Joints[JointType.AnkleLeft];
            var ankleRight = skeleton.Joints[JointType.AnkleRight];
            var footLeft = skeleton.Joints[JointType.FootLeft];
            var footRight = skeleton.Joints[JointType.FootRight];

            // Find which leg is tracked more accurately.
            int legLeftTrackedJoints = NumberOfTrackedJoints(hipLeft,
                                                             kneeLeft,
                                                             ankleLeft,
                                                             footLeft);
            int legRightTrackedJoints = NumberOfTrackedJoints(hipRight,
                                                              kneeRight,
                                                              ankleRight,
                                                              footRight);

            double legLength = legLeftTrackedJoints > legRightTrackedJoints ?
                               Distance(hipLeft, kneeLeft, ankleLeft, footLeft) :
                               Distance(hipRight, kneeRight, ankleRight, footRight);

            double dist = Distance(head, neck, spine, waist) + legLength + HEAD_DIVERGENCE;
            this.UserHeight.Text = string.Format("{0:N2} m", dist);
        }

        private double Distance(params Joint[] joints)
        {
            double dist = 0;

            if (joints.Length > 0)
            {
                var currentJoint = joints[0];
                for (int i = 1; i < joints.Length; i++)
                {
                    dist += Length(currentJoint, joints[i]);
                    currentJoint = joints[i];
                }
            }

            return dist;
        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorChooser.Stop();
        }
    }
}
