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
            Bitmap initialBmp = CreateInitialBitmap(font, maxSize, margin, out initialGlyphs, config.TextGenerationRenderHint);
            initialBmp.Save("initialBmp0.bmp");
            BitmapData initialBitmapData = initialBmp.LockBits(new Rectangle(0, 0, initialBmp.Width, initialBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            var minYOffset = int.MaxValue;
            foreach (GLFontGlyph glyph in initialGlyphs)
            {
                RetargetGlyphRectangleInwards(initialBitmapData, glyph, true, config.KerningConfig.AlphaEmptyPixelTolerance);
                minYOffset = Math.Min(minYOffset, glyph.YOffset);
            }
            minYOffset--; // give one pixel of breathing room?
            initialBmp.Save("initialBmp1.bmp");

            foreach (var glyph in initialGlyphs)
                glyph.YOffset -= minYOffset;

            GLFontGlyph[] glyphs;
            List<GLFontBitmap> bitmapPages = GenerateBitmapSheetsAndRepack(initialGlyphs, new BitmapData[1] { initialBitmapData }, pageWidth, pageHeight, out glyphs, glyphMargin, usePowerOfTwo);
            int index = 0;
            foreach (GLFontBitmap item in bitmapPages)
            {
                item.bitmap.Save(string.Format("bitmapPages{0}.bmp", index++));
            }
            initialBmp.UnlockBits(initialBitmapData);
            initialBmp.Save("initialBmp2.bmp");
            initialBmp.Dispose();

            if (config.SuperSampleLevels != 1)
            {
                ScaleSheetsAndGlyphs(bitmapPages, glyphs, 1.0f / config.SuperSampleLevels);
                RetargetAllGlyphs(bitmapPages, glyphs, config.KerningConfig.AlphaEmptyPixelTolerance);
                index = 0;
                foreach (GLFontBitmap item in bitmapPages)
                {
                    item.bitmap.Save(string.Format("bitmapPages.2.{0}.bmp", index++));
                }
            }

            //create list of texture pages
            var texturePages = new List<GLFontTexture>();
            foreach (var page in bitmapPages)
                texturePages.Add(new GLFontTexture(page.bitmapData));

            var fontData = new GLFontData();
            fontData.CharSetMapping = glyphs.ToDictionary(g => g.Character);
            fontData.TexturePages = texturePages.ToArray();
            fontData.CalculateMeanWidth();
            fontData.CalculateMaxHeight();
            fontData.KerningPairs = GLFontKerningCalculator.CalculateKerning(charSet.ToCharArray(), glyphs, bitmapPages, config.KerningConfig);
            fontData.naturallyMonospaced = IsMonospaced(sizes);

            foreach (var glyph in glyphs)
            {
                var page = texturePages[glyph.Page];
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

    }
}