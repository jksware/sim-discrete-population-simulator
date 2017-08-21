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

namespace PopulationSim
{
    public enum Sex : byte { M = 0, F = 1 }

    public class Person
    {
        public string Name { get; set; }
        public Sex Sex { get; set; }
        public int BirthdayYear { get; set; }
        public int DeceaseYear { get; set; }

        public Relationship Relationship { get; set; }

        public override string ToString()
        {
            string name = string.IsNullOrEmpty(Name) ? (Sex == Sex.M ? "John Doe" : "Jane Doe") : Name;

            // i know is a bad thing to do the thing with the program ...
            return string.Format("{0} ({1})", name, Program.CurrentYear - BirthdayYear); 
        }
    }

    public class Relationship
    {
        public Person Boyfriend;
        public Person Girlfriend;
        public int BeginYear;
        public int ExpNumberChilds;

        public override string ToString()
        {
            return string.Format("{0} and {1}", Boyfriend, Girlfriend);
        }
    }
}
