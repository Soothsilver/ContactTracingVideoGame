using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;

namespace ContactTracingPrototype
{
    internal class ConfirmedCasesCurve
    { 
        public ConfirmedCasesCurve()
        {

            SeriesCollection = new SeriesCollection
            {
                new StackedColumnSeries
                {
                    Values = new ChartValues<int> {},
                    Title = "Cumulative cases",
                    Margin = new Thickness(0),
                    MaxColumnWidth = Double.PositiveInfinity,
                    ColumnPadding = 0
                }
            };
        }

        public HashSet<Person> KnownCases = new HashSet<Person>();

        public void AddCurrentStatus(SituationReport report)
        {
            foreach (Person person in report.PositiveOrdered.Concat(report.PositiveSentinel))
            {
                KnownCases.Add(person);
            }

            SeriesCollection[0].Values.Add(KnownCases.Count);
        }
        
        public SeriesCollection SeriesCollection { get; set; }
    }
}