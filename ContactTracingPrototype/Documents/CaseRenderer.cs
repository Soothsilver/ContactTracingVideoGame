using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ContactTracingPrototype.Documents
{
    static class CaseRenderer
    { 
        public static void RenderCases(InlineCollection inlines, List<Person> cases)
        {
            if (cases.Count == 0)
            {
                inlines.Add("(nobody)");
                return;
            }
            int total = cases.Count;
            int i = 0;
            foreach (var person in cases)
            {
                if (i == 0)
                {
                    
                }
                else if (i == total - 1)
                {
                    inlines.Add(" and ");
                }
                else
                {
                    inlines.Add(", ");
                }

                Hyperlink hyperlink = new Hyperlink(new Run(person.Name));
                hyperlink.ContextMenu = new ContextMenu();
                hyperlink.ContextMenu.Items.Add(new MenuItem()
                {
                    Header = "Quarantine",
                    Command = new SimpleCommand(()=> person.Quarantined = true )
                });
                hyperlink.ContextMenu.Items.Add(new MenuItem()
                {
                    Header = "Test",
                    Command = new SimpleCommand(()=> person.City.OrderedTests.Add(person) )
                });
                hyperlink.ContextMenu.Items.Add(new MenuItem()
                {
                    Header = "Trace",
                    Command = new SimpleCommand(() =>
                        {
                            person.LastTracedAt = person.City.Today;
                            EnsureHasDocument(person);
                        }
                    )
                });
                hyperlink.Click += (sender, args) =>
                {
                    PersonStatusDocument psd = EnsureHasDocument(person);
                    MainWindow.Instance.documentsListBox.SelectedItem = psd;
                };
                inlines.Add(hyperlink);
                i++;
            }
        }

        public static PersonStatusDocument EnsureHasDocument(Person person)
        {
            PersonStatusDocument document = person.City.allDocuments.OfType<PersonStatusDocument>().FirstOrDefault(psd => psd.Person == person);
            if (document == null)
            {
                document = new PersonStatusDocument(person);
                person.City.allDocuments.Add(document);
            }

            return document;
        }
    }
}