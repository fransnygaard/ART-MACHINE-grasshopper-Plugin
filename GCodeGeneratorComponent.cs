using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace ART_MACHINE
{
    public class GCodeGeneratorComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GCodeGeneratorComponent()
          : base("ART+MACHINE: G-Code Generator", "G-Code Generator",
              "Description",
              "ART+MACHINE", "Gcode-Generator")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddCurveParameter("Curve", "Crv", "Input curve", GH_ParamAccess.list);
            pManager.AddNumberParameter("LineRegenTolerance", "T", "Tolerance curve->lines", GH_ParamAccess.item, 0.1);
            pManager.AddIntegerParameter("FeedratePenDown", "F_down", "The feedrate in XY when the pen i down", GH_ParamAccess.item,5000);
            pManager.AddIntegerParameter("FeedratePenUp", "F_up", "The feedrate in XY when the pen i up", GH_ParamAccess.item,8000);
            pManager.AddNumberParameter("PenLiftHeigt", "Lift", "The lift distance to lift pen", GH_ParamAccess.item,3.0f);
            pManager.AddIntegerParameter("FeedRate Z", "F_Z", "The feedrate in Z", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("PenLiftTolerance", "T", "Tolerance for lifting pen between lines.", GH_ParamAccess.item, 0.1);
            pManager.AddBooleanParameter("ArcSupport", "A", "Set to true if controller have support for arc (G2/G3)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("BezierSupport(BETA)", "B", "Set to true if controller have support for bezier (G5)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Sort Lines", "S", "True sorts the lines to minimize pen up movemnets, False draws them in the default order", GH_ParamAccess.item, true);


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
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<Rhino.Geometry.Point3d> debug = new List<Point3d>();
            List<String> debugStr = new List<string>();
            debugStr.Add("v 0.4.02");

            //SET THIS AS INPUTS LATER
            Double simplifyTolerance = 2 ;
            Double divideDistance = simplifyTolerance/ 2;
            Double penLiftHeight = 2;
            Double penLiftTolerance = 0.1;
            int feedRateUP = 1000;
            int feedRateDOWN = 1000;
            int feedRateZ = 1000;
            bool arcSupport = false;
            bool bezierSupport = false;
            bool sortLines = true;

            //OUTPUTS
            List<Point3d> toolPath = new List<Point3d>();

            List<Rhino.Geometry.Curve> inCrv = new List<Curve>();
            DA.GetDataList(0, inCrv);

            DA.GetData(1, ref simplifyTolerance);
            DA.GetData(2, ref feedRateDOWN);
            DA.GetData(3, ref feedRateUP);
            DA.GetData(4, ref penLiftHeight);
            DA.GetData(5, ref feedRateZ);
            DA.GetData(6, ref penLiftTolerance);
            DA.GetData(7, ref arcSupport);
            DA.GetData(8, ref bezierSupport);
            DA.GetData(9, ref sortLines);



            //INPUTS
            if (inCrv.Count == 0)
                return;

            //sort shapes
            //implement laters

            //add to shapesList
            List<Shape2D> shapesToDraw = new List<Shape2D>();
            debugStr.Add("Identifying shapes....");
            foreach (Rhino.Geometry.Curve c in inCrv)
            {
                if (c == null)
                {
                    break;
                }


                else if (c.IsArc() && arcSupport)
                {
                    debugStr.Add("   Found Arc");
                    Rhino.Geometry.Point3d[] _tempArray = new Rhino.Geometry.Point3d[2] { c.PointAtNormalizedLength(0), c.PointAtNormalizedLength(1) };
                    Shape2D new_shape2d = new Shape2D(_tempArray, Shape2D.Shape2DTypes.arc, c);
                    shapesToDraw.Add(new_shape2d);
                }

                //    else if (c.IsPolyline())
                //    {
                //        debugStr.Add("   Found Polyline");

                //        //Rhino.Geometry.Point3d[] _tempArray = new Rhino.Geometry.Point3d[2] { c.PointAtNormalizedLength(0), c.PointAtNormalizedLength(1) };
                //        Rhino.Geometry.Point3d[] _tempArray = (Rhino.Geometry.Polyline)c.
                //        Shape2D new_shape2d = new Shape2D(_tempArray, Shape2D.Shape2DTypes.polyline, c);
                //        shapesToDraw.Add(new_shape2d);
                //    }
                //    else if (c.IsLinear())
                //    {
                //        debugStr.Add("   Found Linear");
                //        Rhino.Geometry.Point3d[] _tempArray = new Rhino.Geometry.Point3d[2] { c.PointAtNormalizedLength(0), c.PointAtNormalizedLength(1) };
                //        Shape2D new_shape2d = new Shape2D(_tempArray, Shape2D.Shape2DTypes.line, c);
                //        shapesToDraw.Add(new_shape2d);
                //    }

                else
                {


                    Rhino.Geometry.Point3d[] _tempArray;
                    var _ = c.DivideByLength(divideDistance, true, out _tempArray);
                    _tempArray[_tempArray.Length - 1] = c.PointAtEnd;

                    Shape2D new_shape2d = new Shape2D(_tempArray, Shape2D.Shape2DTypes.polyline, c);
                    new_shape2d.SimplefyWithDouglasPeuckerReduction(simplifyTolerance);

                    //foreach (Point2d p in new_shape2d.getPointList())
                    //debug.Add(new Point3d(p.X,p.Y,0));

                    shapesToDraw.Add(new_shape2d);
                }


                //else if(false)
                //{
                //    try
                //    {
                //        if(!bezierSupport)
                //        {
                //            throw new Exception("No BezierSupport");
                //        }
                //        BezierCurve[] beziers = Rhino.Geometry.BezierCurve.CreateCubicBeziers(c, simplifyTolerance, simplifyTolerance);
                //        foreach (BezierCurve b in beziers){
                //            if(b.ControlVertexCount !=4)
                //            {
                //                debugStr.Add("   ControlVertexCount = " + b.ControlVertexCount);
                //                throw new Exception("ControlVertexCount != 2");
                //            }
                //            Rhino.Geometry.Point2d[] _tempArray = new Rhino.Geometry.Point2d[4] { b.GetControlVertex2d(0), b.GetControlVertex2d(1), b.GetControlVertex2d(2), b.GetControlVertex2d(3) };
                //            Shape2D new_shape2d = new Shape2D(_tempArray, Shape2D.Shape2DTypes.bezier, c);
                //            shapesToDraw.Add(new_shape2d);
                //        }

                //    }

                //    catch (Exception e)
                //    {
                //        debugStr.Add( "Error " + e.ToString() + " fall back on SimplefyWithDouglasPeuckerReduction");


                //        Rhino.Geometry.Point3d[] _tempArray;
                //        var _ = c.DivideByLength(divideDistance, true, out _tempArray);
                //        _tempArray[_tempArray.Length - 1] = c.PointAtEnd;

                //        Shape2D new_shape2d = new Shape2D(_tempArray, Shape2D.Shape2DTypes.polyline, c);
                //        new_shape2d.SimplefyWithDouglasPeuckerReduction(simplifyTolerance);

                //        //foreach (Point2d p in new_shape2d.getPointList())
                //        //debug.Add(new Point3d(p.X,p.Y,0));

                //        shapesToDraw.Add(new_shape2d);
                //    }




                //}
            }

            debugStr.Add("Done identifying shapes.");



            //Make a G code obj
            G_Code gcode = new G_Code(feedRateUP,feedRateDOWN, feedRateZ, penLiftHeight, 0);

            //variable to hold last point , start at 0,0,0;
            Rhino.Geometry.Point2d lastPoint = new Point2d(0, 0);
            Int32 indexOfNextShape = 0;
            Double distanceToNextPoint = Double.MaxValue;
            bool nextReverse = false;

            debugStr.Add("Sorting shapes...");

            while (true)
            {
                if (shapesToDraw.Count == 0)
                    break;

                distanceToNextPoint = Double.MaxValue; //Reset search distace


                if(sortLines)
                {

                    //Search for closest point
                    foreach (Shape2D s in shapesToDraw)
                    {
                        bool reverse;

                        Double distStart = lastPoint.DistanceTo(s.startPoint());
                        Double distEnd = lastPoint.DistanceTo(s.endPoint());
                        Double shortesDist;

                        if (distEnd < distStart)
                        {
                            shortesDist = distEnd;
                            reverse = true;
                        }
                        else
                        {
                            shortesDist = distStart;
                            reverse = false;
                        }

                        if (shortesDist < distanceToNextPoint) // if this is the best candidate
                        {
                            distanceToNextPoint = shortesDist;
                            nextReverse = reverse;
                            indexOfNextShape = shapesToDraw.IndexOf(s);
                        }

                        if(distanceToNextPoint == 0)
                            break;
                    }
                }
                else
                {
                    nextReverse = false;
                    indexOfNextShape = 0;
                    distanceToNextPoint  = lastPoint.DistanceTo(shapesToDraw[0].startPoint());
                }

                if(sortLines)
                    debugStr.Add("adding closest shape at index [" + indexOfNextShape  +"] distance " + distanceToNextPoint);
                else
                    debugStr.Add("adding next shape at index [" + indexOfNextShape + "] distance " + distanceToNextPoint);


                //add the best candidate to gCode   -- then remove from shapesToDrawList
                bool lift = distanceToNextPoint > penLiftTolerance ? true : false;

                if (shapesToDraw[indexOfNextShape].type == Shape2D.Shape2DTypes.polyline)
                {
                    List<Point2d> pointsToDraw = shapesToDraw[indexOfNextShape].getPointList(nextReverse);
                    debugStr.Add("      AddNextLineSegment with" + pointsToDraw.Count + " points");
                    lastPoint = pointsToDraw[pointsToDraw.Count - 1];
                    gcode.AddNextLineSegment(pointsToDraw, lift);

                }
                else if (shapesToDraw[indexOfNextShape].type == Shape2D.Shape2DTypes.line)
                {
                    debugStr.Add("      adding LineMove");
                    List<Point2d> pointsToDraw = shapesToDraw[indexOfNextShape].getPointList(nextReverse);
                    debugStr.Add("      AddNextLineSegment with" + pointsToDraw.Count + " points");
                    lastPoint = pointsToDraw[pointsToDraw.Count - 1];
                    gcode.AddNextLineSegment(pointsToDraw, lift);

                }
                else if(shapesToDraw[indexOfNextShape].type == Shape2D.Shape2DTypes.arc)
                {
                    debugStr.Add("      adding arcMove");

                    List<Point2d> pointsToDraw = shapesToDraw[indexOfNextShape].getPointList(nextReverse);
                    lastPoint = pointsToDraw[pointsToDraw.Count - 1];
                    Rhino.Geometry.ArcCurve arccurve = (Rhino.Geometry.ArcCurve)shapesToDraw[indexOfNextShape].curve;
                    Rhino.Geometry.Arc arc;
                    arccurve.TryGetArc(out arc);
                    gcode.AddArcMove(new Rhino.Geometry.Point2d(arc.Center.X,arc.Center.Y), pointsToDraw[0], pointsToDraw[1], lift);

                }
                else if (shapesToDraw[indexOfNextShape].type == Shape2D.Shape2DTypes.bezier)
                {
                    debugStr.Add("      adding bezierMove");

                    List<Point2d> pointsToDraw = shapesToDraw[indexOfNextShape].getPointList(nextReverse);
                    lastPoint = pointsToDraw[pointsToDraw.Count - 1];
                    gcode.AddBezierMove(pointsToDraw,lift);

                }

                //Remove the shape from the queue
                shapesToDraw.RemoveAt(indexOfNextShape);

            }
            debugStr.Add("Done sorting shapes...");
            gcode.AddShutdownCode();


            //SET OUTPUTS
            DA.SetDataList(0, debug);
            DA.SetDataList(1, debugStr);
            DA.SetDataList(2, gcode.GetToolPath());
            DA.SetData(3, gcode.OutputText());




        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("644c2009-3a07-4078-be5e-f9fef19fa429"); }
        }
    }
}
