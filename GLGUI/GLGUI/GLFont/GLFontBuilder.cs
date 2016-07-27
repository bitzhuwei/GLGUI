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
        private string charSet;
        private GLFontBuilderConfiguration config;
        private Font font;

        public GLFontBuilder(Font font, GLFontBuilderConfiguration config)
        {
            this.charSet = config.CharSet;
            this.config = config;
            this.font = font;
        }

        private List<SizeF> GetGlyphSizes(Font font)
        {
            Bitmap bmp = new Bitmap(512, 512, PixelFormat.Format24bppRgb);
            Graphics graph = Graphics.FromImage(bmp);
            List<SizeF> sizes = new List<SizeF>();

            for (int i = 0; i < charSet.Length; i++)
            {
                var charSize = graph.MeasureString("" + charSet[i], font);
                sizes.Add(new SizeF(charSize.Width, charSize.Height));
            }

            graph.Dispose();
            bmp.Dispose();

            return sizes;
        }

        private SizeF GetMaxGlyphSize(List<SizeF> sizes)
        {
            SizeF maxSize = new SizeF(0f, 0f);
            for (int i = 0; i < charSet.Length; i++)
            {
                if (sizes[i].Width > maxSize.Width)
                    maxSize.Width = sizes[i].Width;

                if (sizes[i].Height > maxSize.Height)
                    maxSize.Height = sizes[i].Height;
            }

            return maxSize;
        }

        private SizeF GetMinGlyphSize(List<SizeF> sizes)
        {
            SizeF minSize = new SizeF(float.MaxValue, float.MaxValue);
            for (int i = 0; i < charSet.Length; i++)
            {
                if (sizes[i].Width < minSize.Width)
                    minSize.Width = sizes[i].Width;

                if (sizes[i].Height < minSize.Height)
                    minSize.Height = sizes[i].Height;
            }

            return minSize;
        }

        private bool IsMonospaced(List<SizeF> sizes)
        {
            var min = GetMinGlyphSize(sizes);
            var max = GetMaxGlyphSize(sizes);
            if (max.Width - min.Width < max.Width * 0.05f)
                return true;
            return false;
        }

        private static void ScaleSheetsAndGlyphs(List<GLFontBitmap> pages, GLFontGlyph[] glyphs, float scale)
        {
            foreach (var page in pages)
                page.DownScale32((int)(page.bitmap.Width * scale), (int)(page.bitmap.Height * scale));

            foreach (var glyph in glyphs)
            {
                glyph.Rect = new Rectangle((int)(glyph.Rect.X * scale), (int)(glyph.Rect.Y * scale), (int)(glyph.Rect.Width * scale), (int)(glyph.Rect.Height * scale));
                glyph.YOffset = (int)(glyph.YOffset * scale);
            }
        }

        private static void RetargetAllGlyphs(List<GLFontBitmap> pages, GLFontGlyph[] glyphs, byte alphaTolerance)
        {
            foreach (var glyph in glyphs)
                RetargetGlyphRectangleOutwards(pages[glyph.Page].bitmapData, glyph, false, alphaTolerance);
        }

        public static void CreateBitmapPerGlyph(GLFontGlyph[] sourceGlyphs, GLFontBitmap[] sourceBitmaps, out GLFontGlyph[] destGlyphs, out GLFontBitmap[] destBitmaps)
        {
            destBitmaps = new GLFontBitmap[sourceGlyphs.Length];
            destGlyphs = new GLFontGlyph[sourceGlyphs.Length];
            for (int i = 0; i < sourceGlyphs.Length; i++)
            {
                var sg = sourceGlyphs[i];
                destGlyphs[i] = new GLFontGlyph(i, new Rectangle(0, 0, sg.Rect.Width, sg.Rect.Height), sg.YOffset, sg.Character);
                destBitmaps[i] = new GLFontBitmap(new Bitmap(sg.Rect.Width, sg.Rect.Height, PixelFormat.Format32bppArgb));
                GLFontBitmap.Blit(sourceBitmaps[sg.Page].bitmapData, destBitmaps[i].bitmapData, sg.Rect, 0, 0);
            }
        }

        private static char[] FirstIntercept(Dictionary<char, GLFontGlyph> charSet)
        {
            char[] keys = charSet.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
                for (int j = i + 1; j < keys.Length; j++)
                    if (charSet[keys[i]].Page == charSet[keys[j]].Page && charSet[keys[i]].Rect.IntersectsWith(charSet[keys[j]].Rect))
                        return new char[2] { keys[i], keys[j] };
            return null;
        }

        /// <summary>
        /// Returns the power of 2 that is closest to x, but not smaller than x.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static int PowerOfTwo(int x)
        {
            if (x < 0) { return 0; }

            int shifts = 0;
            uint val = (uint)x;

            while (val > 0)
            {
                val = val >> 1;
                shifts++;
            }

            val = (uint)1 << (shifts - 1);
            if (val < x)
            {
                val = val << 1;
            }

            return (int)val;
        }
    }
}
