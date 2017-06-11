using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Diagnostics;

namespace GLGUI.GLControlExample
{
    public class GLCanvas : OpenTK.GLControl
    {
        GLCtrlContainer rootCtrl;
        GLLabel console;
        LineWriter consoleWriter;

        Stopwatch stopwatch;
        double time = 0.0;

        public GLCanvas()
            : base(new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 0, 4))
        {
            consoleWriter = new LineWriter();
            Console.SetOut(consoleWriter);
            Console.SetError(consoleWriter);

            if (!this.DesignMode)
            {
                this.Load += OnLoad;
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.rootCtrl = new GLCtrlContainer(this);
            {
                var mainAreaControl = rootCtrl.Add(new GLGroupLayout(rootCtrl) { Size = new Size(ClientSize.Width, ClientSize.Height - 200), Anchor = GLAnchorStyles.All });
                // change background color:
                var mainSkin = mainAreaControl.Skin;
                mainSkin.BackgroundColor = rootCtrl.Skin.FormActive.BackgroundColor;
                mainSkin.BorderColor = rootCtrl.Skin.FormActive.BorderColor;
                mainAreaControl.Skin = mainSkin;
                {
                    var loremIpsumForm = mainAreaControl.Add(new GLForm(rootCtrl) { Title = "Lorem Ipsum", Location = new Point(600, 100), Size = new Size(300, 200) });
                    loremIpsumForm.Add(new GLTextBox(rootCtrl)
                    {
                        Text = "This is a GLTextBox in a GLForm in a GLGroupLayout.",
                        Multiline = true,
                        WordWrap = true,
                        Outer = new Rectangle(4, 4, loremIpsumForm.Inner.Width - 8, loremIpsumForm.Inner.Height - 8),
                        Anchor = GLAnchorStyles.All
                    }).Changed += (s, w) => Console.WriteLine(s + " text length: " + ((GLTextBox)s).Text.Length);
                }

                var consoleScrollControl = rootCtrl.Add(new GLScrollableControl(rootCtrl) { Outer = new Rectangle(0, ClientSize.Height - 200, ClientSize.Width, 200), Anchor = GLAnchorStyles.Left | GLAnchorStyles.Right | GLAnchorStyles.Bottom });
                {
                    console = consoleScrollControl.Add(new GLLabel(rootCtrl) { AutoSize = true, Multiline = true });
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

            if (this.consoleWriter.isChanged)
            {
                this.console.Text = string.Join("\n", this.consoleWriter.Lines);
                this.consoleWriter.isChanged = false;
            }

            this.rootCtrl.Render();
            SwapBuffers();
        }

        // draws a simple colored cube in a GLViewport control
        private void OnRenderViewport(object sender, double deltaTime)
        {
            var viewport = (GLViewport)sender;

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0, 0, 0, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            var proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), viewport.AspectRatio, 1.0f, 100.0f);
            GL.LoadMatrix(ref proj);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Translate(0, 0, -2.0f);
            GL.Rotate(time * 100.0f, 1, 0, 0);
            GL.Rotate(time * 42.0f, 0, 1, 0);

            GL.Begin(PrimitiveType.Quads);
            GL.Color3(1.0, 0.0, 0.0);
            GL.Vertex3(0.5, -0.5, -0.5);
            GL.Color3(0.0, 1.0, 0.0);
            GL.Vertex3(0.5, 0.5, -0.5);
            GL.Color3(0.0, 0.0, 1.0);
            GL.Vertex3(-0.5, 0.5, -0.5);
            GL.Color3(1.0, 0.0, 1.0);
            GL.Vertex3(-0.5, -0.5, -0.5);

            GL.Color3(1.0, 1.0, 1.0);
            GL.Vertex3(0.5, -0.5, 0.5);
            GL.Vertex3(0.5, 0.5, 0.5);
            GL.Vertex3(-0.5, 0.5, 0.5);
            GL.Vertex3(-0.5, -0.5, 0.5);

            GL.Color3(1.0, 0.0, 1.0);
            GL.Vertex3(0.5, -0.5, -0.5);
            GL.Vertex3(0.5, 0.5, -0.5);
            GL.Vertex3(0.5, 0.5, 0.5);
            GL.Vertex3(0.5, -0.5, 0.5);

            GL.Color3(0.0, 1.0, 0.0);
            GL.Vertex3(-0.5, -0.5, 0.5);
            GL.Vertex3(-0.5, 0.5, 0.5);
            GL.Vertex3(-0.5, 0.5, -0.5);
            GL.Vertex3(-0.5, -0.5, -0.5);

            GL.Color3(0.0, 0.0, 1.0);
            GL.Vertex3(0.5, 0.5, 0.5);
            GL.Vertex3(0.5, 0.5, -0.5);
            GL.Vertex3(-0.5, 0.5, -0.5);
            GL.Vertex3(-0.5, 0.5, 0.5);

            GL.Color3(1.0, 0.0, 0.0);
            GL.Vertex3(0.5, -0.5, -0.5);
            GL.Vertex3(0.5, -0.5, 0.5);
            GL.Vertex3(-0.5, -0.5, 0.5);
            GL.Vertex3(-0.5, -0.5, -0.5);
            GL.End();

            GL.Disable(EnableCap.DepthTest);
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

