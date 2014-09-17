
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Balloon
{
    public class City : FrameworkElement
    {
        VisualCollection _buildingsList;
        double _currentWidth, _leftStartMargin;
        Random _rand;

        /// <summary>
        /// Contains a list of the currently displayed buildings and their size after scale
        /// </summary>
        public List<Building> _buildingImages;

        const double _scale = 0.7;
        const double _speed = 3;

        public DispatcherTimer _timer;

        /// <summary>
        /// Initializations and event handlers
        /// </summary>
        public City()
        {
            // init
            _buildingsList = new VisualCollection(this);
            _buildingImages = new List<Building>();
            _currentWidth = 0;
            _leftStartMargin = 0;
            _rand = new Random();

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 33);
            _timer.Tick += TimerTick;

            // event handlers
            this.Loaded += CityLoaded;

            // initial game offset
            _leftStartMargin = 500;
        }

        /// <summary>
        /// Event loaded was triggred, we find the width of the element and we
        /// draw the initial buildings that fill the object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CityLoaded(object sender, RoutedEventArgs e)
        {
            if (this.IsEnabled)
            {
                _timer.Start();
            }
            else
            {
                All();
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            All();
        }

        void All()
        {
            _leftStartMargin -= _speed;
            Update();
            Draw();
        }

        /// <summary>
        /// Here we make the math
        /// </summary>
        private void Update()
        {
            int width, height, index;

            // remove the first building if it goes out of the screen
            if (_buildingImages.Count > 0 && _leftStartMargin + _buildingImages[0].Border.Width <= 0)
            {
                _currentWidth -= _buildingImages[0].Border.Width;
                _leftStartMargin = 0;
                _buildingImages.RemoveAt(0);
            }

            // add buildings if we can
            while (_currentWidth <= this.ActualWidth + 200)
            {
                index = _rand.Next(1, 7);
                //var uri = new Uri("pack://application:,,,/Images/building" + index + ".png");
                var uri = new Uri(@"C:\Users\Alexandru\Desktop\kinect-for-windows\Agents\Agents\Images\building" + index + ".png");
                var image = new BitmapImage(uri);

                width = (int)(image.PixelWidth * _scale);
                height = (int)(image.PixelHeight * _scale);
                _currentWidth += width;

                _buildingImages.Add(new Building(index, image, new Rect(0, 0, width, height)));
            }
        }

        /// <summary>
        /// Draws all objects to screen
        /// </summary>
        private void Draw()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                DrawingVisual building = new DrawingVisual();
                double currentLeftMargin = _leftStartMargin;
                using (var context = building.RenderOpen())
                {
                    for (int i = 0; i < _buildingImages.Count; i++)
                    {
                        var obj = _buildingImages[i];
                        obj.Border = new Rect(currentLeftMargin, this.ActualHeight - obj.Border.Height - 42, obj.Border.Width, obj.Border.Height);
                        context.DrawImage(obj.Bitmap, obj.Border);
                        currentLeftMargin += obj.Border.Width;
                    }
                }
                _buildingsList.Clear();
                _buildingsList.Add(building);
            }));
        }

        protected override Visual GetVisualChild(int index)
        {
            return _buildingsList[index];
        }

        protected override int VisualChildrenCount
        {
            get { return _buildingsList.Count; }
        }
    }
}
