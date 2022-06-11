using System;
using System.Threading;

namespace FineCodeCoverageTests
{
    internal static class CancellationTokenHelper
    {
        public static CancellationToken GetCancelledCancellationToken()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            return cancellationTokenSource.Token;
        }

        public static bool IsDisposed(this CancellationTokenSource cancellationTokenSource)
        {
            var isDisposed = false;
            try
            {
                var _ = cancellationTokenSource.Token;
            }
            catch (ObjectDisposedException)
            {
                isDisposed = true;
            }
            return isDisposed;
        }
    }
}
