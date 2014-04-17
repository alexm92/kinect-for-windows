using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
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

namespace Agents
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensorChooser _sensorChooser;
        ObservableCollection<Agent> _agents = new ObservableCollection<Agent>();
        ObservableCollection<Agent> _selectedAgents = new ObservableCollection<Agent>();

        public MainWindow()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // initialize the sensor chooser and UI
            this._sensorChooser = new KinectSensorChooser();
            this._sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this._sensorChooser;
            this._sensorChooser.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this._sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

            // load data and navigate to agents page
            LoadData();
            NavigateToAgents();
        }

        /// <summary>
        /// Sensor Chooser status changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }

        /// <summary>
        /// Loads agents from json file
        /// </summary>
        void LoadData()
        {
            string json = File.ReadAllText("Data/agents.json");
            var agents_list = JsonConvert.DeserializeObject<ObservableCollection<Agent>>(json);

            foreach (var agent in agents_list)
            {
                _agents.Add(agent);
            }
        }

        /// <summary>
        /// Observable collection, return a list of agents
        /// </summary>
        public ObservableCollection<Agent> Agents
        {
            get { return _agents; }
        }

        /// <summary>
        /// Observable collection, returns a list of selected agents.
        /// Not more than 1 !!!
        /// </summary>
        public ObservableCollection<Agent> SelectedAgents
        {
            get { return _selectedAgents; }
        }

        /// <summary>
        /// Agent click event => go to agent details.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AgentClick(object sender, RoutedEventArgs e)
        {
            KinectTileButton btn = sender as KinectTileButton;

            if (btn != null) {
                Agent agent = btn.Tag as Agent;
                NavigateToAgentDetails(agent);
            }
        }

        #region Navigation
        /// <summary>
        /// Show agents page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowHomePage(object sender, RoutedEventArgs e)
        {
            NavigateToAgents();
        }

        /// <summary>
        /// Navigate to agents list.
        /// </summary>
        private void NavigateToAgents()
        {
            this.gridAgents.Visibility = Visibility.Visible;
            this.gridAgentDetails.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Navigate to agent details page
        /// </summary>
        /// <param name="agent"></param>
        private void NavigateToAgentDetails(Agent agent)
        {
            if (agent != null)
            {
                _selectedAgents.Clear();
                _selectedAgents.Add(agent);

                this.gridAgents.Visibility = Visibility.Collapsed;
                this.gridAgentDetails.Visibility = Visibility.Visible;
            }
        }

        #endregion

    }
}
