using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Frixel.Rhinoceros.Extensions;

namespace Frixel.Rhinoceros
{
    public class FrixelRhinoCommand : Command
    {
        public FrixelRhinoCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
            Frixel.UI.MainWindow.ReferenceFromRhino += MainWindow_ReferenceFromRhino;
        }

        private UI.FrixelReferenceData MainWindow_ReferenceFromRhino(double xSize, double ySize)
        {
            // Tell user to select objects from doc
            var go = new GetObject();
            go.SetCommandPrompt("Select a closed curve");
            go.GeometryFilter = ObjectType.Curve;
            go.GeometryAttributeFilter = GeometryAttributeFilter.ClosedCurve;
            var curves = go.Objects().Select(o => o.Curve());
            if (curves.Count() == 0) { return null; }
            var curve = curves.First();

            // Create a mesh from the closed curve
            //var patch = Brep.CreatePlanarBreps(curve);
            //if (patch == null | patch.Count() == 0) { return null; }
            //var patchMesh = Mesh.CreateFromBrep(patch[0]);            

            // Generate an array of points
            var bb = curve.GetBoundingBox(true);
            var corns = bb.GetCorners();
            #region Bbox corner order 
            // Remarks:
            //     [0] Min.X, Min.Y, Min.Z
            //     [1] Max.X, Min.Y, Min.Z
            //     [2] Max.X, Max.Y, Min.Z
            //     [3] Min.X, Max.Y, Min.Z
            //     [4] Min.X, Min.Y, Max.Z
            //     [5] Max.X, Min.Y, Max.Z
            //     [6] Max.X, Max.Y, Max.Z
            //     [7] Min.X, Max.Y, Max.Z
            #endregion
            var xDim = corns[0].DistanceTo(corns[1]);
            var yDim = corns[0].DistanceTo(corns[3]);
            var startCorner = corns[0];

            // If its too small fuck off
            if(xSize*2 > xDim | ySize*2 > yDim) { return null; }

            // Get the dimensions of the massing bbox
            var xNumber = Math.Floor(xDim / xSize);
            var yNumber = Math.Floor(yDim / ySize);
            var xSpacing = xDim / xNumber;
            var ySpacing = yDim / xNumber;

            // Generate a point array
            Dictionary<Tuple<int, int>, Core.Geometry.Point2d> points = new Dictionary<Tuple<int, int>, Core.Geometry.Point2d>();
            for(int x = 0; x<xNumber; x++)
            {
                for(int y =0; y<yNumber; y++)
                {
                    var p = new Point3d(x * xSpacing, y * ySpacing, 0);
                    points.Add(
                        new Tuple<int, int>(x,y),
                        p.ToFrixelPoint(Utilities.PointIsInsideOrOnCurve(curve, p, 0.01))
                    );
                }
            }

            // Crawl across the point array
            for(int x = 0; x<xNumber; x++)
            {
                for(int y = 0; y<yNumber; y++)
                {

                }
            }


            //PointIsInsideOrOnCurve()

        }

        ///<summary>The only instance of this command.</summary>
        public static FrixelRhinoCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "Frixel"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            #region Default command 
            // TODO: start here modifying the behaviour of your command.
            // ---
            //RhinoApp.WriteLine("The {0} command will add a line right now.", EnglishName);

            //Point3d pt0;
            //using (GetPoint getPointAction = new GetPoint())
            //{
            //    getPointAction.SetCommandPrompt("Please select the start point");
            //    if (getPointAction.Get() != GetResult.Point)
            //    {
            //        RhinoApp.WriteLine("No start point was selected.");
            //        return getPointAction.CommandResult();
            //    }
            //    pt0 = getPointAction.Point();
            //}

            //Point3d pt1;
            //using (GetPoint getPointAction = new GetPoint())
            //{
            //    getPointAction.SetCommandPrompt("Please select the end point");
            //    getPointAction.SetBasePoint(pt0, true);
            //    getPointAction.DynamicDraw +=
            //      (sender, e) => e.Display.DrawLine(pt0, e.CurrentPoint, System.Drawing.Color.DarkRed);
            //    if (getPointAction.Get() != GetResult.Point)
            //    {
            //        RhinoApp.WriteLine("No end point was selected.");
            //        return getPointAction.CommandResult();
            //    }
            //    pt1 = getPointAction.Point();
            //}

            //doc.Objects.AddLine(pt0, pt1);
            //doc.Views.Redraw();
            //RhinoApp.WriteLine("The {0} command added one line to the document.", EnglishName);

            // ---
            #endregion

            RhinoApp.WriteLine("Launching Frixel Window", EnglishName);
            // Launch a FrixelWindow with a test structure
            var window = new Frixel.UI.MainWindow(Core.Test.TestObjects.TestStructure);

            if ((bool)window.ShowDialog())
            {
                RhinoApp.WriteLine("Operation Sucessful", EnglishName);
            }

            return Result.Success;
        }
    }
}
