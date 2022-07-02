using System;
using System.Windows;

namespace FineCodeCoverage.Automation
{
	public static class TypedDependencyPropertyItemStatusProviderFactory
	{
		public static IDependencyPropertyItemStatusProvider From<T>(
			Func<T, string> typedValueProvider, 
			DependencyProperty dependencyProperty
		)
		{
			return new TypedDependencyPropertyItemStatusProvider<T>(typedValueProvider, dependencyProperty);
		}
	}
}
