namespace FineCodeCoverage.Initialization
{
    internal interface IInitializeStatusProvider
    {
        InitializeStatus InitializeStatus { get; set; }
        string InitializeExceptionMessage { get; set; }
    }
}

