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
using Frixel.Core;

namespace Frixel.Rhinoceros
{
    public class FrixelRhinoCommand : Command
    {
        private bool _handlerIsAttached = false;
        private Frixel.UI.MainWindow _window;
        private RhinoDoc _doc;

        private double _xSize;
        private double _ySize;
        private Curve _lastReffedCurve;
        private Tuple<Point3d, Point3d> _spine;

        public FrixelRhinoCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        private Tuple<Point3d,Point3d> GetSpine()
        {
            // Get user input for spine
            bool unsucessfulGetSpine = false;
            Point3d pt0 = new Point3d(0, 0, 0);
            using (GetPoint getPointAction = new GetPoint())
            {
                getPointAction.SetCommandPrompt("Spine: start point");
                if (getPointAction.Get() != GetResult.Point)
                {
                    unsucessfulGetSpine = true;
                    //RhinoApp.WriteLine("No start point was selected.");
                    //return getPointAction.CommandResult();
                }
                if (!unsucessfulGetSpine) { pt0 = getPointAction.Point(); }

            }

            Point3d pt1 = new Point3d(0, 0, 0);
            using (GetPoint getPointAction = new GetPoint())
            {
                getPointAction.SetCommandPrompt("Spine: end point");
                getPointAction.SetBasePoint(pt0, true);
                getPointAction.DynamicDraw +=
                  (sender, e) => e.Display.DrawLine(pt0, e.CurrentPoint, System.Drawing.Color.DarkRed);
                if (getPointAction.Get() != GetResult.Point)
                {
                    unsucessfulGetSpine = true;
                    //RhinoApp.WriteLine("No end point was selected.");
                    //return getPointAction.CommandResult();
                }
                if (!unsucessfulGetSpine) { pt1 = getPointAction.Point(); }
            }
            if (unsucessfulGetSpine) { return null; }
            return new Tuple<Point3d, Point3d>(pt0, pt1);
        }

