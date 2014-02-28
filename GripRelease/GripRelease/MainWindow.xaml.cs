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

        public MainWindow()
        {
            InitializeComponent();

            skeletons = new List<Skeleton>();
            userInfo = new Dictionary<int, UserInfo>();

            this.sensorChooser = new KinectSensorChooser();
            sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.KinectChanged += sensorChooser_KinectChanged;
            this.sensorChooser.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);
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
                    e.OldSensor.AllFramesReady -= NewSensor_AllFramesReady;

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
                    e.NewSensor.AllFramesReady += NewSensor_AllFramesReady;

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
            var hand = InteractionHandType.Left; // JointType.HandLeft;

            if (user != null && userInfo.TryGetValue(user.TrackingId, out localUserInfo))
            {
                return (from hp in localUserInfo.HandPointers where hp.HandType == hand select hp).FirstOrDefault();
            }

            return null;
        }

        void NewSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
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
                        }
                        else if (handPointer.HandEventType == InteractionHandEventType.GripRelease)
                        {
                            this.Background = Brushes.Purple;
                        }
                    }
                }
            }

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
