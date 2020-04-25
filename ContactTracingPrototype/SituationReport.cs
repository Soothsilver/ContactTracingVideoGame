using System.Collections.Generic;

namespace ContactTracingPrototype
{
    class SituationReport
    {
        public int TestsOrdered;
        public List<Person> PositiveOrdered = new List<Person>();
        public List<Person> PositiveSentinel = new List<Person>();
    }
}