using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;

namespace GLGUI
{
    partial class GLFontBuilder
    {
        private static List<GLFontBitmap> GenerateBitmapSheetsAndRepack(GLFontGlyph[] sourceGlyphs, BitmapData[] sourceBitmaps, int destSheetWidth, int destSheetHeight, out GLFontGlyph[] destGlyphs, int destMargin, bool usePowerOfTwo)
        {
            var pages = new List<GLFontBitmap>();
            destGlyphs = new GLFontGlyph[sourceGlyphs.Length];
            GLFontBitmap currentPage = null;
            int maxY = sourceGlyphs.Max(g => g.Rect.Height);
            int finalPageIndex = 0;
            int finalPageRequiredWidth = 0;
            int finalPageRequiredHeight = 0;

            for (int k = 0; k < 2; k++)
            {
                bool pre = k == 0;  //first iteration is simply to determine the required size of the final page, so that we can crop it in advance
                int xPos = 0;
                int yPos = 0;
                int maxYInRow = 0;
                int totalTries = 0;

                for (int i = 0; i < sourceGlyphs.Length; i++)
                {
                    if (!pre && currentPage == null)
                    {
                        if (finalPageIndex == pages.Count)
                        {
                            int width = Math.Min(destSheetWidth, usePowerOfTwo ? PowerOfTwo(finalPageRequiredWidth) : finalPageRequiredWidth);
                            int height = Math.Min(destSheetHeight, usePowerOfTwo ? PowerOfTwo(finalPageRequiredHeight) : finalPageRequiredHeight);

                            currentPage = new GLFontBitmap(new Bitmap(width, height, PixelFormat.Format32bppArgb));
                            currentPage.Clear32(255, 255, 255, 0); // clear to white, but totally transparent
                        }
                        else
                        {
                            currentPage = new GLFontBitmap(new Bitmap(destSheetWidth, destSheetHeight, PixelFormat.Format32bppArgb));
                            currentPage.Clear32(255, 255, 255, 0); // clear to white, but totally transparent
                        }
                        pages.Add(currentPage);
                    }

                    totalTries++;

                    if (totalTries > 10 * sourceGlyphs.Length)
                        throw new Exception("Failed to fit font into texture pages");

                    var rect = sourceGlyphs[i].Rect;

                    if (xPos + rect.Width + 2 * destMargin <= destSheetWidth && yPos + rect.Height + 2 * destMargin <= destSheetHeight)
                    {
                        if (!pre)
                        {
                            //add to page
                            if (sourceBitmaps[sourceGlyphs[i].Page].PixelFormat == PixelFormat.Format32bppArgb)
                                GLFontBitmap.Blit(sourceBitmaps[sourceGlyphs[i].Page], currentPage.bitmapData, rect.X, rect.Y, rect.Width, rect.Height, xPos + destMargin, yPos + destMargin);
                            else
                                GLFontBitmap.BlitMask(sourceBitmaps[sourceGlyphs[i].Page], currentPage.bitmapData, rect.X, rect.Y, rect.Width, rect.Height, xPos + destMargin, yPos + destMargin);

                            destGlyphs[i] = new GLFontGlyph(pages.Count - 1, new Rectangle(xPos + destMargin, yPos + destMargin, rect.Width, rect.Height), sourceGlyphs[i].YOffset, sourceGlyphs[i].Character);
                        }
                        else
                        {
                            finalPageRequiredWidth = Math.Max(finalPageRequiredWidth, xPos + rect.Width + 2 * destMargin);
                            finalPageRequiredHeight = Math.Max(finalPageRequiredHeight, yPos + rect.Height + 2 * destMargin);
                        }

                        xPos += rect.Width + 2 * destMargin;
                        maxYInRow = Math.Max(maxYInRow, rect.Height);
                        continue;
                    }

                    if (xPos + rect.Width + 2 * destMargin > destSheetWidth)
                    {
                        i--;

                        yPos += maxYInRow + 2 * destMargin;
                        xPos = 0;

                        if (yPos + maxY + 2 * destMargin > destSheetHeight)
                        {
                            yPos = 0;

                            if (!pre)
                            {
                                currentPage = null;
                            }
                            else
                            {
                                finalPageRequiredWidth = 0;
                                finalPageRequiredHeight = 0;
                                finalPageIndex++;
                            }
                        }
                        continue;
                    }
                }
            }
            return pages;
        }

    }
}