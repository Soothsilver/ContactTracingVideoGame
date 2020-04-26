using System.ComponentModel;
using System.Windows.Controls;

namespace ContactTracingPrototype.Documents
{
    class DocumentBrowser : INotifyPropertyChanged
    {
        private const int HISTORY_SIZE = 1000;

        private Document[] history = new Document[HISTORY_SIZE];
        private int firstPosition = -1;
        private int lastPosition = -1;
        private int currentPosition = -1;
        private TextBlock view;
        private bool raisingEvents;

        public DocumentBrowser(TextBlock view)
        {
            this.view = view;
        }

        public bool CanGoBack => currentPosition != firstPosition;

        public bool CanGoForward => currentPosition != lastPosition;

        public Document CurrentDocument
        {
            get
            {
                if (currentPosition < 0 || currentPosition >= HISTORY_SIZE) return null;
                return history[currentPosition];
            }
        }

        public void GoTo(Document document)
        {
            if (raisingEvents) return;

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
            RaisePropertyChangeEvents();
        }

        public void GoBack()
        {
            if (raisingEvents) return;
            if (!CanGoBack) return;
            
            currentPosition = GetPreviousIndex(currentPosition);
            UpdateView();
            RaisePropertyChangeEvents();
        }

        public void GoForward()
        {
            if (raisingEvents) return;
            if (!CanGoForward) return;
            
            currentPosition = GetNextIndex(currentPosition);
            UpdateView();
            RaisePropertyChangeEvents();
        }

        public void Refresh()
        {
            UpdateView();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            this.raisingEvents = true;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            this.raisingEvents = false;
        }

        private void RaisePropertyChangeEvents()
        {
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoForward));
            OnPropertyChanged(nameof(CurrentDocument));
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

            Document document = CurrentDocument;
            if (document == null) return;

            document.Render(view);
        }
    }
}
