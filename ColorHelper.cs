using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RotationHelper
{
    public static class ColorHelper
    {
        private const int DIFF = 10;

        public static bool AreEquivalent(this Color color, Color color2)
        {
            return Math.Abs(color.R - color2.R) <= DIFF && Math.Abs(color.G - color2.G) <= DIFF && Math.Abs(color.B - color2.B) <= DIFF;
        }
    }
}
