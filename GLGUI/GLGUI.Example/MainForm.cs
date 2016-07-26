using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GLGUI.Example
{
    public class MainForm : GameWindow
    {
        GLGui glgui;
        LineWriter consoleWriter;

        int fpsCounter = 0;
        int fpsSecond = 1;
        double time = 0.0;

        public MainForm()
            : base(1024, 600, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 0, 4), "GLGUI Example")
        {
            consoleWriter = new LineWriter();
            Console.SetOut(consoleWriter);
            Console.SetError(consoleWriter);

            this.Load += OnLoad;
            this.Resize += (s, e) => GL.Viewport(ClientSize);
            this.RenderFrame += OnRender;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            VSync = VSyncMode.Off; // vsync is nice, but you can't really measure performance while it's on

            glgui = new GLGui(this);

            var mainAreaControl = glgui.Add(new GLGroupLayout(glgui) { Size = new Size(ClientSize.Width, ClientSize.Height - 200), Anchor = GLAnchorStyles.All });
            // change background color:
            var mainSkin = mainAreaControl.Skin;
            mainSkin.BackgroundColor = glgui.Skin.FormActive.BackgroundColor;
            mainSkin.BorderColor = glgui.Skin.FormActive.BorderColor;
            mainAreaControl.Skin = mainSkin;

            var loremIpsumForm = mainAreaControl.Add(new GLForm(glgui) { Title = "Lorem Ipsum", Location = new Point(600, 100), Size = new Size(300, 200) });
            loremIpsumForm.Add(new GLTextBox(glgui)
            {
                Text = "Lorem ipsum dolor sit amet,\nconsetetur sadipscing elitr,\nsed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat,\nsed diam voluptua.\n\nAt vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.",
                Multiline = true,
                WordWrap = true,
                Outer = new Rectangle(4, 4, loremIpsumForm.Inner.Width - 8, loremIpsumForm.Inner.Height - 8),
                Anchor = GLAnchorStyles.All
            }).Changed += (s, w) => Console.WriteLine(s + " text length: " + ((GLTextBox)s).Text.Length);

        }

        private void OnRender(object sender, FrameEventArgs e)
        {
            glgui.Render();
            SwapBuffers();
        }

    }
}

