﻿using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CrossPlots
{
    public partial class Form1 : Form
    {
        double init_x;
        double init_y;

        private PolygonAnnotation annotation = null;
        private bool editing = false;

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

            // Subscribe to mouse events for interactivity
            plotView.MouseDown += PlotView_MouseDown;
            plotView.KeyUp += CtrlUp;

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

            if (e.Button == MouseButtons.Right)
            {
                // TODO: function to destroy annotation
                return;
            }

            // must be holding "CRTL" key to create
            if (ModifierKeys == Keys.Control)
            {
                if (!editing)
                {
                    CreatePolygon(init_x, init_y);
                }
                else
                {
                    AddPoint(init_x, init_y);
                }
            }
        }

        // TODO: implement
        private void CreatePolygon(double x, double y)
        {
            editing = true;

            annotation = new PolygonAnnotation
            {
                Fill = OxyColor.FromAColor(10, OxyColors.Blue),
                Stroke = OxyColors.Black,
                StrokeThickness = 1,
            };

            plotView.Model.Annotations.Add(annotation);

            AddPoint(x, y);
        }

        // TODO: implement
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

        // TODO: implement
        private void CtrlUp(object sender, KeyEventArgs e)
        {
            editing = false;
            // paint the points inside of the current polygon
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
        }
    }
}
