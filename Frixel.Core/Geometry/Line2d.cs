using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frixel.Core.Extensions;

namespace Frixel.Core.Geometry
{
    public class Line2d
    {
        public Point2d Start;
        public Point2d End;

        public Line2d(Point2d start, Point2d end)
        {
            this.Start = start;
            this.End = end;
        }

        public Point2d MidPoint()
        {
            return new Point2d(
                (this.Start.X + this.End.X) / 2,
                (this.Start.Y + this.End.Y) / 2
             );
        }

        public Line2d Map(Domain2d from, Domain2d to)
        {
            var start = new Point2d(this.Start.X.Map(from.X, to.X),
                            this.Start.Y.Map(from.Y, to.Y));
            var end = new Point2d(this.End.X.Map(from.X, to.X),
                            this.End.Y.Map(from.Y, to.Y));

            var line = new Line2d(start, end);
            return line;
        }

        public double Length()
        {
            return this.Start.DistanceTo(this.End);
        }

        public Line2d Copy()
        {
            return new Line2d(this.Start.Copy(), this.End.Copy());
        }

        public Line2d Move(Line2d translation, bool copy = false)
        {
            Line2d lineToMove;
            if (copy) { lineToMove = this.Copy(); } else { lineToMove = this; }

            lineToMove.Start.Move(translation);
            lineToMove.End.Move(translation);

            return lineToMove;
        }

        /// <summary>
        /// Rotates a line.
        /// </summary>
        /// <param name="line">The line to rotate</param>
        /// <param name="anchor">The pivot point for the rotation</param>
        /// <param name="degrees">The degrees (Counterclockwise)</param>
        /// <param name="copy">Rotate a copy of the line?</param>
        /// <returns></returns>
        public Line2d Rotate(Point2d anchor, double degrees, bool copy = true)
        {
            Line2d rotatedLine;
            if (copy) { rotatedLine = this.Copy(); } else { rotatedLine = this; }

            // Convert degrees to radians
            var rads = (degrees / 180) * Math.PI;

            // Rotate line points
            double rotLineStartX = Math.Cos(rads) * (rotatedLine.Start.X - Math.Sin(rads)) * (rotatedLine.Start.Y - anchor.Y) + anchor.X;
            double rotLineStartY = Math.Sin(rads) * (rotatedLine.Start.X + Math.Cos(rads)) * (rotatedLine.Start.Y - anchor.Y) + anchor.Y;
            double rotLineEndX = Math.Cos(rads) * (rotatedLine.End.X - Math.Sin(rads)) * (rotatedLine.End.Y - anchor.Y) + anchor.X;
            double rotLineEndY = Math.Sin(rads) * (rotatedLine.End.X + Math.Cos(rads)) * (rotatedLine.End.Y - anchor.Y) + anchor.Y;

            // Copy line points
            rotatedLine.Start.X = rotLineStartX;
            rotatedLine.Start.Y = rotLineStartY;
            rotatedLine.End.X = rotLineEndX;
            rotatedLine.End.Y = rotLineEndY;

            return rotatedLine;
        }

        public Line2d RotateLocal(Location anchor, double degrees, bool copy = true)
        {
            switch (anchor)
            {
                case Location.Start:
                    return this.Rotate(this.Start, degrees, copy);
                case Location.End:
                    return this.Rotate(this.End, degrees, copy);
                case Location.Middle:
                    return this.Rotate(this.MidPoint(), degrees, copy);
                default:
                    throw new NotImplementedException("Not a valid local coordinate");
            }
        }

        /// <summary>
        /// Returns a line of length 1.
        /// </summary>
        /// <param name="copy">Produces a copy</param>
        /// <param name="moveToOrigin">Moves the line to origin</param>
        /// <returns></returns>
        public Line2d Unitize(bool copy = true, bool moveToOrigin = false)
        {
            Line2d unitizedLine;
            if (copy) { unitizedLine = this.Copy(); } else { unitizedLine = this; }

            Point2d scaledPt;
            Point2d fixedPt;
            // Find the end that is closest to the origin
            if(this.Start.DistanceTo(Point2d.Origin) < this.End.DistanceTo(Point2d.Origin))
            {
                fixedPt = unitizedLine.Start;
                scaledPt = unitizedLine.End;
            } else {
                fixedPt = unitizedLine.End;
                scaledPt = unitizedLine.Start;
            }

            double magnitude = this.Length();

            scaledPt.X = scaledPt.X / magnitude;
            scaledPt.Y = scaledPt.Y / magnitude;

            if (moveToOrigin)
            {
                Line2d translation = new Line2d(fixedPt, Point2d.Origin);
                unitizedLine.Move(translation);
            }

            return unitizedLine;
        }

        /// <summary>
        /// Scales a line to a new length given a local anchor point.
        /// </summary>
        /// <param name="anchor">The local point to scale from</param>
        /// <param name="factor">Scale factor</param>
        /// <param name="copy">Produces a copy</param>
        /// <returns></returns>
        public Line2d ScaleToLocal(Location anchor, double newLength, bool copy = true)
        {
            Line2d scaledLine;
            if (copy) { scaledLine = this.Copy(); } else { scaledLine = this; }

            List<Point2d> scaledPts = new List<Point2d>();
            Point2d fixedPt = null;

            switch (anchor)
            {
                case Location.Start:
                    fixedPt = scaledLine.Start;
                    scaledPts.Add(scaledLine.End);
                    break;
                case Location.End:
                    fixedPt = scaledLine.End;
                    scaledPts.Add(scaledLine.Start);
                    break;
                case Location.Middle:
                    fixedPt = scaledLine.MidPoint();
                    scaledPts.Add(scaledLine.Start);
                    scaledPts.Add(scaledLine.End);
                    break;
            }
            double factor = newLength / this.Length();

            scaledPts.ForEach(p =>
            {
                p.X = (fixedPt.X + p.X) / (1 / factor);
                p.Y = (fixedPt.Y + p.Y) / (1 / factor);
            });

            return scaledLine;
        } // LJ UNTESTED

        /// <summary>
        /// Scales a line to a new length given a local anchor point.
        /// </summary>
        /// <param name="anchor">The local point to scale from</param>
        /// <param name="factor">Scale factor</param>
        /// <param name="copy">Produces a copy</param>
        /// <returns></returns>
        public Line2d ScaleLocal(Location anchor, double factor, bool copy = true)
        {
            Line2d scaledLine;
            if (copy) { scaledLine = this.Copy(); } else { scaledLine = this; }

            List<Point2d> scaledPts = new List<Point2d>();
            Point2d fixedPt = null;

            switch (anchor)
            {
                case Location.Start:
                    fixedPt = scaledLine.Start;
                    scaledPts.Add(scaledLine.End);
                    break;
                case Location.End:
                    fixedPt = scaledLine.End;
                    scaledPts.Add(scaledLine.Start);
                    break;
                case Location.Middle:
                    fixedPt = scaledLine.MidPoint();
                    scaledPts.Add(scaledLine.Start);
                    scaledPts.Add(scaledLine.End);
                    break;
            }

            scaledPts.ForEach(p =>
            {
                p.X = (fixedPt.X + p.X) / (1 / factor);
                p.Y = (fixedPt.Y + p.Y) / (1 / factor);
            });

            return scaledLine;
        } // LJ UNTESTED
    }

    public enum Location
    {
        Start,
        End,
        Middle
    }
}
