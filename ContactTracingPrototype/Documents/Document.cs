using System.Windows.Controls;

namespace ContactTracingPrototype.Documents
{
    public abstract class Document
    {
        public abstract string Title { get; }

        public abstract void Render(TextBlock textBlock);

        public override string ToString()
        {
            return this.Title;
        }
    }
}
