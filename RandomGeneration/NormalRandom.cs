 /*
 *  Discrete event simulation of population
 *
 *  Copyright (C) 2014 Damián Valdés Santiago, Juan Carlos Pujol Mainegra
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.  
 *
 */
 
#define SAUCIER

using System;
using System.IO;

namespace RandomGeneration
{
    public class NormalRandom : CustomRandom
    {
        public readonly double μ;
        public readonly double σ;

        public NormalRandom(double μ, double σ, StreamWriter writer)
            : base(writer)
        {
            this.σ = σ;
            this.μ = μ;
        }

        public NormalRandom(double μ, double σ)
            : this(μ, σ, null)
        {
        }

#if SAUCIER
        /// <summary>
        /// The present implementation is adapted from 
        /// Saucier, Richard - Computer Generation of Statistical Distributions, Army Research Laboratory
        /// as given in page 29.
        /// </summary>
        /// <returns></returns>
        protected override double Sample()
        {
            double p, p1, p2;
            do
            {
                p1 = base.Sample();
                p2 = base.Sample();
                p = p1 * p1 + p2 * p2;
            } while (p >= 1);
            return μ + σ * p1 * Math.Sqrt(-2 * Math.Log(p, Math.E) / p);
        }

#else
        private static bool _hasSpare;
        private static double _rand1, _rand2;

        /// <summary>
        /// The present implementation is adapted from 
        /// http://en.wikipedia.org/w/index.php?title=Box%E2%80%93Muller_transform&oldid=607158144
        /// as itself is an adaptation from 
        /// Numerical Recipes in C, Second Edition, Cambridge University Press
        /// by William H. Press, Saul A. Teukolsky, William T. Vetterling and Brian P. Flannery.
        /// </summary>
        /// <returns></returns>
        protected override double Sample()
        {
            if (_hasSpare)
            {
                _hasSpare = false;
                return Math.Sqrt(σ * _rand1) * Math.Sin(_rand2);
            }

            _hasSpare = true;

            _rand1 = base.Sample();
            if (_rand1 < 1e-100)
                _rand1 = 1e-100;

            _rand1 = -2 * Math.Log(_rand1);
            _rand2 = base.Sample() * 2 * Math.PI;

            return μ + Math.Sqrt(σ * _rand1) * Math.Cos(_rand2);
        }
#endif
    }

}
