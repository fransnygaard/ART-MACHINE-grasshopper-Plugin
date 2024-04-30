using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.IO;

namespace ART_MACHINE
{
    public class WriteTextToFileComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the WriteTextToFileComponent class.
        /// </summary>
        public WriteTextToFileComponent()
          : base("Write text to file", "Write File",
              "Description",
              "ART+MACHINE", "Tools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text to write", "TXT", "Writes a new line for each item in list", GH_ParamAccess.list);
            pManager.AddTextParameter("Folder", "F", "Folder to write file to", GH_ParamAccess.item);
            pManager.AddTextParameter("FileName", "N", "File name to write to", GH_ParamAccess.item);
            pManager.AddTextParameter("FileType", "T", "Filetype / file ending", GH_ParamAccess.item, "gcode");
            pManager.AddBooleanParameter("Run", "R", "True to write", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "S", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string fullPath = "";
            string _folder = "";
            string _fileName = "";
            string _fileType = "gcode";
            bool _run = false;



            List<string> linesToWrite = new List<string>();

            List<string> rtnStatus = new List<string>();

            DA.GetDataList(0, linesToWrite);
            DA.GetData(1, ref _folder);
            DA.GetData(2, ref _fileName);
            DA.GetData(3, ref _fileType);
            DA.GetData(4, ref _run);

            if(!_run) { return; }

            fullPath = _folder + _fileName + "." + _fileType;
           // ((Action)(() => { }))();

            File.WriteAllLines(fullPath, linesToWrite.ToArray());

            rtnStatus.Add("Written lines to:");
            rtnStatus.Add(fullPath);



            DA.SetDataList(0, rtnStatus);

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
            get { return new Guid("C0D4BF45-1299-4659-B122-46358F73EC62"); }
        }
    }
}