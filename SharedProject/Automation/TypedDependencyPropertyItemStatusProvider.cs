using System;
using System.Windows;

namespace FineCodeCoverage.Automation
{
	public class TypedDependencyPropertyItemStatusProvider<T> : IDependencyPropertyItemStatusProvider
	{
		private readonly Func<T, string> typedValueProvider;

		public TypedDependencyPropertyItemStatusProvider(
			Func<T, string> typedValueProvider, 
			DependencyProperty dependencyProperty
		)
		{
			this.typedValueProvider = typedValueProvider;
			this.DependencyProperty = dependencyProperty;
		}
		public DependencyProperty DependencyProperty { get; set; }
		public string ProvideStatus(object value)
		{
			return this.typedValueProvider((T)value);
		}
	}
}
