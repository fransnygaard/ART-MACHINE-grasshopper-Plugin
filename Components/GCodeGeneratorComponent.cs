using System;
using System.Collections.Generic;
using ART_MACHINE.Constants;
using Grasshopper.Kernel;
using Rhino.Geometry;
using ART_MACHINE.AM_ClassLib;
using Grasshopper.Kernel.Types;

namespace ART_MACHINE.Components
{
    public class GcodeGeneratorComponent : GH_GeneralClassLibrary.UI.GH_MyExtendableComponent
    {
        /// <summary>
        /// Initializes a new instance of the GcodeGeneratorSimplifiedComponent class.
        /// </summary>
        public GcodeGeneratorComponent()
          : base("ART+MACHINE: G-Code", "A+M G-Code",
              "Description",
              "ART+MACHINE", "Gcode")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            
            pManager.AddGeometryParameter(AM_Constants.InputGeo, "Geo", "", GH_ParamAccess.list);
            pManager.AddNumberParameter(AM_Constants.LineTolerance, "T", "Tolerance curve->polyline", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter(AM_Constants.LineAngleTolerance, "aT", "AngleTolerance in radians curve->polyline", GH_ParamAccess.item, 0.0); 
            pManager.AddIntegerParameter(AM_Constants.FeedratePenDown, "F_down", "The feedrate in XY when the pen i down", GH_ParamAccess.item, 5000);
            pManager.AddIntegerParameter(AM_Constants.FeedratePenUp, "F_up", "The feedrate in XY when the pen i up", GH_ParamAccess.item, 8000);
            pManager.AddIntegerParameter(AM_Constants.FeedRateZ, "F_Z", "The feedrate in Z", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter(AM_Constants.PenLiftHeigt, "Lift", "The lift distance to lift pen", GH_ParamAccess.item, 3.0f);
            pManager.AddNumberParameter(AM_Constants.PenLiftTolerance, "T", "If distance between points is below the tolerance, no pen lift is added.", GH_ParamAccess.item, 0.1);


            registrerInputParams(pManager);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddPointParameter(AM_Constants.ToolPath, "TP", "Tool Path", GH_ParamAccess.list);
            pManager.AddTextParameter(AM_Constants.Gcode, "G", "G-Code output string", GH_ParamAccess.item);
            pManager.AddBooleanParameter(AM_Constants.Run, "RUN", "", GH_ParamAccess.item); // ADD this as optional.

            registrerOutputParams(pManager);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //
            //INPUT Variables
            double lineTolerance = 0.1;
            double lineAngleTolerance = 0.1;
            double penLiftHeight = 2;
            double penLiftTolerance = 0.1;
            int feedRateUP = 1000;
            int feedRateDOWN = 1000;
            int feedRateZ = 1000;

            List<IGH_GeometricGoo> inputGeo = new List<IGH_GeometricGoo>();
            

            //OUTPUT Variables
            List<Point3d> toolPath = new List<Point3d>();


            //Handle Component inputs
            if (!DA.GetDataList<IGH_GeometricGoo>(inputParams[AM_Constants.InputGeo], inputGeo)) {return;}

            DA.GetData(inputParams[AM_Constants.LineTolerance], ref lineTolerance);
            DA.GetData(inputParams[AM_Constants.LineAngleTolerance], ref lineAngleTolerance);
            DA.GetData(inputParams[AM_Constants.FeedratePenDown], ref feedRateDOWN);
            DA.GetData(inputParams[AM_Constants.FeedratePenUp], ref feedRateUP);
            DA.GetData(inputParams[AM_Constants.FeedRateZ], ref feedRateZ);
            DA.GetData(inputParams[AM_Constants.PenLiftHeigt], ref penLiftHeight);
            DA.GetData(inputParams[AM_Constants.PenLiftTolerance], ref penLiftTolerance);




            //SOLVE!
            G_Code gcode = new G_Code(feedRateUP, feedRateDOWN, feedRateZ, penLiftHeight, 0, 50);
            gcode.penLiftToleranceSqr = penLiftTolerance * penLiftTolerance;
            double divideDistance = lineTolerance / 2;



            gcode.processAllInputs(inputGeo,lineTolerance, lineAngleTolerance);



            DA.SetDataList(outputParams[AM_Constants.ToolPath], gcode.GetToolPath());
            DA.SetData(outputParams[AM_Constants.Gcode], gcode.GetOutputGcodeAsText());



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
            get { return new Guid("6BC11FAE-ACE3-476A-AEA3-BBD316A2F837"); }
        }
    }
}