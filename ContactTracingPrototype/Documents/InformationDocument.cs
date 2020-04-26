using System.Windows.Controls;
using ContactTracingPrototype.Documents;

namespace ContactTracingPrototype
{
    internal class InformationDocument : Document
    {
        public InformationDocument(string title, string text)
        {
            Title = title;
            Text = text;
        }

        public override string Title { get; }
        public string Text { get; }

        public override void Render(TextBlock textBlock)
        {
            textBlock.Text = Text;
        }
    }
}