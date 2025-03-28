using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Runtime.InteropServices;

namespace DMAW_DND.Source.Classes
{
    public static class WindowHelper
    {
        private const int GWL_EXSTYLE = -20;
        // Prevents the window from being activated when clicked.
        private const int WS_EX_NOACTIVATE = 0x08000000;
        // Keeps the window always on top.
        private const int WS_EX_TOPMOST = 0x00000008;
        // Hides the window from the taskbar.
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static void SetUnfocusedStyles(IntPtr windowHandle)
        {
            int exStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
            exStyle |= WS_EX_NOACTIVATE | WS_EX_TOPMOST | WS_EX_TOOLWINDOW;
            SetWindowLong(windowHandle, GWL_EXSTYLE, exStyle);
        }
    }

}
