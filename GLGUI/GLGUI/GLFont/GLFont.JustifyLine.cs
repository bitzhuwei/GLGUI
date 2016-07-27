using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using OpenTK;

namespace GLGUI
{
    public partial class GLFont
    {
        private void JustifyLine(GLFontTextNode node, float targetLength)
        {
            bool justifiable = false;

            if (node == null)
                return;

            var headNode = node; //keep track of the head node

            //start by finding the length of the block of text that we know will actually fit:

            int charGaps = 0;
            int spaceGaps = 0;

            bool atLeastOneNodeCosumedOnLine = false;
            float length = 0;
            var expandEndNode = node; //the node at the end of the smaller list (before adding additional word)
            for (; node != null; node = node.Next)
            {
                if (node.Type == GLFontTextNodeType.LineBreak)
                    break;

                if (SkipTrailingSpace(node, length, targetLength) && atLeastOneNodeCosumedOnLine)
                {
                    justifiable = true;
                    break;
                }

                if (length + node.Length < targetLength || !atLeastOneNodeCosumedOnLine)
                {
                    expandEndNode = node;

                    if (node.Type == GLFontTextNodeType.Space)
                        spaceGaps++;
                    if (node.Type == GLFontTextNodeType.Tab)
                        spaceGaps += 4;

                    if (node.Type == GLFontTextNodeType.Word)
                    {
                        charGaps += (node.Text.Length - 1);

                        //word was part of a crumbled word, so there's an extra char cap between the two words
                        if (CrumbledWord(node))
                            charGaps++;
                    }

                    atLeastOneNodeCosumedOnLine = true;
                    length += node.Length;
                }
                else
                {
                    justifiable = true;
                    break;
                }
            }

            //now we check how much additional length is added by adding an additional word to the line
            float extraLength = 0f;
            int extraSpaceGaps = 0;
            int extraCharGaps = 0;
            bool contractPossible = false;
            GLFontTextNode contractEndNode = null;
            for (node = expandEndNode.Next; node != null; node = node.Next)
            {
                if (node.Type == GLFontTextNodeType.LineBreak)
                    break;

                if (node.Type == GLFontTextNodeType.Space)
                {
                    extraLength += node.Length;
                    extraSpaceGaps++;
                }
                else if (node.Type == GLFontTextNodeType.Tab)
                {
                    extraLength += node.Length;
                    extraSpaceGaps += 4;
                }
                else if (node.Type == GLFontTextNodeType.Word)
                {
                    contractEndNode = node;
                    contractPossible = true;
                    extraLength += node.Length;
                    extraCharGaps += (node.Text.Length - 1);
                    break;
                }
            }

            if (justifiable)
            {
                //last part of this condition is to ensure that the full contraction is possible (it is all or nothing with contractions, since it looks really bad if we don't manage the full)
                bool contract = contractPossible && (extraLength + length - targetLength) * Options.JustifyContractionPenalty < (targetLength - length) &&
                    ((targetLength - (length + extraLength + 1)) / targetLength > -Options.JustifyCapContract);

                if ((!contract && length < targetLength) || (contract && length + extraLength > targetLength))  //calculate padding pixels per word and char
                {
                    if (contract)
                    {
                        length += extraLength + 1;
                        charGaps += extraCharGaps;
                        spaceGaps += extraSpaceGaps;
                    }

                    int totalPixels = (int)(targetLength - length); //the total number of pixels that need to be added to line to justify it
                    int spacePixels = 0; //number of pixels to spread out amongst spaces
                    int charPixels = 0; //number of pixels to spread out amongst char gaps

                    if (contract)
                    {
                        if (totalPixels / targetLength < -Options.JustifyCapContract)
                            totalPixels = (int)(-Options.JustifyCapContract * targetLength);
                    }
                    else
                    {
                        if (totalPixels / targetLength > Options.JustifyCapExpand)
                            totalPixels = (int)(Options.JustifyCapExpand * targetLength);
                    }

                    //work out how to spread pixles between character gaps and word spaces
                    if (charGaps == 0)
                    {
                        spacePixels = totalPixels;
                    }
                    else if (spaceGaps == 0)
                    {
                        charPixels = totalPixels;
                    }
                    else
                    {
                        if (contract)
                            charPixels = (int)(totalPixels * Options.JustifyCharacterWeightForContract * charGaps / spaceGaps);
                        else
                            charPixels = (int)(totalPixels * Options.JustifyCharacterWeightForExpand * charGaps / spaceGaps);

                        if ((!contract && charPixels > totalPixels) ||
                            (contract && charPixels < totalPixels))
                            charPixels = totalPixels;

                        spacePixels = totalPixels - charPixels;
                    }

                    int pixelsPerChar = 0;  //minimum number of pixels to add per char
                    int leftOverCharPixels = 0; //number of pixels remaining to only add for some chars

                    if (charGaps != 0)
                    {
                        pixelsPerChar = charPixels / charGaps;
                        leftOverCharPixels = charPixels - pixelsPerChar * charGaps;
                    }

                    int pixelsPerSpace = 0; //minimum number of pixels to add per space
                    int leftOverSpacePixels = 0; //number of pixels remaining to only add for some spaces

                    if (spaceGaps != 0)
                    {
                        pixelsPerSpace = spacePixels / spaceGaps;
                        leftOverSpacePixels = spacePixels - pixelsPerSpace * spaceGaps;
                    }

                    //now actually iterate over all nodes and set tweaked length
                    for (node = headNode; node != null; node = node.Next)
                    {
                        if (node.Type == GLFontTextNodeType.Space)
                        {
                            node.LengthTweak = pixelsPerSpace;
                            if (leftOverSpacePixels > 0)
                            {
                                node.LengthTweak += 1;
                                leftOverSpacePixels--;
                            }
                            else if (leftOverSpacePixels < 0)
                            {
                                node.LengthTweak -= 1;
                                leftOverSpacePixels++;
                            }
                        }
                        else if (node.Type == GLFontTextNodeType.Tab)
                        {
                            node.LengthTweak = 4 * pixelsPerSpace;
                            if (leftOverSpacePixels > 0)
                            {
                                node.LengthTweak += 1;
                                leftOverSpacePixels--;
                            }
                            else if (leftOverSpacePixels < 0)
                            {
                                node.LengthTweak -= 1;
                                leftOverSpacePixels++;
                            }
                        }
                        else if (node.Type == GLFontTextNodeType.Word)
                        {
                            int cGaps = (node.Text.Length - 1);
                            if (CrumbledWord(node))
                                cGaps++;

                            node.LengthTweak = cGaps * pixelsPerChar;

                            if (leftOverCharPixels >= cGaps)
                            {
                                node.LengthTweak += cGaps;
                                leftOverCharPixels -= cGaps;
                            }
                            else if (leftOverCharPixels <= -cGaps)
                            {
                                node.LengthTweak -= cGaps;
                                leftOverCharPixels += cGaps;
                            }
                            else
                            {
                                node.LengthTweak += leftOverCharPixels;
                                leftOverCharPixels = 0;
                            }
                        }

                        if ((!contract && node == expandEndNode) || (contract && node == contractEndNode))
                            break;
                    }
                }
            }
        }

    }
}
