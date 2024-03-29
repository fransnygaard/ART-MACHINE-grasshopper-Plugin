﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using GH_GeneralClassLibrary.Utils;
using Rhino.Geometry;

namespace ART_MACHINE
{




    public struct G_Code_Line
    {
        int code;
        static int desimalPression = 2;

        List<String> parameters;



        public void AddParameter(String Name, Double Value)
        {
            parameters.Add(Name + Value.ToString());
        }
        override public String ToString()
        {
            String rtnString = "";
            if (code != -1)
            {
                rtnString += "G";
                rtnString += code.ToString();

                foreach (String s in parameters)
                {
                    rtnString += " ";
                    rtnString += s;
                }

            }
            else
            {
                foreach (String s in parameters)
                {
                    rtnString += s;
                }
            }



            return rtnString;
        }

        public G_Code_Line(String str)
        {
            code = -1;
            parameters = new List<string>();

            parameters.Add(str);
        }

        [Obsolete]
        public G_Code_Line(Rhino.Geometry.Point3d point, int Feedrate = int.MinValue)
        {
            code = 1;
            parameters = new List<String>();

            if (Feedrate != int.MinValue)
                AddParameter("F", Feedrate);



            AddParameter("X", point.X);
            AddParameter("Y", point.Y);
            AddParameter("Z", point.Z);


        }
        [Obsolete]
        public G_Code_Line(Rhino.Geometry.Point3d point, double extruder, int Feedrate = int.MinValue)
        {
            code = 1;
            parameters = new List<String>();

            if (Feedrate != int.MinValue)
                AddParameter("F", Feedrate);



            AddParameter("X", point.X);
            AddParameter("Y", point.Y);
            AddParameter("Z", point.Z);
            AddParameter("E", extruder);


        }

        public G_Code_Line(Rhino.Geometry.Point2d point, int Feedrate = int.MinValue)
        {
            code = 1;
            parameters = new List<String>();
            AddParameter("X", Math.Round(point.X,desimalPression));
            AddParameter("Y", Math.Round(point.Y,desimalPression));


            if (Feedrate != int.MinValue)
                AddParameter("F", Feedrate);
        }

        public G_Code_Line(double Z)
        {
            code = 1;
            parameters = new List<String>();

            AddParameter("Z", Z);



        }

        public G_Code_Line(double Z, int _feedRateZ)
        {
            code = 1;
            parameters = new List<String>();

            AddParameter("Z", Z);
            AddParameter("F", _feedRateZ);


        }

        public G_Code_Line(int Code)
        {
            code = Code;
            parameters = new List<String>();
        }

    }

    public struct Shape2D
    {
        List<Rhino.Geometry.Point2d> points;
        public Rhino.Geometry.Curve curve;

        public enum Shape2DTypes
        {
            polyline,
            line,
            arc,
            bezier
        }

        public Shape2DTypes type;

        public Shape2D(Rhino.Geometry.Point2d[] inpoints, Shape2DTypes _type, Rhino.Geometry.Curve _curve)
        {
            curve = _curve;
            type = _type;

            points = new List<Rhino.Geometry.Point2d>(inpoints);
        }

        public Shape2D(Rhino.Geometry.Point3d[] inpoints3d, Shape2DTypes _type, Rhino.Geometry.Curve _curve)
        {
            curve = _curve;
            type = _type;
            points = new List<Rhino.Geometry.Point2d>();

            foreach (Rhino.Geometry.Point3d p3d in inpoints3d)
            {
                points.Add(new Rhino.Geometry.Point2d(p3d.X, p3d.Y));
            }
        }

        public void SimplefyWithDouglasPeuckerReduction(Double Tolerance)
        {
            var temp_ = DouglasPeucker.DouglasPeuckerReduction(points, Tolerance);
            points = temp_;
        }

        public Rhino.Geometry.Point2d startPoint()
        {
            return points[0];
        }
        public Rhino.Geometry.Point2d endPoint()
        {
            return points[points.Count - 1];
        }

        public List<Rhino.Geometry.Point2d> getPointList(bool reverse = false)
        {
            if (reverse)
                points.Reverse();

            return points;
        }
    }




    public class G_Code
    {

        int decimalPrecision = 4;
        int bezierSpeedDivider = 50;

        List<G_Code_Line> lines;

        double penLiftHeight;
        double penZeroZ;
        public double penLiftToleranceSqr = 0.1 * 0.1;

        int feedRateZ;
        int feedRateUP;
        int feedRateDOWN;
        List<Rhino.Geometry.Point3d> toolPath;
        bool IsLifted = true;

        bool isNewPointInLiftDistance(Point3d next)
        {
            return next.DistanceToSquared2d(toolPath[toolPath.Count - 1]) > penLiftToleranceSqr;

        }


        private Double feedrate()
        {
            if (IsLifted)
                return feedRateUP;
            else
                return feedRateDOWN;
        }

