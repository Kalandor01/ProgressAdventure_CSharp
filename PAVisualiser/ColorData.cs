using DrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace PAVisualiser
{
    public readonly struct ColorData
    {
        #region Public fields
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly byte A;
        #endregion

        #region Constructors
        public ColorData(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        #endregion

        #region Public methods
        public DrawingColor ToDrawingColor()
        {
            return DrawingColor.FromArgb(A, R, G, B);
        }

        public MediaColor ToMediaColor()
        {
            return MediaColor.FromArgb(A, R, G, B);
        }
        #endregion
    }
}
