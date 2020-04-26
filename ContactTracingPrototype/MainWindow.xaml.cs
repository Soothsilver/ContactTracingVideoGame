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

        public static MainWindow Instance;

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            Reset();
        }

        private void Reset()
        {
            city = new City();
            
            theChart.SetValue(Grid.ColumnSpanProperty, 2);
            theChart2.Visibility = Visibility.Collapsed;
            documentBrowser = new DocumentBrowser(this.documentTextBox);
            dailyDocuments = new ObservableCollection<DailyUpdateDocument>();

            this.theChart.DataContext = city.ConfirmedCasesCurve;
            this.theChart2.DataContext = city.Model;

            Binding documentsBinding = new Binding {Source = city.allDocuments};
            this.documentsListBox.SetBinding(ListBox.ItemsSourceProperty, documentsBinding);

            var newGameDocument = new NewGameDocument(city.DailyUpdates.First());
            dailyDocuments.Add(newGameDocument);
            city.allDocuments.Add(newGameDocument);
            documentsListBox.SelectedItem = newGameDocument;
            
            city.allDocuments.Add(new InformationDocument("Tips",
@"Left-click a person to read what you know about that person. Right-click a person to order a test, order home quarantine or trace that person's contacts.

Tips:
- At the beginning, you know about some cases but more cases are hidden in the town.
- Every night, you get test results from last day's test.
- Some people who have symptoms manifest even if you don't order a test for them! This is ""sentinel testing"".
"));
            
            

            //for (int i = 0; i < 50; i++)
            //{
            //    EndDay_Click(null, new RoutedEventArgs());
            //}
        }

        private void EndDay_Click(object sender, RoutedEventArgs e)
        {
            EndOneDay();
        }

        private SituationReport EndOneDay()
        {
            city.EndDay();

            SituationReport situationReport = city.DailyUpdates.Last();
            var document = new DailyUpdateDocument($"Day {city.DailyUpdates.Count}", situationReport);
            dailyDocuments.Add(document);
            city.allDocuments.Add(document);
            documentBrowser.GoTo(document);
            documentsListBox.SelectedItem = document;

            if (city.People.Count(ppl => ppl.IsActiveCase) == 0 && !city.OutbreakEnded)
            {
                city.OutbreakEnded = true;
                theChart.SetValue(Grid.ColumnSpanProperty, 1);
                theChart2.Visibility = Visibility.Visible;
                int cases = city.People.Count(ppl => ppl.DiseaseStatus != DiseaseStage.Susceptible);
                var endDocument = new InformationDocument("Outbreak contained!",
                    $@"There are no more active cases.

We can declare the outbreak in Sorpigal contained.

You contained this outbreak in {city.Today} days. 

A total of {cases} people caught the disease. You needed to quarantine {city.People.Count(ppl => ppl.Quarantined)} people.

" + (cases < 20 ? "Well done and thank you for playing our game!" : "This could have gone a bit better..."));
                city.allDocuments.Add(endDocument);
                documentsListBox.SelectedItem = endDocument;
            }

            return situationReport;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to start a new game?", "Start new game?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Reset();
            }
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

        private void PassTime_Click(object sender, RoutedEventArgs e)
        {
            SituationReport report = EndOneDay();
            while (!city.OutbreakEnded && report.PositiveOrdered.Count == 0 && report.PositiveSentinel.Count == 0)
            {
                report = EndOneDay();
            }
        }
    }
}

    