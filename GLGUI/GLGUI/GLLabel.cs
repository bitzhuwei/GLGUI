using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GLGUI;

namespace GLGUI
{
	public class GLLabel : GLControl
	{
        public string Text { get { return text; } set { if (value != text) { text = value; Invalidate(); } } }
        public bool Enabled { get { return enabled; } set { enabled = value; Invalidate(); } }
        public GLSkin.GLLabelSkin SkinEnabled { get { return skinEnabled; } set { skinEnabled = value; Invalidate(); } }
        public GLSkin.GLLabelSkin SkinDisabled { get { return skinDisabled; } set { skinDisabled = value; Invalidate(); } }

		private string text = "";
		private GLFontText textProcessed = new GLFontText();
		private SizeF textSize;
		private GLSkin.GLLabelSkin skinEnabled, skinDisabled;
		private GLSkin.GLLabelSkin skin;
		private bool enabled = true;

		public GLLabel(GLGui gui) : base(gui)
		{
			Render += OnRender;

			skinEnabled = Gui.Skin.LabelEnabled;
			skinDisabled = Gui.Skin.LabelDisabled;

			outer = new Rectangle(0, 0, 0, 0);
			sizeMin = new Size(1, 1);
			sizeMax = new Size(int.MaxValue, int.MaxValue);

			ContextMenu = new GLContextMenu(gui);
			ContextMenu.Add(new GLContextMenuEntry(gui) { Text = "Copy" }).Click += (s, e) => Clipboard.SetText(text);
		}

        protected override void UpdateLayout()
		{
			skin = Enabled ? skinEnabled : skinDisabled;

            textSize = skin.Font.ProcessText(textProcessed, text, GLFontAlignment.Left);

			if (AutoSize)
			{
				outer.Width = (int)textSize.Width + skin.Padding.Horizontal;
                outer.Height = (int)textSize.Height + skin.Padding.Vertical;
			}

            outer.Width = Math.Min(Math.Max(outer.Width, sizeMin.Width), sizeMax.Width);
            outer.Height = Math.Min(Math.Max(outer.Height, sizeMin.Height), sizeMax.Height);

			Inner = new Rectangle(skin.Padding.Left, skin.Padding.Top, outer.Width - skin.Padding.Horizontal, outer.Height - skin.Padding.Vertical);
		}

        private void OnRender(Rectangle scissorRect, double timeDelta)
		{
			GLDraw.FilledRectangle(outer.Size, skin.BackgroundColor);
			Scissor(scissorRect, Inner);
            skin.Font.Print(textProcessed, new Vector2(Inner.Left, Inner.Top), skin.Color);
		}
	}
}

