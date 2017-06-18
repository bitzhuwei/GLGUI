using System;
using System.Drawing;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GLGUI
{
    public static class GLDraw
    {
        internal static Rectangle CurrentScreenRect;
        internal static GLCtrlContainer CurrentGui;
        internal static Rectangle ScissorRect;

        public static void Fill(ref Color4 color)
        {
            if (color.A == 0.0f)
                return;

            GL.ClearColor(color);
            GL.Scissor(ScissorRect.X, CurrentGui.Outer.Height - ScissorRect.Bottom, ScissorRect.Width, ScissorRect.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public static void FillRect(Rectangle rect, ref Color4 color)
        {
            if (color.A == 0.0f)
                return;

            int w = Math.Min(rect.Width, ScissorRect.Width - rect.X);
            int h = Math.Min(rect.Height, ScissorRect.Height - rect.Y);
            if (w > 0 && h > 0)
            {
                GL.ClearColor(color);
                GL.Scissor(ScissorRect.X + rect.X, CurrentGui.Outer.Height - (ScissorRect.Y + rect.Y + h), w, h);
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }
        }

        public static void Text(GLFontText processedText, Rectangle rect, ref Color4 color)
        {
            Text(processedText, ref rect, ref color);
        }

        public static void Text(GLFontText processedText, ref Rectangle rect, ref Color4 color)
        {
            if (color.A == 0.0f)
                return;

            int w = Math.Min(rect.Width, ScissorRect.Width - rect.X);
            int h = Math.Min(rect.Height, ScissorRect.Height - rect.Y);
            if (w > 0 && h > 0)
            {
                GL.Scissor(ScissorRect.X + rect.X, CurrentGui.Outer.Height - (ScissorRect.Y + rect.Y + h), w, h);
                GL.PushMatrix();
                switch (processedText.alignment)
                {
                    case GLFontAlignment.Left:
                    case GLFontAlignment.Justify:
                        GL.Translate(CurrentScreenRect.X + rect.X, CurrentScreenRect.Y + rect.Y, 0.0f);
                        break;
                    case GLFontAlignment.Centre:
                        GL.Translate(CurrentScreenRect.X + rect.X + rect.Width / 2, CurrentScreenRect.Y + rect.Y, 0.0f);
                        break;
                    case GLFontAlignment.Right:
                        GL.Translate(CurrentScreenRect.X + rect.X + rect.Width, CurrentScreenRect.Y + rect.Y, 0.0f);
                        break;
                }
                GL.Color4(color);
                for (int i = 0; i < processedText.VertexBuffers.Length; i++)
                    processedText.VertexBuffers[i].Draw();
                GL.PopMatrix();
            }
        }
    }
}

