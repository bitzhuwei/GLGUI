using System.Windows.Forms;

namespace GLGUI.GLControlExample
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            Text = "GLGUI.GLControlExample";
            Size = new System.Drawing.Size(1024, 600);

            var canvas = new GLCanvas();
            canvas.Dock = DockStyle.Fill;
            Controls.Add(canvas);
        }
    }
}
