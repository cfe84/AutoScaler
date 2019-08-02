using System.Threading.Tasks;

namespace AutoScaler.Domain
{
    public interface IScaleController
    {
        Task<int> GetCurrentSizeAsync();
        Task ResizeAsync(int instances);
    }
}