        public G_Code(int _feedRateUP, int _feedRateDOWN, int _feedRateZ, Double _penLiftHeight, Double _penZeroZ, int _bezierSpeedDivider)
        {
            feedRateUP = _feedRateUP;
            feedRateDOWN = _feedRateDOWN;
            penLiftHeight = _penLiftHeight;
            penZeroZ = _penZeroZ;
            feedRateZ = _feedRateZ;
            lines = new List<G_Code_Line>();
            toolPath = new List<Rhino.Geometry.Point3d>();
            toolPath.Add(new Rhino.Geometry.Point3d(0, 0, penLiftHeight));
            bezierSpeedDivider = _bezierSpeedDivider;

            AddStartupCode();
        }


        public static string GetAssemblyFileVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersion.FileVersion;
        }

        public void LiftPen()
        {
            if (!IsLifted)
            {
                lines.Add(new G_Code_Line(penLiftHeight, feedRateZ));
                toolPath.Add(new Rhino.Geometry.Point3d(
                    toolPath[toolPath.Count - 1].X,
                    toolPath[toolPath.Count - 1].Y,
                    penLiftHeight));
            }

            IsLifted = true;
        }

        public void LowerPen()
        {
            if (IsLifted)
            {
                lines.Add(new G_Code_Line(penZeroZ, feedRateZ));
                toolPath.Add(new Rhino.Geometry.Point3d(
                    toolPath[toolPath.Count - 1].X,
                    toolPath[toolPath.Count - 1].Y,
                    penZeroZ));
            }
            IsLifted = false;
        }

        public void MoveToZero(bool penLift = true)
        {
            if (penLift)
                LiftPen();

            AddNextLinePoint(new Rhino.Geometry.Point2d(0, 0));


            if (penLift)
                LowerPen();


        }

        public void AddNextLineSegment(List<Rhino.Geometry.Point2d> points, bool penLift = true)
        {

            if (penLift)
            {
                LiftPen();
                AddNextLinePoint(points[0]);
            }
            LowerPen();

            foreach (Rhino.Geometry.Point2d p in points)
            {
                AddNextLinePoint(p);
            }

        }

        public void AddNextLinePoint(Point2d point)
        {
            toolPath.Add(new Rhino.Geometry.Point3d(point.X, point.Y, IsLifted ? penLiftHeight : penZeroZ));
            lines.Add(new G_Code_Line(point, IsLifted ? feedRateUP : feedRateDOWN));
        }

        public void AddPolylineMove(Polyline pl)
        {
            if (isNewPointInLiftDistance(pl.First))
            {
                LiftPen();
            }
            // move to first point
            foreach (var p in pl)
            {
                AddNextLinePoint(p.ToPoint2d());

                if (IsLifted) // LOWER PEN ON FIRST POINT 
                    if(p == pl.First)
                        LowerPen();
            }
        }

        public void AddArcMove(Rhino.Geometry.Point2d centerPoint, Rhino.Geometry.Point2d midpoint, Rhino.Geometry.Point2d startPoint, Rhino.Geometry.Point2d endPoint, Rhino.Geometry.Arc arc, bool penLift = true)
        {
            int arcCode = 2;

            bool fullCircle = false;
            if (startPoint.X == endPoint.X && startPoint.Y == endPoint.Y)
            {
                fullCircle = true;
            }

            if (!fullCircle)
            {
                //Check if arc is drawn cw or ccw
                double angle = Rhino.Geometry.Vector3d.VectorAngle(
                     new Rhino.Geometry.Vector3d(arc.PointAt(0).
                     X - centerPoint.X, arc.PointAt(0).Y - centerPoint.Y, 0),
                     new Rhino.Geometry.Vector3d(arc.PointAt(0.1).X - centerPoint.X, arc.PointAt(0.1).Y - centerPoint.Y, 0),
                     Rhino.Geometry.Plane.WorldXY);



                if (angle < Math.PI)
                {
                    arcCode = 2; //G2 for ccw

                }
                else
                {
                    arcCode = 3; //G3 for cw
                }
            }

            if (penLift)
            {
                LiftPen();
            }

            if (!fullCircle)
            {

                lines.Add(new G_Code_Line(
                "(Drawing ARC with center " + centerPoint.ToString() +
                " from " + startPoint.ToString() +
                " to " + endPoint.ToString() +
                ")")); // Comments


            }

            else
            {
                lines.Add(new G_Code_Line(
                "(Drawing Circle with center " + centerPoint.ToString() +
                " with radius " + startPoint.DistanceTo(centerPoint) +
                ")")); // Comments
            }


            AddNextLinePoint(startPoint);


            LowerPen();

            G_Code_Line g_Code_Line;


            if (!fullCircle)
            {

                g_Code_Line = new G_Code_Line(arcCode); //G3/G2- Arc or Circle Move
                g_Code_Line.AddParameter("X", endPoint.X);
                g_Code_Line.AddParameter("Y", endPoint.Y);
                g_Code_Line.AddParameter("R", startPoint.DistanceTo(centerPoint));
                g_Code_Line.AddParameter("F", feedRateDOWN);

            }
            else
            {
                g_Code_Line = new G_Code_Line(arcCode); //G3 - Arc or Circle Move
                g_Code_Line.AddParameter("I", centerPoint.X - startPoint.X);
                g_Code_Line.AddParameter("J", centerPoint.Y - startPoint.Y);
                g_Code_Line.AddParameter("F", feedRateDOWN);

            }


            lines.Add(g_Code_Line);


        }

