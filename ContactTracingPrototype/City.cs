using System;
using System.Collections.Generic;
using System.Linq;
using Soothsilver.Random;

namespace ContactTracingPrototype
{
    class City
    {
        public List<Person> People = new List<Person>();
        public List<Area> Areas = new List<Area>();
        public List<ApartmentBuilding> ApartmentBuildings = new List<ApartmentBuilding>();
        public string LastLog = "";
        public string EpidemiologicalCurveLog = "";
        public InfectionCurveModel Model = new InfectionCurveModel();


        public City()
        {
            int FOUR_PERSON_FAMILIES = 15;
            int THREE_PERSON_FAMILIES = 10;
            int TWO_PERSON_FAMILIIES = 10;
            int ONE_PERSON_FAMILIES = 5; // 40 children
            
            // Create houses and people
            int BUILDING_COUNT = (int)Math.Ceiling((FOUR_PERSON_FAMILIES + THREE_PERSON_FAMILIES + TWO_PERSON_FAMILIIES + ONE_PERSON_FAMILIES) / 5f);
            for (int i = 0; i < BUILDING_COUNT; i++)
            {
                ApartmentBuildings.Add(new ApartmentBuilding("Residence Building " + (i+1)));
                Areas.Add(ApartmentBuildings.Last());
            }
            for (int i = 0; i < FOUR_PERSON_FAMILIES; i++)
            {
                SpawnFamily(4);
            }
            for (int i = 0; i < THREE_PERSON_FAMILIES; i++)
            {
                SpawnFamily(3);
            }
            for (int i = 0; i < TWO_PERSON_FAMILIIES; i++)
            {
                SpawnFamily(2);
            }
            for (int i = 0; i < ONE_PERSON_FAMILIES; i++)
            {
                SpawnFamily(1);
            }
            
            // Workplaces
            Workplace wpFactory = new Workplace("Factory");
            List<Workplace> wpSchool = new List<Workplace>();
            Workplace wpTemple = new Workplace("Temple");
            Workplace wpClinic = new Workplace("Clinic");
            Workplace wpSupermarket = new Workplace("Supermarket");
            Workplace wpOffice = new Workplace("Office");
            Workplace wpPub = new Workplace("Pub");
            Workplace[] workplaces = new [] { wpFactory, wpTemple, wpClinic, wpSupermarket, wpOffice, wpPub};
            Areas.AddRange(workplaces);
            foreach (Person person in People)
            {
                if (person.AgeCategory == AgeCategory.Adult)
                {
                    int min = workplaces.Min(wp => wp.Workers.Count);
                    Workplace whereIAm = workplaces.Where(wp => wp.Workers.Count == min).ToList().GetRandom();
                    person.Workplace = whereIAm;
                    whereIAm.Workers.Add(person);
                }
            }
            
            // Add infections
            for (int i = 0; i < 4; i++)
            {
                List<Person> susceptibles = People.Where(ppl => ppl.DiseaseStatus == DiseaseStage.Susceptible).ToList();
                Person target = susceptibles.GetRandom();
                target.DiseaseStatus = DiseaseStage.Mild;
            }
        }

        private int creatingPersonId = 0;
        private int creatingHouseId = 0;

        private void SpawnFamily(int familyMemberCount)
        {
            Residence residence = new Residence("Apartment #" + (creatingHouseId + 1), ApartmentBuildings[(creatingHouseId / 5)]);
            Areas.Add(residence);
            if (familyMemberCount > 2)
            {
                for (int ci = 2; ci < familyMemberCount; ci++)
                {
                    Person child = new Person("Person #" + (1 + creatingPersonId), AgeCategory.Child, residence, null);
                    People.Add(child);
                    residence.Family.Add(child);
                    creatingPersonId++;
                }
            }
            for (int ci = 0; ci < Math.Min(2, familyMemberCount); ci++)
            {
                Person adult = new Person("Person #" + (1 + creatingPersonId), AgeCategory.Adult, residence, null);
                People.Add(adult);
                residence.Family.Add(adult);
                creatingPersonId++;
            }
            creatingHouseId++;
        }

        public override string ToString()
        {
            return "Population: " + (People.Count) + "\nBy disease stage:\n" +
                   string.Join("\n", Statics.AllStages.Select(stage => People.Count(ppl => ppl.DiseaseStatus == stage) + " " + stage.ToString()));
        }

