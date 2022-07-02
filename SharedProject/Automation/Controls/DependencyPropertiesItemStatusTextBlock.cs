using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FineCodeCoverage.Automation.Controls
{
	public class DependencyPropertiesItemStatusTextBlock : TextBlock
	{
		static DependencyPropertiesItemStatusTextBlock()
		{
			DependencyPropertiesItemStatusSetup.ForType(
				typeof(DependencyPropertiesItemStatusTextBlock), 
				new List<DependencyProperty>
				{
					FontFamilyProperty,
					FontSizeProperty,
					BackgroundProperty,
					ForegroundProperty
				}
			);
		}
	}
}
