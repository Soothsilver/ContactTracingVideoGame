using System.Collections.Generic;
using System.Linq;
using RandomNameGeneratorLibrary;
using Soothsilver.Random;

namespace ContactTracingPrototype
{
    class NameGenerator
    {
        PersonNameGenerator PersonNameGenerator = new PersonNameGenerator();
        HashSet<string> UsedNames = new HashSet<string>();
        
        public string GenerateRandomFirstName()
        {
            return PersonNameGenerator.GenerateRandomFirstName();
        }

        public string GenerateRandomLastName()
        {
            while (true)
            {
                string name = PersonNameGenerator.GenerateRandomLastName();
                if (!UsedNames.Contains(name))
                {
                    UsedNames.Add(name);
                    return name;
                }
            }
        }
    }
}