﻿
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
        List<Tuple<BitmapFrame, Size>> _buildingImages;

        const double _scale = 0.7;
        const double _speed = 3;

        DispatcherTimer _timer;

        /// <summary>
        /// Initializations and event handlers
        /// </summary>
        public City()
        {
            // init
            _buildingsList = new VisualCollection(this);
            _buildingImages = new List<Tuple<BitmapFrame, Size>>();
            _currentWidth = 0;
            _leftStartMargin = 0;
            _rand = new Random();


            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 33);
            _timer.Tick += TimerTick;

            // event handlers
            this.Loaded += CityLoaded;
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

            //Thread t = new Thread(new ThreadStart(Draw));
            //t.SetApartmentState(ApartmentState.MTA);
            //t.IsBackground = true;
            //t.Priority = ThreadPriority.Highest;
            //t.Start();

            //_timer.Stop();
            //BackgroundWorker worker = new BackgroundWorker();
            //worker.DoWork += worker_DoWork;
            //worker.RunWorkerCompleted += delegate
            //{
            //    _timer.Start();
            //};
            //worker.RunWorkerAsync();
        }

        void All()
        {
            _leftStartMargin -= _speed;
            Update();
            Draw();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            All();
        }

        /// <summary>
        /// Here we make the math
        /// </summary>
        private void Update()
        {
            BitmapFrame image;
            int width, height, index;

            // remove the first building if it goes out of the screen
            if (_buildingImages.Count > 0 && _leftStartMargin + _buildingImages[0].Item2.Width <= 0)
            {
                _currentWidth -= _buildingImages[0].Item2.Width;
                _leftStartMargin = 0;
                _buildingImages.RemoveAt(0);
            }

            // add buoldings if we can
            while (_currentWidth <= this.ActualWidth + 200)
            {
                index = _rand.Next(1, 4);
                image = BitmapDecoder.Create(new Uri(@"D:\GitHub\kinect-for-windows\Balloon\Balloon\Resources\building" + index + ".png", UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames.First();

                width = (int)(image.PixelWidth * _scale);
                height = (int)(image.PixelHeight * _scale);
                _currentWidth += width;

                _buildingImages.Add(new Tuple<BitmapFrame, Size>(image, new Size(width, height)));

                Debug.WriteLine(this.ActualWidth);
                Debug.WriteLine(_currentWidth);
                Debug.WriteLine(_buildingImages.Count);
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
                        context.DrawImage(obj.Item1, new Rect(currentLeftMargin, this.ActualHeight - obj.Item2.Height, obj.Item2.Width, obj.Item2.Height));
                        currentLeftMargin += obj.Item2.Width;
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