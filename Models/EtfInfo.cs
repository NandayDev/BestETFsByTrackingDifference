namespace BestETFsByTD.Models
{
    public record EtfInfo(string Isin, string Name, EtfReplicationType ReplicationType, EtfCategory Category, double Ter);
}
