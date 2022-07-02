using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FineCodeCoverage.Automation
{
	public class DependencyPropertiesItemStatus
	{
		private readonly List<IDependencyPropertyItemStatusProvider> dependencyPropertyItemStatusProviders;
		private readonly Dictionary<DependencyProperty, string> values;

		public DependencyPropertiesItemStatus(List<IDependencyPropertyItemStatusProvider> dependencyPropertyItemStatusProviders)
		{
			values = dependencyPropertyItemStatusProviders.ToDictionary(dpisp => dpisp.DependencyProperty, dpisp => (string)null);
			this.dependencyPropertyItemStatusProviders = dependencyPropertyItemStatusProviders;
		}

		public void PropertyChanged(
			IDependencyPropertyItemStatusProvider dependencyPropertyItemStatusProvider, 
			FrameworkElement element
		)
		{
			UpdateValues(dependencyPropertyItemStatusProvider, element);
			SetAutomationStatus(element);
		}

		private void UpdateValues(
			IDependencyPropertyItemStatusProvider dependencyPropertyItemStatusProvider,
			FrameworkElement element
        )
        {
			var changedDp = dependencyPropertyItemStatusProvider.DependencyProperty;
			values[changedDp] = dependencyPropertyItemStatusProvider.ProvideStatus(element.GetValue(changedDp));
		}

		private void SetAutomationStatus(FrameworkElement element)
        {
			var itemStatus = DependencyPropertiesItemStatusConverter.ToString(values);
			System.Windows.Automation.AutomationProperties.SetItemStatus(element, itemStatus);
		}

	}
}
