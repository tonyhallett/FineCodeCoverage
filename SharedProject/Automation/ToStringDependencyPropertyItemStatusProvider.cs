using System.Windows;

namespace FineCodeCoverage.Automation
{
	public class ToStringDependencyPropertyItemStatusProvider : IDependencyPropertyItemStatusProvider
	{
		public static IDependencyPropertyItemStatusProvider From(DependencyProperty dependencyProperty)
		{
			return new ToStringDependencyPropertyItemStatusProvider
			{
				DependencyProperty = dependencyProperty
			};
		}

		public DependencyProperty DependencyProperty { get; set; }
		public string ProvideStatus(object value)
		{
			return value.ToString();
		}
	}


}
