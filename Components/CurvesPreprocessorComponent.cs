using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Xsl;
using GH_GeneralClassLibrary.DataStructures;
using GH_GeneralClassLibrary.Utils;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ART_MACHINE.Components
{
    public class CurvesPreprocessorComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CurvesPreprocessorComponent class.
        /// </summary>
        public CurvesPreprocessorComponent()
          : base("CurvesPreprocessorComponent", "Nickname",
              "Description",
              "ART+MACHINE", "Gcode")

        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polylines", "PL", "", GH_ParamAccess.list);
            pManager.AddIntegerParameter("QuadTree Cell Capacity", "CC", "", GH_ParamAccess.item,150);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Polylines", "PL", "", GH_ParamAccess.list);
            pManager.AddRectangleParameter("Rect", "R", "", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<Curve> inputCurves = new List<Curve>();
            if (!DA.GetDataList<Curve>(0, inputCurves)) { return; }

            int QT_Capacity = 10;
            DA.GetData<int>(1, ref QT_Capacity);


            List<Polyline> polylines = new List<Polyline>();
            List<Point3d> points_ForBB = new List<Point3d>();
            foreach (Curve curve in inputCurves)
            {
                Polyline polyline = new Polyline();
                if (!curve.TryGetPolyline(out polyline)) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Not PL"); }
                polylines.Add(polyline);

                foreach (Point3d pl in polyline)
                {
                    points_ForBB.Add(pl);

                }

            }

            var boundingRectangle = new BoundingBox(points_ForBB).ToRectangle3D();

            var s = Rhino.Geometry.Transform.Scale(boundingRectangle.Center, 1.01);
            boundingRectangle.Transform(s);


            //CREATE THE QUADTREE
            QuadTree qt = new QuadTree(QT_Capacity, boundingRectangle);

            foreach (Curve c in inputCurves)
            {
                qt.AddDrawElement(c);
            }


            IEnumerable<QT_Cell> AllCells = qt.rootCell.TraverseNested(node => node.subCells);
            var bounds = QT_ClassExtentions.GetAllBoundaries(AllCells);
            DA.SetDataList(1, bounds);

            var drawElements = qt.GetDrawElementsInDrawOrder(new Point2d(0, 0));
            //var drawCurvesSorted = drawElements.Select(x => x.GetDrawCurveInCorrectDirection()).ToList();
            DA.SetDataList(0, drawElements.ToList());
        


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
            get { return new Guid("D9B2EB50-661C-4EC9-824A-E2001F4D7B06"); }
        }
    }
}