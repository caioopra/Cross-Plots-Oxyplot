using OxyPlot.Annotations;

namespace CrossPlots
{
    public class Ellipse_Wrapper
    {
        public EllipseAnnotation ellipse;
        public RectangleAnnotation rectangle;
        public PointAnnotation[] edgeAnchors;   // Top, Right, Bottom, Left
        public PointAnnotation[] cornerAnchors; // TR, BR, BL, TL

        public Ellipse_Wrapper(EllipseAnnotation ellipse, RectangleAnnotation rectangle = null)
        {
            this.ellipse = ellipse;
            this.rectangle = rectangle;
        }
    }
}