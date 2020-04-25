using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ContactTracingPrototype.Documents
{
    class DailyUpdateDocument : Document
    {
        private readonly SituationReport situationReport;

        public DailyUpdateDocument(string title, SituationReport situationReport)
        {
            this.Title = title;
            this.situationReport = situationReport;
        }

        public override string Title { get; }

        protected virtual void RenderIntro(TextBlock textBlock)
        {
            textBlock.Inlines.Add( this.Title + " situation update.\n\n");
        }

        public override void Render(TextBlock textBlock)
        {
            textBlock.Inlines.Clear();
            
            this.RenderIntro(textBlock);

            if (situationReport.TestsOrdered > 0)
            {
                textBlock.Inlines.Add($"Yesterday, you ordered {situationReport.TestsOrdered} tests.\n");
                if (situationReport.PositiveOrdered.Count == 0)
                {
                    textBlock.Inlines.Add("All returned negative.\n");
                }
                else if (situationReport.PositiveOrdered.Count == 1)
                {
                    textBlock.Inlines.Add("One retuned positive.\n");
                }
                else
                {
                    textBlock.Inlines.Add("Some of them were positive.\n");
                }
                textBlock.Inlines.Add(new LineBreak());
            }

            textBlock.Inlines.Add("Sentinel testing.\n");
            if (situationReport.PositiveSentinel.Count == 0)
            {
                textBlock.Inlines.Add("Sentinel testing did not reveal any cases.\n");
            }
            else if (situationReport.PositiveSentinel.Count == 1)
            {
                textBlock.Inlines.Add("One person had symptoms and was tested positive: ");
                textBlock.Inlines.Add(situationReport.PositiveSentinel[0].Name);
                textBlock.Inlines.Add(".\n");

            }
            else
            {
                textBlock.Inlines.Add("Some people were tested because they had symptoms, and they tested positive: ");
                textBlock.Inlines.Add(string.Join(", ", situationReport.PositiveSentinel.Select(p => p.Name)));
                textBlock.Inlines.Add(".\n");
            }
        }
    }
}
