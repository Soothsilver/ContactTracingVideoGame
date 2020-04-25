using System.Windows.Controls;

namespace ContactTracingPrototype.Documents
{
    class DocumentBrowser
    {
        private const int HISTORY_SIZE = 1000;

        private Document[] history = new Document[HISTORY_SIZE];
        private int firstPosition = -1;
        private int lastPosition = -1;
        private int currentPosition = -1;
        private TextBlock view;

        public DocumentBrowser(TextBlock view)
        {
            this.view = view;
        }

        public void GoTo(Document document)
        {
            int nextPosition = GetNextIndex(currentPosition);
            history[nextPosition] = document;
            if (firstPosition == nextPosition)
            {
                firstPosition = GetNextIndex(firstPosition);
            }
            else if (firstPosition < 0)
            { 
                firstPosition = nextPosition; 
            }
            lastPosition = currentPosition = nextPosition;

            UpdateView();
        }

        public void GoBack()
        {
            if (currentPosition == firstPosition) return;
            
            currentPosition = GetPreviousIndex(currentPosition);
            UpdateView();
        }

        public void GoForward()
        {
            if (currentPosition == lastPosition) return;
            
            currentPosition = GetNextIndex(currentPosition);
            UpdateView();
        }

        private int GetNextIndex(int p)
        {
            p++;
            if (p >= HISTORY_SIZE) p = 0;
            return p;
        }

        private int GetPreviousIndex(int p)
        {
            p--;
            if (p < 0) p = HISTORY_SIZE - 1;
            return p;
        }

        private void UpdateView()
        {
            if (currentPosition < 0 && currentPosition >= HISTORY_SIZE) return;

            Document document = history[currentPosition];
            if (document == null) return;

            document.Render(view);
        }
    }
}
