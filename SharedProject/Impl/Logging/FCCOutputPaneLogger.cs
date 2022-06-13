using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Impl;

namespace FineCodeCoverage.Logging.OutputPane
{
    [Export(typeof(ILogger))]
    public class FCCOutputPaneLogger : ILogger, IListener<ShowFCCOutputPaneMessage>
    {
        private readonly IFCCOutputPane fccOutputPane;

        [ImportingConstructor]
        public FCCOutputPaneLogger(
            IEventAggregator eventAggregator,
            IFCCOutputPane fccOutputPane
        )
        {
            this.fccOutputPane = fccOutputPane;
            eventAggregator.AddListener(this);
        }

        private void LogImpl(object[] message, bool withTitle)
        {
            var messageList = message.Select(x => x?.ToString()?.Trim(' ', '\r', '\n'))
                .Where(x => !string.IsNullOrWhiteSpace(x));
            

            if (!messageList.Any())
            {
                return;
            }

            var logs = string.Join(Environment.NewLine, messageList);
            var msg = withTitle ? $"{Environment.NewLine}{Vsix.Name} : {logs}{Environment.NewLine}" : $"{logs}{Environment.NewLine}";
            fccOutputPane.OutputString(msg);
        }

        public void Log(params object[] message)
        {
            LogImpl(message, true);
        }

        public void Log(IEnumerable<object> message)
        {
            LogImpl(message.ToArray(), true);
        }

        void ILogger.Log(params string[] message)
        {
            LogImpl(message, true);
        }

        public void Log(IEnumerable<string> message)
        {
            LogImpl(message.ToArray(), true);
        }

        public void LogWithoutTitle(params object[] message)
        {
            LogImpl(message, false);
        }

        public void LogWithoutTitle(IEnumerable<object> message)
        {
            LogImpl(message.ToArray(), false);
        }

        public void LogWithoutTitle(params string[] message)
        {
            LogImpl(message, false);
        }
        
        public void LogWithoutTitle(IEnumerable<string> message)
        {
            LogImpl(message.ToArray(), false);
        }

        public void Handle(ShowFCCOutputPaneMessage message)
        {
            _ = fccOutputPane.ActivateAsync();
        }
    }
}