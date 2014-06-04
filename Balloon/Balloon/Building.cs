using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Balloon
{
    public class Building
    {
        int _id;
        BitmapImage _bitmap;
        Rect _border;

        public Building(int id, BitmapImage bitmap, Rect border)
        {
            this._id = id;
            this._bitmap = bitmap;
            this._border = border;
        }

        public int Id { 
            get { return this._id; }
            set { this._id = value; }
        }
        public BitmapImage Bitmap
        {
            get { return this._bitmap; }
            set { this._bitmap = value; }
        }
        public Rect Border
        {
            get { return this._border; }
            set { this._border = value; }
        }
    }
}
