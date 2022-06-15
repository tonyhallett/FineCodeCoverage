namespace FineCodeCoverageTests.Tagging_Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Impl;
    using FineCodeCoverage.Options;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Tagging;
    using Moq;
    using NUnit.Framework;

    internal abstract class CoverageLineTaggerProviderBase_Tests<TTaggerProvider, TTaggerListener, TTag>
        where TTaggerProvider : CoverageLineTaggerProviderBase<TTaggerListener, TTag>
        where TTaggerListener : ITagger<TTag>, IListener<NewCoverageLinesMessage>
        where TTag : ITag
    {
        protected AutoMoqer Mocker { get; private set; }
        protected TTaggerProvider TaggerProvider { get; private set; }

        [SetUp]
        public void SetUp()
        {
            this.Mocker = new AutoMoqer();
            this.SetUpBefore();
            this.TaggerProvider = this.Mocker.Create<TTaggerProvider>();
            this.SetUpAfter();

        }

        protected virtual void SetUpBefore() { }
        protected virtual void SetUpAfter() { }

        [Test]
        public void Should_Register_As_Listener_Of_NewCoverageLinesMessage() =>
            this.Mocker.Verify<IEventAggregator>(eventAggregator => eventAggregator.AddListener(this.TaggerProvider, null));

        [Test]
        public void Should_Create_A_CoverageLineTagger_With_The_TextBuffer()
        {
            var textBuffer = new Mock<ITextBuffer>().Object;
            var coverageLineTagger = this.TaggerProvider.CreateTagger<TTag>(textBuffer) as CoverageLineTaggerBase<TTag>;

            Assert.Multiple(() =>
            {
                Assert.That(coverageLineTagger._textBuffer, Is.SameAs(textBuffer));
                Assert.That(coverageLineTagger._coverageLines, Is.Null);
            });
        }

        [Test]
        public void Should_Create_A_CoverageLineTagger_As_A_Weak_Referenced_Listener_Of_NewCoverageLinesMessage()
        {
            var coverageLineTagger = this.TaggerProvider.CreateTagger<TTag>(null) as CoverageLineTaggerBase<TTag>;

            this.Mocker.Verify<IEventAggregator>(eventAggregator => eventAggregator.AddListener(coverageLineTagger, false));
        }

        [Test]
        public void Should_Create_A_CoverageLineTagger_With_The_CoverageLines_From_The_Last_NewCoverageLinesMessage()
        {
            var coverageLines = new List<CoverageLine>();
            this.TaggerProvider.Handle(new NewCoverageLinesMessage { CoverageLines = coverageLines });

            var mockTextBuffer = new Mock<ITextBuffer>();
            _ = mockTextBuffer.SetupGet(textBuffer => textBuffer.CurrentSnapshot).Returns(new Mock<ITextSnapshot>().Object);
            var coverageLineTagger = this.TaggerProvider.CreateTagger<TTag>(mockTextBuffer.Object) as CoverageLineTaggerBase<TTag>;
            Assert.That(coverageLineTagger._coverageLines, Is.SameAs(coverageLines));
        }
    }

    internal class CoverageLineGlyphTaggerProvider_Tests :
        CoverageLineTaggerProviderBase_Tests<
            CoverageLineGlyphTaggerProvider,
            CoverageLineGlyphTagger,
            CoverageLineGlyphTag
        >
    {
        protected override void SetUpAfter() => this.TaggerProvider.ThreadHelper = new TestThreadHelper();

        [Test]
        public void Should_Prepare_CoverageColoursProvider_When_NewCoverageLinesMessage_Handled()
        {
            this.TaggerProvider.Handle(new NewCoverageLinesMessage());
            this.Mocker.Verify<ICoverageColoursProvider>(coverageColoursProvider => coverageColoursProvider.PrepareAsync());
        }

        [Test]
        public void Should_Send_RefreshCoverageGlyphsMessage_When_CoverageColours_Changed()
        {
            var mockCoverageColours = this.Mocker.GetMock<ICoverageColours>();
            mockCoverageColours.Raise(coverageColours => coverageColours.ColoursChanged += null, EventArgs.Empty);

            this.Mocker.Verify<IEventAggregator>(eventAggregator => eventAggregator.SendMessage(It.IsAny<RefreshCoverageGlyphsMessage>(), null));
        }

    }

    internal class CoverageLineMarkTaggerProvider_Tests :
        CoverageLineTaggerProviderBase_Tests<
            CoverageLineMarkTaggerProvider,
            CoverageLineMarkTagger,
            OverviewMarkTag
        >
    {
        private IAppOptions initialAppOptions;

        protected override void SetUpBefore()
        {
            this.initialAppOptions = this.GetInitialCoverageMarginAppOptions();
            _ = this.Mocker.GetMock<IAppOptionsProvider>()
                .Setup(
                    appOptionsProvider => appOptionsProvider.Provide()
                )
                .Returns(this.initialAppOptions);
        }

        private IAppOptions GetInitialCoverageMarginAppOptions() => this.GetCoverageMarginAppOptions(true, true, true, true);

        private IAppOptions GetCoverageMarginAppOptions(
            bool showCoverageInOverviewMargin,
            bool showCoveredInOverviewMargin,
            bool showPartiallyCoveredInOverviewMargin,
            bool showUncoveredInOverviewMargin
        ) => new AppOptions
        {
            ShowCoverageInOverviewMargin = showCoverageInOverviewMargin,
            ShowCoveredInOverviewMargin = showCoveredInOverviewMargin,
            ShowPartiallyCoveredInOverviewMargin = showPartiallyCoveredInOverviewMargin,
            ShowUncoveredInOverviewMargin = showUncoveredInOverviewMargin
        };

        //[Test]
        //public void Should_Create_A_CoverageLineMarkTagger_With_The_TextBuffer()
        //{
        //    var textBuffer = new Mock<ITextBuffer>().Object;
        //    var coverageLineMarkTagger = coverageLineMarkTaggerProvider.CreateTagger<OverviewMarkTag>(textBuffer) as CoverageLineMarkTagger;

        //    Assert.AreSame(textBuffer, coverageLineMarkTagger._textBuffer);
        //}

        [Test]
        public void Should_Create_A_CoverageLineMarkTagger_With_The_Initial_CoverageMarginAppOptions()
        {
            var textBuffer = new Mock<ITextBuffer>().Object;
            var coverageLineMarkTagger = this.TaggerProvider.CreateTagger<OverviewMarkTag>(textBuffer) as CoverageLineMarkTagger;

            Assert.Multiple(() =>
            {
                Assert.That(coverageLineMarkTagger._coverageMarginOptions.Show(CoverageType.Partial), Is.True);
                Assert.That(coverageLineMarkTagger._coverageMarginOptions.Show(CoverageType.NotCovered), Is.True);
                Assert.That(coverageLineMarkTagger._coverageMarginOptions.Show(CoverageType.Covered), Is.True);
            });
        }

        [Test]
        public void Should_Create_A_CoverageLineMarkTagger_With_The_Changed_CoverageMarginAppOptions()
        {
            var appOptions = this.GetCoverageMarginAppOptions(true, false, false, false);
            this.Mocker.GetMock<IAppOptionsProvider>().Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, appOptions);

            var textBuffer = new Mock<ITextBuffer>().Object;
            var coverageLineMarkTagger = this.TaggerProvider.CreateTagger<OverviewMarkTag>(textBuffer) as CoverageLineMarkTagger;

            Assert.Multiple(() =>
            {
                Assert.That(coverageLineMarkTagger._coverageMarginOptions.Show(CoverageType.Partial), Is.False);
                Assert.That(coverageLineMarkTagger._coverageMarginOptions.Show(CoverageType.NotCovered), Is.False);
                Assert.That(coverageLineMarkTagger._coverageMarginOptions.Show(CoverageType.Covered), Is.False);
            });
        }

        [Test]
        public void Should_Send_CoverageMarginOptionsChangedMessage_With_New_CoverageMarginOptions_When_Changes()
        {
            var appOptions = this.GetCoverageMarginAppOptions(true, false, false, false);
            this.Mocker.GetMock<IAppOptionsProvider>().Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, appOptions);

            var coverageMarginOptionsChangedMessage = this.Mocker.GetMock<IEventAggregator>().GetSendMessageMessages<CoverageMarginOptionsChangedMessage>().Single();

            Assert.Multiple(() =>
            {
                Assert.That(coverageMarginOptionsChangedMessage.Options.Show(CoverageType.Partial), Is.False);
                Assert.That(coverageMarginOptionsChangedMessage.Options.Show(CoverageType.NotCovered), Is.False);
                Assert.That(coverageMarginOptionsChangedMessage.Options.Show(CoverageType.Covered), Is.False);
            });
        }

        [Test]
        public void Should_Not_Send_CoverageMarginOptionsChangedMessage_When_AppOptions_Change_But_No_CoverageMarginOptions_Change()
        {
            var appOptions = this.GetInitialCoverageMarginAppOptions();
            this.Mocker.GetMock<IAppOptionsProvider>().Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, appOptions);

            this.Mocker.Verify<IEventAggregator>(eventAggregator =>
                eventAggregator.SendMessage(It.IsAny<CoverageMarginOptionsChangedMessage>(), It.IsAny<Action<Action>>()),
                Times.Never()
            );
        }

    }

    internal class CoverageMarginOptions_Tests
    {
        [Test]
        public void Should_Not_Show_When_ShowCoverageInOverviewMargin_False()
        {
            var coverageMarginOptions = CoverageMarginOptions.Create(
                new AppOptions
                {
                    ShowCoverageInOverviewMargin = false,
                    ShowCoveredInOverviewMargin = true,
                    ShowPartiallyCoveredInOverviewMargin = true,
                    ShowUncoveredInOverviewMargin = true
                }
            );

            Assert.Multiple(() =>
            {
                Assert.That(coverageMarginOptions.Show(CoverageType.Covered), Is.False);
                Assert.That(coverageMarginOptions.Show(CoverageType.NotCovered), Is.False);
                Assert.That(coverageMarginOptions.Show(CoverageType.Partial), Is.False);
            });
        }
    }

}
