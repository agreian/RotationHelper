﻿using System.Windows.Controls;

namespace RotationHelper.View
{
    public class ScrollingTextBox : TextBox
    {
        #region Methods

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            CaretIndex = Text.Length;
            ScrollToEnd();
        }

        #endregion
    }
}