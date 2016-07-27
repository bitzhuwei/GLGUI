using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using OpenTK;

namespace GLGUI
{
    public partial class GLFont
    {

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

    }
}
