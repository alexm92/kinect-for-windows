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

namespace Balloon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 33);
            timer.Tick += timer_Tick;
            timer.Start();
        }

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

            var balloonPoint = PlayerBalloon.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));
            Polygon balloon_polygon = GetPolygon(BalloonPolygon, new Building(-1, null, new Rect(balloonPoint.X, balloonPoint.Y, PlayerBalloon.Width, PlayerBalloon.Height)));

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
            PlayerBalloon.Margin = new Thickness(50, point.Y, 0, 0);
        }
    }
}
