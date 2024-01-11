using GH_GeneralClassLibrary.Utils;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ART_MACHINE.AM_ClassLib
{
    public class DrawGeo
    {
        Curve curve;
        Polyline polyline;
        Point2d point;
        public bool isCurve { get => (curve != null) ? true : false; }
        public bool isPolyline { get => (polyline != null) ? true : false; }
        public bool isPoint { get => (point != null) ? true : false; }


        public DrawGeo(Polyline pl)
        {
            polyline = pl;
        }
        public DrawGeo(Curve crv)
        {
            curve = crv;
        }

        public DrawGeo(Point2d pt)
        {
            point = pt;
        }

        public void ToPolyline(double lineTolerance, double lineAngleTolerance, double min, double max)
        {
            PolylineCurve plc = curve.ToPolyline(lineTolerance, lineAngleTolerance, min, max);
            plc.TryGetPolyline(out polyline);

        }

        public Point2d GetPoint { get => point; }
        public Polyline GetPolyline { get => polyline; }
    }
    public static class InputProsessor
    {

        public static void processAllInputs(this ART_MACHINE.G_Code gcode, IEnumerable<IGH_GeometricGoo> inputGeo, double lineTolerance, double lineAngleTolerance)
        {

            //Create drawGeo object for all inputs. // To keep same order with points and curves.

            List<DrawGeo> drawGeo = new List<DrawGeo>();
            foreach (var geo in inputGeo)
            {
                if (geo == null) continue;

                Curve crv = default;
                
                Point3d point;

                if (GH_Convert.ToCurve(geo, ref crv, GH_Conversion.Both))
                {
                    drawGeo.Add(new DrawGeo(crv));
                }
                else if (geo.CastTo<Point3d>(out point))
                {
                    drawGeo.Add(new DrawGeo(point.ToPoint2d()));
                }
            }


            //Paralell convert curves to polyline
            var queue = new ConcurrentQueue<DrawGeo>(drawGeo);

            Parallel.ForEach(
                queue,
                _ =>
                {
                    DrawGeo x;
                    if (queue.TryDequeue(out x))
                    {
                        //IF curve
                        if (x.isCurve)
                        {
                            x.ToPolyline(lineTolerance, lineAngleTolerance, lineTolerance, 1e5);
                        }
                    }
                });


            //ADD To gcode object.

            foreach (var geo in drawGeo)
            {
                if (geo.isPolyline)
                {
                    gcode.AddPolylineMove(geo.GetPolyline);
                }
                else if (geo.isPoint)
                {
                    gcode.AddDotMove(geo.GetPoint);
                }
            }

            gcode.AddShutdownCode();

        }
    }
}
