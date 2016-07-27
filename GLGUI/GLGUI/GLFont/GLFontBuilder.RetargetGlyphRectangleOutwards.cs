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
        private static void RetargetGlyphRectangleOutwards(BitmapData bitmapData, GLFontGlyph glyph, bool setYOffset, byte alphaTolerance)
        {
            int startX, endX;
            int startY, endY;
            var rect = glyph.Rect;

            EmptyDel emptyPix;

            if (bitmapData.PixelFormat == PixelFormat.Format32bppArgb)
                emptyPix = delegate(BitmapData data, int x, int y) { return GLFontBitmap.EmptyAlphaPixel(data, x, y, alphaTolerance); };
            else
                emptyPix = delegate(BitmapData data, int x, int y) { return GLFontBitmap.EmptyPixel(data, x, y); };

            unsafe
            {
                for (startX = rect.X; startX >= 0; startX--)
                {
                    bool foundPix = false;
                    for (int j = rect.Y; j <= rect.Y + rect.Height; j++)
                    {
                        if (!emptyPix(bitmapData, startX, j))
                        {
                            foundPix = true;
                            break;
                        }
                    }

                    if (foundPix)
                    {
                        startX++;
                        break;
                    }
                }

                for (endX = rect.X + rect.Width; endX < bitmapData.Width; endX++)
                {
                    bool foundPix = false;
                    for (int j = rect.Y; j <= rect.Y + rect.Height; j++)
                    {
                        if (!emptyPix(bitmapData, endX, j))
                        {
                            foundPix = true;
                            break;
                        }
                    }

                    if (foundPix)
                    {
                        endX--;
                        break;
                    }
                }

                for (startY = rect.Y; startY >= 0; startY--)
                {
                    bool foundPix = false;
                    for (int i = startX; i <= endX; i++)
                    {
                        if (!emptyPix(bitmapData, i, startY))
                        {
                            foundPix = true;
                            break;
                        }
                    }

                    if (foundPix)
                    {
                        startY++;
                        break;
                    }
                }

                for (endY = rect.Y + rect.Height; endY < bitmapData.Height; endY++)
                {
                    bool foundPix = false;
                    for (int i = startX; i <= endX; i++)
                    {
                        if (!emptyPix(bitmapData, i, endY))
                        {
                            foundPix = true;
                            break;
                        }
                    }

                    if (foundPix)
                    {
                        endY--;
                        break;
                    }
                }
            }

            glyph.Rect = new Rectangle(startX, startY, endX - startX + 1, endY - startY + 1);

            if (setYOffset)
                glyph.YOffset = glyph.Rect.Y;
        }
    }
}