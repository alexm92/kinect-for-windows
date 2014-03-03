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

using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit.Interaction;
using System.Windows.Threading;
using System.Diagnostics;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal readonly KinectSensorChooser sensorChooser;

        InteractionStream interactionStream;
        List<Skeleton> skeletons;
        Dictionary<int, UserInfo> userInfo;

        DispatcherTimer timerUp, timerDown;
        int speed;

        public MainWindow()
        {
            InitializeComponent();

            skeletons = new List<Skeleton>();
            userInfo = new Dictionary<int, UserInfo>();

            this.sensorChooser = new KinectSensorChooser();
            sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.KinectChanged += sensorChooser_KinectChanged;
            this.sensorChooser.Start();

            timerUp = new DispatcherTimer();
            timerUp.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timerUp.Tick += timer_Tick;

            timerDown = new DispatcherTimer();
            timerDown.Interval = new TimeSpan(0, 0, 0, 0, 2);
            timerDown.Tick += timerDown_Tick;

            Canvas.SetLeft(rocketMan, rocketMan.Margin.Left);
            Canvas.SetTop(rocketMan, rocketMan.Margin.Top);
            Canvas.SetLeft(rocketManPath, rocketManPath.Margin.Left);
            Canvas.SetTop(rocketManPath, rocketManPath.Margin.Top);

            speed = 3;
        }

        /// <summary>
        /// Tick every time and it's moovind the rocket man down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timerDown_Tick(object sender, EventArgs e)
        {
            double left = Canvas.GetLeft(rocketMan);
            double top = Canvas.GetTop(rocketMan);

            Canvas.SetLeft(rocketMan, left + 1);
            Canvas.SetTop(rocketMan, top + 1);
            Canvas.SetLeft(rocketManPath, left + 1);
            Canvas.SetTop(rocketManPath, top + 1);

            Cover.Margin = new Thickness(Cover.Margin.Left - 1, Cover.Margin.Top, Cover.Margin.Right, Cover.Margin.Bottom);

            CheckCollision();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            double left = Canvas.GetLeft(rocketMan);
            double top = Canvas.GetTop(rocketMan);

            Canvas.SetLeft(rocketMan, left + 1);
            Canvas.SetTop(rocketMan, top - speed);
            Canvas.SetLeft(rocketManPath, left + 1);
            Canvas.SetTop(rocketManPath, top - speed);

            Cover.Margin = new Thickness(Cover.Margin.Left - 1, Cover.Margin.Top, Cover.Margin.Right, Cover.Margin.Bottom);

            CheckCollision();
        }

        private void CheckCollision()
        {
            if (IsIntersetWith(rocketManPath.Data, Building2.Data))
            {
                MessageBox.Show("You are dead!");
            }
        }

        void sensorChooser_KinectChanged(object sender, KinectChangedEventArgs e)
        {
            if (e.OldSensor != null)
            {
                try
                {
                    e.OldSensor.ColorStream.Disable();
                    e.OldSensor.DepthStream.Disable();
                    e.OldSensor.SkeletonStream.Disable();
                    //e.OldSensor.AllFramesReady -= NewSensor_AllFramesReady;
                    e.OldSensor.DepthFrameReady -= NewSensor_DepthFrameReady;
                    e.OldSensor.SkeletonFrameReady -= NewSensor_SkeletonFrameReady;

                    this.interactionStream.InteractionFrameReady -= this.InteractionFrameReady;
                    this.interactionStream.Dispose();
                    this.interactionStream = null;
                }
                catch
                {
                    // smth
                }
            }

            if (e.NewSensor != null)
            {
                try
                {
                    e.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    e.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    e.NewSensor.SkeletonStream.Enable();
                    //e.NewSensor.AllFramesReady += NewSensor_AllFramesReady;
                    e.NewSensor.DepthFrameReady += NewSensor_DepthFrameReady;
                    e.NewSensor.SkeletonFrameReady += NewSensor_SkeletonFrameReady;

                    try
                    {
                        e.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch
                    {
                        e.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    // interaction
                    this.interactionStream = new InteractionStream(e.NewSensor, new InteractionClient());
                    this.interactionStream.InteractionFrameReady += this.InteractionFrameReady;
                }
                catch
                {
                    // smth
                }
            }
        }

        void NewSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    Skeleton[] localSkeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(localSkeletons);

                    var accelerometer = sensorChooser.Kinect.AccelerometerGetCurrentReading();
                    interactionStream.ProcessSkeleton(localSkeletons, accelerometer, skeletonFrame.Timestamp);

                    skeletons.Clear();
                    foreach (var userSkeleton in localSkeletons)
                    {
                        if (userSkeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            skeletons.Add(userSkeleton);
                        }
                    }

                    var user = (from u in localSkeletons where u.TrackingState == SkeletonTrackingState.Tracked select u).FirstOrDefault();
                    var handPointer = getHandPointer(user);

                    if (handPointer != null)
                    {
                        if (handPointer.HandEventType == InteractionHandEventType.Grip)
                        {
                            this.Background = Brushes.Green;
                            this.timerDown.Stop();
                            this.timerUp.Start();
                        }
                        else if (handPointer.HandEventType == InteractionHandEventType.GripRelease)
                        {
                            this.Background = Brushes.Purple;
                            this.timerUp.Stop();
                            this.timerDown.Start();
                        }
                    }
                }
            }

            var d = IsIntersetWith(Building1.Data, Building2.Data);
        }

        /// <summary>
        /// Check if two Geometry objects intersect.
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns>True if geometry objects intersect, false otherwise</returns>
        public static bool IsIntersetWith(Geometry g1, Geometry g2)
        {
            Geometry og1 = g1.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0));
            Geometry og2 = g2.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0));

            CombinedGeometry cg = new CombinedGeometry(GeometryCombineMode.Intersect, og1, og2);

            PathGeometry pg = cg.GetFlattenedPathGeometry();
            return pg.Figures.Count > 0;
        }

        void NewSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (var depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    DepthImagePixel[] data = new DepthImagePixel[depthFrame.PixelDataLength];
                    depthFrame.CopyDepthImagePixelDataTo(data);
                    interactionStream.ProcessDepth(data, depthFrame.Timestamp);
                }
            }
        }

        void InteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
            using (var interactionFrame = e.OpenInteractionFrame())
            {
                if (interactionFrame != null)
                {
                    UserInfo[] localUserInfo = new UserInfo[InteractionFrame.UserInfoArrayLength];
                    interactionFrame.CopyInteractionDataTo(localUserInfo);

                    userInfo.Clear();
                    foreach (var userSkeleton in skeletons)
                    {
                        var ui = (from u in localUserInfo where u.SkeletonTrackingId == userSkeleton.TrackingId select u).FirstOrDefault();
                        if (ui != null)
                        {
                            userInfo.Add(userSkeleton.TrackingId, ui);
                        }
                    }
                }
            }
        }

        private InteractionHandPointer getHandPointer(Skeleton user)
        {
            UserInfo localUserInfo;
            //var hand = InteractionHandType.Left; // JointType.HandLeft;

            if (user != null && userInfo.TryGetValue(user.TrackingId, out localUserInfo))
            {
                return (from hp in localUserInfo.HandPointers where hp.HandType == InteractionHandType.Right select hp).FirstOrDefault();
            }

            return null;
        }
    }

    class InteractionClient : IInteractionClient
    {
        public InteractionInfo GetInteractionInfoAtLocation(int skeletonTrackingId, InteractionHandType handType, double x, double y)
        {
            var info = new InteractionInfo();
            info.IsGripTarget = true;
            info.IsPressTarget = false;
            info.PressAttractionPointX = 0f;
            info.PressAttractionPointY = 0f;
            info.PressTargetControlId = 0;

            return info;
        }
    }
}
