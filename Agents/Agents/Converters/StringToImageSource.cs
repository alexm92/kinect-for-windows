using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Agents.Converters
{

    public class StringToImageSource : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string filename = value as string;
                string img_path = System.AppDomain.CurrentDomain.BaseDirectory + "Data\\" + filename;
                Uri uri = new Uri(img_path);
                BitmapImage bmp = new BitmapImage(uri);
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}