using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Annotations;

namespace CrossPlots
{
    public class Ellipse_Wrapper
    {
        public EllipseAnnotation ellipse;
        public RectangleAnnotation rectangle;

        /// <summary>
        /// Anchors in the middle of each edge of the rectangle, in the order:
        /// TOP, RIGHT, BOTTOM, LEFT
        /// </summary>
        public List<PointAnnotation> edgeAnchors = new List<PointAnnotation>();

        /// <summary>
        /// Anchors in the corners/vertex of the rectangle, in the order:
        /// TOP-RIGHT, BOTTOM-RIGHT, BOTTOM-LEFT, TOP-LEFT
        /// </summary>
        public List<PointAnnotation> cornerAnchors = new List<PointAnnotation>(); // TR, BR, BL, TL

        public Ellipse_Wrapper(EllipseAnnotation ellipse, RectangleAnnotation rectangle = null)
        {
            this.ellipse = ellipse;
            this.rectangle = rectangle;
        }

        public RectangleAnnotation CreateRectangleAroundEllipse(PlotModel model)
        {
            double left = ellipse.X - ellipse.Width / 2;
            double top = ellipse.Y + ellipse.Height / 2;
            double right = ellipse.X + ellipse.Width / 2;
            double bottom = ellipse.Y - ellipse.Height / 2;

            var rectangle = new RectangleAnnotation
            {
                MinimumX = left,
                MaximumX = right,
                MinimumY = bottom,
                MaximumY = top,
                Fill = OxyColors.Transparent,
                StrokeThickness = 2,
                Stroke = OxyColors.Black,
            };

            CreateAnchors(model, left, top, right, bottom);

            this.rectangle = rectangle;

            return rectangle;
        }

        private void CreateAnchors(PlotModel model, double left, double top, double right, double bottom)
        {
            // edge points
            var top_point = new PointAnnotation
            {
                X = (right + left) / 2,
                Y = top,
                Fill = OxyColors.Black,
                Size = 3
            };
            var right_point = new PointAnnotation
            {
                X = right,
                Y = (top + bottom) / 2,
                Fill = OxyColors.Black,
                Size = 3
            };
            var bottom_point = new PointAnnotation
            {
                X = (right + left) / 2,
                Y = bottom,
                Fill = OxyColors.Black,
                Size = 3
            };
            var left_point = new PointAnnotation
            {
                X = left,
                Y = (top + bottom) / 2,
                Fill = OxyColors.Black,
                Size = 3
            };

            edgeAnchors.Add(top_point);
            edgeAnchors.Add(right_point);
            edgeAnchors.Add(bottom_point);
            edgeAnchors.Add(left_point);

            foreach (var p in edgeAnchors)
            {
                model.Annotations.Add(p);
            }

            // corner points
            var top_right_point = new PointAnnotation
            {
                X = right,
                Y = top,
                Fill = OxyColors.Black,
                Size = 3
            };
            var bottom_right_point = new PointAnnotation
            {
                X = right,
                Y = bottom,
                Fill = OxyColors.Black,
                Size = 3
            };
            var bottom_left_point = new PointAnnotation
            {
                X = left,
                Y = bottom,
                Fill = OxyColors.Black,
                Size = 3
            };
            var top_left_point = new PointAnnotation
            {
                X = left,
                Y = top,
                Fill = OxyColors.Black,
                Size = 3
            };

            cornerAnchors.Add(top_right_point);
            cornerAnchors.Add(bottom_right_point);
            cornerAnchors.Add(bottom_left_point);
            cornerAnchors.Add(top_left_point);

            foreach (var p in cornerAnchors)
            {
                model.Annotations.Add(p);
            }
        }

        public void DestroyAnchors(PlotModel model)
        {
            for (int i = 0; i < 4; i++)
            {
                model.Annotations.Remove(edgeAnchors[i]);
                edgeAnchors[i] = null;
            }

            for (int i = 0; i < 4; i++)
            {
                model.Annotations.Remove(cornerAnchors[i]);
                cornerAnchors[i] = null;
            }
        }
    }
}