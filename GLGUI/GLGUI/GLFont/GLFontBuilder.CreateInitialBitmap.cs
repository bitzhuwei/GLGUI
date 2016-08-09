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
        private delegate bool EmptyDel(BitmapData data, int x, int y);

        /// <summary>
        /// The initial bitmap is simply a long thin strip of all glyphs in a row
        /// </summary>
        /// <param name="font"></param>
        /// <param name="maxSize"></param>
        /// <param name="initialMargin"></param>
        /// <param name="glyphs"></param>
        /// <param name="renderHint"></param>
        /// <returns></returns>
        private Bitmap CreateInitialBitmap(Font font, SizeF maxSize, int initialMargin, out GLFontGlyph[] glyphs, GLFontRenderHint renderHint)
        {
            glyphs = new GLFontGlyph[charSet.Length];

            int spacing = (int)Math.Ceiling(maxSize.Width) + 2 * initialMargin;
            Bitmap bmp = new Bitmap(spacing * charSet.Length, (int)Math.Ceiling(maxSize.Height) + 2 * initialMargin + 1, PixelFormat.Format24bppRgb);
            Graphics graph = Graphics.FromImage(bmp);

            switch (renderHint)
            {
                case GLFontRenderHint.SizeDependent:
                    graph.TextRenderingHint = font.Size <= 12.0f ? TextRenderingHint.ClearTypeGridFit : TextRenderingHint.AntiAlias;
                    break;
                case GLFontRenderHint.AntiAlias:
                    graph.TextRenderingHint = TextRenderingHint.AntiAlias;
                    break;
                case GLFontRenderHint.AntiAliasGridFit:
                    graph.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    break;
                case GLFontRenderHint.ClearTypeGridFit:
                    graph.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    break;
                case GLFontRenderHint.SystemDefault:
                    graph.TextRenderingHint = TextRenderingHint.SystemDefault;
                    break;
            }

            int xOffset = initialMargin;
            for (int i = 0; i < charSet.Length; i++)
            {
                graph.DrawString("" + charSet[i], font, Brushes.White, xOffset, initialMargin);
                SizeF charSize = graph.MeasureString("" + charSet[i], font);
                glyphs[i] = new GLFontGlyph(0, new Rectangle(xOffset - initialMargin, 0, (int)charSize.Width + initialMargin * 2, (int)charSize.Height + initialMargin * 2), 0, charSet[i]);
                xOffset += (int)charSize.Width + initialMargin * 2;
            }

            graph.Flush();
            graph.Dispose();

            return bmp;
        }


    }
}
