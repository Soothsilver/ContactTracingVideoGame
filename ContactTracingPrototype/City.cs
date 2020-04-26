using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using ContactTracingPrototype.Documents;
using RandomNameGeneratorLibrary;
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
        public ConfirmedCasesCurve ConfirmedCasesCurve = new ConfirmedCasesCurve();
        public List<Person> OrderedTests = new List<Person>();
        public List<Person> InitialSentinelTests = new List<Person>();
        public List<SituationReport> DailyUpdates = new List<SituationReport>();
        public ObservableCollection<Document> allDocuments = new ObservableCollection<Document>();

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
            
            // Add infections at day 0
            for (int i = 0; i < 4; i++)
            {
                List<Person> susceptibles = People.Where(ppl => ppl.DiseaseStatus == DiseaseStage.Susceptible).ToList();
                Person target = susceptibles.GetRandom();
                target.DiseaseStatus = DiseaseStage.Mild;
                if (i <= 1)
                {
                    InitialSentinelTests.Add(target);
                }
            }

            EndDay();
        }

        public bool OutbreakEnded { get; set; }

        private void AdministerTest(Person target, SituationReport report, bool isSentinel)
        {
            if (target.DiseaseStatus >= DiseaseStage.AsymptomaticInfectious1 && target.DiseaseStatus <= DiseaseStage.Immune)
            {
                target.LastTestResult = PCRTestResult.Positive;
                if (isSentinel)
                {
                    report.PositiveSentinel.Add(target);
                }
                else
                {
                    report.PositiveOrdered.Add(target);
                }
            }
            else
            {
                target.LastTestResult = PCRTestResult.Negative;
            }

            target.LastTestDate = Today;
        }

        private int creatingPersonId = 0;
        private int creatingHouseId = 0;
        NameGenerator nameGenerator = new NameGenerator();
        
        private void SpawnFamily(int familyMemberCount)
        {
            string familyName = nameGenerator.GenerateRandomLastName();
            Residence residence = new Residence("Apartment " + familyName, ApartmentBuildings[(creatingHouseId / 5)]);
            Areas.Add(residence);
            if (familyMemberCount > 2)
            {
                for (int ci = 2; ci < familyMemberCount; ci++)
                {
                    Person child = new Person(nameGenerator.GenerateRandomFirstName() + " " + familyName, AgeCategory.Child, residence, null, this);
                    People.Add(child);
                    residence.Family.Add(child);
                    creatingPersonId++;
                }
            }
            for (int ci = 0; ci < Math.Min(2, familyMemberCount); ci++)
            {
                Person adult = new Person(nameGenerator.GenerateRandomFirstName() + " " + familyName, AgeCategory.Adult, residence, null, this);
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
            SituationReport report = new SituationReport();
            foreach (Person person in People)
            {
                Day day = new Day(Today);
                person.History.Add(day);
            }
            
            // Administer tests
            foreach (Person sentinelTest in InitialSentinelTests)
            {
                AdministerTest(sentinelTest, report, true);
            }
            InitialSentinelTests.Clear();
            report.TestsOrdered = OrderedTests.Count;
            foreach (Person orderedTest in OrderedTests)
            {
                AdministerTest(orderedTest, report, false);
            }
            OrderedTests.Clear();
            foreach (Person person in People.Where(ppl => ppl.DiseaseStatus == DiseaseStage.Mild && ppl.LastTestResult != PCRTestResult.Positive))
            {
                if (R.PercentChance(10))
                    AdministerTest(person, report, true);
            }
            foreach (Person person in People.Where(ppl => ppl.DiseaseStatus == DiseaseStage.Severe && ppl.LastTestResult != PCRTestResult.Positive))
            {
                if (R.PercentChance(80))
                    AdministerTest(person, report, true);
            }
            
            // Quarantine contacts
//            foreach (Person person in People)
//            {
//                if (person.DiseaseStatus == DiseaseStage.Mild && !person.Quarantined)
//                {
//                    person.Quarantined = true;
//                    foreach (Day day in person.History.Reverse<Day>().Take(7))
//                    {
//                        foreach (Contact contact in day.Contacts)
//                        {
//                            contact.TargetContact.Quarantined = true;
//                        }
//                    }
//                }
//            }

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
                                one.History[Today].Contacts.Add(new Contact(two, residence));
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
                        w1.History[Today].Contacts.Add(new Contact(w2, workplace));
                        w2.History[Today].Contacts.Add(new Contact(w1, workplace));
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
            ConfirmedCasesCurve.AddCurrentStatus(report);
            DailyUpdates.Add(report);
            // Next day:
            Today++;
        }
    }

    internal enum PCRTestResult
    {
        None,
        Positive,
        Negative
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
        public Day(int date)
        {
            Date = date;
        }
        public int Date;
        public List<Contact> Contacts = new List<Contact>();
    }

    internal class Contact
    {
        public Person TargetContact;
        public Area WhereTheyMet;

        public Contact(Person two, Area whereTheyMet)
        {
            TargetContact = two;
            WhereTheyMet = whereTheyMet;
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
        public City City;
        public AgeCategory AgeCategory;
        public Residence Residence;
        public Area Workplace;
        public DiseaseStage DiseaseStatus;
        public List<Day> History = new List<Day>();
        public int LastTracedAt = -1;
        public DiseaseStage LastKnownDiseaseStatus;
        public int LastDiseaseStatusCheckAt = -1;

        public Person(string name, AgeCategory ageCategory, Residence residence, Area workplace, City city)
        {
            Name = name;
            AgeCategory = ageCategory;
            City = city;
            Residence = residence;
            Workplace = workplace;
            DiseaseStatus = DiseaseStage.Susceptible;
        }

        public bool IsInfectious => DiseaseStatus >= DiseaseStage.AsymptomaticInfectious1 && DiseaseStatus < DiseaseStage.Immune;
        public bool IsActiveCase => DiseaseStatus > DiseaseStage.Susceptible && DiseaseStatus < DiseaseStage.Immune;
        public bool Quarantined { get; set; }
        public PCRTestResult LastTestResult { get; set; } = PCRTestResult.None;
        public int LastTestDate { get; set; } = -1;
        public PCRTestResult LastTestResult { get; set; }
        public int LastTestDate { get; set; }

        public void Quarantine()
        {
            this.Quarantined = true;
        }

        public void Test()
        {
            this.City.OrderedTests.Add(this);
        }

        public void Trace()
        {
            this.LastTracedAt = this.City.Today;
            this.EnsureHasDocument();
        }

        public void CheckDiseaseStatus()
        {
            this.LastDiseaseStatusCheckAt = this.City.Today;
            this.LastKnownDiseaseStatus = this.DiseaseStatus;
        }

        public PersonStatusDocument EnsureHasDocument()
        {
            PersonStatusDocument document = this.City.allDocuments.OfType<PersonStatusDocument>().FirstOrDefault(psd => psd.Person == this);
            if (document == null)
            {
                document = new PersonStatusDocument(this);
                this.City.allDocuments.Add(document);
            }

            return document;
        }
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