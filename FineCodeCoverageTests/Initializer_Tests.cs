using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Impl;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests.Initializer_Tests
{
    public class Initializer_Tests
    {
        private AutoMoqer mocker;
        private Initializer initializer;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            initializer = mocker.Create<Initializer>();
        }

		[Test]
		public void Should_Have_Initial_InitializeStatus_As_Initializing()
        {
			Assert.AreEqual(InitializeStatus.Initializing, initializer.InitializeStatus);
        }

		[Test]
		public async Task Should_Log_Initializing_When_Initialize()
        {
			await initializer.InitializeAsync(CancellationToken.None);
			mocker.Verify<ILogger>(l => l.Log("Initializing"));
        }

		private async Task InitializeWithExceptionAsync(Action<Exception> callback = null)
		{
			var initializeException = new Exception("initialize exception");
			mocker.Setup<ICoverageProjectFactory>(a => a.Initialize()).Throws(initializeException);
			
			await initializer.InitializeAsync(CancellationToken.None);
			callback?.Invoke(initializeException);

		}
		
		[Test]
		public async Task Should_Set_InitializeStatus_To_Error_If_Exception_When_Initialize()
		{
			await InitializeWithExceptionAsync();
			Assert.AreEqual(InitializeStatus.Error, initializer.InitializeStatus);
		}

		[Test]
		public async Task Should_Set_InitializeExceptionMessage_If_Exception_When_Initialize()
		{
			await InitializeWithExceptionAsync();
			Assert.AreEqual("initialize exception", initializer.InitializeExceptionMessage);
		}

		[Test]
		public async Task Should_Log_Failed_Initialization_With_Exception_if_Exception_When_Initialize()
        {
			Exception initializeException = null;
			await InitializeWithExceptionAsync(exc => initializeException = exc);
			mocker.Verify<ILogger>(l => l.Log("Failed Initialization", initializeException));
		}

		[Test]
		public async Task Should_Not_Log_Failed_Initialization_When_Initialize_Cancelled()
		{
			await initializer.InitializeAsync(CancellationTokenHelper.GetCancelledCancellationToken());
			mocker.Verify<ILogger>(l => l.Log("Failed Initialization", It.IsAny<Exception>()), Times.Never());
		}

		[Test]
		public async Task Should_Set_InitializeStatus_To_Initialized_When_Successfully_Completed()
		{
			await initializer.InitializeAsync(CancellationToken.None);
			Assert.AreEqual(InitializeStatus.Initialized, initializer.InitializeStatus);
		}

		[Test]
		public async Task Should_Log_Initialized_When_Successfully_Completed()
		{
			await initializer.InitializeAsync(CancellationToken.None);
			mocker.Verify<ILogger>(l => l.Log("Initialized"));
		}

		[Test]
		public async Task Should_Initialize_Dependencies_In_Order()
        {
			var disposalToken = CancellationToken.None;
			List<int> callOrder = new List<int>();
			mocker.GetMock<ICoverageProjectFactory>().Setup(cp => cp.Initialize()).Callback(() =>
			{
				callOrder.Add(1);
			});
			mocker.GetMock<IFCCEngine>().Setup(engine => engine.Initialize(initializer, disposalToken)).Callback(() =>
			{
				callOrder.Add(2);
			});

			mocker.GetMock<IPackageInitializer>().Setup(p => p.InitializeAsync(disposalToken)).Callback(() =>
			{
				callOrder.Add(3);
			});

			await initializer.InitializeAsync(disposalToken);
			Assert.AreEqual(new List<int> { 1, 2, 3 }, callOrder);
		}

		[Test]
		public async Task Should_Pass_Itself_To_FCCEngine_For_InitializeStatus()
        {
			var disposalToken = CancellationToken.None;
			await initializer.InitializeAsync(disposalToken);
			mocker.Verify<IFCCEngine>(engine => engine.Initialize(initializer, disposalToken));
        }

		[Test]
		public void Should_ThrowIfCancellationRequested_When_PollInitializedStatusAsync()
        {
			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.Cancel();

			Assert.ThrowsAsync<OperationCanceledException>(
				async () => await initializer.WaitForInitializedAsync(cancellationTokenSource.Token));
        }

		[Test]
		public async Task Should_Throw_If_InitializationFailed_When_PollInitializedStatusAsync()
        {
			var mockCoverageProjectFactory = mocker.GetMock<ICoverageProjectFactory>();
			mockCoverageProjectFactory.Setup(
				coverageProjectFactory => coverageProjectFactory.Initialize()
				).Throws(new Exception("The exception message"));

			await initializer.InitializeAsync(CancellationToken.None);

			Assert.ThrowsAsync<Exception>(
				async () => await initializer.WaitForInitializedAsync(CancellationToken.None),
				"Initialization failed.  Please check the following error which may be resolved by reopening visual studio which will start the initialization process again.  The exception message"
				);
		}

		[Test]
		public async Task Should_PollInitializedStatus_Logging_If_Initializing()
        {
		    var times = 5;
			initializer.initializeWait = 100;
			var pollInitializedStatusTask =  initializer.WaitForInitializedAsync(CancellationToken.None);

            var setInitializedTask = Task.Delay(times * initializer.initializeWait).ContinueWith(_ =>
            {
                initializer.InitializeStatus = InitializeStatus.Initialized;
            });

			await Task.WhenAll(pollInitializedStatusTask, setInitializedTask);

            mocker.Verify<ILogger>(l => l.Log(CoverageStatus.Initializing.Message()), Times.AtLeast(times));
        }
	}
}