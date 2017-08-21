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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RandomGeneration;

namespace PopulationSim
{
    internal class Program
    {
        public const double SqrtTwoPi = 2.506628274631000502415765284811; // sqrt(2 * pi)

        private const int LifeExpectancy = 70;
        private const int NewCoupleLambda = 18;
        private const int MaxCoupleAgeDiff = 5;

        private const double FirstChildLambda = 1.0 / 8;

        private const int NormalPregnancy = 28;
        private const double NormalPregnancyStdDev = 2.8284271247461900976033774484194; // sqrt(8)

        private const int NormalChilds = 2;
        private const double NormalChildsStdDev = 2.4494897427831780981972840747059; // sqrt(6)

        private static readonly PoissonRandom LifeExpectancyPoisson = new PoissonRandom(LifeExpectancy);
        private static readonly PoissonRandom NewRelationPoisson = new PoissonRandom(NewCoupleLambda);

        private static readonly NormalRandom NumberChildsNormal = new NormalRandom(NormalChilds, NormalChildsStdDev);
        private static readonly ExponentialRandom FirstChildExponential = new ExponentialRandom(FirstChildLambda);

        private static string[] _maleNames;
        private static string[] _femaleNames;
        
        public static double Normal(double x, double μ, double σ)
        {
            double toSqr = (x - μ) / σ;
            return 1 / (σ * SqrtTwoPi) * Math.Exp(-1 / 2 * toSqr * toSqr);
        }

        public static bool SheIsKnockedUp(double x)
        {
            return Rnd.NextDouble() < Normal(x, NormalPregnancy, NormalPregnancyStdDev);
        }

        public static bool IsABoy()
        {
            return Rnd.NextDouble() < 0.5;
        }

        public static bool CoupleBreakup(int age)
        {
            if (age < 20)
                return Rnd.NextDouble() < 0.7;
            if (age < 28)
                return Rnd.NextDouble() < 0.5;
            return Rnd.NextDouble() < 0.2;
        }

        public static int CurrentYear;
        private static readonly Random Rnd = new Random();

        public static List<Person> LivePersons = new List<Person>();
        public static List<Relationship> ActiveRelationships = new List<Relationship>();
        public static PriorityQueue<Event> EventsQueue = new PriorityQueue<Event>(new Event.Comparer());

