using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ART_MACHINE
{
    /// <summary>
    /// Uses the Douglas Peucker algorithm to reduce the number of points.
    /// </summary>
    /// <param name="Points">The points.</param>
    /// <param name="Tolerance">The tolerance.</param>
    /// <returns></returns>
    public static class DouglasPeucker
    {

        public static List<Rhino.Geometry.Point2d> DouglasPeuckerReduction(List<Rhino.Geometry.Point2d> Points, Double Tolerance)
        {
            if (Points == null || Points.Count < 3)
            {
                return Points;
            }

            Int32 firstPoint = 0;
            Int32 lastPoint = Points.Count - 1;
            List<Int32> pointIndexsToKeep = new List<Int32>();


            //Add the first and last index to the keepers
            pointIndexsToKeep.Add(firstPoint);
            pointIndexsToKeep.Add(lastPoint);

            //First and last point can not bee the same
            while (Points[firstPoint].Equals(Points[lastPoint]))
            {
                lastPoint--;
            }

            DouglasPeuckerReduction(Points, firstPoint, lastPoint, Tolerance, ref pointIndexsToKeep);

            List<Rhino.Geometry.Point2d> returnPoints = new List<Rhino.Geometry.Point2d>();
            pointIndexsToKeep.Sort();

            foreach (Int32 indexxx in pointIndexsToKeep)
            {
                returnPoints.Add(Points[indexxx]);
            }

            return returnPoints;

        }



        private static void DouglasPeuckerReduction(List<Rhino.Geometry.Point2d> points, Int32 firstPoint, Int32 lastPoint, Double tolerance, ref List<Int32> pointIndexsToKeep)
        {
            Double maxDistance = 0;
            Int32 indexFarthest = 0;

            for (Int32 indexx = firstPoint; indexx < lastPoint; indexx++)
            {
                Double distance = PerpendicularDistance(points[firstPoint], points[lastPoint], points[indexx]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    indexFarthest = indexx;
                }
            }

            if (maxDistance > tolerance && indexFarthest != 0)
            {
                //Add the largest point that exceeds the tolerance
                pointIndexsToKeep.Add(indexFarthest);

                //Recurtion on both sides of the added point
                DouglasPeuckerReduction(points, firstPoint, indexFarthest, tolerance, ref pointIndexsToKeep);
                DouglasPeuckerReduction(points, indexFarthest, lastPoint, tolerance, ref pointIndexsToKeep);

            }
        }

        private static Double PerpendicularDistance(Rhino.Geometry.Point2d Point1, Rhino.Geometry.Point2d Point2, Rhino.Geometry.Point2d Point)
        {

            //Area = |(1/2)(x1y2 + x2y3 + x3y1 - x2y1 - x3y2 - x1y3)|   *Area of triangle
            //Base = v((x1-x2)²+(x1-x2)²)                               *Base of Triangle*
            //Area = .5*Base*H                                          *Solve for height
            //Height = Area/.5/Base


            Double area = Math.Abs(.5 * (Point1.X * Point2.Y + Point2.X *
            Point.Y + Point.X * Point1.Y - Point2.X * Point1.Y - Point.X *
            Point2.Y - Point1.X * Point.Y));
            Double bottom = Math.Sqrt(Math.Pow(Point1.X - Point2.X, 2) +
            Math.Pow(Point1.Y - Point2.Y, 2));
            Double height = area / bottom * 2;

            return height;

            //Another option
            //Double A = Point.X - Point1.X;
            //Double B = Point.Y - Point1.Y;
            //Double C = Point2.X - Point1.X;
            //Double D = Point2.Y - Point1.Y;

            //Double dot = A * C + B * D;
            //Double len_sq = C * C + D * D;
            //Double param = dot / len_sq;

            //Double xx, yy;

            //if (param < 0)
            //{
            //    xx = Point1.X;
            //    yy = Point1.Y;
            //}
            //else if (param > 1)
            //{
            //    xx = Point2.X;
            //    yy = Point2.Y;
            //}
            //else
            //{
            //    xx = Point1.X + param * C;
            //    yy = Point1.Y + param * D;
            //}

            //Double d = DistanceBetweenOn2DPlane(Point, new Point(xx, yy));
        }

    }
}
