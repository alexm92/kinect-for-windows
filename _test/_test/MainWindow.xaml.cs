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

namespace _test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const double speed = 2;

        DispatcherTimer timer;
        Random rand;
        double cityWidth;

        public MainWindow()
        {
            InitializeComponent();

            this.timer = new DispatcherTimer();
            this.timer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            //this.timer.Tick += this.TimerTick;

            this.rand = new Random();
            this.cityWidth = 0;
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            //while (this.cityWidth <= this.ActualWidth)
            //{
            //    // add building to city
            //    this.AddBuilding();
            //}
        }

        /*
        void TimerTick(object sender, EventArgs e)
        {
             // stop if we don't have buildings to move
            if (panel_city.Children.Count == 0)
            {
                this.timer.Stop();
                return;
            }

            var building = panel_city.Children[0] as Image;
            building.Margin = new Thickness(building.Margin.Left - speed, 0, 0, 0);
            this.cityWidth -= speed;

            // remove current building
            if (building.Margin.Left + building.ActualWidth <= 0)
            {
                panel_city.Children.RemoveAt(0);
            }

            // add new buildings if we can
            if (this.cityWidth <= this.ActualWidth)
            {
                AddBuilding();
            }
        }

        public void AddBuilding()
        {
            int index = this.rand.Next(1, 4);
            Uri uri = new Uri("pack://application:,,,/" + index + ".png");
            BitmapImage bmp = new BitmapImage(uri);

            DrawingVisual dw = new DrawingVisual();
            using (var context = dw.RenderOpen())
            {
                context.DrawImage(bmp, new Rect(0, 0, 100, 100));
            }

            Image building = new Image();
            building.Source = bmp;
            building.Height = 500;
            building.Stretch = Stretch.Uniform;

            cityWidth += bmp.Width * 1.0 / 6;
            panel_city.Children.Add(building);
        }
        */
        private void Window_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            this.timer.Start();
        }
    }
}
