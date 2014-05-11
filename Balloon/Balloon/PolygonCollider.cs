using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Balloon
{
    /// <summary>
    /// Article can be found here
    /// http://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
    /// </summary>
    public class PolygonCollider
    {
        public PolygonCollider()
        {
        }

        /// <summary>
        /// Given three colinear points p, q, r, the function checks if
        /// point q lies on line segment 'pr'
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        static bool onSegment(Point p, Point q, Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;

            return false;
        }

        /// <summary>
        /// To find orientation of ordered triplet (p, q, r).
        /// The function returns following values
        /// 0 --> p, q and r are colinear
        /// 1 --> Clockwise
        /// 2 --> Counterclockwise
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        static int orientation(Point p, Point q, Point r)
        {
            // See 10th slides from following link for derivation of the formula
            // http://www.dcs.gla.ac.uk/~pat/52233/slides/Geometry1x1.pdf
            double val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);

            if (val == 0) return 0;  // colinear

            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }

        /// <summary>
        /// The main function that returns true if line segment 'p1q1'
        /// and 'p2q2' intersect.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="q1"></param>
        /// <param name="p2"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        static bool doIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            // Find the four orientations needed for general and
            // special cases
            int o1 = orientation(p1, q1, p2);
            int o2 = orientation(p1, q1, q2);
            int o3 = orientation(p2, q2, p1);
            int o4 = orientation(p2, q2, q1);

            // General case
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1
            if (o1 == 0 && onSegment(p1, p2, q1)) return true;

            // p1, q1 and p2 are colinear and q2 lies on segment p1q1
            if (o2 == 0 && onSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2
            if (o3 == 0 && onSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2
            if (o4 == 0 && onSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases
        }

        /// <summary>
        /// Check if a segment from the first polygon
        /// is intersecting with one from the second polygon.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static bool AreIntersecting(Polygon p, Polygon q)
        {
            int i, j;

            if (p.Points.Count >= 2 && q.Points.Count >= 2)
            {
                p.Points.Add(p.Points[0]);
                q.Points.Add(q.Points[0]);
                for (i = 1; i < p.Points.Count; i++)
                {
                    for (j = 1; j < q.Points.Count; j++) {
                        bool isIntersection = doIntersect(p.Points[i - 1], p.Points[i], q.Points[j - 1], q.Points[j]);
                        if (isIntersection)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
