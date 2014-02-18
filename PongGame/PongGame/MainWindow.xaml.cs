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

namespace PongGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal readonly KinectSensorChooser sensorChooser;


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
                oldSensor.DepthStream.Disable();
                oldSensor.SkeletonStream.Disable();
            }
        }
    }
}
