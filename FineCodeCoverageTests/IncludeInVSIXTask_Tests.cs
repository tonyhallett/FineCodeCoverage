using NUnit.Framework;
using ReadMeIncludeInVsixTask;

namespace FineCodeCoverageTests
{
    internal class IncludeInVSIXTask_Tests
    {
        [Test]
        public void Execute()
        {
            var includeInVSIXTask = new IncludeInVSIXTask();
            Assert.IsTrue(includeInVSIXTask.Execute());
        }
    }
}
