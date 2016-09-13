using System.Windows;

namespace RotationHelper
{
    /// <summary>
    ///     Logique d'interaction pour EditRotationWindow.xaml
    /// </summary>
    public partial class EditRotationWindow : Window
    {
        #region Constructors

        public EditRotationWindow(RotationHelperFile rotationHelperFile)
        {
            InitializeComponent();

            DataContext = new EditRotationViewModel(rotationHelperFile);
        }

        #endregion
    }
}