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

using System.Collections.Generic;

namespace PopulationSim
{
    public abstract class Event
    {
        protected Event(int year)
        {
            this.Year = year;
        }

        public readonly int Year;

        public class Comparer : IComparer<Event>
        {
            public int Compare(Event x, Event y)
            {
                return x.Year.CompareTo(y.Year);
            }
        }
    }
    
    public class RelationshipBeginEvent : Event
    {
        public RelationshipBeginEvent(int year)
            : base(year)
        {
        }

        public Person Starter;
        public Relationship Relationship;
    }


    public class RelationshipEndEvent : Event
    {
        public RelationshipEndEvent(int year)
            : base(year)
        {
        }

        public Relationship Relationship;
    }

    public class PregnancyEvent : Event
    {
        public PregnancyEvent(int year)
            : base(year)
        {
        }

        public Relationship Relationship;
        public Person Unborn;
    }

    public class BirthEvent : Event
    {
        public BirthEvent(int year)
            : base(year)
        {
        }

        public Relationship Relationship;
        public Person Newborn;
    }

    public class DeadEvent : Event
    {
        public DeadEvent(int year)
            : base(year)
        {
        }

        public Person Deceased;
    }

    public class BeginSimEvent : Event
    {
        // it always starts at year 0
        public BeginSimEvent() : base(0)
        {
        }
    }

    public class EndSimEvent : Event
    {
        public EndSimEvent(int year)
            : base(year)
        {
        }
    }
}
