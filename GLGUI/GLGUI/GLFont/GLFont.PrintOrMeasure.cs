using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using OpenTK;

namespace GLGUI
{
    public partial class GLFont
    {
        private SizeF PrintOrMeasure(GLFontVertexBuffer[] vbos, GLFontText processedText, bool measureOnly)
        {
            // init values we'll return
            float maxMeasuredWidth = 0f;

            float xOffset = 0f;
            float yOffset = 0f;

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
                }
                else
                {
                    if (Options.WordWrap && SkipTrailingSpace(node, length, maxWidth) && atLeastOneNodeCosumedOnLine)
                    {
                        newLine = true;
                    }
                    else if (length + node.ModifiedLength <= maxWidth || !atLeastOneNodeCosumedOnLine)
                    {
                        atLeastOneNodeCosumedOnLine = true;

                        if (!measureOnly)
                            RenderWord(vbos, xOffset + length, yOffset, node);
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

            return new SizeF(maxMeasuredWidth, yOffset + (nodeList.Head == null ? 0 : lineSpacingCache));
        }

    }
}