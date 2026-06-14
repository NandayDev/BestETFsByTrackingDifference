using BestETFsByTD.Resources;
using Microsoft.Extensions.Localization;

namespace BestETFsByTD.Models
{
    public enum EtfCategory
    {
        ALL_WORLD,
        DEVELOPED,
        EMERGING
    }

    public static class EtfCategoryExtensions
    {
        public static string GetLocalizedName(this EtfCategory category, IStringLocalizer<AppStrings> localizer)
        {
            return category switch
            {
                EtfCategory.ALL_WORLD => localizer["AllWorld"],
                EtfCategory.DEVELOPED => localizer["Developed"],
                EtfCategory.EMERGING => localizer["Emerging"],
                _ => throw new ArgumentException($"{nameof(GetLocalizedName)} - Missing category {category}")
            };
        }

        public static string GetPath(this EtfCategory category)
        {
            return category switch
            {
                EtfCategory.ALL_WORLD => "All World",
                EtfCategory.DEVELOPED => "Developed",
                EtfCategory.EMERGING => "Emerging Markets",
                _ => throw new ArgumentException($"{nameof(GetPath)} - Missing category {category}")
            };
        }
    }
}