using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;

namespace FineCodeCoverage.Output.EnvironmentFont
{
	[Export(typeof(IEnvironmentFont))]
	public class EnvironmentFont : DependencyObject, IEnvironmentFont
	{

		private static DependencyProperty EnvironmentFontSizeProperty;

		private static DependencyProperty EnvironmentFontFamilyProperty;

		private double Size { get; set; }

		private FontFamily Family { get; set; }


		private Action<FontDetails> fontDetailsChangedHandler;

		public void Initialize(FrameworkElement frameworkElement, Action<FontDetails> fontDetailsChangedHandler)
		{
			this.fontDetailsChangedHandler = fontDetailsChangedHandler;
			RegisterDependencyProperties(frameworkElement.GetType());
			frameworkElement.SetResourceReference(EnvironmentFontSizeProperty, VsFonts.EnvironmentFontSizeKey);
			frameworkElement.SetResourceReference(EnvironmentFontFamilyProperty, VsFonts.EnvironmentFontFamilyKey);
		}

		private void RegisterDependencyProperties(Type controlType)
		{
			EnvironmentFontSizeProperty = DependencyProperty.Register("EnvironmentFontSize", typeof(double), controlType, new PropertyMetadata((obj, args) =>
			{
				var newSize = (double)args.NewValue;
				if (newSize != Size)
                {
					Size = newSize;
					ValueChanged();
				}
				
			}));

			EnvironmentFontFamilyProperty = DependencyProperty.Register("EnvironmentFontFamily", typeof(FontFamily), controlType, new PropertyMetadata((obj, args) =>
			{
				var newFamily = (FontFamily)args.NewValue;
				if (Family == null || newFamily.Source != Family.Source)
                {
					Family = newFamily;
					ValueChanged();
				}
			}));
		}

		private void ValueChanged()
		{
			if (Family != null && Size != default)
			{
				fontDetailsChangedHandler(new FontDetails(Size, Family.Source));
			}
		}
	}
}
