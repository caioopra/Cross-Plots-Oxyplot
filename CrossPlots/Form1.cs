using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CrossPlots
{
    public partial class Form1 : Form
    {
        double init_x;
        double init_y;

        private PolygonAnnotation annotation = null;
        private bool annotation_created = false;

        private ScatterSeries scatterSeries;
        private List<ScatterPoint> scatterSource = new List<ScatterPoint>();

        Random rnd = new Random();

        public Form1()
        {
            InitializeComponent();
            InitializePlot();
        }

        private void InitializePlot()
        {
            // Create a scatter plot series
            scatterSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 1
            };

            // Add the scatter series to the plot model
            var plotModel = new PlotModel();
            plotModel.Series.Add(scatterSeries);

            // Assign the plot model to the plot view
            plotView.Model = plotModel;

            plotView.MouseDown += PlotView_MouseDown;

            // Add the plot view to the form
            Controls.Add(plotView);

            plotModel.InvalidatePlot(true);
        }

        private void PlotData()
        {

            // Add some sample data points
            for (int i = 0; i < 1000; i++)
            {
                scatterSource.Add(new ScatterPoint(rnd.Next(0, 100), rnd.Next(0, 100)));
            }

            scatterSeries.ItemsSource = scatterSource;

            // Refresh the plot view
            plotView.InvalidatePlot(true);
        }


        private void PlotView_MouseDown(object sender, MouseEventArgs e)
        {
            init_x = plotView.Model?.Axes[0].InverseTransform(e.X) ?? 0;
            init_y = plotView.Model?.Axes[1].InverseTransform(e.Y) ?? 0;

            if (e.Button == MouseButtons.Right && IsPointInPolygon(init_x, init_y))
            {
                HandlePointsDeletion();
                return;
            }

            // must be holding "CRTL" key to create
            if (ModifierKeys == Keys.Control)
            {
                if (!annotation_created)
                {
                    CreatePolygon(init_x, init_y);
                }
                else
                {
                    AddPoint(init_x, init_y);
                }
            }
        }

        private void CreatePolygon(double x, double y)
        {
            annotation_created = true;

            annotation = new PolygonAnnotation
            {
                Fill = OxyColor.FromAColor(10, OxyColors.Blue),
                Stroke = OxyColors.Black,
                StrokeThickness = 1,
            };

            plotView.Model.Annotations.Add(annotation);

            AddPoint(x, y);
        }

        private void AddPoint(double x, double y)
        {
            annotation.Points.Add(new DataPoint(x, y));

            plotView.Model.Annotations.Add(new PointAnnotation
            {
                X = x,
                Y = y,
                Fill = OxyColors.Black,
                Size = 3
            });

            UpdateWindow();
        }

        private void UpdateWindow()
        {
            plotView.Model.InvalidatePlot(true);
        }

        public void HandlePointsDeletion()
        {
            if (annotation != null && annotation.Points.Count > 2)
            {
                List<int> erased_indexes = ErasePointsInsidePolygon();
            }

            DeleteAnnotation();
        }

        /// <summary>
        /// Erase the points inside the polygon and returns the index of the ones that were removed
        /// </summary>
        /// <returns>List of the indexes of the points removed</returns>
        private List<int> ErasePointsInsidePolygon()
        {
            List<int> indexes = new List<int>();

            scatterSource.RemoveAll(point =>
            {
                var dataPoint = new DataPoint(point.X, point.Y);
                var pointInPolygon = IsPointInPolygon(dataPoint);

                if (pointInPolygon)
                {
                    indexes.Add(scatterSource.IndexOf(point));
                }

                return IsPointInPolygon(dataPoint);
            });

            UpdateWindow();

            return indexes;
        }

        private bool IsPointInPolygon(DataPoint point)
        {
            if (annotation == null || annotation.Points.Count < 3)
            {
                return false;
            }

            var polygonPoints = annotation.Points.ToList();
            bool inside = false;
            int j = polygonPoints.Count - 1;

            for (int i = 0; i < polygonPoints.Count; j = i++)
            {
                if ((polygonPoints[i].Y > point.Y) != (polygonPoints[j].Y > point.Y) &&
                    point.X < (polygonPoints[j].X - polygonPoints[i].X) *
                              (point.Y - polygonPoints[i].Y) /
                              (polygonPoints[j].Y - polygonPoints[i].Y) + polygonPoints[i].X)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private bool IsPointInPolygon(double x, double y)
        {
            if (annotation == null || annotation.Points.Count < 3)
            {
                return false;
            }

            var polygonPoints = annotation.Points.ToList();
            bool inside = false;
            int j = polygonPoints.Count - 1;

            for (int i = 0; i < polygonPoints.Count; j = i++)
            {
                if ((polygonPoints[i].Y > y) != (polygonPoints[j].Y > y) &&
                    x < (polygonPoints[j].X - polygonPoints[i].X) *
                        (y - polygonPoints[i].Y) /
                        (polygonPoints[j].Y - polygonPoints[i].Y) + polygonPoints[i].X)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private void DeleteAnnotation()
        {

            if (!annotation_created)
            {
                return;
            }

            annotation_created = false;

            plotView.Model.Annotations.Remove(annotation);
            annotation = null;

            plotView.Model.Annotations.Clear();

            UpdateWindow();
        }


        private void Form1_Load(object sender, EventArgs e) { }

        private void plotBtn_Click(object sender, EventArgs e)
        {
            PlotData();
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            scatterSource.Clear();
            plotView.InvalidatePlot(true);
        }
    }
}
