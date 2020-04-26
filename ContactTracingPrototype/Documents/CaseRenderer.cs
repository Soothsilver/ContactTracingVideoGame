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
                    Command = new SimpleCommand(()=> person.Quarantine() )
                });
                hyperlink.ContextMenu.Items.Add(new MenuItem()
                {
                    Header = "Test",
                    Command = new SimpleCommand(()=> person.Test() )
                });
                hyperlink.ContextMenu.Items.Add(new MenuItem()
                {
                    Header = "Trace",
                    Command = new SimpleCommand(() => person.Trace())
                });
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