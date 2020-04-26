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
            textBlock.Inlines.Add("Test status: TODO\n");
            textBlock.Inlines.Add("Clinical status: TODO\n");
            textBlock.Inlines.Add("Quarantine status: TODO\n");
            if (Person.LastTracedAt == -1)
            {
                textBlock.Inlines.Add("Contract tracing not yet done. ");
                textBlock.Inlines.Add(new Hyperlink(new Run("(trace (TODO))")));
            }
            else
            {
                textBlock.Inlines.Add("Contracts traced on day " + Person.LastTracedAt + ": ");
                textBlock.Inlines.Add(new Hyperlink(new Run("(retrace (TODO))")));
                textBlock.Inlines.Add(new LineBreak());
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
