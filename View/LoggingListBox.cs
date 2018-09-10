using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Media;

namespace RotationHelper.View
{
    public class LoggingListBox : ListBox
    {

        #region Methods

        #region Protected Methods

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            var border = (Border)VisualTreeHelper.GetChild(this, 0);
            var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
            scrollViewer.ScrollToBottom();
        }

        #endregion

        #endregion

    }
}