using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace ART_MACHINE
{
    public class G_Code_GeneratorInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "ARTMACHINE";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "ART...MACHINE penplotter companion plugin. Used to generate gcode from rhino geomerty";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("69590aeb-9583-491b-915d-f8ff3f276f25");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Frans Nygaard";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "fransnygaard.com";
            }
        }
    }
}
