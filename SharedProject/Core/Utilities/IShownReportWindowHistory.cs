namespace FineCodeCoverage.Core.Utilities
{
    interface IShownReportWindowHistory
    {
        bool HasShown { get; }
        void Showed();
    }
}
