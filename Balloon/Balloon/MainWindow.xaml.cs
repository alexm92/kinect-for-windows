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

                foreach (var building in buildings)
                {
                    if (building.Border != null && building.Border.X >= x && building.Border.X <= y)
                    {
                        Debug.WriteLine(building.Border.ToString());
                        if (building.Border.Height >= 423 * 0.7 - 1 && building.Border.Height <= 423 * 0.7 + 1)
                            Check(building);
                    }
                }
                Debug.WriteLine("-------------");
            }
        }

        void Check(Building building)
        {
            Polygon building_polygon = GetPolyline(Building2Polygon.Points, building);

            var balloonPoint = PlayerBalloon.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));
            Polygon balloon_polygon = GetPolyline(BalloonPolygon.Points, new Building(0, null, new Rect(balloonPoint.X, balloonPoint.Y, PlayerBalloon.Width, PlayerBalloon.Height)));

            buildingsOverlay.Children.Clear();
            buildingsOverlay.Children.Add(building_polygon);
            buildingsOverlay.Children.Add(balloon_polygon);

            //this.Title = "" + (geometry.GetFlattenedPathGeometry().Figures.Count > 0);
        }

        Polygon GetPolyline(PointCollection points, Building building)
        {
            Polygon poli = new Polygon();
            foreach (var point in points)
            {
                var size = new Size(225, 339);
                if (building.Id == 2)
                {
                    size = new Size(Building2Polygon.Width, Building2Polygon.Height);
                }
                poli.Points.Add(new Point(building.Border.X + point.X * building.Border.Width / size.Width, building.Border.Y + point.Y * building.Border.Height / size.Height));
            }
            poli.Fill = Brushes.Green;
            poli.Opacity = 0.5;

            return poli;
        }
    }
}