        private UI.FrixelReferenceData Regenerate()
        {
            // Generate an array of points
            var bb = _lastReffedCurve.GetBoundingBox(true);
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

            // TEST
            //bb.GetCorners().ToList().ForEach(p => _doc.Objects.AddPoint(p));

            // Create a 2d domain bbox 
            Core.Domain2d Boundingbox = new Domain2d(new Core.Domain(corns[0].X, corns[1].X), new Core.Domain(corns[0].Y, corns[3].Y));

            // If its too small fuck off
            if (_xSize * 2 > xDim | _ySize * 2 > yDim) { return null; }

            // Get the dimensions of the massing bbox
            var xNumber = Math.Ceiling(xDim / _xSize);
            var yNumber = Math.Ceiling(yDim / _ySize);
            var xSpacing = xDim / xNumber;
            var ySpacing = yDim / yNumber;

            List<Point3d> nodePointTests = new List<Point3d>();
            List<Point3d> nodePoints = new List<Point3d>();

            // Generate a point array
            Dictionary<Tuple<int, int>, Core.Geometry.Point2d> nodeDictionary = new Dictionary<Tuple<int, int>, Core.Geometry.Point2d>();
            for (int x = 0; x <= xNumber; x++)
            {
                for (int y = 0; y <= yNumber; y++)
                {
                    var p = new Point3d((x * xSpacing) + startCorner.X, (y * ySpacing) + startCorner.Y, 0);
                    nodePoints.Add(p);
                    nodeDictionary.Add(
                        new Tuple<int, int>(x, y),
                        p.ToFrixelPoint(Utilities.PointIsInsideOrOnCurve(_lastReffedCurve, p, 0.01)));
                    if (Utilities.PointIsInsideOrOnCurve(_lastReffedCurve, p, 0.01)) { nodePointTests.Add(p); }
                }
            }

            // Crawl across the point array and crete pixels
            var nodeList = nodeDictionary.Values.ToList();
            List<Pixel> pixelList = new List<Pixel>();
            for (int x = 0; x < xNumber; x++)
            {
                for (int y = 0; y < yNumber; y++)
                {
                    if (!nodeDictionary.ContainsKey(new Tuple<int, int>(x, y))) continue;
                    var currentPoint = nodeDictionary[new Tuple<int, int>(x, y)];
                    var topLeft = new Tuple<int, int>(x, y + 1);
                    var topRight = new Tuple<int, int>(x + 1, y + 1);
                    var botLeft = new Tuple<int, int>(x, y);
                    var botRight = new Tuple<int, int>(x + 1, y);
                    Core.Geometry.Point2d topLeftPt;
                    Core.Geometry.Point2d topRightPt;
                    Core.Geometry.Point2d botLeftPt;
                    Core.Geometry.Point2d botRightPt;

                    if (
                        nodeDictionary.TryGetValue(botLeft, out botLeftPt) &&
                        nodeDictionary.TryGetValue(topLeft, out topLeftPt) &&
                        nodeDictionary.TryGetValue(topRight, out topRightPt) &&
                        nodeDictionary.TryGetValue(botRight, out botRightPt) &&
                        Utilities.AtLeastXInside(topLeftPt.IsInside, topRightPt.IsInside, botLeftPt.IsInside, botRightPt.IsInside, 2)
                        )
                    {
                        pixelList.Add(new Pixel(
                            nodeList.IndexOf(topLeftPt),
                            nodeList.IndexOf(topRightPt),
                            nodeList.IndexOf(botLeftPt),
                            nodeList.IndexOf(botRightPt),
                            PixelState.None
                            ));
                    }
                }
            }

            // Find the closest node to the spine line
            Tuple<double, int> closestPoint = null;
            var crv = new Rhino.Geometry.Line(_spine.Item1, _spine.Item2).ToNurbsCurve();
            int spi = 0;
            foreach (var p in nodeList.Select(n => new Point3d(n.X, n.Y, 0)))
            {
                double dist = double.PositiveInfinity;
                double t = 0;
                crv.ClosestPoint(p, out t, 100);
                var crvPt = crv.PointAt(t);
                dist = crvPt.DistanceTo(p);
                // First cycle
                if (closestPoint == null) { closestPoint = new Tuple<double, int>(dist, spi); spi++; continue; }

                // If its closer
                if (!double.IsInfinity(dist) && dist < closestPoint.Item1)
                {
                    closestPoint = new Tuple<double, int>(dist, spi);
                }
                spi++;
            }
            var closestNode = nodeList[closestPoint.Item2];
            var coord = nodeDictionary.Where(x => x.Value == closestNode).First().Key;

            // Find orientation of spine curve, vert or horz, to create spine
            bool xDir = Math.Abs(_spine.Item1.X - _spine.Item2.X) > Math.Abs(_spine.Item1.Y - _spine.Item2.Y);
            if (xDir)
            {
                var horzPts = nodeDictionary.Where(x => x.Key.Item2 == coord.Item2).ToList().Select(x => x.Value);
                foreach (var pt in horzPts)
                {
                    var myCoord = nodeList.IndexOf(pt);
                    foreach (var p in pixelList.Where(p => p.ContainsNode(myCoord,true)).ToList())
                    {
                        p.ChangeStateTo(PixelState.Moment);
                    }
                }
            }
            else
            {
                var vertPts = nodeDictionary.Where(x => x.Key.Item1 == coord.Item1).ToList().Select(x => x.Value);
                foreach (var pt in vertPts)
                {
                    var myCoord = nodeList.IndexOf(pt);
                    foreach (var p in pixelList.Where(p => p.ContainsNode(myCoord, true)).ToList())
                    {
                        p.ChangeStateTo(PixelState.Moment);
                    }
                }
            }

            foreach (var p in pixelList.Where(p => p.ContainsNode(spi)).ToList())
            {
                p.ChangeStateTo(PixelState.Moment);
            }

            // Create the pixel structure
            var pixelStruct = new Core.PixelStructure(nodeList, pixelList);

            // Get the line representation of our massing
            List<Core.Geometry.Line2d> massingLines = new List<Core.Geometry.Line2d>();
            var pline = _lastReffedCurve.ToPolyline(0.01, Math.PI, 1, 1);
            var plinePoints = pline.ToPolyline().ToArray();
            for (int i = 0; i < plinePoints.Count() - 1; i++)
            {
                massingLines.Add(new Core.Geometry.Line2d(plinePoints[i].ToFrixelPoint(),
                                                          plinePoints[i + 1].ToFrixelPoint())
                );
            }

            // Return the data
            return new UI.FrixelReferenceData(pixelStruct, massingLines, Boundingbox);
        }

        private UI.FrixelReferenceData MainWindow_ReferenceFromRhino(double xSize, double ySize)
        {
            // Tell user to select objects from doc
            var go = new GetObject();
            go.SetCommandPrompt("Select a closed curve");
            go.GeometryFilter = ObjectType.Curve;
            go.GeometryAttributeFilter = GeometryAttributeFilter.ClosedCurve;
            go.Get();
            var curves = go.Objects().Select(o => o.Curve());
            if (curves.Count() == 0) { return null; }

            this._lastReffedCurve = curves.First();
            this._xSize = xSize;
            this._ySize = ySize;
            this._spine = GetSpine();

            if(this._lastReffedCurve != null && this._spine != null) { return Regenerate(); }
            else { return null; }
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

            if (!_handlerIsAttached) {
                Frixel.UI.MainWindow.ReferenceFromRhino += MainWindow_ReferenceFromRhino;
                Frixel.UI.MainWindow.UpdateRhino += MainWindow_UpdateRhino;
                _handlerIsAttached = true;
            }

            // Create a FrixelWindow
            if(_window == null) { _window = new Frixel.UI.MainWindow();
                RhinoApp.WriteLine("Launching Frixel Window", EnglishName);
                new System.Windows.Interop.WindowInteropHelper(_window).Owner = Rhino.RhinoApp.MainWindowHandle();
                this._doc = doc;
            }

            // Show theFrixelWindow
            _window.Show();

            return Result.Success;
        }

        private UI.FrixelReferenceData MainWindow_UpdateRhino(double xSize, double ySize)
        {
            this._xSize = xSize;
            this._ySize = ySize;
            return this.Regenerate();
        }
    }
}
