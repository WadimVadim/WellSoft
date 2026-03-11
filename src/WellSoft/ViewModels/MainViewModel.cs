using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using WellSoft.Models;
using WellSoft.Services;

namespace WellSoft.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IWellDataService _wellDataService;
        private readonly IFileDialogService _fileDialogService;

        private string _statusMessage;
        private ObservableCollection<WellSummary> _summaries;
        private ObservableCollection<ErrorItem> _errors;
        private WellSummary _selectedWellSummary;
        private ObservableCollection<Interval> _selectedWellIntervals;
        private List<Well> _allWells;

        public MainViewModel(IWellDataService wellDataService, IFileDialogService fileDialogService)
        {
            _wellDataService = wellDataService;
            _fileDialogService = fileDialogService;

            _summaries = new ObservableCollection<WellSummary>();
            _errors = new ObservableCollection<ErrorItem>();
            _selectedWellIntervals = new ObservableCollection<Interval>();
            _allWells = new List<Well>();

            LoadFileCommand = new RelayCommand(async _ => await LoadFileAsync(), _ => true);
            ExportJsonCommand = new RelayCommand(async _ => await ExportJsonAsync(), _ => Summaries.Any());
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ObservableCollection<WellSummary> Summaries
        {
            get => _summaries;
            set { _summaries = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ErrorItem> Errors
        {
            get => _errors;
            set { _errors = value; OnPropertyChanged(); }
        }

        public WellSummary SelectedWellSummary
        {
            get => _selectedWellSummary;
            set
            {
                if (_selectedWellSummary != value)
                {
                    _selectedWellSummary = value;
                    OnPropertyChanged();
                    UpdateSelectedWellIntervals();
                }
            }
        }

        public ObservableCollection<Interval> SelectedWellIntervals
        {
            get => _selectedWellIntervals;
            set { _selectedWellIntervals = value; OnPropertyChanged(); }
        }

        public ICommand LoadFileCommand { get; }
        public ICommand ExportJsonCommand { get; }


        private async Task LoadFileAsync()
        {
            string filePath = _fileDialogService.OpenFileDialog("CSV files (*.csv)|*.csv|All files (*.*)|*.*");
            if (string.IsNullOrEmpty(filePath))
            {
                StatusMessage = "Файл не выбран.";
                return;
            }

            StatusMessage = "Загрузка...";
            Summaries.Clear();
            Errors.Clear();
            SelectedWellIntervals.Clear();
            SelectedWellSummary = null;

            try
            {
                await Task.Run(() =>
                {
                    using var stream = File.OpenRead(filePath);
                    var result = _wellDataService.ProcessCsvFile(stream);
                    var summaries = _wellDataService.CalculateSummaries(result.Wells);

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _allWells = result.Wells;
                        foreach (var s in summaries)
                            Summaries.Add(s);
                        foreach (var e in result.Errors)
                            Errors.Add(e);

                        if (Summaries.Any())
                            SelectedWellSummary = Summaries.First();
                    });
                });

                StatusMessage = Summaries.Any() ? $"Загружено {Summaries.Count} скважин" : "Нет корректных данных";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
        }

        private async Task ExportJsonAsync()
        {
            if (!Summaries.Any())
            {
                StatusMessage = "Нет данных для экспорта.";
                return;
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string defaultFileName = $"well_data_{timestamp}.json";

            string filePath = _fileDialogService.SaveFileDialog(
                "JSON files (*.json)|*.json|All files (*.*)|*.*",
                defaultFileName);

            if (string.IsNullOrEmpty(filePath))
                return;

            StatusMessage = "Экспорт...";
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Summaries.ToList(), options);
                await File.WriteAllTextAsync(filePath, json);
                StatusMessage = $"Экспорт завершён: {System.IO.Path.GetFileName(filePath)}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка экспорта: {ex.Message}";
            }
        }

        private void UpdateSelectedWellIntervals()
        {
            SelectedWellIntervals.Clear();
            if (SelectedWellSummary == null)
                return;

            var well = _allWells.FirstOrDefault(w => w.WellId == SelectedWellSummary.WellId);
            if (well != null)
            {
                foreach (var interval in well.Intervals.OrderBy(i => i.DepthFrom))
                    SelectedWellIntervals.Add(interval);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}