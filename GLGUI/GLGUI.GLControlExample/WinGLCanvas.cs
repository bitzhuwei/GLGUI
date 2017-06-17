using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Diagnostics;

namespace GLGUI.GLControlExample
{
    public class WinGLCanvas : OpenTK.GLControl
    {
        GLCtrlContainer rootCtrl;

        Stopwatch stopwatch;
        double time = 0.0;

        public WinGLCanvas()
            : base(new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 0, 4))
        {
            if (!this.DesignMode)
            {
                this.Load += OnLoad;
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.rootCtrl = new GLCtrlContainer(this);
            {
                var mainAreaControl = rootCtrl.Add(new GLGroupLayout(rootCtrl) { Size = new Size(ClientSize.Width, ClientSize.Height - 100), Anchor = GLAnchorStyles.All });
                var mainSkin = mainAreaControl.Skin;
                mainSkin.BackgroundColor = rootCtrl.Skin.FormActive.BackgroundColor;
                mainSkin.BorderColor = rootCtrl.Skin.FormActive.BorderColor;
                mainAreaControl.Skin = mainSkin;
                {
                    var form = mainAreaControl.Add(new GLForm(rootCtrl) { Title = "Lorem Ipsum", Location = new Point(600, 100), Size = new Size(600, 400), Anchor = GLAnchorStyles.All });
                    var textBox = form.Add(new GLTextBox(rootCtrl)
                       {
                           Text = "This is a GLTextBox in a GLForm in a GLGroupLayout.",
                           Multiline = true,
                           WordWrap = true,
                           Outer = new Rectangle(4, 4, form.Inner.Width - 8, form.Inner.Height - 8),
                           Anchor = GLAnchorStyles.All
                       });
                    textBox.Changed += (s, w) => Console.WriteLine(s + " text length: " + ((GLTextBox)s).Text.Length);
                }

                this.stopwatch = new Stopwatch();
                this.Resize += (s, ev) => GL.Viewport(ClientSize);
                this.Paint += OnRender;

                this.stopwatch.Start();
                Application.Idle += (s, ev) => Invalidate();
            }
        }

        private void OnRender(object sender, PaintEventArgs e)
        {
            this.stopwatch.Stop();
            double delta = this.stopwatch.Elapsed.TotalMilliseconds * 0.001;
            this.stopwatch.Restart();
            this.time += delta;

            this.rootCtrl.Render();
            SwapBuffers();
        }

        protected override bool IsInputKey(Keys key)
        {
            return true;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GuiControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.BackColor = System.Drawing.Color.Silver;
            this.Name = "GuiControl";
            this.ResumeLayout(false);

        }
    }
}