        public void AddDotMove(Rhino.Geometry.Point2d point)
        {
            if (isNewPointInLiftDistance(point.ToPoint3d()))
            {
                LiftPen();
            }
            AddNextLinePoint(point);
            LowerPen();
            LiftPen();
        }


        //G5 - Bézier cubic spline
        public void AddBezierMove(List<Rhino.Geometry.Point2d> curvePoints, bool penLift = true)
        {
            if (penLift)
            {
                LiftPen();
            }

            Rhino.Geometry.Point2d startPoint = curvePoints[0];
            Rhino.Geometry.Point2d endPoint = curvePoints[3];

            AddNextLinePoint(startPoint);

            lines.Add(new G_Code_Line(
                "(Drawing Bezier " +
                "from " + startPoint.ToString() +
                " to " + endPoint.ToString() +
                ")")); // Comments


            LowerPen();

            G_Code_Line g_Code_Line = new G_Code_Line((int)5); //G5 - Bézier cubic spline
            g_Code_Line.AddParameter("X", Math.Round(endPoint.X, decimalPrecision));
            g_Code_Line.AddParameter("Y", Math.Round(endPoint.Y, decimalPrecision));
            g_Code_Line.AddParameter("I", Math.Round(curvePoints[1].X - startPoint.X, decimalPrecision));
            g_Code_Line.AddParameter("J", Math.Round(curvePoints[1].Y - startPoint.Y, decimalPrecision));
            g_Code_Line.AddParameter("P", Math.Round(curvePoints[2].X - endPoint.X, decimalPrecision));
            g_Code_Line.AddParameter("Q", Math.Round(curvePoints[2].Y - endPoint.Y, decimalPrecision));
            g_Code_Line.AddParameter("F", feedRateDOWN / bezierSpeedDivider);


            lines.Add(g_Code_Line);


        }

        private void AddStartupCode()
        {


            lines.Add(new G_Code_Line(";G-Code from Grasshopper plugin version " + GetAssemblyFileVersion()));

            lines.Add(new G_Code_Line("G21 (Millimeter Units)")); //G21 - Millimeter Units
            //lines.Add(new G_Code_Line("G17 ; XY workspace")); //G17, G18, G19 - CNC Workspace Planes   G17 = xy
            //lines.Add(new G_Code_Line("G90 (Absolute Positioning)")); //G90 - Absolute Positioning
            //lines.Add(new G_Code_Line("G92  X 0 Y( Set Position)")); //G92 - Set Position
            //lines.Add(new G_Code_Line("M92  X 80 Y 80 Z 80 (set Axis Steps-per-unit)")); // M92 Set Axis Steps-per-unit
            //lines.Add(new G_Code_Line("M201  X 3000 Y 3000 Z 3000 (  Set Print Max Acceleration)")); // M201 - Set Print Max Acceleration
            //lines.Add(new G_Code_Line($"M203  X {feedRate} Y {feedRate} Z {feedRate} ( Set Max Feedrate)")); // M203 - Set Max Feedrate

            lines.Add(new G_Code_Line(";/ StartupCode END")); //

            lines.Add(new G_Code_Line(" ")); // Blank Line
            lines.Add(new G_Code_Line(" ")); // Blank Line

            lines.Add(new G_Code_Line("G1 Z" + penLiftHeight * 5 + " F" + feedRateZ + "(LIFT up 5x )")); // M0 go to location , , home.
            IsLifted = true;

            lines.Add(new G_Code_Line("")); // Blank Line
            lines.Add(new G_Code_Line("(----START DRAWING----)")); // Blank Line

        }

        public void AddShutdownCode()
        {

            lines.Add(new G_Code_Line("(----END DRAWING----)")); //
            lines.Add(new G_Code_Line(" ")); // Blank Line
                               
            lines.Add(new G_Code_Line("G1 Z" + penLiftHeight * 5 + " F" + feedRateZ + "(LIFT up 5x )")); // M0 go to location , , home.
            IsLifted = true;

            lines.Add(new G_Code_Line("G1 X0 Y0 F" + feedRateUP + "(Move Home)")); // M0 go to location , , home.
            //lines.Add(new G_Code_Line("M84 (Motors off)")); // M84 Motors off.
            lines.Add(new G_Code_Line(";END G-code"));

            //lines.Add(new G_Code_Line(" ")); // Blank Line

        }

        public String GetOutputGcodeAsText()
        {
            String rtnString = "";

            foreach (G_Code_Line l in lines)
            {
                rtnString += "\n" + l.ToString();
            }

            return rtnString;
        }

        public void AddLiftedMove(Rhino.Geometry.Point2d point)
        {
            LiftPen();
            AddNextLinePoint(point);
        }

        public List<Rhino.Geometry.Point3d> GetToolPath()
        {
            return toolPath;
        }
    }



}
