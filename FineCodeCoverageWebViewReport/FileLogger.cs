using FineCodeCoverage.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FineCodeCoverageWebViewReport.WebViewControllerDependencies
{
    internal class FileLogger : ILogger
    {
        private readonly string logPath;

        public FileLogger(string logPath = null)
        {
            this.logPath = logPath;
        }

        private void FileLog(IEnumerable<object> message)
        {
            if (logPath != null)
            {
                File.AppendAllText(
                    logPath,
                    string.Join(Environment.NewLine, message.Concat(new object[] { Environment.NewLine }))
                );
            }
                
        }
        public void Log(IEnumerable<object> message)
        {
            FileLog(message);
        }

        public void Log(IEnumerable<string> message)
        {
            FileLog(message);
        }

        public void Log(params object[] message)
        {
            FileLog(message);
        }

        public void Log(params string[] message)
        {
            FileLog(message);
        }

        public void LogWithoutTitle(IEnumerable<object> message)
        {
            FileLog(message);
        }

        public void LogWithoutTitle(IEnumerable<string> message)
        {
            FileLog(message);
        }

        public void LogWithoutTitle(params object[] message)
        {
            FileLog(message);
        }

        public void LogWithoutTitle(params string[] message)
        {
            FileLog(message);
        }
    }
}
