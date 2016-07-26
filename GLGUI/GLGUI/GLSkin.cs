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

        public struct GLGroupLayoutSkin
        {
            public GLPadding Border;
            public Color4 BorderColor;
            public Color4 BackgroundColor;
        }

        public struct GLFlowLayoutSkin
        {
            public GLPadding Padding;
            public GLPadding Border;
            public Color4 BorderColor;
            public Color4 BackgroundColor;
            public int Space;
        }

        public struct GLScrollableControlSkin
        {
            public GLPadding Border;
            public Color4 BorderColor;
            public Color4 BackgroundColor;
        }

        public GLFormSkin FormActive = new GLFormSkin();
        public GLFormSkin FormInactive = new GLFormSkin();

        public GLLabelSkin LabelEnabled = new GLLabelSkin();
        public GLLabelSkin LabelDisabled = new GLLabelSkin();

        public GLLabelSkin LinkLabelEnabled = new GLLabelSkin();
        public GLLabelSkin LinkLabelDisabled = new GLLabelSkin();

        public GLGroupLayoutSkin GroupLayout = new GLGroupLayoutSkin();

        public GLFlowLayoutSkin FlowLayout = new GLFlowLayoutSkin();

        public GLScrollableControlSkin ScrollableControl = new GLScrollableControlSkin();

        public GLFlowLayoutSkin ContextMenu = new GLFlowLayoutSkin();


        public GLSkin(GLFont defaultFont = null)
        {
            if (defaultFont == null)
                defaultFont = new GLFont(new Font("Arial", 8.0f));

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


            LinkLabelEnabled.Font = defaultFont;
            LinkLabelEnabled.Color = Color.FromArgb(128, 128, 255);
            LinkLabelEnabled.TextAlign = GLFontAlignment.Left;
            LinkLabelEnabled.Padding = new GLPadding(1, 1, 1, 1);
            LinkLabelEnabled.BackgroundColor = Color.Transparent;

            LinkLabelDisabled.Font = defaultFont;
            LinkLabelDisabled.Color = Color.FromArgb(96, 96, 192);
            LinkLabelDisabled.TextAlign = GLFontAlignment.Left;
            LinkLabelDisabled.Padding = new GLPadding(1, 1, 1, 1);
            LinkLabelDisabled.BackgroundColor = Color.Transparent;


            GroupLayout.Border = new GLPadding(1);
            GroupLayout.BorderColor = Color.Transparent;//Color.FromArgb(96, 96, 96);
            GroupLayout.BackgroundColor = Color.Transparent;//Color.FromArgb(240, 240, 240);


            FlowLayout.Padding = new GLPadding(2);
            FlowLayout.Border = new GLPadding(0);
            FlowLayout.BorderColor = Color.Transparent;
            FlowLayout.BackgroundColor = Color.Transparent;
            FlowLayout.Space = 2;


            ScrollableControl.Border = new GLPadding(1);
            ScrollableControl.BorderColor = Color.FromArgb(56, 56, 56);
            ScrollableControl.BackgroundColor = Color.FromArgb(41, 41, 41);


            ContextMenu.Padding = new GLPadding(1);
            ContextMenu.Border = new GLPadding(1);
            ContextMenu.BorderColor = Color.FromArgb(128, 128, 128);
            ContextMenu.BackgroundColor = Color.FromArgb(32, 32, 32);
            ContextMenu.Space = 1;


        }
    }
}
