namespace FineCodeCoverageTests_AppOptions_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMoq;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Options;
    using Moq;
    using NUnit.Framework;

    public class AppOptionsProvider_Tests
    {
        private AutoMoqer autoMocker;
        private AppOptionsProvider appOptionsProvider;
        private Mock<IWritableSettingsStore> mockWritableSettingsStore;

        [SetUp]
        public void Setup()
        {
            this.autoMocker = new AutoMoqer();
            this.appOptionsProvider = this.autoMocker.Create<AppOptionsProvider>();
            this.mockWritableSettingsStore = new Mock<IWritableSettingsStore>();
            var mockWritableSettingsStoreProvider = this.autoMocker.GetMock<IWritableSettingsStoreProvider>();
            _ = mockWritableSettingsStoreProvider.Setup(
                writableSettingsStoreProvider => writableSettingsStoreProvider.Provide()
            ).Returns(this.mockWritableSettingsStore.Object);
        }


        [Test]
        public void Should_Ensure_Store_When_LoadSettingsFromStorage()
        {
            this.appOptionsProvider.LoadSettingsFromStorage(new Mock<IAppOptions>().Object);
            this.mockWritableSettingsStore.Verify(writableSettingsStore =>
                writableSettingsStore.CreateCollection("FineCodeCoverage")
            );
        }

        [Test]
        public void Should_Not_Create_Settings_Collection_If_Already_Exists()
        {
            _ = this.mockWritableSettingsStore.Setup(
                writableSettingsStore => writableSettingsStore.CollectionExists("FineCodeCoverage")).Returns(true);
            this.appOptionsProvider.LoadSettingsFromStorage(new Mock<IAppOptions>().Object);
            this.mockWritableSettingsStore.Verify(writableSettingsStore => writableSettingsStore.CreateCollection("FineCodeCoverage"), Times.Never());
        }

        [Test]
        public void Should_Ensure_Store_When_SaveSettingsToStorage()
        {
            this.appOptionsProvider.SaveSettingsToStorage(new Mock<IAppOptions>().Object);
            this.mockWritableSettingsStore.Verify(writableSettingsStore => writableSettingsStore.CreateCollection("FineCodeCoverage"));
        }

        [Test]
        public void Should_Not_Create_Settings_Collection_If_Already_Exists_When_SaveSettingsToStorage()
        {
            _ = this.mockWritableSettingsStore.Setup(
                writableSettingsStore => writableSettingsStore.CollectionExists("FineCodeCoverage")).Returns(true);
            this.appOptionsProvider.SaveSettingsToStorage(new Mock<IAppOptions>().Object);
            this.mockWritableSettingsStore.Verify(writableSettingsStore => writableSettingsStore.CreateCollection("FineCodeCoverage"), Times.Never());
        }

        [Test]
        public void Should_Have_Default_AppOptions_Property_When_Load_And_Does_Not_Exist_In_Storage()
        {
            var mockJsonConvertService = this.autoMocker.GetMock<IJsonConvertService>();
            _ = mockJsonConvertService.Setup(
                jsonConvertService =>
                jsonConvertService.DeserializeObject(It.IsAny<string>(), typeof(string[]))
            ).Returns(new string[] { });

            _ = this.mockWritableSettingsStore.Setup(
                writableSettingsStore =>
                writableSettingsStore.PropertyExists("FineCodeCoverage", nameof(IAppOptions.AttributesExclude))
            ).Returns(false);
            var mockAppOptions = new Mock<IAppOptions>
            {
                DefaultValueProvider = new NullStringArrayDefaultValueProvider()
            };
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;

            this.appOptionsProvider.LoadSettingsFromStorage(appOptions);

            Assert.That(appOptions.AttributesExclude, Is.Null);
            this.mockWritableSettingsStore.VerifyAll();
        }

        [Test]
        public void Should_Default_NamespacedClasses_True() =>
            this.DefaultTest(appOptions => appOptions.NamespacedClasses = true);

        private void DefaultTest(Action<IAppOptions> verifyOptions)
        {
            _ = this.mockWritableSettingsStore.Setup(
                writableSettingsStore => writableSettingsStore.PropertyExists("FineCodeCoverage", It.IsAny<string>())
            ).Returns(false);

            var mockAppOptions = new Mock<IAppOptions>();

            this.appOptionsProvider.LoadSettingsFromStorage(mockAppOptions.Object);

            mockAppOptions.VerifySet(verifyOptions);
        }

        [Test]
        public void Should_Default_Thresholds()
        {
            this.DefaultTest(appOptions => appOptions.ThresholdForCrapScore = 15);
            this.DefaultTest(appOptions => appOptions.ThresholdForNPathComplexity = 200);
            this.DefaultTest(appOptions => appOptions.ThresholdForCyclomaticComplexity = 30);
        }

        [Test]
        public void Should_Default_RunSettingsOnly_True() =>
            this.DefaultTest(appOptions => appOptions.RunSettingsOnly = true);

        [Test]
        public void Should_Default_RunWhenTestsFail_True() =>
            this.DefaultTest(appOptions => appOptions.RunWhenTestsFail = true);

        [Test]
        public void Should_Default_ExcludeByAttribute_GeneratedCode() =>
            this.DefaultTest(appOptions => appOptions.ExcludeByAttribute = new[] { "GeneratedCode" });

        [Test]
        public void Should_Default_IncludeTestAssembly_True() =>
            this.DefaultTest(appOptions => appOptions.IncludeTestAssembly = true);

        [Test]
        public void Should_Default_ExcludeByFile_Migrations() =>
            this.DefaultTest(appOptions => appOptions.ExcludeByFile = new[] { "**/Migrations/*" });

        [Test]
        public void Should_Default_Enabled_True() => this.DefaultTest(appOptions => appOptions.Enabled = true);

        [Test]
        public void Should_Default_True_ShowCoverageInOverviewMargin() =>
            this.DefaultTest(appOptions => appOptions.ShowCoverageInOverviewMargin = true);

        [Test]
        public void Should_Default_True_ShowCoveredInOverviewMargin() =>
            this.DefaultTest(appOptions => appOptions.ShowCoveredInOverviewMargin = true);

        [Test]
        public void Should_Default_True_ShowUncoveredInOverviewMargin() =>
            this.DefaultTest(appOptions => appOptions.ShowUncoveredInOverviewMargin = true);

        [Test]
        public void Should_Default_True_ShowPartiallyCoveredInOverviewMargin() =>
            this.DefaultTest(appOptions => appOptions.ShowPartiallyCoveredInOverviewMargin = true);

        [Test]
        public void Should_Not_Default_Any_Other_AppOptions_Properties()
        {
            _ = this.mockWritableSettingsStore.Setup(
                writableSettingsStore => writableSettingsStore.PropertyExists("FineCodeCoverage", It.IsAny<string>())
            ).Returns(false);

            var mockAppOptions = new Mock<IAppOptions>();

            this.appOptionsProvider.LoadSettingsFromStorage(mockAppOptions.Object);

            var invocationNames = mockAppOptions.Invocations.Select(invocation => invocation.Method.Name).ToList();

            var expectedSetters = new List<string>
            {
                nameof(IAppOptions.Enabled),
                nameof(IAppOptions.ExcludeByFile),
                nameof(IAppOptions.IncludeTestAssembly),
                nameof(IAppOptions.ExcludeByAttribute),
                nameof(IAppOptions.RunWhenTestsFail),
                nameof(IAppOptions.RunSettingsOnly),
                nameof(IAppOptions.ThresholdForCrapScore),
                nameof(IAppOptions.ThresholdForNPathComplexity),
                nameof(IAppOptions.ThresholdForCyclomaticComplexity),
                nameof(IAppOptions.NamespacedClasses),
                nameof(IAppOptions.ShowCoverageInOverviewMargin),
                nameof(IAppOptions.ShowCoveredInOverviewMargin),
                nameof(IAppOptions.ShowUncoveredInOverviewMargin),
                nameof(IAppOptions.ShowPartiallyCoveredInOverviewMargin)
            };
            Assert.That(expectedSetters.Select(s => $"set_{s}"), Is.EquivalentTo(invocationNames));
        }

        [TestCase(null)]
        [TestCase("  ")]
        public void Should_Have_Default_AppOptions_Property_When_Load_And_Is_Null_Or_Whitespace_In_Storage(string nullOrWhitespace)
        {
            var mockJsonConvertService = this.autoMocker.GetMock<IJsonConvertService>();
            _ = mockJsonConvertService.Setup(
                jsonConvertService =>
                jsonConvertService.DeserializeObject(It.IsAny<string>(), typeof(string[]))
            ).Returns(new string[] { });

            _ = this.mockWritableSettingsStore.Setup(
                writableSettingsStore =>
                writableSettingsStore.PropertyExists("FineCodeCoverage", nameof(IAppOptions.AttributesExclude)
            )
            ).Returns(true);
            _ = this.mockWritableSettingsStore.Setup(
                writableSettingsStore =>
                writableSettingsStore.GetString("FineCodeCoverage", nameof(IAppOptions.AttributesExclude))
            ).Returns(nullOrWhitespace);

            var mockAppOptions = new Mock<IAppOptions>
            {
                DefaultValueProvider = new NullStringArrayDefaultValueProvider()
            };
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;

            this.appOptionsProvider.LoadSettingsFromStorage(appOptions);

            Assert.That(appOptions.AttributesExclude, Is.Null);
            this.mockWritableSettingsStore.VerifyAll();
        }

        [Test]
        public void Should_Use_Deseralized_String_From_Store_For_AppOption_Property_LoadSettingsFromStorage()
        {
            var mockAppOptions = new Mock<IAppOptions>
            {
                DefaultValueProvider = new NullStringArrayDefaultValueProvider()
            };
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;

            this.Should_Use_Deseralized_String_From_Store_For_AppOption_Property(() =>
            {
                this.appOptionsProvider.LoadSettingsFromStorage(appOptions);
                return appOptions;
            });
        }

        [Test]
        public void Should_Use_Deseralized_String_From_Store_For_AppOption_Property_Get() =>
            this.Should_Use_Deseralized_String_From_Store_For_AppOption_Property(() => this.appOptionsProvider.Provide());

        private void Should_Use_Deseralized_String_From_Store_For_AppOption_Property(Func<IAppOptions> act)
        {
            var deserializedValues = new Dictionary<string, object>
            {
                { nameof(IAppOptions.AdjacentBuildOutput), false},
                { nameof(IAppOptions.AttributesExclude), new string[]{ "aexclude"}},
                { nameof(IAppOptions.AttributesInclude), new string[]{ "ainclude"}},
                { nameof(IAppOptions.CompanyNamesExclude), new string[]{ "cexclude"}},
                { nameof(IAppOptions.CompanyNamesInclude), new string[]{ "cinclude"}},
                { nameof(IAppOptions.CoverageColoursFromFontsAndColours), true},
                { nameof(IAppOptions.CoverletCollectorDirectoryPath), "p"},
                { nameof(IAppOptions.CoverletConsoleCustomPath), "cp"},
                { nameof(IAppOptions.CoverletConsoleGlobal), true},
                { nameof(IAppOptions.CoverletConsoleLocal), true},
                { nameof(IAppOptions.Enabled), true},
                { nameof(IAppOptions.Exclude), new string[]{"exclude" } },
                { nameof(IAppOptions.ExcludeByAttribute), new string[]{ "ebyatt"} },
                { nameof(IAppOptions.ExcludeByFile), new string[]{ "ebyfile"} },
                { nameof(IAppOptions.FCCSolutionOutputDirectoryName), "FCCSolutionOutputDirectoryName"},
                { nameof(IAppOptions.FunctionsExclude), new string[]{ "FunctionsExclude" } },
                { nameof(IAppOptions.FunctionsInclude), new string[]{ "FunctionsInclude" } },
                { nameof(IAppOptions.HideFullyCovered), true },
                { nameof(IAppOptions.Include), new string[]{ "Include" } },
                { nameof(IAppOptions.IncludeReferencedProjects),true},
                { nameof(IAppOptions.IncludeTestAssembly),true},
                { nameof(IAppOptions.ModulePathsExclude),new string[]{ "ModulePathsExclude" }},
                { nameof(IAppOptions.ModulePathsInclude),new string[]{ "ModulePathsInclude" }},
                { nameof(IAppOptions.NamespacedClasses),true},
                { nameof(IAppOptions.OpenCoverCustomPath),"OpenCoverCustomPath"},
                { nameof(IAppOptions.PublicKeyTokensExclude),new string[]{ "PublicKeyTokensExclude" }},
                { nameof(IAppOptions.PublicKeyTokensInclude),new string[]{ "PublicKeyTokensInclude" }},
                { nameof(IAppOptions.RunInParallel),true},
                { nameof(IAppOptions.RunSettingsOnly),true},
                { nameof(IAppOptions.RunWhenTestsExceed),1},
                { nameof(IAppOptions.RunWhenTestsFail),true},
                { nameof(IAppOptions.SourcesExclude),new string[]{ "SourcesExclude" }},
                { nameof(IAppOptions.SourcesInclude),new string[]{ "SourcesInclude" }},
                { nameof(IAppOptions.StickyCoverageTable),true},
                { nameof(IAppOptions.ThresholdForCrapScore),1},
                { nameof(IAppOptions.ThresholdForCyclomaticComplexity),1},
                { nameof(IAppOptions.ThresholdForNPathComplexity),1},
                { nameof(IAppOptions.ToolsDirectory),"ToolsDirectory"},
                { nameof(IAppOptions.RunMsCodeCoverage), RunMsCodeCoverage.IfInRunSettings},
                { nameof(IAppOptions.ShowCoverageInOverviewMargin),true},
                { nameof(IAppOptions.ShowCoveredInOverviewMargin),true},
                { nameof(IAppOptions.ShowPartiallyCoveredInOverviewMargin),true},
                { nameof(IAppOptions.ShowUncoveredInOverviewMargin),true},
            };
            var mockJsonConvertService = this.autoMocker.GetMock<IJsonConvertService>();
            _ = mockJsonConvertService.Setup(
                jsonConvertService =>
                jsonConvertService.DeserializeObject(It.IsAny<string>(), It.IsAny<Type>())
            ).Returns<string, Type>((serializedValueFromStore, _) =>
            {
                if (deserializedValues.ContainsKey(serializedValueFromStore))
                {
                    return deserializedValues[serializedValueFromStore];
                }
                return null;
            });

            _ = this.mockWritableSettingsStore.Setup(
                writableSettingsStore => writableSettingsStore.PropertyExists("FineCodeCoverage", It.IsAny<string>())
            ).Returns(true);

            _ = this.mockWritableSettingsStore.Setup(
                writableSettingsStore => writableSettingsStore.GetString("FineCodeCoverage", It.IsAny<string>())
            ).Returns<string, string>((_, propertyName) => propertyName);

            var appOptions = act();

            var appOptionsPropertyInfos = typeof(IAppOptions).GetPublicProperties();
            foreach (var appOptionsPropertyInfo in appOptionsPropertyInfos)
            {
                if (appOptionsPropertyInfo.PropertyType.IsValueType)
                {
                    Assert.That(appOptionsPropertyInfo.GetValue(appOptions), Is.EqualTo(deserializedValues[appOptionsPropertyInfo.Name]));
                }
                else
                {
                    Assert.That(appOptionsPropertyInfo.GetValue(appOptions), Is.SameAs(deserializedValues[appOptionsPropertyInfo.Name]));
                }
            }

        }

        [Test]
        public void Should_Log_Exception_Thrown_In_LoadSettingsFromStorage()
        {
            _ = this.mockWritableSettingsStore.Setup(
                writableSettingsStore => writableSettingsStore.PropertyExists("FineCodeCoverage", It.IsAny<string>())
            ).Returns(true);

            var exception = new Exception("msg");
            string failedToLoadPropertyName = null;
            _ = this.mockWritableSettingsStore.Setup(
                writableSettingsStore => writableSettingsStore.GetString("FineCodeCoverage", It.IsAny<string>())
            ).Callback<string, string>((_, propertyName) => failedToLoadPropertyName = propertyName).Throws(exception);

            this.appOptionsProvider.LoadSettingsFromStorage(new Mock<IAppOptions>().Object);

            this.autoMocker.Verify<ILogger>(logger => logger.Log($"Failed to load '{failedToLoadPropertyName}' setting", exception));
        }

        [Test]
        public void IAppOptions_Should_Have_A_Getter_And_Setter_For_Each_Property()
        {
            var propertyInfos = typeof(IAppOptions).GetPublicProperties();
            Assert.That(propertyInfos.All(pi => pi.GetMethod != null && pi.SetMethod != null), Is.True);
        }

        [Test]
        public void Should_Write_The_Serialized_Property_Value_To_The_Store()
        {
            var propertyValue = new string[] { "CompanyNamesExclude" };
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupGet(appOptions => appOptions.CompanyNamesExclude).Returns(propertyValue);

            var mockJsonConvertService = this.autoMocker.GetMock<IJsonConvertService>();
            _ = mockJsonConvertService.Setup(
                jsonConvertService =>
                jsonConvertService.SerializeObject(propertyValue)
            ).Returns("Serialized");

            this.appOptionsProvider.SaveSettingsToStorage(mockAppOptions.Object);

            this.mockWritableSettingsStore.Verify(
                writableSettingsStore => writableSettingsStore.SetString("FineCodeCoverage", nameof(IAppOptions.CompanyNamesExclude), "Serialized")
            );

        }

        [Test]
        public void Should_Raise_Options_Changed_When_SaveSettingsToStorage()
        {
            var mockAppOptions = new Mock<IAppOptions>();
            var appOptions = mockAppOptions.Object;

            IAppOptions changedOptions = null;
            this.appOptionsProvider.OptionsChanged += (options) => changedOptions = options;

            this.appOptionsProvider.SaveSettingsToStorage(appOptions);

            Assert.That(changedOptions, Is.SameAs(appOptions));
        }

        [Test]
        public void Should_Log_If_Exception_When_SaveSettingsToStorage()
        {
            var exception = new Exception();
            _ = this.mockWritableSettingsStore.Setup(
                writeableSettingsStore => writeableSettingsStore.SetString("FineCodeCoverage", nameof(IAppOptions.Enabled), It.IsAny<string>())
            ).Throws(exception);

            var mockAppOptions = new Mock<IAppOptions>();

            this.appOptionsProvider.SaveSettingsToStorage(mockAppOptions.Object);

            this.autoMocker.Verify<ILogger>(logger => logger.Log($"Failed to save '{nameof(IAppOptions.Enabled)}' setting", exception));
        }
    }

    internal class NullStringArrayDefaultValueProvider : LookupOrFallbackDefaultValueProvider
    {
        public NullStringArrayDefaultValueProvider() => this.Register(typeof(string[]), (type, mock) => null);
    }
}
