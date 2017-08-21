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

using RandomGeneration;
using System;
using System.IO;

namespace testGenerate
{
    class Program
    {
        private static string programName;

        enum Distribution {Poisson, Normal, Uniforme}

        static void Usage()
        {
            Console.WriteLine("Usage: ");
            Console.WriteLine("\t {0} [nombre_distribucion] par0 par1 par2?", programName);
            Console.WriteLine("Example");
            Console.WriteLine("\t {0} poisson 20 0.1", programName);
            Console.WriteLine("\t {0} normal 100 28 3.71", programName);
        }

        private static int _c = 0;
        private static int argsLeft { get { return _args.Length - _c; } }
        private static string[] _args;
        private static string Argument { get { return _args[_c++];  } }

        static int Main(string[] args)
        {
            programName = typeof(Program).Assembly.GetName().Name;
            _args = args;

            if (args.Length < 1)
            {
                Console.Error.WriteLine("Se necesita más de un argumento...");
                Usage();
                return -1;
            }


            Distribution selected;
            if (!Enum.TryParse(Argument, true, out selected))
            {
                Console.Error.WriteLine("No se entendió la distribución a entrar. Por favor prueba más tarde.");
                return -2;
            }


            int amount;
            if (!int.TryParse(Argument, out amount))
            {
                Console.Error.WriteLine("No se entendió (o pudo parsear) la cantidad de muestras.");
                return -2;
            }

            CustomRandom random = null;

            switch (selected)
            {
                case Distribution.Normal:
                    {
                        if (argsLeft != 2)
                        {
                            Console.Error.WriteLine("Cantidad de argumentos inválidos.");
                            Usage();
                            return -2;
                        }

                        double mu, sigma;

                        if (!double.TryParse(Argument, out mu))
                        {
                            Console.Error.WriteLine("No se entendió el primer parámetro ($\\mu$) de la distribución.");
                            Usage();
                            return -3;
                        }

                        if (!double.TryParse(Argument, out sigma))
                        {
                            Console.Error.WriteLine(
                                "No se entendió el segundo parámetro ($\\sigma$) de la distribución.");
                            Usage();
                            return -3;
                        }

                        var filename = string.Format("generated\\{0} {1} {2} {3}.txt", selected, amount, mu, sigma);
                        var sw = new StreamWriter(filename);

                        random = new NormalRandom(mu, sigma, sw);
                    }
                    break;

                case Distribution.Poisson:
                    {
                        if (argsLeft != 1)
                        {
                            Console.Error.WriteLine("Cantidad de argumentos inválidos.");
                            Usage();
                            return -2;
                        }

                        double lambda;

                        if (!double.TryParse(Argument, out lambda))
                        {
                            Console.Error.WriteLine(
                                "No se entendió el primer parámetro ($\\lambda$) de la distribución.");
                            Usage();
                            return -3;
                        }

                        var filename = string.Format("generated\\{0} {1} {2}.txt", selected, amount, lambda);
                        var sw = new StreamWriter(filename);

                        random = new PoissonRandom(lambda, sw);
                    }
                    break;

                case Distribution.Uniforme:
                    {
                        if (argsLeft != 2)
                        {
                            Console.Error.WriteLine("Cantidad de argumentos inválidos.");
                            Usage();
                            return -2;
                        }

                        double a, b;
                        if (!double.TryParse(Argument, out a))
                        {
                            Console.Error.WriteLine(
                                "No se entendió el primer parámetro ($a$) de la distribución.");
                            Usage();
                            return -3;
                        }

                        if (!double.TryParse(Argument, out b))
                        {
                            Console.Error.WriteLine(
                                "No se entendió el primer parámetro ($b$) de la distribución.");
                            Usage();
                            return -3;
                        }

                        var filename = string.Format("generated\\{0} {1} {2} {3}.txt", selected, amount, a, b);
                        var sw = new StreamWriter(filename);

                        random = new UniformRandom(a, b, sw);
                    }
                    break;

                default:
                    Console.Error.WriteLine("No sé como llegó aquí.");
                    return -5;
            }

            for (int i = 0; i < amount; i++)
            {
                double tmp = random.NextDouble();
                Console.WriteLine(tmp);
            }

            random.Close();

            return 0;
        }
    }
}
