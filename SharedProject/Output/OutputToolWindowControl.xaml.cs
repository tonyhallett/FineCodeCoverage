using System.Windows.Controls;

namespace FineCodeCoverage.Output
{
	/// <summary>
	/// Interaction logic for OutputToolWindowControl.
	/// </summary>
	internal partial class OutputToolWindowControl : 
		UserControl
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="OutputToolWindowControl"/> class.
        /// </summary>
        public OutputToolWindowControl(ReportViewModel reportViewModel)
		{
			this.DataContext = reportViewModel;
			InitializeComponent();
		}
    }
}