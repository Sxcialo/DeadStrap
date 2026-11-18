using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for OverlaysPage.xaml
    /// </summary>
    public partial class OverlaysPage
    {
        public OverlaysPage()
        {
            DataContext = new OverlaysViewModel();
            InitializeComponent();
        }
    }
}
