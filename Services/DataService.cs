using BestETFsByTD.Models;
using BestETFsByTD.Resources;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace BestETFsByTD.Services
{
    public interface IDataService
    {
        Task<List<EtfInfo>> LoadEtfList(EtfCategory category);

        Task<List<EtfPerformance>> LoadPerformance(EtfCategory category, string isin);
    }

    public class DataService(HttpClient httpClient, IStringLocalizer<AppStrings> localizer) : IDataService
    {
        public async Task<List<EtfInfo>> LoadEtfList(EtfCategory category)
        {
            var csv = await httpClient.GetStringAsync("etfs.csv");
            var csvLines = csv.Split('\n');
            List<EtfInfo> etfInfos = [];
            for (int i = 1; i < csvLines.Length; i++)
            {
                var line = csvLines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                EtfInfo etfInfo = ParseEtf(line);
                if (etfInfo.Category == category)
                {
                    etfInfos.Add(etfInfo);
                }
            }
            return etfInfos;
        }

        public async Task<List<EtfPerformance>> LoadPerformance(EtfCategory category, string isin)
        {
            string categoryName = category.GetPath();
            var csv = await httpClient.GetStringAsync($"{categoryName}/{isin}.csv");

            return [.. csv.Split('\n')
                      .Skip(1)
                      .Where(l => !string.IsNullOrWhiteSpace(l))
                      .Select(ParsePerf)];
        }

        private static EtfInfo ParseEtf(string line)
        {
            var parts = line.Trim().Split(';');
            return new EtfInfo(
                Isin: parts[0],
                Name: parts[1],
                ReplicationType: EtfReplicationTypeFromString(parts[2]),
                Category: EtfCategoryFromString(parts[3])
            );
        }

        private static EtfPerformance ParsePerf(string line)
        {
            var parts = line.Split(';');
            return new EtfPerformance(
                Year: int.Parse(parts[0]),
                FundPerformance: double.Parse(parts[1], CultureInfo.InvariantCulture),
                IndexPerformance: double.Parse(parts[2], CultureInfo.InvariantCulture)
            );
        }

        private static EtfReplicationType EtfReplicationTypeFromString(string typeString)
        {
            return typeString switch
            {
                "FC" => EtfReplicationType.SAMPLING,
                "FT" => EtfReplicationType.FULL,
                "S" => EtfReplicationType.SYNTHETIC,
                "I" => EtfReplicationType.HYBRID,
                _ => throw new ArgumentException($"ETF replication type string \"{typeString}\" is invalid")
            };
        }

        private static EtfCategory EtfCategoryFromString(string typeString)
        {
            return typeString switch
            {
                "Developed" => EtfCategory.DEVELOPED,
                "All World" => EtfCategory.ALL_WORLD,
                "Emerging" => EtfCategory.EMERGING,
                _ => throw new ArgumentException($"ETF category type string \"{typeString}\" is invalid")
            };
        }
    }
}
