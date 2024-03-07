﻿using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TextInfo : ITextInfo
    {
        private bool triedGetProperty;
        private ITextDocument document;
        private ITextDocument TextDocument
        {
            get
            {
                if (!this.triedGetProperty)
                {
                    this.triedGetProperty = true;
                    if (this.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument document))
                    {
                        this.document = document;
                    }
                }

                return this.document;
            }
        }
        public TextInfo(ITextView textView, ITextBuffer textBuffer)
        {
            this.TextView = textView;
            this.TextBuffer = textBuffer as ITextBuffer2;
        }

        public ITextView TextView { get; }
        public ITextBuffer2 TextBuffer { get; }
        public string FilePath => this.TextDocument?.FilePath;
    }
}