        public int Today = 0;
        public void EndDay()
        {
            
            foreach (Person person in People)
            {
                Day day = new Day();
                person.History.Add(day);
            }
            
            // Quarantine contacts
            foreach (Person person in People)
            {
                if (person.DiseaseStatus == DiseaseStage.Mild && !person.Quarantined)
                {
                    person.Quarantined = true;
                    foreach (Day day in person.History.Reverse<Day>().Take(7))
                    {
                        foreach (Contact contact in day.Contacts)
                        {
                            contact.TargetContact.Quarantined = true;
                        }
                    }
                }
            }
            

            // Form contacts
            LastLog = "Stuff happened today.";
            foreach (Area area in Areas)
            {
                if (area is Residence residence)
                {
                    foreach (Person one in residence.Family)
                    {
                        foreach (Person two in residence.Family)
                        {
                            if (one != two)
                            {
                                one.History[Today].Contacts.Add(new Contact(two));
                            }
                        }
                    }
                }

                if (area is Workplace workplace)
                {
                    int count = workplace.PresentWorkers.Count();
                    for (int i = 0; i < count; i++)
                    {
                        Person w1 = workplace.PresentWorkers.GetRandom();
                        Person w2 = workplace.PresentWorkers.GetRandom();
                        w1.History[Today].Contacts.Add(new Contact(w2));
                        w2.History[Today].Contacts.Add(new Contact(w1));
                    }
                }
            }
            
            // Progress transmission
            foreach (Person person in People)
            {
                if (person.IsInfectious)
                {
                    foreach (Contact contact in person.History[Today].Contacts)
                    {
                        if (contact.TargetContact.DiseaseStatus == DiseaseStage.Susceptible)
                        {
                            if (R.PercentChance(10))
                            {
                                contact.TargetContact.DiseaseStatus = DiseaseStage.Asymptomatic;
                            }
                        }
                    }
                }
            }

            // Progress disease
            foreach (Person person in People)
            {
                int percent = R.Next(0, 100);
                if (person.DiseaseStatus == DiseaseStage.Asymptomatic)
                {
                    if (percent < 20)
                    {
                        person.DiseaseStatus = DiseaseStage.AsymptomaticInfectious1;
                    }
                }
                else if (person.DiseaseStatus == DiseaseStage.AsymptomaticInfectious1)
                {
                    person.DiseaseStatus = DiseaseStage.AsymptomaticInfectious2;
                }
                else if (person.DiseaseStatus == DiseaseStage.AsymptomaticInfectious2)
                {
                    person.DiseaseStatus = DiseaseStage.Mild;
                }
                else if (person.DiseaseStatus == DiseaseStage.Mild)
                {
                    if (person.AgeCategory == AgeCategory.Child)
                    {
                        if (percent < 79)
                        {
                            // do nothing
                        }
                        else if (percent < 99)
                        {
                            person.DiseaseStatus = DiseaseStage.Immune;
                        }
                        else
                        {
                            person.DiseaseStatus = DiseaseStage.Severe;
                        }
                    }
                    else if (person.AgeCategory == AgeCategory.Adult)
                    {
                        if (percent < 85)
                        {
                            // do nothing
                        }
                        else if (percent < 85 + 12)
                        {
                            person.DiseaseStatus = DiseaseStage.Immune;
                        }
                        else
                        {
                            person.DiseaseStatus = DiseaseStage.Severe;
                        }
                    }
                }
                else if (person.DiseaseStatus == DiseaseStage.Severe)
                {
                    if (person.AgeCategory == AgeCategory.Child)
                    {
                        if (percent < 80)
                        {
                            // do nothing
                        }
                        else if (percent < 99)
                        {
                            person.DiseaseStatus = DiseaseStage.Immune;
                        }
                        else
                        {
                            person.DiseaseStatus = DiseaseStage.Dead;
                        }
                    }
                    else if (person.AgeCategory == AgeCategory.Adult)
                    {
                        if (percent < 80)
                        {
                            // do nothing
                        }
                        else if (percent < 80 + 16)
                        {
                            person.DiseaseStatus = DiseaseStage.Immune;
                        }
                        else
                        {
                            person.DiseaseStatus = DiseaseStage.Dead;
                        }
                    }
                }
            }
            EpidemiologicalCurveLog +=  "Day " + (1+Today) + ": " + string.Join("/", Statics.AllStages.Select(stage => People.Count(ppl => ppl.DiseaseStatus == stage))) + "\n";
            Model.AddCurrentStatus(this);
            // Next day:
            Today++;
        }
    }

    internal class Workplace : Area
    {
        public Workplace(string name) : base(name)
        {
        }

        public List<Person> Workers { get; set; } = new List<Person>();
        public List<Person> PresentWorkers => Workers.Where(w => w.DiseaseStatus != DiseaseStage.Severe && 
                                                                 w.DiseaseStatus != DiseaseStage.Dead &&
                                                                 !w.Quarantined).ToList();
    }

    internal class Day
    {
        public List<Contact> Contacts = new List<Contact>();
    }

    internal class Contact
    {
        public Person TargetContact;

        public Contact(Person two)
        {
            TargetContact = two;
        }
    }


    public static class Statics
    {
        public static DiseaseStage[] AllStages = (DiseaseStage[]) Enum.GetValues(typeof(DiseaseStage));
    }
    internal class ApartmentBuilding : Area
    {
        public ApartmentBuilding(string name) : base(name)
        {
        }
    }

    internal class Person
    {
        public string Name;
        public AgeCategory AgeCategory;
        public Residence Residence;
        public Area Workplace;
        public DiseaseStage DiseaseStatus;
        public List<Day> History = new List<Day>();

        public Person(string name, AgeCategory ageCategory, Residence residence, Area workplace)
        {
            Name = name;
            AgeCategory = ageCategory;
            Residence = residence;
            Workplace = workplace;
            DiseaseStatus = DiseaseStage.Susceptible;
        }

        public bool IsInfectious => DiseaseStatus >= DiseaseStage.AsymptomaticInfectious1 && DiseaseStatus < DiseaseStage.Immune;
        public bool IsActiveCase => DiseaseStatus > DiseaseStage.Susceptible && DiseaseStatus < DiseaseStage.Immune;
        public bool Quarantined { get; set; }
    }

    internal class Residence : Area
    {
        public Residence(string name, ApartmentBuilding apartmentBuilding) : base(name)
        {
        }
        public List<Person> Family = new List<Person>();
    }

    internal class Area
    {
        public string Name { get; }

        protected Area(string name)
        {
            Name = name;
        }
    }

    public enum DiseaseStage
    {
        Susceptible,
        Asymptomatic,
        AsymptomaticInfectious1,
        AsymptomaticInfectious2,
        Mild,
        Severe,
        Immune,
        Dead
    }

    internal enum AgeCategory
    {
        Child,
        Adult
    }
}