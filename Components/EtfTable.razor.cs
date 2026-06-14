using BestETFsByTD.Models;
using BestETFsByTD.Resources;
using BestETFsByTD.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BestETFsByTD.Components
{
    public partial class EtfTable : ComponentBase
    {
#pragma warning disable CS8618
        [Inject] public IDataService DataService { get; set; }
        [Inject] public IStringLocalizer<AppStrings> Localizer { get; set; }
#pragma warning restore CS8618

        [Parameter] public EtfCategory Category { get; set; }

        public List<EtfRow> Etfs { get; set; } = [];

        public bool FilterFC { get; set; } = true;
        public bool FilterFT { get; set; } = true;
        public bool FilterS { get; set; } = true;

        private int startYear = 2023;
        public int StartYear
        {
            get => startYear;
            set
            {
                startYear = value;
                _ = LoadDataAndRefresh();
            }
        }

        public EtfRow? SelectedEtf { get; set; }

        private string SortColumn = nameof(EtfRow.AverageTrackingDifference);
        private bool SortAsc = false;

        protected override async Task OnParametersSetAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            Error = "";
            List<EtfInfo> list = [];
            try
            {
                list = await DataService.LoadEtfList(Category);
            }
            catch (Exception ex)
            {
                Error += $"An error occurred:\n{ex}";
            }

            List<EtfRow> etfRows = [];

            foreach (var e in list)
            {
                try
                {
                    List<EtfPerformance> perf = await DataService.LoadPerformance(Category, e.Isin);

                    if (!perf.Any(p => p.Year == StartYear))
                    {
                        continue;
                    }

                    IEnumerable<EtfPerformance> filtered = perf.Where(p => p.Year >= StartYear);

                    double avg = filtered.Any()
                        ? filtered.Average(p => p.TrackingDifference)
                        : 0;

                    EtfRow etfRow = new(
                        Isin: e.Isin,
                        Name: e.Name,
                        ReplicationType: e.ReplicationType,
                        ReplicationTypeString: e.ReplicationType.GetLocalizedName(Localizer),
                        AverageTrackingDifference: avg,
                        Performances: [.. perf.OrderBy(p => p.Year)]
                    );

                    etfRows.Add(etfRow);
                }
                catch (Exception ex)
                {
                    Error += $"Errore per etf {e.Name}: \n{ex}";
                }
            }

            Etfs = etfRows;
        }

        private async Task LoadDataAndRefresh()
        {
            await LoadData();
            await InvokeAsync(StateHasChanged);
        }

        private IEnumerable<EtfRow> FilteredEtfs
        {
            get
            {
                var filtered = Etfs
                    .Where(e =>
                        (FilterFC && e.ReplicationType == EtfReplicationType.SAMPLING) ||
                        (FilterFT && e.ReplicationType == EtfReplicationType.FULL) ||
                        (FilterS && e.ReplicationType == EtfReplicationType.SYNTHETIC));
                return SortAsc
                ? [.. Etfs.OrderBy(e => GetSortValue(e))]
                : [.. Etfs.OrderByDescending(e => GetSortValue(e))];
            }
        }

        private string Error = "";

        private object GetSortValue(EtfRow e) =>
            SortColumn switch
            {
                nameof(EtfRow.Isin) => e.Isin,
                nameof(EtfRow.Name) => e.Name,
                nameof(EtfRow.ReplicationType) => e.ReplicationType,
                nameof(EtfRow.AverageTrackingDifference) => e.AverageTrackingDifference,
                _ => e.Isin
            };

        private string GetSortArrow(string column)
        {
            if (SortColumn != column)
                return string.Empty;

            return SortAsc ? "▲" : "▼";
        }

        private void SortBy(string column)
        {
            if (SortColumn == column)
                SortAsc = !SortAsc;
            else
            {
                SortColumn = column;
                SortAsc = true;
            }

            Etfs = SortAsc
                ? [.. Etfs.OrderBy(e => GetSortValue(e))]
                : [.. Etfs.OrderByDescending(e => GetSortValue(e))];
        }

        public void OpenDialog(EtfRow etf)
        {
            SelectedEtf = etf;
        }

        public void CloseDialog()
        {
            SelectedEtf = null;
        }

        public record EtfRow(string Isin, string Name, EtfReplicationType ReplicationType, string ReplicationTypeString, double AverageTrackingDifference, List<EtfPerformance> Performances);
    }
}
