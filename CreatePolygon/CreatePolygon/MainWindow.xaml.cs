using System;
using System.Collections.Generic;
using System.IO;
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

namespace CreatePolygon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> files;
        int index;

        public MainWindow()
        {
            InitializeComponent();

            files = new List<string>();
            index = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var files_list = Directory.GetFiles(@"../../Images");
            foreach (var file in files_list) {
                var filename = file.Split(new char[] {'\\'});
                files.Add(filename.Last());
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (myPolygon.Points.Count > 0)
            {
                myPolygon.Points.RemoveAt(myPolygon.Points.Count - 1);
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            // copy result points to clipboard
            Clipboard.SetText(myPolygon.Points.ToString());
            myPolygon.Points.Clear();

            // go to first if at end
            index = index % files.Count;

            // load bitmap
            var uri = new Uri("pack://application:,,,/Images/" + files[index]);
            var bitmap = new BitmapImage(uri);
            myImage.Source = bitmap;
            myImage.Width = myCanvas.Width = bitmap.PixelWidth;
            myImage.Height = myCanvas.Height = bitmap.PixelHeight;

            // set title
            this.Title = files[index++] + " (" + bitmap.PixelWidth + " * " + bitmap.PixelHeight + ")";
        }

        private void myCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(myCanvas);
            myPolygon.Points.Add(point);
        }

        private void myCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            myCanvas.Background = Brushes.LightGray;
        }

        private void myCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            myCanvas.Background = Brushes.White;
        }


    }
}
