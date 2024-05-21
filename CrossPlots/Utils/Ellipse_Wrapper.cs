using System;
using System.Collections.Generic;
using CrossPlots.Utils;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace CrossPlots
{
    public class Ellipse_Wrapper
    {
        public PlotModel model;

        // "ellipse" is just a temporary reference for the ellipse drawing and,
        // after ellipse_annotation is constructed, it is destroyed
        public EllipseAnnotation ellipse;
        public CustomEllipse ellipse_annotation; // main ellipse
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
            ROTATION = 8,
        }

        /// <summary>
        /// Anchors in edges and corners:
        /// 0-3: TOP, RIGHT, BOTTOM, LEFT
        /// 4-7: TOP-RIGHT, BOTTOM-RIGHT, BOTTOM-LEFT, TOP-LEFT
        /// 8  : ROTATION
        /// </summary>
        public List<PointAnnotation> anchors = new List<PointAnnotation>();
        public int current_anchor = -1;

        // line that connects top anchor to rotation anchor
        public LineSeries line;
        public PointAnnotation rotation_anchor;

        public Ellipse_Wrapper(EllipseAnnotation ellipse, PlotModel model, RectangleAnnotation rectangle = null)
        {
            this.ellipse = ellipse;
            this.model = model;
            this.rectangle = rectangle;
        }

        public Ellipse_Wrapper(PlotModel model, RectangleAnnotation rectangle = null)
        {
            this.model = model;
            this.rectangle = rectangle;
        }

        public void InitializeCustomEllipse(double init_x, double init_y, double width, double height)
        {
            ellipse_annotation = new CustomEllipse(init_x, init_y, width, height, model);

            if (ellipse != null)
            {
                DestroyEllipseAnnotation();
            }
        }

        public void DestroyEllipse()
        {
            if (ellipse != null)
            {
                DestroyEllipseAnnotation();
            }

            ellipse_annotation.Destroy();
            ellipse_annotation = null;

            if (rectangle != null)
            {
                model.Annotations.Remove(rectangle);
                rectangle = null;
                DestroyAnchors();
            }

            model.InvalidatePlot(true);
        }

        // once the user stops initializing the ellipse shape, starts to use ellipse_annotation (CustomEllipse)
        void DestroyEllipseAnnotation()
        {
            model.Annotations.Remove(ellipse);
            ellipse = null;

            model.InvalidatePlot(true);
        }

        public void CreateRectangleAroundEllipse()
        {
            double left = ellipse_annotation.X - ellipse_annotation.Width;
            double top = ellipse_annotation.Y + ellipse_annotation.Height;
            double right = ellipse_annotation.X + ellipse_annotation.Width;
            double bottom = ellipse_annotation.Y - ellipse_annotation.Height;

            var rectangle = new RectangleAnnotation
            {
                MinimumX = left,
                MaximumX = right,
                MinimumY = bottom,
                MaximumY = top,
                Fill = OxyColors.Transparent,
                StrokeThickness = 1,
                Stroke = OxyColors.Black,
            };

            this.rectangle = rectangle;

            CreateAnchors(left, top, right, bottom);
            CreateLine();

            model.Annotations.Add(rectangle);

            model.InvalidatePlot(true);
        }

        private void CreateAnchors(double left, double top, double right, double bottom)
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

            var rotation_point = new PointAnnotation
            {
                X = (right + left) / 2,
                Y = top + 10,
                Fill = OxyColors.Black,
                Size = 3
            };
            anchors.Add(rotation_point);

            foreach (var p in anchors)
            {
                model.Annotations.Add(p);
            }
        }

        public void CreateLine()
        {
            line = new LineSeries
            {
                Points = {
                    new DataPoint(anchors[(int)Anchors.TOP].X, anchors[(int)Anchors.TOP].Y),
                    new DataPoint(anchors[(int)Anchors.TOP].X, anchors[(int)Anchors.TOP].Y + 10)
                },
                Color = OxyColors.Black,
                StrokeThickness = 1,
                LineStyle = LineStyle.Dash,
            };

            model.Series.Add(line);
        }

        public void DestroyAnchors()
        {
            for (int i = 0; i < 9; i++)
            {
                model.Annotations.Remove(anchors[i]);
            }

            anchors.Clear();

            model.Series.Remove(line);
            line = null;
        }

        // considering a small error on the anchor click
        /// <summary>
        /// Verifies if a anchor was clicked. If dind't clicked in any, returns -1, else returns the number that identifies an achor
        /// </summary>
        /// <param name="x_pos"></param>
        /// <param name="y_pos"></param>
        public void ClickedInAnchor(double x_pos, double y_pos)
        {
            var min_x_low = rectangle.MinimumX - 5;
            var min_x_high = rectangle.MinimumX + 5;
            var max_x_low = rectangle.MaximumX - 5;
            var max_x_high = rectangle.MaximumX + 5;

            var min_y_low = rectangle.MinimumY - 5;
            var min_y_high = rectangle.MinimumY + 5;
            var max_y_low = rectangle.MaximumY - 5;
            var max_y_high = rectangle.MaximumY + 5;

            var median_x_low = rectangle.MinimumX + (rectangle.MaximumX - rectangle.MinimumX) / 2 - 5;
            var median_x_high = rectangle.MinimumX + (rectangle.MaximumX - rectangle.MinimumX) / 2 + 5;

            var median_y_low = rectangle.MinimumY + (rectangle.MaximumY - rectangle.MinimumY) / 2 - 5;
            var median_y_high = rectangle.MinimumY + (rectangle.MaximumY - rectangle.MinimumY) / 2 + 5;

            // clicked on one of the top anchors
            if (y_pos >= max_y_low && y_pos <= max_y_high)
            {
                // clicked on top right anchor
                if (x_pos >= max_x_low && x_pos <= max_x_high)
                {
                    current_anchor = (int)Anchors.TOP_RIGHT;
                }
                // clicked on top left anchor
                if (x_pos >= min_x_low && x_pos <= min_x_high)
                {
                    current_anchor = (int)Anchors.TOP_LEFT;
                }
                // clicked on the top edge anchor
                if (x_pos >= median_x_low && x_pos <= median_x_high)
                {
                    current_anchor = (int)Anchors.TOP;
                }
                return;
            }

            // clicked on one of the lower anchors
            if (y_pos >= min_y_low && y_pos <= min_y_high)
            {
                // clicked on bottom right anchor
                if (x_pos >= max_x_low && x_pos <= max_x_high)
                {
                    current_anchor = (int)Anchors.BOTTOM_RIGHT;
                }
                // clicked on bottom left anchor
                if (x_pos >= min_x_low && x_pos <= min_x_high)
                {
                    current_anchor = (int)Anchors.BOTTOM_LEFT;
                }
                // clicked on the bottom edge anchor
                if (x_pos >= median_x_low && x_pos <= median_x_high)
                {
                    current_anchor = (int)Anchors.BOTTOM;
                }
                return;
            }
            // clicked one of the lateral edges
            if (y_pos >= median_y_low && y_pos <= median_y_high)
            {
                // clicked on right edge
                if (x_pos >= max_x_low && x_pos <= max_x_high)
                {
                    current_anchor = (int)Anchors.RIGHT;
                }
                // clicked on left edge
                if (x_pos >= min_x_low && x_pos <= min_x_high)
                {
                    current_anchor = (int)Anchors.LEFT;
                }
                return;
            }

            // clicked on rotation anchor
            if (x_pos >= median_x_low &&
                x_pos <= median_x_high &&
                y_pos >= max_y_low + 10 &&
                y_pos <= max_y_high + 10
            )
            {
                current_anchor = (int)Anchors.ROTATION;
                return;
            }

            current_anchor = -1;
        }

        public void EditEllipse(double x, double y)
        {
            double centerX = ellipse_annotation.X;
            double centerY = ellipse_annotation.Y;

            switch ((Anchors)current_anchor)
            {
                case Anchors.TOP:
                case Anchors.BOTTOM:
                    ellipse_annotation.Height = Math.Abs(centerY - y);
                    break;

                case Anchors.LEFT:
                case Anchors.RIGHT:
                    ellipse_annotation.Width = Math.Abs(centerX - x);
                    break;

                case Anchors.TOP_LEFT:
                case Anchors.TOP_RIGHT:
                    ellipse_annotation.Width = Math.Abs(centerX - x);
                    ellipse_annotation.Height = Math.Abs(centerY - y);
                    break;

                case Anchors.BOTTOM_LEFT:
                case Anchors.BOTTOM_RIGHT:
                    ellipse_annotation.Width = Math.Abs(centerX - x);
                    ellipse_annotation.Height = Math.Abs(centerY - y);
                    break;
            }

            UpdateCustomEllipse();

            // updating rectangle annotation
            double left = ellipse_annotation.X - ellipse_annotation.Width;
            double top = ellipse_annotation.Y + ellipse_annotation.Height;
            double right = ellipse_annotation.X + ellipse_annotation.Width;
            double bottom = ellipse_annotation.Y - ellipse_annotation.Height;

            rectangle.MinimumX = left;
            rectangle.MaximumX = right;
            rectangle.MinimumY = bottom;
            rectangle.MaximumY = top;

            // updating anchors
            DestroyAnchors();
            CreateAnchors(left, top, right, bottom);
            CreateLine();
        }

        void UpdateCustomEllipse()
        {
            ellipse_annotation.Destroy();

            ellipse_annotation.UpdateAnnotation();

            ellipse_annotation.DrawEllipse();
        }

        // angle in degrees (inside the function gets converted to radians)
        public void RotateObject(double angle)
        {
            angle = (angle * Math.PI / 180);

            var operationMatrix = Utils.MatrixOperations.CreateFullRotationMatrix(angle, ellipse.X, ellipse.Y);

        }
    }
}