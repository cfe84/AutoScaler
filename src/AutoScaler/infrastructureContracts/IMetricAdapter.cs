using System.Threading.Tasks;

namespace AutoScaler.Domain
{
    public interface IMetricAdapter
    {
        Task<double> GetMetricAsync();
    }
}