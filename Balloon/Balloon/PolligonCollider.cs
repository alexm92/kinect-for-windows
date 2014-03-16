using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Balloon
{
    public class PolligonCollider
    {

        public PolligonCollider()
        {

        }

        /// <summary>
        /// Check if two Geometry objects intersect.
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns>True if geometry objects intersect, false otherwise</returns>
        public static bool IsIntersetWith(Geometry g1, Geometry g2)
        {
            Geometry og1 = g1.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0));
            Geometry og2 = g2.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0));

            CombinedGeometry cg = new CombinedGeometry(GeometryCombineMode.Intersect, og1, og2);

            PathGeometry pg = cg.GetFlattenedPathGeometry();
            return pg.Figures.Count > 0;
        }
    }
}
