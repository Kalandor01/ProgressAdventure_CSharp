using System;
using DrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace PAVisualizer
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opacityMultiplier">The number to multiply the opacity of the color by.</param>
        /// <returns></returns>
        public ColorData MultiplyOpacity(double opacityMultiplier)
        {
            return new ColorData(R, G, B, (byte)Math.Clamp(A * opacityMultiplier, 0, 255));
        }
        #endregion
    }
}
