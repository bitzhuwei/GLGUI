using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GLGUI.Example
{
    public class MainForm : GameWindow
    {
        GLCtrlContainer glgui;
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

            glgui = new GLCtrlContainer(this);

            var mainAreaControl = glgui.Add(new GLGroupLayout(glgui) { Size = new Size(ClientSize.Width, ClientSize.Height - 200), Anchor = GLAnchorStyles.All });
            // change background color:
            var mainSkin = mainAreaControl.Skin;
            mainSkin.BackgroundColor = glgui.Skin.FormActive.BackgroundColor;
            mainSkin.BorderColor = glgui.Skin.FormActive.BorderColor;
            mainAreaControl.Skin = mainSkin;

            var consoleScrollControl = glgui.Add(new GLScrollableControl(glgui) { Outer = new Rectangle(0, ClientSize.Height - 200, ClientSize.Width, 200), Anchor = GLAnchorStyles.Left | GLAnchorStyles.Right | GLAnchorStyles.Bottom });
            console = consoleScrollControl.Add(new GLLabel(glgui) { AutoSize = true, Multiline = true });

            fpsLabel = mainAreaControl.Add(new GLLabel(glgui) { Location = new Point(10, 10), AutoSize = true });
            // change font and background color:
            var skin = fpsLabel.SkinEnabled;
            skin.Font = new GLFont(new Font("Arial", 12.0f));
            skin.BackgroundColor = glgui.Skin.TextBoxActive.BackgroundColor;
            fpsLabel.SkinEnabled = skin;

            var helloWorldForm = mainAreaControl.Add(new GLForm(glgui) { Title = "Hello World", Outer = new Rectangle(50, 100, 200, 150), AutoSize = false });
            helloWorldForm.Add(new GLForm(glgui) { Title = "Hello Form", Outer = new Rectangle(100, 32, 100, 100), AutoSize = false })
                .MouseMove += (s, w) => Console.WriteLine(w.Position.ToString());

            var flow = helloWorldForm.Add(new GLFlowLayout(glgui) { FlowDirection = GLFlowDirection.BottomUp, Location = new Point(10, 10), Size = helloWorldForm.InnerSize, AutoSize = true });
            for (int i = 0; i < 5; i++)
                flow.Add(new GLButton(glgui) { Text = "Button" + i, Size = new Size(150, 0) })
                    .Click += (s, w) => Console.WriteLine(s + " pressed.");
            flow.Add(new GLButton(glgui) { Text = "Hide Cursor", Size = new Size(150, 0) })
                .Click += (s, w) => glgui.Cursor = GLCursor.None;

            var loremIpsumForm = mainAreaControl.Add(new GLForm(glgui) { Title = "Lorem Ipsum", Location = new Point(600, 100), Size = new Size(300, 200) });
            loremIpsumForm.Add(new GLTextBox(glgui)
            {
                Text = "Lorem ipsum dolor sit amet,\nconsetetur sadipscing elitr,\nsed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat,\nsed diam voluptua.\n\nAt vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.",
                Multiline = true,
                WordWrap = true,
                Outer = new Rectangle(4, 4, loremIpsumForm.Inner.Width - 8, loremIpsumForm.Inner.Height - 8),
                Anchor = GLAnchorStyles.All
            }).Changed += (s, w) => Console.WriteLine(s + " text length: " + ((GLTextBox)s).Text.Length);

            var fixedSizeForm = mainAreaControl.Add(new GLForm(glgui) { Title = "Fixed size Form", Location = new Point(64, 300), Size = new Size(100, 200), AutoSize = true });
            var fooBarFlow = fixedSizeForm.Add(new GLFlowLayout(glgui) { FlowDirection = GLFlowDirection.TopDown, AutoSize = true });
            fooBarFlow.Add(new GLCheckBox(glgui) { Text = "CheckBox A", AutoSize = true })
                .Changed += (s, w) => Console.WriteLine(s + ": " + ((GLCheckBox)s).Checked);
            fooBarFlow.Add(new GLCheckBox(glgui) { Text = "CheckBox B", AutoSize = true })
                .Changed += (s, w) => Console.WriteLine(s + ": " + ((GLCheckBox)s).Checked);
            fooBarFlow.Add(new GLCheckBox(glgui) { Text = "Totally different CheckBox", AutoSize = true })
                .Changed += (s, w) => Console.WriteLine(s + ": " + ((GLCheckBox)s).Checked);
            fooBarFlow.Add(new GLCheckBox(glgui) { Text = "Go away!", AutoSize = true })
                .Changed += (s, w) => Console.WriteLine(s + ": " + ((GLCheckBox)s).Checked);
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

