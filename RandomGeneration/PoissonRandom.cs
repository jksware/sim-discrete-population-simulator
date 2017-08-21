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
    public class PoissonRandom : CustomRandom
    {
        public readonly double λ;

        public PoissonRandom(double λ, StreamWriter writer)
            : base(writer)
        {
            this.λ = λ;
        }

        public PoissonRandom(double λ)
            : this(λ, null)
        {
        }

#if SAUCIER
        /// <summary>
        /// The present implementation is adapted from 
        /// Saucier, Richard - Computer Generation of Statistical Distributions, Army Research Laboratory
        /// as given in page 59.
        /// </summary>
        /// <returns></returns>
        protected override double Sample()
        {
            double b = 1;
            int i;
            for (i = 0; b >= Math.Exp(-λ); i++)
                b *= base.Sample();
            return i - 1;
        }
#else
        protected override double Sample()
        {
            return -Math.Log(1.0 - base.Sample(), Math.E) / λ;
        }
#endif
    }
}
