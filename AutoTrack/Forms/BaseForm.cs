using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoTrack.Forms
{
    public class BaseForm : Form
    {
        public BaseForm()
        {
            // Set application icon
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("AutoTrack.Resources.autotrack.ico"))
                {
                    if (stream != null)
                        this.Icon = new Icon(stream);
                }
            }
            catch
            {
                // Icon not found - continue without icon
            }
        }
    }
}