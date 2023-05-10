using NPrng;
using NPrng.Generators;

namespace ProgressAdventure
{
    /// <summary>
    /// A perlin noise generator, capable of generating 2D perlin noise.<br/>
    /// Based on the <see href="https://www.nuget.org/packages/Simplex"><c>Simplex</c></see> package by DanClipca.
    /// </summary>
    public class PerlinNoise
    {
        #region Private fields
        private readonly byte[] _perm;
        private IPseudoRandomGenerator _generator;
        #endregion

        #region Public properties
        public IPseudoRandomGenerator Generator
        {
            get => _generator;
            set
            {
                GeneratePermBytes(value);
                _generator = value;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new perlin noise generator, with the specified generator.
        /// </summary>
        /// <param name="generator">The generator to initialise the perlin generator with.</param>
        public PerlinNoise(IPseudoRandomGenerator generator)
        {
            _perm = new byte[512];
            Generator = generator;
        }

        /// <summary>
        /// Creates a new perlin noise generator, with the specified seed.
        /// </summary>
        /// <param name="seed">The seed to initialise the perlin generator with.</param>
        public PerlinNoise(ulong seed)
            : this(new SplittableRandom(seed)) { }

        /// <summary>
        /// Creates a new perlin noise generator, with a random seed.
        /// </summary>
        /// <param name="seed">The seed to initialise the perlin generator with.</param>
        public PerlinNoise()
            : this(new SplittableRandom()) { }
        #endregion

        #region Public methods
        /// <summary>
        /// Generates a perlin noise value, ranging from -1 to 1.
        /// </summary>
        /// <param name="x">The X coordinate of the value.</param>
        /// <param name="y">The Y coordinate of the value.</param>
        /// <param name="scale">The scale factor of the generation. The larger, and closer to 1, the granier.</param>
        public double Generate(double x, double y, double scale)
        {
            x *= scale;
            y *= scale;
            const double F2 = 0.366025403;
            const double G2 = 0.211324865;

            double n0, n1, n2;

            var s = (x + y) * F2;
            var xs = x + s;
            var ys = y + s;
            var i = (int)Math.Floor(xs);
            var j = (int)Math.Floor(ys);

            var t = (i + j) * G2;
            var X0 = i - t;
            var Y0 = j - t;
            var x0 = x - X0;
            var y0 = y - Y0;

            int i1, j1;
            if (x0 > y0)
            {
                i1 = 1;
                j1 = 0;
            }
            else
            {
                i1 = 0;
                j1 = 1;
            }

            var x1 = x0 - i1 + G2;
            var y1 = y0 - j1 + G2;
            var x2 = x0 - 1.0 + 2.0 * G2;
            var y2 = y0 - 1.0 + 2.0 * G2;

            var ii = Utils.Mod(i, 256);
            var jj = Utils.Mod(j, 256);

            var t0 = 0.5 - x0 * x0 - y0 * y0;
            if (t0 < 0.0)
            {
                n0 = 0.0;
            }
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Grad(_perm[ii + _perm[jj]], x0, y0);
            }

            var t1 = 0.5 - x1 * x1 - y1 * y1;
            if (t1 < 0.0)
            {
                n1 = 0.0;
            }
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Grad(_perm[ii + i1 + _perm[jj + j1]], x1, y1);
            }

            var t2 = 0.5 - x2 * x2 - y2 * y2;
            if (t2 < 0.0)
            {
                n2 = 0.0;
            }
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Grad(_perm[ii + 1 + _perm[jj + 1]], x2, y2);
            }

            return 40.0 * (n0 + n1 + n2);
        }
        #endregion

        #region Private functions
        private static double Grad(int hash, double x, double y)
        {
            var h = hash & 7;
            var u = h < 4 ? x : y;
            var v = h < 4 ? y : x;
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0 * v : 2.0 * v);
        }

        /// <summary>
        /// Fills the elements of the perm array with ramdom bytes, using an <c>NPrng</c> generator.
        /// </summary>
        /// <param name="generator">The <c>NPrng</c> generator to use.</param>
        private void GeneratePermBytes(IPseudoRandomGenerator generator)
        {
            var halfArray = new byte[256];
            
            for (int x = 0; x < halfArray.Length; x++)
            {
                halfArray[x] = (byte)generator.GenerateInRange(0, 255);
            }
            halfArray.CopyTo(_perm, 0);
            halfArray.CopyTo(_perm, 256);
        }
        #endregion
    }
}