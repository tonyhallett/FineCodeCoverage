using System.Threading.Tasks;

namespace FineCodeCoverage.Logging
{
    public interface IFCCOutputPane
    {
        void OutputString(string outputString);
        Task ActivateAsync();
    }
}
