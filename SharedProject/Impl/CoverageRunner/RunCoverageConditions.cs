using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Logging;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output.JsMessages.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Impl
{
    [Export(typeof(IRunCoverageConditions))]
    internal class RunCoverageConditions : IRunCoverageConditions
    {
        private readonly IEventAggregator eventAggregator;
        private readonly ILogger logger;
        private readonly List<Func<ITestOperation, IAppOptions, bool>> conditions;

        [ImportingConstructor]
        public RunCoverageConditions(
            IEventAggregator eventAggregator,
            ILogger logger
        )
        {
            this.eventAggregator = eventAggregator;
            this.logger = logger;
            conditions = new List<Func<ITestOperation, IAppOptions, bool>>
            {
                FailedTestConditionMet,
                NumTestsExceedConditionMet
            };
        }

        private void CombinedLog(string message, MessageContext messageContext)
        {
            eventAggregator.SendMessage(LogMessage.Simple(messageContext, message));
            logger.Log(message);
        }

        private bool FailedTestConditionMet(ITestOperation testOperation, IAppOptions settings)
        {
            var met = true;
            if (!settings.RunWhenTestsFail && testOperation.FailedTests > 0)
            {
                CombinedLog(
                    $"Skipping coverage due to failed tests.  Option {nameof(AppOptions.RunWhenTestsFail)} is false", 
                    MessageContext.Warning
                );
                met = false;
            }
            return met;
        }

        private bool NumTestsExceedConditionMet(ITestOperation testOperation, IAppOptions settings)
        {
            var met = true;
            var totalTests = testOperation.TotalTests;
            var runWhenTestsExceed = settings.RunWhenTestsExceed;
            if (totalTests > 0) // in case this changes to not reporting total tests
            {
                if (totalTests <= runWhenTestsExceed)
                {
                    CombinedLog($"Skipping coverage as total tests ({totalTests}) <= {nameof(AppOptions.RunWhenTestsExceed)} ({runWhenTestsExceed})", MessageContext.Warning);
                    met = false;
                }
            }
            return met;
        }

        public bool Met(ITestOperation testOperation, IAppOptions settings)
        {
            foreach(var condition in conditions)
            {
                if (!condition(testOperation, settings))
                {
                    return false;
                }
            }

            return true;
        }
    }

}
