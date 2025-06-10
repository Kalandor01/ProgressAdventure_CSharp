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

        /// <summary>Blends the specified colors together.</summary>
        /// <param name="otherColor">Color to blend the other color onto.</param>
        /// <param name="amount">How much of the original color to keep,
        /// “on top of” <paramref name="otherColor"/>.</param>
        /// <returns>The blended colors.</returns>
        public ColorData Blend(ColorData otherColor, double amount)
        {
            var r = (byte)(R * amount + otherColor.R * (1 - amount));
            var g = (byte)(G * amount + otherColor.G * (1 - amount));
            var b = (byte)(B * amount + otherColor.B * (1 - amount));
            return new ColorData(r, g, b);
        }

        /// <summary>Blends the specified colors together based on their opacity.</summary>
        /// <param name="color">Color to blend onto the background color.</param>
        /// <param name="otherColor">Color to blend the other color onto.</param>
        /// <returns>The blended colors that hase an opacity that is the sum of the two opacities.</returns>
        public ColorData Blend(ColorData otherColor)
        {
            var a1 = A / 255d;
            var a2 = otherColor.A / 255d;
            var r = (byte)(R * a1 + otherColor.R * a2);
            var g = (byte)(G * a1 + otherColor.G * a2);
            var b = (byte)(B * a1 + otherColor.B * a2);
            return new ColorData(r, g, b, (byte)(A + otherColor.A));
        }
        #endregion
    }
}
