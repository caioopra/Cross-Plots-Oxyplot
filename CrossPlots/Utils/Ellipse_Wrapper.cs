using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Annotations;

namespace CrossPlots
{
    public class Ellipse_Wrapper
    {
        public EllipseAnnotation ellipse;
        public RectangleAnnotation rectangle;
        public bool editing = false;

        public enum Anchors : int
        {
            TOP = 0,
            BOTTOM = 1,
            LEFT = 2,
            RIGHT = 3,
            TOP_RIGHT = 4,
            BOTTOM_RIGHT = 5,
            BOTTOM_LEFT = 6,
            TOP_LEFT = 7,
        }

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

        /// <summary>
        /// Anchors in edges and corners:
        /// 0-3: TOP, RIGHT, BOTTOM, LEFT
        /// 4-7: TOP-RIGHT, BOTTOM-RIGHT, BOTTOM-LEFT, TOP-LEFT
        /// </summary>
        public List<PointAnnotation> anchors = new List<PointAnnotation>();

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

            anchors.Add(top_point);
            anchors.Add(right_point);
            anchors.Add(bottom_point);
            anchors.Add(left_point);

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

            anchors.Add(top_right_point);
            anchors.Add(bottom_right_point);
            anchors.Add(bottom_left_point);
            anchors.Add(top_left_point);

            foreach (var p in anchors)
            {
                model.Annotations.Add(p);
            }
        }

        public void DestroyAnchors(PlotModel model)
        {
            for (int i = 0; i < 8; i++)
            {
                model.Annotations.Remove(anchors[i]);
                anchors[i] = null;
            }
        }

        // considering a small error on the anchor
        public int ClickedInAnchor(double x_pos, double y_pos)
        {
            var min_x_low = rectangle.MinimumX - 5;
            var min_x_high = rectangle.MinimumX + 5;
            var max_x_low = rectangle.MaximumX - 5;
            var max_x_high = rectangle.MaximumX + 5;

            var min_y_low = rectangle.MinimumY - 5;
            var min_y_high = rectangle.MinimumY + 5;
            var max_y_low = rectangle.MaximumY - 5;
            var max_y_high = rectangle.MaximumY + 5;

            var median_x_low = (rectangle.MaximumX - rectangle.MinimumX) / 2 - 5;
            var median_x_high = (rectangle.MaximumX - rectangle.MinimumX) / 2 + 5;

            var median_y_low = (rectangle.MaximumY - rectangle.MinimumY) / 2 - 5;
            var median_y_high = (rectangle.MaximumY - rectangle.MinimumY) / 2 + 5;

            // clicked on one of the top anchors
            if (y_pos >= max_y_low && y_pos <= max_y_high)
            {
                // clicked on top right anchor
                if (x_pos >= max_x_low && x_pos <= max_x_high)
                {
                    return (int)Anchors.TOP_RIGHT;
                }
                // clicked on top left anchor
                if (x_pos >= min_x_low && x_pos <= min_x_high)
                {
                    return (int)Anchors.TOP_LEFT;
                }
                // clicked on the top edge anchor
                if (x_pos >= median_x_low && x_pos <= median_x_high)
                {
                    return (int)Anchors.TOP;
                }
            }

            // clicked on one of the lower anchors
            if (y_pos >= min_y_low && y_pos <= min_y_high)
            {
                // clicked on bottom right anchor
                if (x_pos >= max_x_low && x_pos <= max_x_high)
                {
                    return (int)Anchors.BOTTOM_RIGHT;
                }
                // clicked on bottom left anchor
                if (x_pos >= min_x_low && x_pos <= min_x_high)
                {
                    return (int)Anchors.BOTTOM_LEFT;
                }
                // clicked on the bottom edge anchor
                if (x_pos >= median_x_low && x_pos <= median_x_high)
                {
                    return (int)Anchors.BOTTOM;
                }
            }
            // clicked one of the lateral edges
            if (y_pos >= median_y_low && y_pos <= median_y_high)
            {
                // clicked on right edge
                if (x_pos >= max_x_low && x_pos <= max_x_high)
                {
                    return (int)Anchors.RIGHT;
                }
                // clicked on left edge
                if (x_pos >= min_x_low && x_pos <= min_x_high)
                {
                    return (int)Anchors.LEFT;
                }
            }

            return -1;
        }
    }
}