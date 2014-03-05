using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace _test
{
    public class City : FrameworkElement
    {
        // Create a collection of child visual objects. 
        private VisualCollection _children;
        private DrawingVisual drawingVisual = new DrawingVisual();
        private List<BitmapImage> imagesList;

        private Random rand;
        private double cityWidth;
        private Point point;

        private DispatcherTimer timer;
        private const double speed = 2;
        private int frame;

        public City()
        {
            _children = new VisualCollection(this);
            
            rand = new Random();
            cityWidth = 0;
            frame = 0;
            point = new Point();

            imagesList = new List<BitmapImage>();

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 17);
            timer.Tick += timer_Tick;
            timer.Start();

            this.Loaded += new RoutedEventHandler(CityLoaded);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            double initX = -speed * frame;

            _children.Clear();
            point.X = initX;

            foreach (var bmp in imagesList)
                AddBuilding(bmp);

            if (initX + imagesList[0].Width * 1.0 / 5 <= 0)
            {
                imagesList.RemoveAt(0);
                AddBuilding();
                frame = 0;
            }

            frame++;
        }

        private void CityLoaded(object sender, RoutedEventArgs e)
        {
            while (this.cityWidth <= this.ActualWidth)
            {
                // add building to city
                this.AddBuilding();
            }
        }

        public void AddBuilding(BitmapImage bmp = null)
        {
            if (bmp == null)
            {
                int index = this.rand.Next(1, 4);
                Uri uri = new Uri("pack://application:,,,/" + index + ".png");
                //Uri uri = new Uri(@"" + index + ".png");
                bmp = new BitmapImage(uri);
                imagesList.Add(bmp);
            }

            DrawingVisual building = new DrawingVisual();
            using (var context = building.RenderOpen())
            {
                context.DrawImage(bmp, new Rect(point.X, point.Y, bmp.Width * 1.0 / 5, bmp.Height * 1.0 / 5));
                point.X += bmp.Width * 1.0 / 5;
            }

            cityWidth += bmp.Width * 1.0 / 6;
            _children.Add(building);
        }

        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return _children.Count;
            }
        }
    }
}
