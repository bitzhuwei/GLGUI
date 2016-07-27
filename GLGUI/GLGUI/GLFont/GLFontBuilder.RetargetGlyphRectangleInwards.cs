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
        private static void RetargetGlyphRectangleInwards(BitmapData bitmapData, GLFontGlyph glyph, bool setYOffset, byte alphaTolerance)
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
                for (startX = rect.X; startX < bitmapData.Width; startX++)
                    for (int j = rect.Y; j < rect.Y + rect.Height; j++)
                        if (!emptyPix(bitmapData, startX, j))
                            goto Done1;
            Done1:
                for (endX = rect.X + rect.Width - 1; endX >= 0; endX--)
                    for (int j = rect.Y; j < rect.Y + rect.Height; j++)
                        if (!emptyPix(bitmapData, endX, j))
                            goto Done2;
            Done2:
                for (startY = rect.Y; startY < bitmapData.Height; startY++)
                    for (int i = startX; i < endX; i++)
                        if (!emptyPix(bitmapData, i, startY))
                            goto Done3;

            Done3:
                for (endY = rect.Y + rect.Height - 1; endY >= 0; endY--)
                    for (int i = startX; i < endX; i++)
                        if (!emptyPix(bitmapData, i, endY))
                            goto Done4;
            Done4: ;
            }

            if (endY < startY)
                startY = endY = rect.Y;

            if (endX < startX)
                startX = endX = rect.X;

            glyph.Rect = new Rectangle(startX, startY, endX - startX + 1, endY - startY + 1);

            if (setYOffset)
                glyph.YOffset = glyph.Rect.Y;
        }

    }
}
