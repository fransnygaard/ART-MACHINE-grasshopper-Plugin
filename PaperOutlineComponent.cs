using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ART_MACHINE
{
    public class PaperOutlineComponent : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the PaperSizes class.
        /// </summary>
        public PaperOutlineComponent()
          : base("Paper Outline", "Paper",
              "Return the outline for a given paper type Only A-type paper is implementet",
              "ART+MACHINE", "Tools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Paper Size", "S", "Name of paper e.g. 'A4'", GH_ParamAccess.item, "A3");
            pManager.AddBooleanParameter("Landscape", "L", "True for landsacape, False for Portrait", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Margins", "M", "Margins offset from ountline", GH_ParamAccess.item, 0.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Paper Outline", "P", "Outline curve of paper", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string paperName = "A3";
            bool landscape = true;
            DA.GetData(0, ref paperName);
            DA.GetData(1, ref landscape);

            int x = -1, y = -1;



            switch (paperName)
            {
                case "A0":
                    x = 841;
                    y = 1189;
                    break;

                case "A1":
                    x = 594;
                    y = 841;
                    break;

                case "A2":
                    x = 420;
                    y = 594;
                    break;

                case "A3":
                    x = 297;
                    y = 420;
                    break;

                case "A4":
                    x = 210;
                    y = 297;
                    break;

                case "A5":
                    x = 148;
                    y = 210;
                    break;
                case "A6":
                    x = 105;
                    y = 148;
                    break;

                case "A7":
                    x = 74;
                    y = 105;
                    break;

                case "A8":
                    x = 52;
                    y = 74;
                    break;

                default:
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Paper Type not found, remember to capitolize letter. e.g A4 not a4");
                    return;
            }

            if (landscape)
            {
                int temp = x;
                x = y;
                y = temp;

            }

            Polyline p = new Polyline();
            p.Add(0, 0, 0);
            p.Add(0, y, 0);
            p.Add(x, y, 0);
            p.Add(x, 0, 0);
            p.Add(0, 0, 0);



            DA.SetData(0,p);

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
            get { return new Guid("51039D63-BF22-40B1-B783-2345A1CC4D56"); }
        }
    }
}