        public static IEnumerable<Event> PopulationEvents()
        {
            CurrentYear = 0;

            while (EventsQueue.Count > 0)
            {
                var processEvent = EventsQueue.Pop();

                // hasta ahora no sirve de nada
                int yearsDelta = CurrentYear - processEvent.Year;
                CurrentYear = processEvent.Year;

                var beginSimEvent = processEvent as BeginSimEvent;
                if (beginSimEvent != null)
                {
                    // todas las personas al momento de iniciar la simulacion tienen BirthdayYear negativo o cero,
                    // dado que se toma el comienzo como año cero.

                    foreach (var person in LivePersons)
                    {
                        // hay que generar eventos con tiempo < 0
                        // puedo sin embargo generar eventos de nacimientos para cada uno de los vivos ...
                        EventsQueue.Add(new BirthEvent(person.BirthdayYear) { Newborn = person });
                    }

                    // ... y eliminarlos de la lista de vivos (la simulacion ahora recomienza donde nacio el primero).
                    LivePersons.Clear();

                    continue;
                }

                var endSimEvent = processEvent as EndSimEvent;
                if (endSimEvent != null)
                {
                    yield break;
                }

                var relationshipBeginEvent = processEvent as RelationshipBeginEvent;
                if (relationshipBeginEvent != null)
                {
                    Person starter = relationshipBeginEvent.Starter;

                    // if the guy or girl is dead continue ...
                    if (starter.DeceaseYear < CurrentYear)
                        continue;

                    // if either of them is already dating
                    if (starter.Relationship != null)
                        continue;

                    // los viudos o viudas no se emparejan nuevamente
                    List<Person> pretenders = LivePersons.
                        Where(p => p.Relationship == null && p.Sex != starter.Sex &&
                                   Math.Abs(p.BirthdayYear - starter.BirthdayYear) <= MaxCoupleAgeDiff).ToList();

                    if (pretenders.Count == 0)
                        // so sorry, none at the time
                        continue;

                    Person other = pretenders[Rnd.Next(pretenders.Count)];

                    Person he = starter.Sex == Sex.M ? starter : other;
                    Person she = starter.Sex == Sex.F ? starter : other;

                    int expNumberChilds = NumberChildsNormal.Next();

                    var relationship = new Relationship()
                                           {
                                               BeginYear = CurrentYear,
                                               Boyfriend = he,
                                               Girlfriend = she,
                                               ExpNumberChilds = expNumberChilds
                                           };

                    // set that both are dating
                    he.Relationship = relationship;
                    she.Relationship = relationship;
                    relationshipBeginEvent.Relationship = relationship;

                    for (int i = 0; i < expNumberChilds; i++)
                    {
                        int pregnancy = CurrentYear + FirstChildExponential.Next() - 1;
                        bool isABoy = IsABoy();
                        Person unborn = new Person()
                                            {
                                                Name = isABoy ? PickMaleName() : PickFemaleName(),
                                                BirthdayYear = pregnancy + 1,
                                                Sex = isABoy ? Sex.M : Sex.F
                                            };
                        EventsQueue.Add(new PregnancyEvent(pregnancy)
                                            {
                                                Relationship = relationship,
                                                Unborn = unborn
                                            });
                    }

                    // cuando la relacion acaba ?

                    var geometricRandom = new GeometricRandom(0.4);
                    int relationEnds = CurrentYear + geometricRandom.Next();

                    EventsQueue.Add(new RelationshipEndEvent(relationEnds) {Relationship = relationship});

                    yield return relationshipBeginEvent;
                }

                var pregnancyEvent = processEvent as PregnancyEvent;
                if (pregnancyEvent != null)
                {
                    // she died before expecting a child !
                    if (pregnancyEvent.Relationship.Girlfriend.DeceaseYear < CurrentYear)
                        continue;

                    int herAge = CurrentYear - pregnancyEvent.Relationship.Girlfriend.BirthdayYear;

                    // todo comentar esto para ver mas largo el cuento !
                    //if (SheIsKnockedUp(herAge))
                        EventsQueue.Add(new BirthEvent(CurrentYear + 1)
                                                {
                                                    Newborn = pregnancyEvent.Unborn,
                                                    Relationship = pregnancyEvent.Relationship
                                                });

                    yield return pregnancyEvent;
                }

                var birthEvent = processEvent as BirthEvent;
                if (birthEvent != null)
                {
                    // she died whilst expecting a child !
                    if (birthEvent.Relationship != null && birthEvent.Relationship.Girlfriend.DeceaseYear < CurrentYear)
                        continue;

                    var person = birthEvent.Newborn;
                    int timeTillDead = LifeExpectancyPoisson.Next();
                    int deadDate = CurrentYear + timeTillDead;

                    LivePersons.Add(person);

                    int relationDate = CurrentYear + NewRelationPoisson.Next();

                    if (deadDate > relationDate)
                    {
                        // if he or she is not dead to soon, schedule him/her to meet someone
                        EventsQueue.Add(new RelationshipBeginEvent(relationDate) { Starter = person });
                    }

                    person.DeceaseYear = deadDate;
                    EventsQueue.Add(new DeadEvent(deadDate) { Deceased = person });

                    yield return birthEvent;
                }

                var deadEvent = processEvent as DeadEvent;
                if (deadEvent != null)
                {
                    var person = deadEvent.Deceased;
                    LivePersons.Remove(person);

                    yield return deadEvent;
                }
            }
        }

        public static string PickMaleName()
        {
            return _maleNames[Rnd.Next(_maleNames.Length)];
        }

        public static string PickFemaleName()
        {
            return _femaleNames[Rnd.Next(_femaleNames.Length)];
        }

