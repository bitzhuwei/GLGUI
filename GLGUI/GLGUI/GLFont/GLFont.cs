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

            fontData = new GLFontBuilder(font, config).BuildFontData();
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
                fontData = new GLFontBuilder(font, config).BuildFontData();
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



        private void RenderWord(GLFontVertexBuffer[] vbos, float x, float y, GLFontTextNode node)
        {
            if (node.Type != GLFontTextNodeType.Word)
                return;

            int charGaps = node.Text.Length - 1;
            bool isCrumbleWord = CrumbledWord(node);
            if (isCrumbleWord)
                charGaps++;

            int pixelsPerGap = 0;
            int leftOverPixels = 0;

            if (charGaps != 0)
            {
                pixelsPerGap = (int)node.LengthTweak / charGaps;
                leftOverPixels = (int)node.LengthTweak - pixelsPerGap * charGaps;
            }

            for (int i = 0; i < node.Text.Length; i++)
            {
                char c = node.Text[i];
                GLFontGlyph glyph;
                if (fontData.CharSetMapping.TryGetValue(c, out glyph))
                {
                    vbos[glyph.Page].AddQuad(x, y + glyph.YOffset, x + glyph.Rect.Width, y + glyph.YOffset + glyph.Rect.Height,
                        glyph.TextureMin.X, glyph.TextureMin.Y, glyph.TextureMax.X, glyph.TextureMax.Y);

                    if (isMonospacingActiveCache)
                        x += monoSpaceWidthCache;
                    else
                        x += (int)Math.Ceiling(glyph.Rect.Width + fontData.meanGlyphWidth * Options.CharacterSpacing + fontData.GetKerningPairCorrection(i, node.Text, node));

                    x += pixelsPerGap;
                    if (leftOverPixels > 0)
                    {
                        x += 1.0f;
                        leftOverPixels--;
                    }
                    else if (leftOverPixels < 0)
                    {
                        x -= 1.0f;
                        leftOverPixels++;
                    }
                }
            }
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
