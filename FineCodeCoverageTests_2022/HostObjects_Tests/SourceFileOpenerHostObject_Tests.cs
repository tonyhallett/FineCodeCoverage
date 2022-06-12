namespace FineCodeCoverageTests.HostObjectTests
{
    using System;
    using EnvDTE;
    using EnvDTE80;
    using FineCodeCoverage;
    using FineCodeCoverage.Output.HostObjects;
    using Microsoft.VisualStudio.Shell.Interop;
    using Moq;
    using NUnit.Framework;

    internal class SourceFileOpenerHostObject_Tests
    {
        private Mock<ILogger> mockLogger;
        private Mock<Window> mockMainWindow;
        private Mock<ItemOperations> mockItemOperations;
        private Mock<Document> mockActiveDocument;
        private Mock<TextSelection> mockTextSelection;

        private SourceFileOpenerHostObjectRegistration sourceFileOpenerHostObjectRegistration;
        private SourceFileOpenerHostObject sourceFileOpenerHostObject;

        [SetUp]
        public void SetUp()
        {
            this.mockLogger = new Mock<ILogger>();

            this.mockMainWindow = new Mock<Window>();
            this.mockItemOperations = new Mock<ItemOperations>();
            this.mockActiveDocument = new Mock<Document>();
            this.mockTextSelection = new Mock<TextSelection>();
            _ = this.mockActiveDocument.SetupGet(document => document.Selection).Returns(this.mockTextSelection.Object);

            var mockDTE = new Mock<DTE2>();
            _ = mockDTE.SetupGet(dte => dte.MainWindow).Returns(this.mockMainWindow.Object);
            _ = mockDTE.SetupGet(dte => dte.ItemOperations).Returns(this.mockItemOperations.Object);
            _ = mockDTE.SetupGet(dte => dte.ActiveDocument).Returns(this.mockActiveDocument.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            _ = mockServiceProvider.Setup(x => x.GetService(typeof(SDTE))).Returns(mockDTE.Object);

            this.sourceFileOpenerHostObjectRegistration = new SourceFileOpenerHostObjectRegistration(
                mockServiceProvider.Object,
                this.mockLogger.Object
            );
            this.sourceFileOpenerHostObject = this.sourceFileOpenerHostObjectRegistration.HostObject as SourceFileOpenerHostObject;

        }

        [Test]
        public void Should_Have_Name() =>
            Assert.That(this.sourceFileOpenerHostObjectRegistration.Name, Is.EqualTo("sourceFileOpener"));

        [Test]
        public void Should_Activate_MainWindow_Before_openFiles_When_Called_From_Js()
        {
            var mainWindowActivated = false;
            _ = this.mockMainWindow.Setup(mw => mw.Activate()).Callback(() => mainWindowActivated = true);
            void expectMainWindowActivated() => Assert.That(mainWindowActivated, Is.True);
            _ = this.mockItemOperations.Setup(itemOperations => itemOperations.OpenFile("class1.partial1.cs", EnvDTE.Constants.vsViewKindCode)).Callback(expectMainWindowActivated);
            _ = this.mockItemOperations.Setup(itemOperations => itemOperations.OpenFile("class1.partial2.cs", EnvDTE.Constants.vsViewKindCode)).Callback(expectMainWindowActivated);

            this.sourceFileOpenerHostObject.openFiles(new object[] { "class1.partial1.cs", "class1.partial2.cs" });

            this.mockItemOperations.VerifyAll();
        }

        [Test]
        public void Should_Log_When_Exception_Opening_File()
        {
            _ = this.mockItemOperations.Setup(
                itemOperations => itemOperations.OpenFile(
                    "class.cs",
                    EnvDTE.Constants.vsViewKindCode
                )
            ).Throws(new Exception());
            this.sourceFileOpenerHostObject.openFiles(new object[] { "class.cs" });

            this.mockLogger.Verify(logger => logger.Log("Unable to open file - class.cs"));

        }

        [Test]
        public void Should_Activate_MainWindow_Open_File_And_GotoLine_When_Called_From_Js()
        {
            var mainWindowActivated = false;
            var fileOpened = false;
            var goneToLine = false;
            _ = this.mockMainWindow.Setup(mw => mw.Activate()).Callback(() =>
              {
                  Assert.Multiple(() =>
                  {
                      Assert.That(fileOpened, Is.False);
                      Assert.That(goneToLine, Is.False);
                  });

                  mainWindowActivated = true;
              });
            _ = this.mockItemOperations.Setup(itemOperations => itemOperations.OpenFile("class.cs", EnvDTE.Constants.vsViewKindCode))
                .Callback(() =>
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(mainWindowActivated, Is.True);
                        Assert.That(goneToLine, Is.False);
                    });

                    fileOpened = true;
                });
            _ = this.mockTextSelection.Setup(textSelection => textSelection.GotoLine(42, false)).Callback(() =>
               {
                   Assert.Multiple(() =>
                   {
                       Assert.That(mainWindowActivated, Is.True);
                       Assert.That(fileOpened, Is.True);
                   });

                   goneToLine = true;
               });

            this.sourceFileOpenerHostObject.openAtLine("class.cs", 42);

            this.mockItemOperations.VerifyAll();
            this.mockTextSelection.VerifyAll();
            this.mockMainWindow.VerifyAll();
        }

        [Test]
        public void Should_Not_GotoLine_When_Exception_Opening_File()
        {
            _ = this.mockItemOperations.Setup(itemOperations => itemOperations.OpenFile("class.cs", EnvDTE.Constants.vsViewKindCode)).Throws(new Exception());

            this.sourceFileOpenerHostObject.openAtLine("class.cs", 42);

            this.mockTextSelection.Verify(textSelection => textSelection.GotoLine(42, false), Times.Never);

        }

    }


}
