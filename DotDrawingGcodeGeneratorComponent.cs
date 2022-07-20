using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ART_MACHINE
{
    public class DotDrawingGcodeGeneratorComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DotDrawingGcodeGeneratorComponent class.
        /// </summary>
        public DotDrawingGcodeGeneratorComponent()
          : base("ART+MACHINE: G-Code DOTS", "G-Code DOTS",
              "Description",
              "ART+MACHINE", "Gcode")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddPointParameter("Points", "Crv", "Input points", GH_ParamAccess.list);
            // pManager.AddNumberParameter("LineRegenTolerance", "T", "Tolerance curve->lines", GH_ParamAccess.item, 0.1);
            // pManager.AddIntegerParameter("FeedratePenDown", "F_down", "The feedrate in XY when the pen i down", GH_ParamAccess.item, 5000);
            pManager.AddIntegerParameter("FeedratePenUp", "F_up", "The feedrate in XY when the pen i up", GH_ParamAccess.item, 8000);
            pManager.AddNumberParameter("PenLiftHeigt", "Lift", "The lift distance to lift pen", GH_ParamAccess.item, 3.0f);
            pManager.AddIntegerParameter("FeedRate Z", "F_Z", "The feedrate in Z", GH_ParamAccess.item, 1000);
            // pManager.AddNumberParameter("PenLiftTolerance", "T", "Tolerance for lifting pen between lines.", GH_ParamAccess.item, 0.1);
            // pManager.AddBooleanParameter("ArcSupport(BETA)", "A", "Set to true if controller have support for arc (G2/G3)", GH_ParamAccess.item, false);
            // pManager.AddBooleanParameter("BezierSupport(NOT READY)", "B", "Set to true if controller have support for bezier (G5)", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Sort dots Mode(Not fully implementet)", "S", "0-No sort ,1 sort closest 2-sort by X, 3- sort by Y", GH_ParamAccess.item, 1);


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Debug", "Debug", "Debug", GH_ParamAccess.list);

            pManager.AddTextParameter("DebugStr", "DebugStr", "DebugStr", GH_ParamAccess.list);

            pManager.AddPointParameter("ToolPath", "TP", "Tool Path", GH_ParamAccess.list);

            pManager.AddTextParameter("G-Code", "G", "G-Code output string", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Rhino.Geometry.Point3d> debug = new List<Point3d>();
            List<String> debugStr = new List<string>();

            //INPUTS
            Double penLiftHeight = 2;
            int feedRateUP = 1000;
            int feedRateDOWN = 1000;
            int feedRateZ = 1000;
            int sortPoints = 1;

            //OUTPUTS
            List<Point3d> toolPath = new List<Point3d>();

            List<Rhino.Geometry.Point3d> inPoints = new List<Point3d>();
            List<Point2d> sortedPoints = new List<Point2d>();


            DA.GetDataList(0, inPoints);

            DA.GetData(1, ref feedRateUP);
            DA.GetData(2, ref penLiftHeight);
            DA.GetData(3, ref feedRateZ);
            DA.GetData(4, ref sortPoints);


            if (inPoints.Count == 0)
                return;

            G_Code gcode = new G_Code(feedRateUP, feedRateDOWN, feedRateZ, penLiftHeight, 0);


            List<Point2d> pointsToSort = new List<Point2d>();
            switch (sortPoints)
            {
                case 1: //Sort Closest

                    foreach (Point3d point in inPoints)
                    {
                        pointsToSort.Add(new Point2d(point.X, point.Y));
                    }
                    Point2d lastPoint = new Point2d(0, 0);

                    while (pointsToSort.Count > 0)
                    {
                        int nextPointIndex = 0;
                        Double nextPointDistance = double.MaxValue;
                        foreach (Point2d _p in pointsToSort)
                        {
                            double distanceTo_p = lastPoint.DistanceTo(_p);
                            if (distanceTo_p < nextPointDistance)
                            {
                                nextPointDistance = distanceTo_p;
                                nextPointIndex = pointsToSort.IndexOf(_p);

                            }
                        }

                        sortedPoints.Add(pointsToSort[nextPointIndex]);
                        pointsToSort.RemoveAt(nextPointIndex);
                    }


                    break;


                case 2: // Sort X

                    foreach (Point3d point in inPoints)
                    {
                        pointsToSort.Add(new Point2d(point.X, point.Y));
                    }

                    while (pointsToSort.Count > 0)
                    {
                        int nextPointIndex = 0;
                        Double nextPointValue = double.MaxValue;
                        foreach (Point2d _p in pointsToSort)
                        {
                            double valueOf_p = _p.X;
                            if (valueOf_p < nextPointValue)
                            {
                                nextPointValue = valueOf_p;
                                nextPointIndex = pointsToSort.IndexOf(_p);

                            }
                        }

                        sortedPoints.Add(pointsToSort[nextPointIndex]);
                        pointsToSort.RemoveAt(nextPointIndex);
                    }


                    break;

                case 3: // Sort Y

                    foreach (Point3d point in inPoints)
                    {
                        pointsToSort.Add(new Point2d(point.X, point.Y));
                    }

                    while (pointsToSort.Count > 0)
                    {
                        int nextPointIndex = 0;
                        Double nextPointValue = double.MaxValue;
                        foreach (Point2d _p in pointsToSort)
                        {
                            double valueOf_p = _p.Y;
                            if (valueOf_p < nextPointValue)
                            {
                                nextPointValue = valueOf_p;
                                nextPointIndex = pointsToSort.IndexOf(_p);

                            }
                        }

                        sortedPoints.Add(pointsToSort[nextPointIndex]);
                        pointsToSort.RemoveAt(nextPointIndex);
                    }


                    break;

                default:
                    foreach (Point3d point in inPoints)
                    {
                        sortedPoints.Add(new Point2d(point.X, point.Y));
                    }
                    break;
            }



            foreach (Point2d p in sortedPoints)
            {

                gcode.AddDotMove(p);
            }

            gcode.AddShutdownCode();


            //SET OUTPUTS
            DA.SetDataList(0, debug);
            DA.SetDataList(1, debugStr);
            DA.SetDataList(2, gcode.GetToolPath());
            DA.SetData(3, gcode.OutputText());



        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("D3F52E1E-822B-42B6-976B-E743F3C1D3F9"); }
        }
    }
}