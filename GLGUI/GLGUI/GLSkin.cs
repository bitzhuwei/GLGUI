using System.Drawing;
using OpenTK.Graphics;

namespace GLGUI
{
    public class GLSkin
    {
        public struct GLFormSkin
        {
            public GLFont Font;
            public Color4 Color;
            public GLPadding Border;
            public Color4 BorderColor;
            public Color4 BackgroundColor;
        }

        public struct GLLabelSkin
        {
            public GLFont Font;
            public Color4 Color;
            public GLFontAlignment TextAlign;
            public GLPadding Padding;
            public Color4 BackgroundColor;
        }

        public GLFormSkin FormActive = new GLFormSkin();
        public GLFormSkin FormInactive = new GLFormSkin();

        public GLLabelSkin LabelEnabled = new GLLabelSkin();
        public GLLabelSkin LabelDisabled = new GLLabelSkin();

        public GLSkin(GLFont defaultFont = null)
        {
            if (defaultFont == null)
                defaultFont = new GLFont(new Font("Arial", 48.0f));

            FormActive.Font = defaultFont;
            FormActive.Color = Color.FromArgb(240, 240, 240);
            FormActive.Border = new GLPadding(2);
            FormActive.BorderColor = Color.FromArgb(192, 56, 56, 56);
            FormActive.BackgroundColor = Color.FromArgb(41, 41, 41);

            FormInactive.Font = defaultFont;
            FormInactive.Color = Color.FromArgb(160, 160, 160);
            FormInactive.Border = new GLPadding(2);
            FormInactive.BorderColor = Color.FromArgb(192, 56, 56, 56);
            FormInactive.BackgroundColor = Color.FromArgb(41, 41, 41);


            LabelEnabled.Font = defaultFont;
            LabelEnabled.Color = Color.FromArgb(192, 192, 192);
            LabelEnabled.TextAlign = GLFontAlignment.Left;
            LabelEnabled.Padding = new GLPadding(1, 1, 1, 1);
            LabelEnabled.BackgroundColor = Color.Transparent;

            LabelDisabled.Font = defaultFont;
            LabelDisabled.Color = Color.FromArgb(128, 128, 128);
            LabelDisabled.TextAlign = GLFontAlignment.Left;
            LabelDisabled.Padding = new GLPadding(1, 1, 1, 1);
            LabelDisabled.BackgroundColor = Color.Transparent;

        }
    }
}
