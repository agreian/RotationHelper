using RotationHelper.Model;
using RotationHelper.ViewModel;

namespace RotationHelper.View
{
    /// <summary>
    ///     Logique d'interaction pour EditRotationWindow.xaml
    /// </summary>
    public partial class EditRotationWindow
    {
        #region Constructors

        public EditRotationWindow(RotationHelperFile rotationHelperFile)
        {
            InitializeComponent();

            DataContext = new EditRotationViewModel(this, rotationHelperFile);
        }

        #endregion
    }
}