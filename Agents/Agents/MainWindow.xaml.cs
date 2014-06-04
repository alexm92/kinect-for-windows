using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        ObservableCollection<Listing> _listings = new ObservableCollection<Listing>();
        ObservableCollection<Listing> _selectedListings = new ObservableCollection<Listing>();

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
            NavigateToListings(null, null);
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
            string json = File.ReadAllText(@"Data/agents.json");
            var agents_list = JsonConvert.DeserializeObject<ObservableCollection<Agent>>(json);

            foreach (var agent in agents_list)
            {
                _agents.Add(agent);
            }

            json = File.ReadAllText(@"Data/listings.json");
            var listings_list = JsonConvert.DeserializeObject<ObservableCollection<Listing>>(json);

            foreach (var listing in listings_list)
            {
                _listings.Add(listing);
                //if (_listings.Count == 10)
                //{
                //    break;
                //}
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
        /// Observable collection, return a list of listings
        /// </summary>
        public ObservableCollection<Listing> Listings
        {
            get { return _listings; }
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
        /// Observable collection, returns a list of selected listing.
        /// Not more than 1 !!!
        /// </summary>
        public ObservableCollection<Listing> SelectedListings
        {
            get { return _selectedListings; }
        }

        /// <summary>
        /// Listing click event => go to listing details.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListingClick(object sender, RoutedEventArgs e)
        {
            KinectTileButton btn = sender as KinectTileButton;

            if (btn != null)
            {
                Listing listing = btn.Tag as Listing;
                NavigateToListingDetails(listing);
            }
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
            NavigateToListings(sender, e);
        }

        /// <summary>
        /// Navigate to listings (sales or rentals).
        /// </summary>
        private void NavigateToListings(object sender, RoutedEventArgs e)
        {
            // menu items
            this.menuListings.Background = Brushes.WhiteSmoke;
            this.menuAgents.Background = Brushes.White;
            this.menuGames.Background = Brushes.White;

            /// grids
            CollapseAllGrids();
            this.gridListings.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Navigate to agent details page
        /// </summary>
        /// <param name="agent"></param>
        private void NavigateToListingDetails(Listing listing)
        {
            if (listing != null)
            {
                _selectedListings.Clear();
                _selectedListings.Add(listing);

                // grids
                CollapseAllGrids();
                this.gridListingDetails.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Navigate to games.
        /// </summary>
        private void NavigateToGames(object sender, RoutedEventArgs e)
        {
            // menu items
            this.menuListings.Background = Brushes.White;
            this.menuAgents.Background = Brushes.White;
            this.menuGames.Background = Brushes.WhiteSmoke;

            /// grids
            // this.gridAgents.Visibility = Visibility.Visible;
            // this.gridAgentDetails.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Navigate to agents list.
        /// </summary>
        private void NavigateToAgents(object sender, RoutedEventArgs e)
        {
            // menu items
            this.menuListings.Background = Brushes.White;
            this.menuAgents.Background = Brushes.WhiteSmoke;
            this.menuGames.Background = Brushes.White;

            // grids
            CollapseAllGrids();
            this.gridAgents.Visibility = Visibility.Visible;
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

                // grids
                CollapseAllGrids();
                this.gridAgentDetails.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Hides all pages
        /// </summary>
        private void CollapseAllGrids()
        {
            this.gridListings.Visibility = Visibility.Collapsed;
            this.gridListingDetails.Visibility = Visibility.Collapsed;
            this.gridAgents.Visibility = Visibility.Collapsed;
            this.gridAgentDetails.Visibility = Visibility.Collapsed;
        }

        #endregion

    }
}