        static int Main(string[] args)
        {
            int totalSimYears;

            try
            {
                var maleNamesReader = new StreamReader("maleNames.txt");
                var femaleNamesReader = new StreamReader("femaleNames.txt");

                _maleNames = maleNamesReader.ReadToEnd().Split('\r').Select(x => x.Trim('\n')).ToArray();
                _femaleNames = femaleNamesReader.ReadToEnd().Split('\r').Select(x => x.Trim('\n')).ToArray();

                var streamReader = new StreamReader("in.txt");

                if (!int.TryParse(streamReader.ReadLine(), out totalSimYears))
                {
                    Console.Error.WriteLine("The file does not contain the number of years to simulate.");
                    return -1;
                }

                int line = 1;
                while (!streamReader.EndOfStream)
                {
                    string[] person = streamReader.ReadLine().Split(' ');
                    int age;
                    Sex sex;

                    if (!Sex.TryParse(person[0], out sex))
                    {
                        Console.Error.WriteLine("That is not a sex (M or F) on line {0}.", line);
                        return -1;
                    }

                    if (!int.TryParse(person[1], out age))
                    {
                        Console.Error.WriteLine("That is not an age number on line {0}.", line);
                        return -1;
                    }

                    LivePersons.Add(new Person()
                                        {
                                            Name = sex == Sex.M ? PickMaleName() : PickFemaleName(),
                                            BirthdayYear = -age, // it is -age, not a typo
                                            Sex = sex
                                        });

                    line++;
                }

                streamReader.Close();
            }
            catch (Exception exc)
            {
                Console.Error.WriteLine("Error on reading input file: {0}", exc.Message);
                return -2;
            }

            EventsQueue.Add(new BeginSimEvent());
            EventsQueue.Add(new EndSimEvent(totalSimYears));

            foreach (var populationEvent in PopulationEvents())
            {
#if DEBUG
                Console.WriteLine();
                Console.WriteLine("The year is {0} with {1}. ", populationEvent.Year, populationEvent.GetType().Name);

                Console.Write("The livings are: ");
                foreach (var livePerson in LivePersons)
                    Console.Write(livePerson + " ");
                Console.WriteLine();

                var relationBeginEvent = populationEvent as RelationshipBeginEvent;
                if (relationBeginEvent != null)
                {
                    Console.WriteLine("New relationship, they are {0}. They will have {1} childs.",
                        relationBeginEvent.Relationship, relationBeginEvent.Relationship.ExpNumberChilds);
                    continue;
                }

                var relationshipEndEvent = populationEvent as RelationshipEndEvent;
                if (relationshipEndEvent != null)
                {
                    Console.WriteLine("Relationship is over, the one of {0}.",
                                      relationshipEndEvent.Relationship);
                    continue;
                }

                var pregnancyEvent = populationEvent as PregnancyEvent;
                if (pregnancyEvent != null)
                {
                    Console.WriteLine("New pregnancy, of {0}. The unborn is {1}.",
                        pregnancyEvent.Relationship, pregnancyEvent.Unborn);
                    continue;
                }

                var birthEvent = populationEvent as BirthEvent;
                if (birthEvent != null)
                {
                    if (birthEvent.Relationship == null)
                        Console.WriteLine("New birth, born out of nowhere. His/her name is {0}.", birthEvent.Newborn);
                    else
                        Console.WriteLine("New birth, the newborn is {0} from mother {1} and father {2}.",
                            birthEvent.Newborn, birthEvent.Relationship.Boyfriend, birthEvent.Relationship.Girlfriend);
                    continue;
                }

                var deadEvent = populationEvent as DeadEvent;
                if (deadEvent != null)
                {
                    Console.WriteLine("We had suffer a lost: long live {0}, because he/she is dead! He/she had a great life.",
                        deadEvent.Deceased);
                }
#endif
            }

            try
            {
                var streamWriter = new StreamWriter("out.txt");
                streamWriter.WriteLine(totalSimYears);
                foreach (var livePerson in LivePersons)
                    streamWriter.WriteLine("{0} {1}", livePerson.Sex, totalSimYears - livePerson.BirthdayYear);

                streamWriter.Flush();
                streamWriter.Close();
            }
            catch (Exception exc)
            {
                Console.Error.WriteLine("Error on writing output file: {0}", exc.Message);
                return -2;
            }

            return 0;
        }
    }
}
