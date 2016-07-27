using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using OpenTK;

namespace GLGUI
{
    public partial class GLFont
    {

        public GLFontTextPosition GetTextPosition(GLFontText processedText, GLFontTextPosition position)
        {
            float maxMeasuredWidth = 0f;

            float xOffset = 0f;
            float yOffset = 0f;

            int character = 0;

            lineSpacingCache = LineSpacing;
            isMonospacingActiveCache = IsMonospacingActive;
            monoSpaceWidthCache = MonoSpaceWidth;
            float maxWidth = processedText.maxSize.Width;
            var alignment = processedText.alignment;
            var nodeList = processedText.textNodeList;
            for (GLFontTextNode node = nodeList.Head; node != null; node = node.Next)
                node.LengthTweak = 0f;  //reset tweaks

            if (alignment == GLFontAlignment.Right)
                xOffset -= (float)Math.Ceiling(TextNodeLineLength(nodeList.Head, maxWidth) - maxWidth);
            else if (alignment == GLFontAlignment.Centre)
                xOffset -= (float)Math.Ceiling(0.5f * TextNodeLineLength(nodeList.Head, maxWidth));
            else if (alignment == GLFontAlignment.Justify)
                JustifyLine(nodeList.Head, maxWidth);

            bool atLeastOneNodeCosumedOnLine = false;
            float length = 0f;
            for (GLFontTextNode node = nodeList.Head; node != null; node = node.Next)
            {
                bool newLine = false;

                if (node.Type == GLFontTextNodeType.LineBreak)
                {
                    newLine = true;
                    if (character == position.Index)
                        return new GLFontTextPosition() { Index = character, Position = new Vector2(xOffset + length, yOffset) };
                    character++;
                }
                else
                {
                    if (Options.WordWrap && SkipTrailingSpace(node, length, maxWidth) && atLeastOneNodeCosumedOnLine)
                    {
                        newLine = true;
                        if (character == position.Index)
                            return new GLFontTextPosition() { Index = character, Position = new Vector2(xOffset + length, yOffset) };
                        character++;
                    }
                    else if (length + node.ModifiedLength <= maxWidth || !atLeastOneNodeCosumedOnLine)
                    {
                        atLeastOneNodeCosumedOnLine = true;

                        Vector2 p;
                        if (GetWordPosition(xOffset + length, yOffset, node, position, ref character, out p))
                            return new GLFontTextPosition() { Index = character, Position = p };

                        length += node.ModifiedLength;

                        maxMeasuredWidth = Math.Max(length, maxMeasuredWidth);
                    }
                    else if (Options.WordWrap)
                    {
                        newLine = true;
                        if (node.Previous != null)
                            node = node.Previous;
                    }
                    else
                        continue; // continue so we still read line breaks even if reached max width
                }

                if (newLine)
                {
                    if (yOffset + lineSpacingCache >= processedText.maxSize.Height)
                        break;

                    if ((long)(yOffset / lineSpacingCache) == (long)(position.Position.Y / lineSpacingCache))
                        return new GLFontTextPosition() { Index = character - 1, Position = new Vector2(xOffset + length, yOffset) };

                    yOffset += lineSpacingCache;
                    xOffset = 0f;
                    length = 0f;
                    atLeastOneNodeCosumedOnLine = false;

                    if (node.Next != null)
                    {
                        if (alignment == GLFontAlignment.Right)
                            xOffset -= (float)Math.Ceiling(TextNodeLineLength(node.Next, maxWidth) - maxWidth);
                        else if (alignment == GLFontAlignment.Centre)
                            xOffset -= (float)Math.Ceiling(0.5f * TextNodeLineLength(node.Next, maxWidth));
                        else if (alignment == GLFontAlignment.Justify)
                            JustifyLine(node.Next, maxWidth);
                    }
                }
            }

            return new GLFontTextPosition() { Index = character, Position = new Vector2(xOffset + length, yOffset) };
        }

    }
}