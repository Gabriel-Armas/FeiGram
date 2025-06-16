using FeigramClient.Models;
using FeigramClient.Services;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using LiveCharts.Defaults;
using LiveCharts.Configurations;
using System.Windows;

namespace FeigramClient.Views
{
    public partial class Statistics : Page, INotifyPropertyChanged
    {
        private readonly StatisticsService _statisticsService;

        private SeriesCollection _seriesCollection;
        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            set
            {
                _seriesCollection = value;
                OnPropertyChanged();
            }
        }

        public Statistics(ProfileSingleton me)
        {
            InitializeComponent();

            _statisticsService = App.Services.GetRequiredService<StatisticsService>();

            DataContext = this;

            Loaded += Statistics_Loaded;
        }

        private async void Statistics_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await LoadWeeklyStatsAsync();
        }

        private async Task LoadWeeklyStatsAsync()
        {
            var response = await _statisticsService.GetWeeklyStatsAsync();
            if (response != null)
            {
                var dayCounts = response.Counts.ToDictionary(p => DateTime.Parse(p.Day), p => p.Count);

                var firstDay = dayCounts.Keys.Min();
                var monday = firstDay.AddDays(-(int)firstDay.DayOfWeek + (firstDay.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
                var weekDays = Enumerable.Range(0, 7).Select(i => monday.AddDays(i)).ToList();

                var labels = new List<string>();
                var values = new ChartValues<int>();

                foreach (var day in weekDays)
                {
                    labels.Add(day.ToString("yyyy-MM-dd"));
                    values.Add(dayCounts.TryGetValue(day, out var count) ? count : 0);
                }

                int max = values.Max();

                SeriesCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Publicaciones",
                        Values = values,
                        Fill = System.Windows.Media.Brushes.MediumPurple
                    }
                };

                WeeklyChart.AxisX.Clear();
                WeeklyChart.AxisX.Add(new Axis
                {
                    Title = "Fecha",
                    Labels = labels
                });

                WeeklyChart.AxisY.Clear();
                WeeklyChart.AxisY.Add(new Axis
                {
                    Title = "Cantidad",
                    MinValue = 0,
                    MaxValue = max + 1,
                    LabelFormatter = value => ((int)value).ToString(),
                    Separator = new LiveCharts.Wpf.Separator
                    {
                        Step = 1
                    }
                });

                WeekRangeText.Text = $"Del {labels.First()} al {labels.Last()} ✨";
            }
            else
            {
                WeekRangeText.Text = "No se pudieron cargar las estadísticas";
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
