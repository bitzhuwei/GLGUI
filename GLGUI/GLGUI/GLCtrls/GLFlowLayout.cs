using System;
using System.Drawing;
using System.Linq;

namespace GLGUI
{
    public class GLFlowLayout : GLCtrl
    {
        public GLFlowDirection FlowDirection { get { return flowDirection; } set { flowDirection = value; Invalidate(); } }
        public GLSkin.GLFlowLayoutSkin Skin { get { return skin; } set { skin = value; Invalidate(); } }

        private GLFlowDirection flowDirection = GLFlowDirection.LeftToRight;
        private GLSkin.GLFlowLayoutSkin skin;
        private Rectangle background;

        public GLFlowLayout(GLCtrlContainer container)
            : base(container)
        {
            this.Render += this.OnRender;

            this.skin = this.Container.Skin.FlowLayout;

            this.outer = new Rectangle(0, 0, 100, 100);
            this.sizeMin = new Size(0, 0);
            this.sizeMax = new Size(int.MaxValue, int.MaxValue);
        }

        private void Invalidate(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void UpdatePositions()
        {
            int current = 0;
            switch (FlowDirection)
            {
                case GLFlowDirection.LeftToRight:
                    foreach (GLCtrl control in this.Controls)
                    {
                        Rectangle o = control.Outer;
                        control.Outer = new Rectangle(current, 0, o.Width, o.Height);
                        current += o.Width + skin.Space;
                    }
                    break;
                case GLFlowDirection.RightToLeft:
                    current = Inner.Width;
                    foreach (GLCtrl control in this.Controls)
                    {
                        Rectangle o = control.Outer;
                        current -= o.Width;
                        control.Outer = new Rectangle(current, 0, o.Width, o.Height);
                        current -= skin.Space;
                    }
                    break;
                case GLFlowDirection.TopToBottom:
                    foreach (GLCtrl control in this.Controls)
                    {
                        Rectangle o = control.Outer;
                        control.Outer = new Rectangle(0, current, o.Width, o.Height);
                        current += o.Height + skin.Space;
                    }
                    break;
                case GLFlowDirection.BottomToTop:
                    current = Inner.Height;
                    foreach (GLCtrl control in this.Controls)
                    {
                        Rectangle o = control.Outer;
                        current -= o.Height;
                        control.Outer = new Rectangle(0, current, o.Width, o.Height);
                        current -= skin.Space;
                    }
                    break;
            }
        }

        protected override void UpdateLayout()
        {
            this.UpdatePositions();

            if (this.AutoSize)
            {
                if (this.Controls.Count() > 0)
                {
                    this.outer.Width = this.Controls.Max(c => c.Outer.Right) - this.Controls.Min(c => c.Outer.Left) + skin.Padding.Horizontal + skin.Border.Horizontal;
                    this.outer.Height = this.Controls.Max(c => c.Outer.Bottom) - this.Controls.Min(c => c.Outer.Top) + skin.Padding.Vertical + skin.Border.Vertical;
                }
                else
                {
                    this.outer.Width = this.skin.Padding.Horizontal + this.skin.Border.Horizontal;
                    this.outer.Height = this.skin.Padding.Vertical + this.skin.Border.Vertical;
                }
            }

            this.outer.Width = Math.Min(Math.Max(outer.Width, sizeMin.Width), sizeMax.Width);
            this.outer.Height = Math.Min(Math.Max(outer.Height, sizeMin.Height), sizeMax.Height);
            this.background = new Rectangle(
             this.skin.Border.Left, this.skin.Border.Top,
               this.outer.Width - this.skin.Border.Horizontal, this.outer.Height - this.skin.Border.Vertical);
            this.Inner = new Rectangle(
               this.background.Left + this.skin.Padding.Left, this.background.Top + this.skin.Padding.Top,
               this.background.Width - this.skin.Padding.Horizontal, this.background.Height - this.skin.Padding.Vertical);

            if (this.flowDirection == GLFlowDirection.BottomToTop || this.flowDirection == GLFlowDirection.RightToLeft)
                this.UpdatePositions();
        }

        private void OnRender(object sender, double timeDelta)
        {
            GLDraw.Fill(ref skin.BorderColor);
            GLDraw.FillRect(ref background, ref skin.BackgroundColor);
        }

        public override T Add<T>(T control)
        {
            base.Add(control);
            control.Resize += this.Invalidate;
            this.Invalidate();
            return control;
        }

        public override void Remove(GLCtrl control)
        {
            base.Remove(control);
            control.Resize -= this.Invalidate;
            this.Invalidate();
        }
    }
}
