using Microsoft.VisualStudio.TestWindow.Extensibility;
using ReflectObject;
using System;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Impl
{
    [Export(typeof(ITestRunRequestFactory))]
    internal class TestRunRequestFactory : ITestRunRequestFactory
    {
        private readonly ILogger logger;

        [ImportingConstructor]
        public TestRunRequestFactory(ILogger logger)
        {
            this.logger = logger;
        }

        public TestRunRequest Create(IOperation operation)
        {
            try
            {

                return new TestRunRequest(operation);
            }
            catch (PropertyDoesNotExistException propertyDoesNotExistException)
            {
                logger.Log("Error test container discoverer reflection");
                throw new Exception(propertyDoesNotExistException.Message);
            }

        }
    }
}
