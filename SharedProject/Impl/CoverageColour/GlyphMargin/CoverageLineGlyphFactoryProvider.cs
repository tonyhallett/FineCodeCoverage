﻿using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using OrderAttribute = Microsoft.VisualStudio.Utilities.OrderAttribute;

namespace FineCodeCoverage.Impl
{
	[ContentType("code")]
	[TagType(typeof(CoverageLineGlyphTag))]
	[Order(Before = "VsTextMarker")]
	[Name(Vsix.GlyphFactoryProviderName)]
	[Export(typeof(IGlyphFactoryProvider))]
	internal class CoverageLineGlyphFactoryProvider: IGlyphFactoryProvider
	{
        public IGlyphFactory GetGlyphFactory(IWpfTextView textView, IWpfTextViewMargin textViewMargin)
		{
			return new CoverageLineGlyphFactory();
        }

    }
}