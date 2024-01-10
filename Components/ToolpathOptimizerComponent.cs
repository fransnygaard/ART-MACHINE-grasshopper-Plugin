using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Xsl;
using GH_GeneralClassLibrary.DataStructures;
using GH_GeneralClassLibrary.Utils;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ART_MACHINE.Components
{
    public class ToolpathOptimizerComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CurvesPreprocessorComponent_v2_v2 class.
        /// </summary>
        public ToolpathOptimizerComponent()
          : base("ART+MACHINE: Toolpath optimizer ", "Toolpath",
              "Description",
              "ART+MACHINE", "Tools")

        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("InputGeomerty", "Geo", "", GH_ParamAccess.list);
            pManager.AddIntegerParameter("QuadTree Cell Capacity", "CC", "", GH_ParamAccess.item,150);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("OutputGeomerty", "Geo_Out", "", GH_ParamAccess.list);
            pManager.AddRectangleParameter("QuadTree", "QT", "", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<IGH_GeometricGoo> inputGeo = new List<IGH_GeometricGoo>();
            if (!DA.GetDataList<IGH_GeometricGoo>(0, inputGeo)) { return; }

            int QT_Capacity = 10;
            DA.GetData<int>(1, ref QT_Capacity);


            //List<Polyline> polylines = new List<Polyline>();
            List<Point3d> points_ForBB = new List<Point3d>();



            foreach (IGH_GeometricGoo goo in inputGeo)
            {
                if (goo == null) continue;

                Point3d point;
                Curve c = goo.ToCurve();

                if (c != null)
                {
                    //Curve c = GHc.Value;
                    points_ForBB.Add(c.PointAtStart);
                    points_ForBB.Add(c.PointAtEnd);
                }
                else if (goo.CastTo<Point3d>(out point))
                {
                    points_ForBB.Add(point);
                }

            }


            //CREATE BOUNDING RECT  AND SCALE by 2% So no rounding errors puts points outside.
            var boundingRectangle = new BoundingBox(points_ForBB).ToRectangle3D();
            var s = Transform.Scale(boundingRectangle.Center, 1.02);
            boundingRectangle.Transform(s);


            //CREATE THE QUADTREE
            QuadTree qt = new QuadTree(QT_Capacity, boundingRectangle);

            foreach (IGH_GeometricGoo goo in inputGeo)
            {

                qt.AddDrawElement(goo);
            }


            IEnumerable<QT_Cell> AllCells = qt.rootCell.TraverseNested(node => node.subCells);
            var bounds = QT_ClassExtentions.GetAllBoundaries(AllCells);
            DA.SetDataList(1, bounds.ToList());

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
            get { return new Guid("bb8f099a-d84f-4ba6-843a-6513788b0e02"); }
        }
    }
}