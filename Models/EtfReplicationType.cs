using BestETFsByTD.Resources;
using Microsoft.Extensions.Localization;

namespace BestETFsByTD.Models
{
    public enum EtfReplicationType
    {
        FULL,
        SAMPLING,
        SYNTHETIC,
        HYBRID
    }

    public static class EtfReplicationTypeExtensions
    {
        public static string GetLocalizedName(this EtfReplicationType replicationType, IStringLocalizer<AppStrings> localizer)
        {
            return replicationType switch
            {
                EtfReplicationType.FULL => localizer["ReplicationFull"],
                EtfReplicationType.SAMPLING => localizer["ReplicationSampling"],
                EtfReplicationType.SYNTHETIC => localizer["ReplicationSynthetic"],
                EtfReplicationType.HYBRID => localizer["ReplicationHybrid"],
            };
        }
    }
}
