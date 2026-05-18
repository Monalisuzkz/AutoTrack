using System;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
    internal static class LabelExt
    {
        public static Label Let(this Label l, Action<Label> action) { action(l); return l; }
    }
}
