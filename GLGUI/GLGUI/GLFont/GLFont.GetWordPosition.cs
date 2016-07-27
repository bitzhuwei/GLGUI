using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using OpenTK;

namespace GLGUI
{
    public partial class GLFont
    {
        private bool GetWordPosition(float x, float y, GLFontTextNode node, GLFontTextPosition position, ref int character, out Vector2 p)
        {
            bool sameLine = (long)(y / lineSpacingCache) == (long)(position.Position.Y / lineSpacingCache);

            if (node.Type == GLFontTextNodeType.Space || node.Type == GLFontTextNodeType.Tab)
            {
                p = new Vector2(x, y);
                if (position.Index == character || (sameLine && x + node.ModifiedLength * 0.5f > position.Position.X))
                    return true;
                character++;
                return false;
            }

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
                if (fontData.CharSetMapping.ContainsKey(c))
                {
                    var glyph = fontData.CharSetMapping[c];

                    float oldX = x;

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

                    if (position.Index == character || (sameLine && (oldX + x) * 0.5f > position.Position.X))
                    {
                        p = new Vector2(oldX, y);
                        return true;
                    }
                }
                character++;
            }
            p = new Vector2();
            return false;
        }

    }
}