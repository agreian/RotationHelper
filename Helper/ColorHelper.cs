using System;
using System.Windows.Media;

namespace RotationHelper.Helper
{
    public static class ColorHelper
    {
        #region Constants

        private const int DIFF = 10;

        #endregion

        #region Methods

        public static bool AreEquivalent(this Color color, Color color2)
        {
            return Math.Abs(color.R - color2.R) <= DIFF && Math.Abs(color.G - color2.G) <= DIFF && Math.Abs(color.B - color2.B) <= DIFF;
        }

        #endregion
    }
}