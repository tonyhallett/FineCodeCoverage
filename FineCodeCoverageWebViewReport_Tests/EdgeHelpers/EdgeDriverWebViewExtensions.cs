namespace FineCodeCoverageWebViewReport_Tests.EdgeHelpers
{
    using Newtonsoft.Json;
    using OpenQA.Selenium.Edge;
    using System.Linq;

    internal static class EdgeDriverWebViewExtensions
    {
        internal static string GetHostObjectExecuteScriptArguments(object[] args)
        {
            var arguments = args.Select((_, i) => $"arguments[{i}]");
            return string.Join(",", arguments);
        }

        public static object ExecuteHostObjectScript(
            this EdgeDriver edgeDriver,
            string hostObjectName,
            string methodName,
            params object[] args
        )
        {
            var arguments = args.Length > 0 ? GetHostObjectExecuteScriptArguments(args) : "";
            var script = $"return window.chrome.webview.hostObjects.{hostObjectName}.{methodName}({arguments})";
            return edgeDriver.ExecuteScript(script, args);
        }

        public static object ExecuteHostObjectSerialized(
            this EdgeDriver edgeDriver,
            string hostObjectName,
            string methodName,
            params object[] args
        )
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var serializedArguments = args.Select(arg => JsonConvert.SerializeObject(arg, settings)).ToArray();
            return edgeDriver.ExecuteHostObjectScript(hostObjectName, methodName, serializedArguments);
        }
    }


}
