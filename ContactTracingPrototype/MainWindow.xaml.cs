using System;
using System.Linq;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;

namespace ContactTracingPrototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        City city = new City();
        
        public MainWindow()
        {
            InitializeComponent();
            Reset();
        }

        private void Reset()
        {
            city = new City();
            this.theChart.DataContext = city.Model;
            this.everything.Text = city.ToString();
            for (int i = 0; i < 50; i++)
            {
                EndDay_Click(null, new RoutedEventArgs());
            }
        }

        private void EndDay_Click(object sender, RoutedEventArgs e)
        {
            city.EndDay();
            this.log.Text = city.LastLog + "\n" + city.EpidemiologicalCurveLog;
            this.everything.Text = city.ToString();
            if (city.People.Count(ppl => ppl.IsActiveCase) == 0)
            {
                // END
            }

        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }
    }

    internal class InfectionCurveModel
    { 
        public InfectionCurveModel()
        {

            SeriesCollection = new SeriesCollection
            {
                new StackedColumnSeries
                {
                    Values = new ChartValues<int> {},
                    StackMode = StackMode.Values, // this is not necessary, values is the default stack mode
                    Title = "Active",
                    Margin = new Thickness(0),
                    MaxColumnWidth = Double.PositiveInfinity,
                    ColumnPadding = 0
                },
                new StackedColumnSeries
                {
                    Values = new ChartValues<int> {},
                    StackMode = StackMode.Values,
                    Title = "Recovered",
                    Margin = new Thickness(0),
                    MaxColumnWidth = Double.PositiveInfinity,
                    ColumnPadding = 0
                }
                ,
                new StackedColumnSeries
                {
                    Values = new ChartValues<int> {},
                    StackMode = StackMode.Values,
                    Title = "Deaths",
                    Margin = new Thickness(0),
                    MaxColumnWidth = Double.PositiveInfinity,
                    ColumnPadding = 0
                    
                }
            };
        }

        public void AddCurrentStatus(City city)
        {
            SeriesCollection[0].Values.Add((int)city.People.Count(ppl => ppl.DiseaseStatus != DiseaseStage.Susceptible && ppl.DiseaseStatus != DiseaseStage.Immune && ppl.DiseaseStatus != DiseaseStage.Dead));
            SeriesCollection[1].Values.Add((int)city.People.Count(ppl => ppl.DiseaseStatus == DiseaseStage.Immune));
            SeriesCollection[2].Values.Add((int)city.People.Count(ppl => ppl.DiseaseStatus == DiseaseStage.Dead));
        }
        
        public SeriesCollection SeriesCollection { get; set; }
    }
}