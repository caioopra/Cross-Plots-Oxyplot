using OxyPlot;
using OxyPlot.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlots.Utils
{
    /// <summary>
    /// Class that draws an Ellipse using PolygonAnnotations and DrawPoints.
    /// This class accepts rotations and resizing
    /// </summary>
    public class CustomEllipse
    {
        public double rotation_angle = 0;

        /// <summary>
        /// Center of the ellipse in the X axis.
        /// </summary>
        public double X;
        /// <summary>
        /// Center of the ellipse in the Y axis.
        /// </summary>
        public double Y;
        public double Width;
        public double Height;

        public PlotModel model;

        const double step = 2 * Math.PI / 200;

        public PolygonAnnotation annotation;

        public CustomEllipse(double xCenter, double yCenter, double width, double height, PlotModel plotModel)
        {
            X = xCenter;
            Y = yCenter;
            Width = width / 2;
            Height = height / 2;
            model = plotModel;

            annotation = new PolygonAnnotation
            {
                Fill = OxyColor.FromAColor(10, OxyColors.Blue),
                Stroke = OxyColors.Black,
                StrokeThickness = 1,
            };

            model.Annotations.Add(annotation);
            DrawEllipse();
        }

        public void DrawEllipse(double rotation = 0)
        {
            for (double theta = 0; theta < 2 * Math.PI; theta += step)
            {
                var x = Width * Math.Cos(rotation) * Math.Cos(theta) - Height * Math.Sin(rotation) * Math.Sin(theta) + X;
                var y = Height * Math.Cos(rotation) * Math.Sin(theta) + Width * Math.Sin(rotation) * Math.Cos(theta) + Y;
                annotation.Points.Add(new DataPoint(x, y));
            }

            model.InvalidatePlot(true);
        }

        public void UpdateAnnotation()
        {
            annotation = new PolygonAnnotation
            {
                Fill = OxyColor.FromAColor(10, OxyColors.Blue),
                Stroke = OxyColors.Black,
                StrokeThickness = 1,
            };
            model.Annotations.Add(annotation);
            DrawEllipse();
        }

        public void Destroy()
        {
            for (int i = 0; i < annotation.Points.Count; i++)
            {
                annotation.Points.Remove(annotation.Points[i]);
            }

            model.Annotations.Remove(annotation);

            annotation = null;

            model.InvalidatePlot(true);
        }
    }
}
