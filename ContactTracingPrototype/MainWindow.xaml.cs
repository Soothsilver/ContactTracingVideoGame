using System.Linq;
using System.Windows;

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
            this.theChart.DataContext = city.ConfirmedCasesCurve;
            this.theChart2.DataContext = city.Model;
            this.everything.Text = city.ToString();
            for (int i = 0; i < 50; i++)
            {
                EndDay_Click(null, new RoutedEventArgs());
            }
        }

        private void EndDay_Click(object sender, RoutedEventArgs e)
        {
            city.EndDay();
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
}