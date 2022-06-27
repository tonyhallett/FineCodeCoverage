namespace FineCodeCoverage.Impl
{
    internal interface IOperationStateChangedHandler
    {
        void Initialize(IChangingOperationState changingOperationState);
    }
}
