﻿using OxyPlot;
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
        double fin_x;
        double fin_y;

        private EllipseAnnotation ellipseAnnotation;

        private ScatterSeries scatterSeries;
        private List<ScatterPoint> scatterSource = new List<ScatterPoint>();

        private Ellipse_Wrapper ellipse_wrapper;

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

            // Subscribe to mouse events for interactivity
            plotView.MouseDown += PlotView_MouseDown;
            plotView.MouseUp += PlotView_MouseUp;
            plotView.MouseMove += PlotView_MouseMove;
            plotView.KeyUp += CtrlUp;

            // Add the plot view to the form
            Controls.Add(plotView);

            // TEST BEGIN
            var vertices = new List<DataPoint>
            {
                new DataPoint(10, 20),
                new DataPoint(20, 20),
                new DataPoint(20, 10),
                new DataPoint(10, 10)
            };

            var pol = new PolygonAnnotation
            {
                Fill = OxyColor.FromAColor(10, OxyColors.Blue),
                Stroke = OxyColors.Black,
                StrokeThickness = 1,
            };
            pol.Points.Add(vertices[0]);
            pol.Points.Add(vertices[1]);
            pol.Points.Add(vertices[2]);
            pol.Points.Add(vertices[3]);
            plotModel.Annotations.Add(pol);
            plotModel.InvalidatePlot(true);

            double asd = (45 * Math.PI) / 180.0;
            var a = Utils.MatrixOperations.CreateFullRotationMatrix(asd, 15, 15);

            for (int i = 0; i < 4; i++)
            {
                var point = Utils.MatrixOperations.MatrixMultiplication(
                    new double[] { pol.Points[i].X, pol.Points[i].Y, 1 }, a
                );

                pol.Points[i] = new DataPoint(point[0], point[1]);
            }

            plotModel.InvalidatePlot(true);
            // END OF TEST
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

            // null if didn't clicked inside the ellipse
            var clicked_inside_ellipse = IsMouseInsideEllipseAnnotation(init_x, init_y);

            if (clicked_inside_ellipse && e.Button == MouseButtons.Right)
            {
                DestroyEllipse();
                return;
            }

            // must be holding "CRTL" key to create
            if (ModifierKeys == Keys.Control)
            {
                if (!clicked_inside_ellipse)
                {
                    CreateEllipse(init_x, init_y);
                }
            }

            if (ellipse_wrapper != null && ellipse_wrapper.rectangle != null)
            {
                ellipse_wrapper.ClickedInAnchor(init_x, init_y);

                if (ellipse_wrapper.current_anchor >= 0)
                {
                    ellipse_wrapper.editing = true;  // todo: do it inside Ellipse_Wrapper
                }
            }

            if (clicked_inside_ellipse && e.Button == MouseButtons.Left && ellipse_wrapper.rectangle is null)
            {
                ellipse_wrapper.CreateRectangleAroundEllipse();
            }
        }

        private bool IsMouseInsideEllipseAnnotation(double x, double y)
        {
            if (ellipse_wrapper is null)
            {
                return false;
            }

            if (ellipse_wrapper.ellipse != null)
            {
                var ellipse = ellipse_wrapper.ellipse;
                if (x >= ellipse.X - ellipse.Width / 2 &&
                    x <= ellipse.X + ellipse.Width / 2 &&
                    y >= ellipse.Y - ellipse.Height / 2 &&
                    y <= ellipse.Y + ellipse.Height / 2)
                {
                    return true;
                }
            }

            if (ellipse_wrapper.ellipse_annotation != null)
            {
                var ellipse = ellipse_wrapper.ellipse_annotation;
                if (x >= ellipse.X - ellipse.Width &&   // in ellipse_annotation, W and H are half of the value
                    x <= ellipse.X + ellipse.Width &&
                    y >= ellipse.Y - ellipse.Height &&
                    y <= ellipse.Y + ellipse.Height)
                {
                    return true;
                }
            }

            return false;
        }

        private void CreateEllipse(double init_x, double init_y)
        {
            if (ellipse_wrapper != null)
            {
                DestroyEllipse();
            }

            ellipseAnnotation = new EllipseAnnotation
            {
                X = init_x,
                Y = init_y,
                Width = 0,
                Height = 0,
                Fill = OxyColor.FromAColor(10, OxyColors.Blue),
                Stroke = OxyColors.Black,
                StrokeThickness = 1
            };

            (plotView.Model)?.Annotations.Add(ellipseAnnotation);
            ellipse_wrapper = new Ellipse_Wrapper(ellipseAnnotation, plotView.Model);

            plotView.InvalidatePlot(true);
        }

        private void DestroyEllipse()
        {
            ellipse_wrapper.DestroyEllipse();
            ellipse_wrapper = null;

            plotView.InvalidatePlot(true);
        }

        private void PlotView_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(ellipse_wrapper is null) && ellipse_wrapper.editing)
            {
                ellipse_wrapper.EditEllipse(
                    plotView.Model?.Axes[0].InverseTransform(e.X) ?? 0,
                    plotView.Model?.Axes[1].InverseTransform(e.Y) ?? 0
                );
                plotView.InvalidatePlot(true);

                return;
            }

            if (ModifierKeys == Keys.Control)
            {
                if (ellipseAnnotation != null)
                {
                    fin_x = plotView.Model?.Axes[0].InverseTransform(e.X) ?? 0;
                    fin_y = plotView.Model?.Axes[1].InverseTransform(e.Y) ?? 0;

                    double x = init_x + ((fin_x - init_x) / 2);
                    double y = init_y + ((fin_y - init_y) / 2);

                    // Update the width and height of the ellipse based on the mouse movement
                    ellipseAnnotation.Width = Math.Abs(fin_x - init_x);
                    ellipseAnnotation.Height = Math.Abs(fin_y - init_y);
                    ellipseAnnotation.X = x;
                    ellipseAnnotation.Y = y;

                    // Refresh the plot view
                    plotView.InvalidatePlot(true);
                }
                return;
            }
        }

        private void PlotView_MouseUp(object sender, MouseEventArgs e)
        {
            if (ellipseAnnotation != null && ellipse_wrapper?.ellipse != null)
            {
                InitializeCustomEllipse();
            }

            if (ellipse_wrapper != null && ellipse_wrapper.editing)
            {
                ellipse_wrapper.current_anchor = -1;
                ellipse_wrapper.editing = false;
            }
        }

        private void CtrlUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey && ellipseAnnotation != null && ellipse_wrapper.ellipse != null)
            {
                InitializeCustomEllipse();
            }
        }

        private void InitializeCustomEllipse()
        {
            ellipse_wrapper.InitializeCustomEllipse(
                    ellipseAnnotation.X,
                    ellipseAnnotation.Y,
                    ellipseAnnotation.Width,
                    ellipseAnnotation.Height
            );
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void plotBtn_Click(object sender, EventArgs e)
        {
            PlotData();
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            scatterSource.Clear();
            plotView.InvalidatePlot(true);
            ellipse_wrapper.RotateObject(45);
        }
    }
}
