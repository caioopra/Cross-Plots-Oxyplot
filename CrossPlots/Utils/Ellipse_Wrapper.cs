using OxyPlot.Annotations;

namespace CrossPlots
{
    public class Ellipse_Wrapper
    {
        public EllipseAnnotation ellipse;
        public RectangleAnnotation rectangle;

        public Ellipse_Wrapper(EllipseAnnotation ellipse, RectangleAnnotation rectangle = null)
        {
            this.ellipse = ellipse;
            this.rectangle = rectangle;
        }
    }
}