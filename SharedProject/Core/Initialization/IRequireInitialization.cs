﻿using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Initialization
{
    internal interface IRequireInitialization
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}
