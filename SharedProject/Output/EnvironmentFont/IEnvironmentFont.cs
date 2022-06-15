using System;
using System.Windows;

namespace FineCodeCoverage.Output.EnvironmentFont
{
	public interface IEnvironmentFont
	{
		void Initialize(FrameworkElement frameworkElement, Action<FontDetails> fontDetailsChangedHandler);
	}
}
