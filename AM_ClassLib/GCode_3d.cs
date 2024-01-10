using Rhino.Geometry;
using System;
using System.Collections.Generic;


namespace ART_MACHINE
{

    public class GCode_3d
    {
        Double extruderPosition;
        Double extrudeAmountPerMM = 1;
        List<G_Code_Line> lines;
        int feedRate_extruding;
        int feedrate_NOTextruding;
        List<Rhino.Geometry.Point3d> toolPath;

        bool IsExtruding = false;

        private int feedrate()
        {
            if (IsExtruding)
                return feedRate_extruding;
            else
                return feedrate_NOTextruding;
        }
        public void Extrude()
        {
            if (!IsExtruding)
            {
                //Start extrude rutine

            }

            IsExtruding = true;
        }
        public void StopExtrude()
        {
            if (!IsExtruding)
            {
                //Stop extrude rutine
            }
            IsExtruding = false;
            ResetextruderPosition();
        }

        Double GetExtrudeAmount(double distance)
        {
            return extrudeAmountPerMM * distance;
        }
        Double GetExtrudeAmount(Point3d from, Point3d to)
        {
            double distance = from.DistanceTo(to);
            return GetExtrudeAmount(distance);
        }
        Double GetIncrementedExtruderPosition(double incrementAmount)
        {
            extruderPosition = extruderPosition + incrementAmount;
            return extruderPosition;
        }

        void ResetextruderPosition()
        {
            lines.Add(new G_Code_Line("G92 E0"));
            extruderPosition = 0;
        }




        public GCode_3d(int _feedRate_extruding, int _feedrate_NOTextruding)
        {
            lines = new List<G_Code_Line>();
            toolPath = new List<Rhino.Geometry.Point3d>();
            feedRate_extruding = _feedRate_extruding;
            feedrate_NOTextruding = _feedrate_NOTextruding;
            extruderPosition = 0;

        }

        public void AddNextLineSegment(List<Rhino.Geometry.Point3d> points, bool extruderMove)
        {

            if (extruderMove)
            {
                StopExtrude();
                Add_MoveToPoint(points[0]);
                Extrude();
            }
            else
            {
                StopExtrude();
            }



            Point3d prevPoint = points[0];
            foreach (Point3d p in points)
            {
                if (extruderMove)
                {
                    Add_MoveAndExtrudeToPoint(prevPoint, p, GetIncrementedExtruderPosition(GetExtrudeAmount(prevPoint, p)));
                    prevPoint = p;
                }
                else
                {
                    Add_MoveToPoint(p);

                }


            }

        }

        public void Add_MoveAndExtrudeToPoint(Point3d fromPt, Point3d toPt, double extruderTargetPos)
        {
            toolPath.Add(new Point3d(toPt));
            lines.Add(new G_Code_Line(toPt, extruderTargetPos, feedrate()));

        }
        public void Add_MoveToPoint(Point3d toPt)
        {
            toolPath.Add(new Point3d(toPt));
            lines.Add(new G_Code_Line(toPt));
        }

        public List<Rhino.Geometry.Point3d> GetToolPath()
        {
            return toolPath;
        }

        public String OutputText()
        {
            String rtnString = "";

            foreach (G_Code_Line l in lines)
            {
                rtnString += "\n" + l.ToString();
            }

            return rtnString;
        }
    }

}
