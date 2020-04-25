using ContactTracingPrototype.Documents;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ContactTracingPrototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        City city = new City();
        DocumentBrowser documentBrowser;
        ObservableCollection<DailyUpdateDocument> dailyDocuments;
        
        public MainWindow()
        {
            InitializeComponent();
            Reset();
        }

        private void Reset()
        {
            city = new City();
            documentBrowser = new DocumentBrowser(this.documentTextBox);
            dailyDocuments = new ObservableCollection<DailyUpdateDocument>();

            this.theChart.DataContext = city.ConfirmedCasesCurve;
            this.theChart2.DataContext = city.Model;

            Binding documentsBinding = new Binding { Source = dailyDocuments };
            this.documentsListBox.SetBinding(ListBox.ItemsSourceProperty, documentsBinding);

            var newGameDocument = new NewGameDocument(city.DailyUpdates.First());
            dailyDocuments.Add(newGameDocument);
            documentBrowser.GoTo(newGameDocument);
            
            //for (int i = 0; i < 50; i++)
            //{
            //    EndDay_Click(null, new RoutedEventArgs());
            //}
        }

        private void EndDay_Click(object sender, RoutedEventArgs e)
        {
            city.EndDay();

            var document = new DailyUpdateDocument($"Day {city.DailyUpdates.Count}", city.DailyUpdates.Last());
            dailyDocuments.Add(document);
            documentBrowser.GoTo(document);
            
            if (city.People.Count(ppl => ppl.IsActiveCase) == 0)
            {
                // END
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            documentBrowser.GoBack();
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            documentBrowser.GoForward();
        }

        private void documentsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            documentBrowser.GoTo((Document)e.AddedItems[0]);
        }
    }
}