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
        GLLabel fpsLabel;
        GLLabel console;
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

            fpsLabel = mainAreaControl.Add(new GLLabel(glgui) { Location = new Point(10, 10), AutoSize = true });
            // change font and background color:
            var skin = fpsLabel.SkinEnabled;
            skin.Font = new GLFont(new Font("Arial", 12.0f));
            skin.BackgroundColor = glgui.Skin.TextBoxActive.BackgroundColor;
            fpsLabel.SkinEnabled = skin;
        }

        private void OnRender(object sender, FrameEventArgs e)
        {
            time += e.Time;

            if (time >= fpsSecond)
            {
                fpsLabel.Text = string.Format("Application: {0:0}FPS. GLGUI: {1:0.0}ms", fpsCounter, glgui.RenderDuration);
                fpsCounter = 0;
                fpsSecond++;
            }

            if (consoleWriter.Changed)
            {
                console.Text = string.Join("\n", consoleWriter.Lines);
                consoleWriter.Changed = false;
            }

            glgui.Render();
            SwapBuffers();

            fpsCounter++;
        }

    }
}

