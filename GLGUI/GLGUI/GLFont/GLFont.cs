using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using OpenTK;

namespace GLGUI
{
    public partial class GLFont
    {
        public float LineSpacing { get { return (float)Math.Ceiling(fontData.maxGlyphHeight * Options.LineSpacing); } }
        public bool IsMonospacingActive { get { return fontData.IsMonospacingActive(Options); } }
        public float MonoSpaceWidth { get { return fontData.GetMonoSpaceWidth(Options); } }
        public GLFontRenderOptions Options { get { if (options == null) options = new GLFontRenderOptions(); return options; } private set { options = value; } }
        private GLFontRenderOptions options;

        private GLFontData fontData;
        private float lineSpacingCache;
        private bool isMonospacingActiveCache;
        private float monoSpaceWidthCache;

        public GLFont(Font font, GLFontBuilderConfiguration config = null)
        {
            options = new GLFontRenderOptions();

            if (config == null)
                config = new GLFontBuilderConfiguration();

            var builder = new GLFontBuilder(font, config);
            fontData = builder.BuildFontData();
        }

        public GLFont(string fileName, float size, FontStyle style = FontStyle.Regular) : this(fileName, size, null, style) { }
        public GLFont(string fileName, float size, GLFontBuilderConfiguration config, FontStyle style = FontStyle.Regular)
        {
            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile(fileName);
            var fontFamily = pfc.Families[0];

            if (!fontFamily.IsStyleAvailable(style))
                throw new ArgumentException("Font file: " + fileName + " does not support style: " + style);

            if (config == null)
                config = new GLFontBuilderConfiguration();

            using (var font = new Font(fontFamily, size * config.SuperSampleLevels, style))
            {
                var builder = new GLFontBuilder(font, config);
                fontData = builder.BuildFontData();
            }
            pfc.Dispose();
        }

        private Vector2 LockToPixel(Vector2 input)
        {
            if (Options.LockToPixel)
            {
                float r = Options.LockToPixelRatio;
                return new Vector2((1 - r) * input.X + r * ((int)Math.Round(input.X)), (1 - r) * input.Y + r * ((int)Math.Round(input.Y)));
            }
            return input;
        }

        private float TextNodeLineLength(GLFontTextNode node, float maxLength)
        {
            if (node == null)
                return 0;

            bool atLeastOneNodeCosumedOnLine = false;
            float length = 0;
            for (; node != null; node = node.Next)
            {
                if (node.Type == GLFontTextNodeType.LineBreak)
                    break;
                if (SkipTrailingSpace(node, length, maxLength) && atLeastOneNodeCosumedOnLine)
                    break;
                if (length + node.Length <= maxLength || !atLeastOneNodeCosumedOnLine)
                {
                    atLeastOneNodeCosumedOnLine = true;
                    length += node.Length;
                }
                else
                    break;
            }
            return length;
        }

        private bool CrumbledWord(GLFontTextNode node)
        {
            return (node.Type == GLFontTextNodeType.Word && node.Next != null && node.Next.Type == GLFontTextNodeType.Word);
        }



        private bool SkipTrailingSpace(GLFontTextNode node, float lengthSoFar, float boundWidth)
        {
            if ((node.Type == GLFontTextNodeType.Space || node.Type == GLFontTextNodeType.Tab) &&
                node.Next != null &&
                node.Next.Type == GLFontTextNodeType.Word &&
                node.ModifiedLength + node.Next.ModifiedLength + lengthSoFar > boundWidth)
                return true;
            return false;
        }
    }
}
