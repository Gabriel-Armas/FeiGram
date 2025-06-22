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
using System.Net.Http;

namespace FeigramClient.Views
{
    public partial class Statistics : Page, INotifyPropertyChanged
    {
        private readonly StatisticsService _statisticsService;
        private ProfileSingleton _me;
        private Frame _ModalFrame;
        private Grid _ModalOverlay;
        private MainWindow _mainWindow;

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

        public Statistics(ProfileSingleton me, MainWindow mainWindow, Frame modalFrame, Grid modalOverlay)
        {
            InitializeComponent();
            _me = me;
            _mainWindow = mainWindow;
            _ModalFrame = modalFrame;
            _ModalOverlay = modalOverlay;
            _statisticsService = App.Services.GetRequiredService<StatisticsService>();

            DataContext = this;

            Loaded += Statistics_Loaded;
        }

        private async void Statistics_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await LoadWeeklyStatsAsync();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var home = new MainMenu(_me, _mainWindow);
            _ModalFrame.Navigate(home);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var profilePage = new Profile(_me, _mainWindow, _ModalFrame, _ModalOverlay, true);
            _ModalFrame.Navigate(profilePage);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Messages_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var messagesPage = new Messages(_me, _mainWindow, _ModalFrame, _ModalOverlay);
            _ModalFrame.Navigate(messagesPage);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Accounts_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var consultAccounts = new ConsultAccount(_me, _mainWindow, _ModalFrame, _ModalOverlay);
            _ModalFrame.Navigate(consultAccounts);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Stadistic_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var consultAccounts = new Statistics(_me, _mainWindow, _ModalFrame, _ModalOverlay);
            _ModalFrame.Navigate(consultAccounts);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void CloseSession_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Content = null;
            _mainWindow.GridLogin.Visibility = Visibility.Visible;
            _mainWindow.GridMainMenu.Visibility = Visibility.Hidden;
            _mainWindow.EmailTextBox.Text = "";
            _mainWindow.PasswordBox.Password = "";
        }

        private async Task LoadWeeklyStatsAsync()
        {
            try
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
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error de HTTP: {httpEx.Message}",
                                "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar grafico:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
