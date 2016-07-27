using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using OpenTK;

namespace GLGUI
{
    public partial class GLFont
    {

        public SizeF ProcessText(GLFontText processedText, string text, GLFontAlignment alignment = GLFontAlignment.Left)
        {
            return ProcessText(processedText, text, new SizeF(float.MaxValue, float.MaxValue), alignment);
        }
        public SizeF ProcessText(GLFontText processedText, string text, SizeF maxSize, GLFontAlignment alignment = GLFontAlignment.Left)
        {
            if (processedText == null)
                throw new ArgumentNullException("processedText");

            var nodeList = new GLFontTextNodeList(text);
            nodeList.MeasureNodes(fontData, Options);

            if (!Options.WordWrap)
            {
                //we "crumble" words that are two long so that that can be split up
                var nodesToCrumble = new List<GLFontTextNode>();
                foreach (GLFontTextNode node in nodeList)
                    if (node.Length >= maxSize.Width && node.Type == GLFontTextNodeType.Word)
                        nodesToCrumble.Add(node);

                if (nodesToCrumble.Count > 0)
                {
                    foreach (var node in nodesToCrumble)
                        nodeList.Crumble(node, 1);

                    //need to measure crumbled words
                    nodeList.MeasureNodes(fontData, Options);
                }
            }

            if (processedText.VertexBuffers == null)
            {
                processedText.VertexBuffers = new GLFontVertexBuffer[fontData.Pages.Length];
                for (int i = 0; i < processedText.VertexBuffers.Length; i++)
                    processedText.VertexBuffers[i] = new GLFontVertexBuffer(fontData.Pages[i].TextureID);
            }
            if (processedText.VertexBuffers[0].TextureID != fontData.Pages[0].TextureID)
            {
                for (int i = 0; i < processedText.VertexBuffers.Length; i++)
                    processedText.VertexBuffers[i].TextureID = fontData.Pages[i].TextureID;
            }
            processedText.textNodeList = nodeList;
            processedText.maxSize = maxSize;
            processedText.alignment = alignment;

            foreach (var buffer in processedText.VertexBuffers)
                buffer.Reset();
            SizeF size = PrintOrMeasure(processedText.VertexBuffers, processedText, false);
            foreach (var buffer in processedText.VertexBuffers)
                buffer.Load();
            return size;
        }

    }
}