namespace FineCodeCoverageWebViewReport_Tests
{
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverageWebViewReport.InvocationsRecordingRegistration;
    using FineCodeCoverageWebViewReport.JsonPosterRegistration;
    using FineCodeCoverageWebViewReport_Tests.SeleniumExtensions;
    using Newtonsoft.Json;
    using OpenQA.Selenium.Edge;
    using System;
    using System.Collections.Generic;

    internal static class HostObjectsEdgeDriverExtensions
    {
        public static void ExecutePostBack(
            this EdgeDriver edgeDriver,
            string hostObjectRegistrationName,
            object objectToPostBack
        ) => edgeDriver.ExecuteHostObjectSerialized(
            hostObjectRegistrationName,
            nameof(IHostObject.postBack),
            objectToPostBack
        );

        public static List<Invocation> ExecuteGetInvocations(
            this EdgeDriver edgeDriver,
            string hostObjectRegistrationName)
        {
            var serialized = edgeDriver.ExecuteHostObjectScript(
                hostObjectRegistrationName,
                nameof(InvocationsRecordingHostObject.getInvocations)
            );
            return JsonConvert.DeserializeObject<List<Invocation>>((string)serialized);
        }

        public static List<Invocation> GetSourceFileOpenerHostObjectInvocations(this EdgeDriver edgeDriver) =>
            edgeDriver.ExecuteGetInvocations(SourceFileOpenerHostObjectRegistration.HostObjectName);

        private static Invocation GetSingleInvocation(List<Invocation> invocations)
        {
            if (invocations.Count > 1)
            {
                throw new Exception("Multiple invocations");
            }

            return invocations[0];
        }

        public static Invocation GetSourceFileOpenerHostObjectInvocation(this EdgeDriver edgeDriver) =>
            GetSingleInvocation(edgeDriver.GetSourceFileOpenerHostObjectInvocations());

        public static List<Invocation> GetFCCResourcesNavigatorHostObjectInvocations(this EdgeDriver edgeDriver) =>
            edgeDriver.ExecuteGetInvocations(FCCResourcesNavigatorRegistration.HostObjectName);

        public static Invocation GetFCCResourcesNavigatorHostObjectInvocation(this EdgeDriver edgeDriver) =>
            GetSingleInvocation(edgeDriver.GetFCCResourcesNavigatorHostObjectInvocations());

        public static List<Invocation> GetFCCOutputPaneHostObjectInvocations(this EdgeDriver edgeDriver) =>
            edgeDriver.ExecuteGetInvocations(FCCOutputPaneHostObjectRegistration.HostObjectName);

        public static Invocation GetFCCOutputPaneHostObjectInvocation(this EdgeDriver edgeDriver) =>
            GetSingleInvocation(edgeDriver.GetFCCOutputPaneHostObjectInvocations());
    }


}
