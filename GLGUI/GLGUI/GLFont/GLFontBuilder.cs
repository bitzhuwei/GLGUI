﻿using System;
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

        public GLFontData BuildFontData()
        {
            if (config.ForcePowerOfTwo && config.SuperSampleLevels != PowerOfTwo(config.SuperSampleLevels))
                throw new ArgumentOutOfRangeException("SuperSampleLevels must be a power of two when using ForcePowerOfTwo.");
            if (config.SuperSampleLevels <= 0 || config.SuperSampleLevels > 8)
                throw new ArgumentOutOfRangeException("SuperSampleLevels = [" + config.SuperSampleLevels + "] is an unsupported value. Please use values in the range [1,8]");

            int margin = 2; // margin in initial bitmap (don't bother to make configurable - likely to cause confusion
            int pageWidth = config.PageWidth * config.SuperSampleLevels;
            int pageHeight = config.PageHeight * config.SuperSampleLevels;
            bool usePowerOfTwo = config.ForcePowerOfTwo;
            int glyphMargin = config.GlyphMargin * config.SuperSampleLevels;

            List<SizeF> sizes = GetGlyphSizes(font);
            SizeF maxSize = GetMaxGlyphSize(sizes);
            GLFontGlyph[] initialGlyphs;
            var initialBmp = CreateInitialBitmap(font, maxSize, margin, out initialGlyphs, config.TextGenerationRenderHint);
            var initialBitmapData = initialBmp.LockBits(new Rectangle(0, 0, initialBmp.Width, initialBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int minYOffset = int.MaxValue;
            foreach (var glyph in initialGlyphs)
            {
                RetargetGlyphRectangleInwards(initialBitmapData, glyph, true, config.KerningConfig.AlphaEmptyPixelTolerance);
                minYOffset = Math.Min(minYOffset, glyph.YOffset);
            }
            minYOffset--; // give one pixel of breathing room?

            foreach (var glyph in initialGlyphs)
                glyph.YOffset -= minYOffset;

            GLFontGlyph[] glyphs;
            var bitmapPages = GenerateBitmapSheetsAndRepack(initialGlyphs, new BitmapData[1] { initialBitmapData }, pageWidth, pageHeight, out glyphs, glyphMargin, usePowerOfTwo);

            initialBmp.UnlockBits(initialBitmapData);
            initialBmp.Dispose();

            if (config.SuperSampleLevels != 1)
            {
                ScaleSheetsAndGlyphs(bitmapPages, glyphs, 1.0f / config.SuperSampleLevels);
                RetargetAllGlyphs(bitmapPages, glyphs, config.KerningConfig.AlphaEmptyPixelTolerance);
            }

            //create list of texture pages
            var pages = new List<GLFontTexture>();
            foreach (var page in bitmapPages)
                pages.Add(new GLFontTexture(page.bitmapData));

            var fontData = new GLFontData();
            fontData.CharSetMapping = glyphs.ToDictionary(g => g.Character);
            fontData.Pages = pages.ToArray();
            fontData.CalculateMeanWidth();
            fontData.CalculateMaxHeight();
            fontData.KerningPairs = GLFontKerningCalculator.CalculateKerning(charSet.ToCharArray(), glyphs, bitmapPages, config.KerningConfig);
            fontData.naturallyMonospaced = IsMonospaced(sizes);

            foreach (var glyph in glyphs)
            {
                var page = pages[glyph.Page];
                glyph.TextureMin = new PointF((float)glyph.Rect.X / page.Width, (float)glyph.Rect.Y / page.Height);
                glyph.TextureMax = new PointF((float)glyph.Rect.Right / page.Width, (float)glyph.Rect.Bottom / page.Height);
            }

            foreach (var page in bitmapPages)
                page.Free();

            //validate glyphs
            var intercept = FirstIntercept(fontData.CharSetMapping);
            if (intercept != null)
                throw new Exception("Failed to create glyph set. Glyphs '" + intercept[0] + "' and '" + intercept[1] + "' were overlapping. This is could be due to an error in the font, or a bug in Graphics.MeasureString().");

            return fontData;
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
