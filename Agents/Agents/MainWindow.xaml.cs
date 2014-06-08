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
using System.Windows.Threading;
using Balloon;
using Gestures;

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

        DispatcherTimer _timerGame;
        Skeleton [] _skeletons;
        GestureController _gestureController;

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

            // set event
            kinectRegion.HandPointersUpdated += kinectRegion_HandPointersUpdated;

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
                    args.OldSensor.SkeletonFrameReady -= NewSensor_SkeletonFrameReady;
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

                    args.NewSensor.SkeletonFrameReady += NewSensor_SkeletonFrameReady;
                    _skeletons = new Skeleton[args.NewSensor.SkeletonStream.FrameSkeletonArrayLength];
                    _gestureController = new GestureController(new GestureType[] {
                        GestureType.WaveRight,
                        GestureType.WaveLeft,
                        GestureType.SwipeLeft,
                        GestureType.SwipeRight
                    });
                    _gestureController.GestureRecognized += OnGestureRecognized;
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }

        void NewSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletonFrame.CopySkeletonDataTo(_skeletons);
                    var user = (from s in _skeletons where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                    if (user != null)
                    {
                        _gestureController.Update(user);
                    }
                }
            }
        }

        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            switch (e.Type)
            {
                case GestureType.WaveLeft:
                    this.Background = Brushes.Blue;
                    break;

                case GestureType.WaveRight:
                    this.Background = Brushes.Red;
                    break;

                case GestureType.SwipeLeft:
                    this.Background = Brushes.Green;
                    break;

                case GestureType.SwipeRight:
                    this.Background = Brushes.Pink;
                    break;
            }
        }

        #region HL
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
                if (_listings.Count == 10)
                {
                    break;
                }
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
        #endregion

        #region Game
        void timer_Tick(object sender, EventArgs e)
        {
            var buildings = myCity._buildingImages;

            if (buildings.Count > 0)
            {
                var x = PlayerBalloon.Margin.Left;
                var y = x + PlayerBalloon.Width;

                buildingsOverlay.Children.Clear();
                foreach (var building in buildings)
                {
                    if (building.Border != null &&
                        x <= building.Border.X + building.Border.Width &&
                        building.Border.X <= y)
                    {
                        Check(building);
                    }
                }
            }
        }

        /// <summary>
        /// Check if the balloon hits a building
        /// </summary>
        /// <param name="building"></param>
        void Check(Building building)
        {
            var obj = this.FindName("Building" + building.Id + "Polygon") as Polygon;
            Polygon building_polygon = GetPolygon(obj, building);
            Polygon balloon_polygon = GetPolygon(BalloonPolygon, new Building(-1, null, new Rect(PlayerBalloon.Margin.Left, PlayerBalloon.Margin.Top, PlayerBalloon.Width, PlayerBalloon.Height)));

            buildingsOverlay.Children.Add(building_polygon);
            buildingsOverlay.Children.Add(balloon_polygon);

            bool isIntersection = PolygonCollider.AreIntersecting(balloon_polygon, building_polygon);
            if (isIntersection)
            {
                //timer.Stop();
                this.Title = "Hit";
            }
        }

        /// <summary>
        /// Generates a polygon for building
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="building"></param>
        /// <returns></returns>
        Polygon GetPolygon(Polygon obj, Building building)
        {
            Polygon poli = new Polygon();
            foreach (var point in obj.Points)
            {
                var size = new Size(225, 339);
                if (building.Id > 0)
                {
                    size = new Size(obj.Width, obj.Height);
                }
                poli.Points.Add(new Point(building.Border.X + point.X * building.Border.Width / size.Width, building.Border.Y + point.Y * building.Border.Height / size.Height));
            }
            poli.Fill = Brushes.Green;
            poli.Opacity = 0.5;

            return poli;
        }

        /// <summary>
        /// Move the balloon on the Y axis
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(Application.Current.MainWindow);
            this.Title = point.ToString();
            double y = point.Y - gridHeader.ActualHeight - PlayerBalloon.ActualHeight * 0.5;
            PlayerBalloon.Margin = new Thickness(50, y, 0, 0);
        }

        void kinectRegion_HandPointersUpdated(object sender, EventArgs e)
        {
            var hand = (from h in kinectRegion.HandPointers where h.IsActive && h.IsPrimaryHandOfUser select h).FirstOrDefault();
            if (hand != null)
            {
                var point = hand.GetPosition(Application.Current.MainWindow);
                double y = point.Y - gridHeader.ActualHeight - PlayerBalloon.ActualHeight * 0.5;
                this.Title = point.Y + "";
                PlayerBalloon.Margin = new Thickness(50, y, 0, 0);
            }
        }

        #endregion

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

            _timerGame = new DispatcherTimer();
            _timerGame.Interval = new TimeSpan(0, 0, 0, 0, 33);
            _timerGame.Tick += timer_Tick;
            _timerGame.Start();

            myCity._timer.Start();

            /// grids
            CollapseAllGrids();
            this.gridGames.Visibility = Visibility.Visible;
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
            this.gridGames.Visibility = Visibility.Collapsed;
        }

        #endregion

    }
}
