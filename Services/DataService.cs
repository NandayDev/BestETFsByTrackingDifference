using BestETFsByTD.Models;
using System.Collections.Concurrent;
using System.Globalization;

namespace BestETFsByTD.Services
{
    public interface IDataService
    {
        void StartLoadingCache();

        Task<List<EtfInfo>> LoadEtfList(EtfCategory category);

        Task<List<EtfPerformance>> LoadPerformance(EtfCategory category, string isin);
    }

    public class DataService(HttpClient httpClient) : IDataService
    {
        private readonly ConcurrentDictionary<EtfCategory, List<EtfInfo>> infosCache = [];
        private readonly ConcurrentDictionary<string, List<EtfPerformance>> performancesCache = [];

        public async void StartLoadingCache()
        {
            foreach (EtfCategory category in Enum.GetValues<EtfCategory>())
            {
                List<EtfInfo> etfList = await LoadEtfList(category);
                foreach (EtfInfo etf in etfList)
                {
                    _ = LoadPerformance(category, etf.Isin);
                }
            }
        }

        public async Task<List<EtfInfo>> LoadEtfList(EtfCategory category)
        {
            if (infosCache.TryGetValue(category, out List<EtfInfo>? etfInfos))
            {
                return etfInfos;
            }
            etfInfos = [];
            var csv = await httpClient.GetStringAsync("etfs.csv");
            var csvLines = csv.Split('\n');
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
            infosCache[category] = etfInfos;
            return etfInfos;
        }

        public async Task<List<EtfPerformance>> LoadPerformance(EtfCategory category, string isin)
        {
            if (performancesCache.TryGetValue(isin, out List<EtfPerformance>? performances))
            {
                return performances;
            }

            string categoryName = category.GetPath();
            var csv = await httpClient.GetStringAsync($"{categoryName}/{isin}.csv");

            performances = [.. csv.Split('\n')
                      .Skip(1)
                      .Where(l => !string.IsNullOrWhiteSpace(l))
                      .Select(ParsePerf)];
            performancesCache[isin] = performances;
            return performances;
        }

        private static EtfInfo ParseEtf(string line)
        {
            var parts = line.Trim().Split(';');
            return new EtfInfo(
                Isin: parts[0],
                Name: parts[1],
                ReplicationType: EtfReplicationTypeFromString(parts[2]),
                Category: EtfCategoryFromString(parts[3]),
                Ter: double.Parse(parts[4], CultureInfo.InvariantCulture)
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
