using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ContactTracingPrototype.Documents
{
    class PersonStatusDocument : Document
    {
        public Person Person { get; }

        public PersonStatusDocument(Person person)
        {
            Person = person;
        }

        public override string Title => Person.Name;
        
        public override void Render(TextBlock textBlock)
        {
            textBlock.Inlines.Clear();

            textBlock.Inlines.Add(Person.Name);
            textBlock.Inlines.Add(new LineBreak());
            textBlock.Inlines.Add(new LineBreak());

            textBlock.Inlines.Add("Test status: ");
            switch (Person.LastTestResult)
            {
                case PCRTestResult.NotTested:
                    if (Person.City.OrderedTests.Contains(Person))
                    {
                        textBlock.Inlines.Add("Test results will be available tomorrow.\n");
                    }
                    else
                    {
                        textBlock.Inlines.Add("Never tested. ");
                        Hyperlink testLink = new Hyperlink(new Run("(test)"));
                        textBlock.Inlines.Add(testLink);
                        textBlock.Inlines.Add(new LineBreak());

                        testLink.Click += (sender, args) =>
                        {
                            Person.Test();
                            MainWindow.Instance.DocumentBrowser.Refresh();
                        };
                    }
                    break;
                case PCRTestResult.Positive:
                    textBlock.Inlines.Add($"Confirmed positive (on day {Person.LastTestDate}).\n");
                    break;
                case PCRTestResult.Negative:
                    textBlock.Inlines.Add($"Tested negative (on day {Person.LastTestDate}). ");
                    Hyperlink retestLink = new Hyperlink(new Run("(retest)"));
                    textBlock.Inlines.Add(retestLink);
                    textBlock.Inlines.Add(new LineBreak());

                    retestLink.Click += (sender, args) =>
                    {
                        Person.Test();
                        MainWindow.Instance.DocumentBrowser.Refresh();
                    };
                    break;
            }

            textBlock.Inlines.Add("Clinical status: ");
            Hyperlink checkStatusLink = null;
            if (Person.LastDiseaseStatusCheckAt < 0)
            {
                textBlock.Inlines.Add("Unknown. ");
                checkStatusLink = new Hyperlink(new Run("(ask for status)"));
            }
            else
            {
                switch (Person.LastKnownDiseaseStatus)
                {
                    case DiseaseStage.Susceptible:
                        textBlock.Inlines.Add($"Healthy and susceptible (on day {Person.LastDiseaseStatusCheckAt}). ");
                        checkStatusLink = new Hyperlink(new Run("(ask again)"));
                        break;
                    case DiseaseStage.Asymptomatic:
                    case DiseaseStage.AsymptomaticInfectious1:
                    case DiseaseStage.AsymptomaticInfectious2:
                        textBlock.Inlines.Add($"Asymptomatic (on day {Person.LastDiseaseStatusCheckAt}). ");
                        checkStatusLink = new Hyperlink(new Run("(ask again)"));
                        break;
                    case DiseaseStage.Mild:
                        textBlock.Inlines.Add($"Mild disease (on day {Person.LastDiseaseStatusCheckAt}). ");
                        checkStatusLink = new Hyperlink(new Run("(ask again)"));
                        break;
                    case DiseaseStage.Severe:
                        textBlock.Inlines.Add($"Severe disease (on day {Person.LastDiseaseStatusCheckAt}). ");
                        checkStatusLink = new Hyperlink(new Run("(ask again)"));
                        break;
                    case DiseaseStage.Immune:
                        textBlock.Inlines.Add("Immune.\n");
                        break;
                    case DiseaseStage.Dead:
                        textBlock.Inlines.Add("Dead.\n");
                        break;
                }
            }

            if (checkStatusLink != null)
            {
                textBlock.Inlines.Add(checkStatusLink);
                textBlock.Inlines.Add(new LineBreak());
                checkStatusLink.Click += (sender, args) =>
                {
                    Person.CheckDiseaseStatus();
                    MainWindow.Instance.DocumentBrowser.Refresh();
                };
            }

            textBlock.Inlines.Add("Quarantine status: ");
            if (Person.Quarantined)
            {
                textBlock.Inlines.Add("Quarantined at home.\n");

            }
            else
            {
                textBlock.Inlines.Add("Free. ");
                Hyperlink quarantineLink = new Hyperlink(new Run("(quarantine)"));
                textBlock.Inlines.Add(quarantineLink);
                textBlock.Inlines.Add(new LineBreak());

                quarantineLink.Click += (sender, args) =>
                {
                    Person.Quarantine();
                    MainWindow.Instance.DocumentBrowser.Refresh();
                };
            }

            if (Person.LastTracedAt == -1)
            {
                textBlock.Inlines.Add("Contract tracing not done yet. ");
                Hyperlink traceLink = new Hyperlink(new Run("(trace)"));
                textBlock.Inlines.Add(traceLink);
                
                traceLink.Click += (sender, args) =>
                {
                    Person.Trace();
                    MainWindow.Instance.DocumentBrowser.Refresh();
                };
            }
            else
            {
                textBlock.Inlines.Add("Contracts traced on day " + Person.LastTracedAt + ": ");
                Hyperlink retraceLink = new Hyperlink(new Run("(retrace)"));
                textBlock.Inlines.Add(retraceLink);
                textBlock.Inlines.Add(new LineBreak());
                
                retraceLink.Click += (sender, args) =>
                {
                    Person.Trace();
                    MainWindow.Instance.DocumentBrowser.Refresh();
                };

                textBlock.Inlines.Add(Person.Name + " lives with their family in " + Person.Residence.Name + ". The family consists of ");
                CaseRenderer.RenderCases(textBlock.Inlines, Person.Residence.Family.Where(ppl => ppl != Person).ToList());
                textBlock.Inlines.Add(".\n\n");
                if (Person.AgeCategory == AgeCategory.Child)
                {
                    textBlock.Inlines.Add("This is a child.");
                }
                else
                {
                    textBlock.Inlines.Add(Person.Name + " works at " + Person.Workplace.Name + ", where they met:\n");
                    foreach (Day day in Person.History.Take(Person.LastTracedAt).Reverse<Day>())
                    {
                        textBlock.Inlines.Add("  Day " + day.Date + ": ");
                        CaseRenderer.RenderCases(textBlock.Inlines, day.Contacts.Where(cc => cc.WhereTheyMet is Workplace).Select(c => c.TargetContact).ToList());
                        textBlock.Inlines.Add(new LineBreak());
                    }
                }
            }
        }
    }
}
