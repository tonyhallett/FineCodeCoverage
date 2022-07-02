using System.Windows;

namespace FineCodeCoverage.Automation
{
    public interface IDependencyPropertyItemStatusProvider
    {
        DependencyProperty DependencyProperty { get; }
        string ProvideStatus(object value);
    }
}
