using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace ART_MACHINE
{
    public class GcodeGenerator_from_3D_SlicedCurves_OBSOLETE : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GcodeGenerator_from_3D_SlicedCurves class.
        /// </summary>
        public GcodeGenerator_from_3D_SlicedCurves_OBSOLETE()
          : base("ART + MACHINE: G - Code extruder 3d(STILL IN DEVELOPMENT)", "G-Code Extruder(STILL IN DEVELOPMENT)",
              "Generates Gcode for 3d extuder",
              "ART+MACHINE", "UNDER DEVELOPMENT")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "Curves", "Curves representing sliced toolpaths", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Feedrate", "feedrate", "The feedrate", GH_ParamAccess.item, 5000);

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

            //Variables
            List<Rhino.Geometry.Point3d> debug = new List<Point3d>();
            List<String> debugStr = new List<string>();


            List<Rhino.Geometry.Curve> curves = new List<Rhino.Geometry.Curve>();
            int feedrate = 1000;
            //OUTPUTS

            //REGISTRER INPUTS
            if (!DA.GetDataList(0, curves)) return;

            DA.GetData(1, ref feedrate);


            //Make a G code obj   /this adds startup code

            GCode_3d gcode = new GCode_3d(feedrate, feedrate);



            /////////////////////////////////////////////////////////
            //////////////// GENERATE GCODE ////////////////////////
            ///////////////////////////////////////////////////////
            Polyline pline;
            foreach (Rhino.Geometry.Curve c in curves)
            {
                if (c == null)
                {
                    break;
                }

                else if (c.IsLinear() && true)
                {
                    List<Point3d> _tempList = new List<Point3d>();

                    _tempList.Add(c.PointAtNormalizedLength(0));
                    _tempList.Add(c.PointAtNormalizedLength(1));
                    gcode.AddNextLineSegment(_tempList, true);
                }

                else if (c.IsPolyline() && c.TryGetPolyline(out pline) && true)
                {
                    List<Point3d> _tempList = new List<Point3d>();
                    for (int i = 0; i < pline.Count; i++)
                    {
                        _tempList.Add(pline[i]);
                    }
                    gcode.AddNextLineSegment(_tempList, true);
                }
            }


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
            get { return new Guid("DF3EC705-E4DE-4F92-94CC-CC98133C733C"); }
        }
    }
}