using System.Windows.Controls;

namespace ContactTracingPrototype.Documents
{
    class NewGameDocument : DailyUpdateDocument
    {
        public NewGameDocument(SituationReport situationReport)
            : base("Day 1. Welcome to Sorpigal.", situationReport)
        {
        }

        protected override void RenderIntro(TextBlock textBlock)
        {
            textBlock.Text =
@"Welcome to Sorpigal
You are the epidemiologist responsible for the town of Sorpigal. 

Your objective: Contain the outbreak of the disease. You win when 10 days pass without a new case.

The first cases of the disease have already appeared in the town. Look at the situation update and take action!

Situation update.

";
        }
    }
}
