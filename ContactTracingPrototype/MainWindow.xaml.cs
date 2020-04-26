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
        ObservableCollection<Document> allDocuments = new ObservableCollection<Document>();

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
            allDocuments.Clear();

            this.theChart.DataContext = city.ConfirmedCasesCurve;
            this.theChart2.DataContext = city.Model;

            Binding documentsBinding = new Binding {Source = allDocuments};
            this.documentsListBox.SetBinding(ListBox.ItemsSourceProperty, documentsBinding);

            var newGameDocument = new NewGameDocument(city.DailyUpdates.First());
            dailyDocuments.Add(newGameDocument);
            documentBrowser.GoTo(newGameDocument);
            allDocuments.Add(newGameDocument);

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
            allDocuments.Add(document);
            documentBrowser.GoTo(document);
            documentsListBox.SelectedItem = document;

            if (city.People.Count(ppl => ppl.IsActiveCase) == 0 && !city.OutbreakEnded)
            {
                // END
                city.OutbreakEnded = true;
                int cases = city.People.Count(ppl => ppl.DiseaseStatus != DiseaseStage.Susceptible);
                var endDocument = new InformationDocument("Outbreak contained!",
                    $@"There are no more active cases.

We can declare the outbreak in Sorpigal contained.

You contained this outbreak in {city.Today} days. 

A total of {cases} people caught the disease. You needed to quarantine {city.People.Count(ppl => ppl.Quarantined)} people.

" + (cases < 20 ? "Well done and thank you for playing our game!" : "This could have gone a bit better..."));
                allDocuments.Add(endDocument);
                documentsListBox.SelectedItem = endDocument;

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
            if (e.AddedItems.Count > 0)
            {
                documentBrowser.GoTo((Document) e.AddedItems[0]);
            }
            else
            {
                
            }
        }

        private void End50Day_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 50; i++)
            {
                EndDay_Click(sender, e);
            }
        }
    }
}

    