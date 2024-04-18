using OxyPlot;
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
        double fin_x;
        double fin_y;
        private EllipseAnnotation ellipseAnnotation;
        private ScatterSeries scatterSeries;
        private List<ScatterPoint> scatterSource = new List<ScatterPoint>();
        private List<Ellipse_Wrapper> ellipse_wrappers = new List<Ellipse_Wrapper>();

        private Ellipse_Wrapper lastSelectedEllipseWrapper = null;

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

            ellipseAnnotation = new EllipseAnnotation
            {
                X = 10,
                Y = 10,
                Width = 15,
                Height = 15,
                Fill = OxyColor.FromAColor(10, OxyColors.Blue),
                Stroke = OxyColors.Black,
                StrokeThickness = 1,

            };
        }


        private void PlotView_MouseDown(object sender, MouseEventArgs e)
        {
            init_x = plotView.Model?.Axes[0].InverseTransform(e.X) ?? 0;
            init_y = plotView.Model?.Axes[1].InverseTransform(e.Y) ?? 0;

            // null if didn't find any ellipse where clicked
            var found_ellipse = IsMouseInsideEllipseAnnotation(init_x, init_y);

            if (found_ellipse != null && e.Button == MouseButtons.Right)
            {
                DestroyEllipseAnnotation(found_ellipse);
                return;
            }

            if (found_ellipse != null && e.Button == MouseButtons.Left)
            {
                if (lastSelectedEllipseWrapper != null)
                {
                    DestroyRectangleAnnotation();
                }
                CreateRectangleAroundEllipse(found_ellipse);
                return;
            }

            if (ModifierKeys == Keys.Control)
            {
                if (found_ellipse == null)
                {
                    CreateEllipse(init_x, init_y);
                }
                else
                {
                    EditEllipse();
                }
            }
        }

        // returns null when mouse wasn't inside an ellipse
        private EllipseAnnotation IsMouseInsideEllipseAnnotation(double x, double y)
        {
            foreach (var wrapper in ellipse_wrappers)
            {
                var ellipse = wrapper.ellipse;
                if (x >= ellipse.X - ellipse.Width / 2 &&
                    x <= ellipse.X + ellipse.Width / 2 &&
                    y >= ellipse.Y - ellipse.Height / 2 &&
                    y <= ellipse.Y + ellipse.Height / 2)
                {
                    return ellipse;
                }
            }

            return null;
        }

        private void CreateEllipse(double init_x, double init_y)
        {
            // Create a new ellipse annotation
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

            // Add the ellipse annotation to the plot model
            (plotView.Model)?.Annotations.Add(ellipseAnnotation);
            ellipse_wrappers.Add(new Ellipse_Wrapper(ellipseAnnotation));

            plotView.InvalidatePlot(true);  // refresh
        }

        private void CreateRectangleAroundEllipse(EllipseAnnotation ellipse)
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

            var model = plotView.Model;
            model.Annotations.Add(rectangle);
            plotView.InvalidatePlot(true);

            foreach (var ellipse_wrapper in ellipse_wrappers)
            {
                if (ellipse_wrapper.ellipse == ellipse)
                {
                    ellipse_wrapper.rectangle = rectangle;
                    lastSelectedEllipseWrapper = ellipse_wrapper;
                    return;
                }
            }
        }

        private void EditEllipse() { }

        // TODO: check if is not the lastSelectedEllipseWrapper
        private void DestroyEllipseAnnotation(EllipseAnnotation ellipse)
        {
            var model = plotView.Model;

            if (ellipse == lastSelectedEllipseWrapper.ellipse)
            {
                lastSelectedEllipseWrapper = null;
            }

            foreach (var wrapper in ellipse_wrappers)
            {
                if (wrapper.ellipse == ellipse)
                {
                    model.Annotations.Remove(wrapper.ellipse);
                    if (wrapper.rectangle != null)
                    {
                        model.Annotations.Remove(wrapper.rectangle);
                        lastSelectedEllipseWrapper = null;
                    }
                    ellipse_wrappers.Remove(wrapper);

                    plotView.InvalidatePlot(true);

                    return;
                }
            }
        }

        private void DestroyRectangleAnnotation()
        {
            plotView.Model.Annotations.Remove(lastSelectedEllipseWrapper.rectangle);
            plotView.InvalidatePlot(true);

            lastSelectedEllipseWrapper.rectangle = null;
            lastSelectedEllipseWrapper = null;
        }

        private void PlotView_MouseMove(object sender, MouseEventArgs e)
        {
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
            }
        }

        private void PlotView_MouseUp(object sender, MouseEventArgs e)
        {
            // Reset the ellipse annotation once the mouse is released
            ellipseAnnotation = null;
        }

        private void CtrlUp(object sender, KeyEventArgs e)
        {
            // Check if the Ctrl key is pressed
            if (e.KeyCode == Keys.ControlKey)
            {
                // Reset the ellipse annotation once the mouse is released
                ellipseAnnotation = null;
            }

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
