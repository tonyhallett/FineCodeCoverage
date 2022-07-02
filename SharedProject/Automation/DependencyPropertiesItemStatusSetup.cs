using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FineCodeCoverage.Automation
{
	public static class DependencyPropertiesItemStatusSetup
	{
		public static void ForType(Type elementType, List<DependencyProperty> dependencyProperties)
		{
			ForType(elementType, dependencyProperties.Select(dp => ToStringDependencyPropertyItemStatusProvider.From(dp)).ToList());
		}

		public static void ForType(Type elementType, List<IDependencyPropertyItemStatusProvider> dependencyPropertyItemStatusProviders)
		{
			var itemStatus = new DependencyPropertiesItemStatus(dependencyPropertyItemStatusProviders);
			foreach (var dependencyPropertyItemStatusProvider in dependencyPropertyItemStatusProviders)
			{
				var dependencyProperty = dependencyPropertyItemStatusProvider.DependencyProperty;
				dependencyProperty.OverrideMetadata(elementType, new FrameworkPropertyMetadata((depObj, args) =>
				{
					itemStatus.PropertyChanged(dependencyPropertyItemStatusProvider, depObj as FrameworkElement);
				}));
			}
		}
	}

}
