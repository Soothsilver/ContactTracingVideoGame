﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ContactTracingPrototype.Documents
{
    static class CaseRenderer
    { 
        public static void RenderCases(InlineCollection inlines, List<Person> cases)
        {
            List<Person> aCases = cases.Distinct().ToList();
            if (aCases.Count == 0)
            {
                inlines.Add("(nobody)");
                return;
            }
            int total = aCases.Count;
            int i = 0;
            foreach (var person in aCases)
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
                hyperlink.ContextMenu.Opened += (sender, args) =>
                {
                    hyperlink.ContextMenu.Items.Clear();
                    if (person.Quarantined)
                    {
                        hyperlink.ContextMenu.Items.Add(new MenuItem()
                        {
                            Header = "Quarantine (already quarantined)",
                            IsEnabled = false
                        });
                    }
                    else
                    {
                        hyperlink.ContextMenu.Items.Add(new MenuItem()
                        {
                            Header = "Quarantine",
                            Command = new SimpleCommand(() => person.Quarantine())
                        });
                    }

                    bool scheduledForTesting = person.City.OrderedTests.Contains(person);
                    bool alreadyPositive = person.LastTestResult == PCRTestResult.Positive;
                    hyperlink.ContextMenu.Items.Add(new MenuItem()
                    {
                        Header = scheduledForTesting ? "Test (already scheduled for testing)" :
                            (alreadyPositive ? "Test (tested positive already)" : (person.LastTestResult == PCRTestResult.Negative ? "Test again" : "Test")),
                        IsEnabled = !alreadyPositive && !scheduledForTesting,
                        Command = new SimpleCommand(()=> person.Test() )
                    });
                    hyperlink.ContextMenu.Items.Add(new MenuItem()
                    {
                        Header = person.LastTracedAt == -1 ? "Trace" : "Trace again",
                        Command = new SimpleCommand(() =>
                            {
                                person.Trace();
                                person.EnsureHasDocument();
                            }
                        )
                    });
                };
              
                hyperlink.Click += (sender, args) =>
                {
                    MainWindow.Instance.DocumentBrowser.GoTo(person.EnsureHasDocument());
                };
                inlines.Add(hyperlink);
                i++;
            }
        }

    }
}