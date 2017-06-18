using System;
using System.Drawing;
using System.Linq;
using OpenTK.Input;

namespace GLGUI
{
    public class GLForm : GLCtrl
    {
        public string Title { get { return title; } set { if (value != title) { title = value; Invalidate(); } } }
        public bool Maximized { get { return maximized; } set { Maximize(value); } }
        public GLSkin.GLFormSkin SkinActive { get { return skinActive; } set { skinActive = value; Invalidate(); } }
        public GLSkin.GLFormSkin SkinInactive { get { return skinInactive; } set { skinInactive = value; Invalidate(); } }

        private string title = "";
        private GLFontText titleProcessed = new GLFontText();
        private GLSkin.GLFormSkin skinActive, skinInactive;
        private GLSkin.GLFormSkin skin;
        private enum DragOperation { Move = 0, ResizeNW, ResizeN, ResizeNE, ResizeE, ResizeSE, ResizeS, ResizeSW, ResizeW }
        private DragOperation dragOp;
        private Rectangle moveClickRegion;
        private Point mouseOffset;
        private SizeF titleSize;
        private int minHeight;
        private DateTime lastClick;

        private static GLCursor[] dragOpCursors = new GLCursor[]
		{
			GLCursor.SizeAll,
			GLCursor.SizeNWSE, GLCursor.SizeNS, GLCursor.SizeNESW, GLCursor.SizeWE,
			GLCursor.SizeNWSE, GLCursor.SizeNS, GLCursor.SizeNESW, GLCursor.SizeWE
		};

        public GLForm(GLCtrlContainer container)
            : base(container)
        {
            this.Render += this.OnRender;
            this.MouseDown += this.OnMouseDown;
            this.MouseUp += this.OnMouseUp;
            this.MouseMove += this.OnMouseMove;
            this.MouseLeave += this.OnMouseLeave;
            //MouseDoubleClick += OnMouseDoubleClick;
            this.Focus += (s, e) => this.Invalidate();
            this.FocusLost += (s, e) => this.Invalidate();

            this.skinActive = this.Container.Skin.FormActive;
            this.skinInactive = this.Container.Skin.FormInactive;

            this.outer = new Rectangle(0, 0, 100, 100);
            this.sizeMin = new Size(64, 32);
            this.sizeMax = new Size(int.MaxValue, int.MaxValue);
        }

        protected override void UpdateLayout()
        {
            this.skin = this.HasFocus ? this.skinActive : this.skinInactive;

            if (this.AutoSize)
            {
                if (this.Controls.Count() > 0)
                {
                    this.outer.Width = this.Controls.Max(c => c.Outer.Right) + this.skin.Border.Horizontal;
                    this.outer.Height = this.Controls.Max(c => c.Outer.Bottom) + this.skin.Border.Vertical + (int)this.titleSize.Height + this.skin.Border.Top;
                }
                else
                {
                    this.outer.Width = 0;
                    this.outer.Height = 0;
                }
            }

            this.outer.Width = Math.Min(Math.Max(this.outer.Width, this.sizeMin.Width), this.sizeMax.Width);

            int innerWidth = this.outer.Width - this.skin.Border.Horizontal;
            this.titleSize = this.skin.Font.ProcessText(this.titleProcessed, this.title, GLFontAlignment.Left);
            this.minHeight = Math.Max(sizeMin.Height, (int)this.titleSize.Height + this.skin.Border.Vertical + this.skin.Border.Top);

            this.outer.Height = Math.Min(Math.Max(this.outer.Height, this.minHeight), this.sizeMax.Height);
            if (this.Parent != null)
            {
                this.outer.X = Math.Max(Math.Min(this.outer.X, this.Parent.Inner.Width - this.outer.Width), 0);
                this.outer.Y = Math.Max(Math.Min(this.outer.Y, this.Parent.Inner.Height - this.outer.Height), 0);
                this.outer.Width = Math.Min(this.outer.Right, this.Parent.Inner.Width) - this.outer.X;
                this.outer.Height = Math.Min(this.outer.Bottom, this.Parent.Inner.Height) - this.outer.Y;
            }

            this.Inner = new Rectangle(
                this.skin.Border.Left,
                this.skin.Border.Top + (int)this.titleSize.Height + this.skin.Border.Top,
                innerWidth,
                this.outer.Height - this.skin.Border.Vertical - (int)this.titleSize.Height - this.skin.Border.Top);
            this.moveClickRegion = new Rectangle(this.skin.Border.Left, this.skin.Border.Top, this.Inner.Width, (int)this.titleSize.Height);
        }

        private void OnRender(object sender, double timeDelta)
        {
            GLDraw.Fill(ref skin.BorderColor);
            GLDraw.FillRect(Inner, ref skin.BackgroundColor);
            GLDraw.Text(titleProcessed, ref moveClickRegion, ref skin.Color);
        }

        private void StartDragOperation(DragOperation op, Point p)
        {
            if ((this.AutoSize || this.maximized) && op != DragOperation.Move)
                return;

            this.mouseOffset = p;
            this.isDragged = true;
            this.dragOp = op;
            this.Container.Cursor = GLForm.dragOpCursors[(int)op];
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                if (moveClickRegion.Contains(e.Position))
                    StartDragOperation(DragOperation.Move, e.Position);
                else if (e.X < skin.Border.Left)
                {
                    if (e.Y < skin.Border.Top)
                        StartDragOperation(DragOperation.ResizeNW, e.Position);
                    else if (e.Y >= outer.Height - skin.Border.Bottom)
                        StartDragOperation(DragOperation.ResizeSW, e.Position);
                    else
                        StartDragOperation(DragOperation.ResizeW, e.Position);
                }
                else if (e.X >= outer.Width - skin.Border.Right)
                {
                    if (e.Y < skin.Border.Top)
                        StartDragOperation(DragOperation.ResizeNE, e.Position);
                    else if (e.Y >= outer.Height - skin.Border.Bottom)
                        StartDragOperation(DragOperation.ResizeSE, e.Position);
                    else
                        StartDragOperation(DragOperation.ResizeE, e.Position);
                }
                else if (e.Y < skin.Border.Top)
                    StartDragOperation(DragOperation.ResizeN, e.Position);
                else if (e.Y >= outer.Height - skin.Border.Bottom)
                    StartDragOperation(DragOperation.ResizeS, e.Position);
            }

            justDoubleClicked = false;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                if (isDragged)
                {
                    isDragged = false;
                    Container.Cursor = GLCursor.Default;
                }
                var now = DateTime.Now;
                if ((now - lastClick).TotalMilliseconds < 500.0)
                    OnMouseDoubleClick(this, e);
                lastClick = now;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragged)
            {
                Point p = e.Position;
                if (Parent != null)
                {
                    p.X = Math.Min(Math.Max(p.X + outer.X, 0), Parent.Inner.Width) - outer.X;
                    p.Y = Math.Min(Math.Max(p.Y + outer.Y, 0), Parent.Inner.Height) - outer.Y;
                }
                int dx = p.X - mouseOffset.X;
                int dy = p.Y - mouseOffset.Y;
                switch (dragOp)
                {
                    case DragOperation.Move:
                        if (maximized && !justDoubleClicked)
                        {
                            if (Math.Max(dx, dy) > 16)
                            {
                                mouseOffset.X = restoreOuter.Width / 2;
                                Maximize(false);
                            }
                        }
                        else
                            Outer = new Rectangle(outer.X + dx, outer.Y + dy, outer.Width, outer.Height);
                        return;
                    case DragOperation.ResizeNW:
                        dx = outer.Width - Math.Min(Math.Max(outer.Width - dx, sizeMin.Width), sizeMax.Width);
                        dy = outer.Height - Math.Min(Math.Max(outer.Height - dy, minHeight), sizeMax.Height);
                        Outer = new Rectangle(outer.X + dx, outer.Y + dy, outer.Width - dx, outer.Height - dy);
                        return;
                    case DragOperation.ResizeN:
                        dy = outer.Height - Math.Min(Math.Max(outer.Height - dy, minHeight), sizeMax.Height);
                        Outer = new Rectangle(outer.X, outer.Y + dy, outer.Width, outer.Height - dy);
                        return;
                    case DragOperation.ResizeNE:
                        dx = Math.Min(Math.Max(p.X, sizeMin.Width), sizeMax.Width);
                        dy = outer.Height - Math.Min(Math.Max(outer.Height - dy, minHeight), sizeMax.Height);
                        Outer = new Rectangle(outer.X, outer.Y + dy, dx, outer.Height - dy);
                        return;
                    case DragOperation.ResizeE:
                        dx = Math.Min(Math.Max(p.X, sizeMin.Width), sizeMax.Width);
                        Outer = new Rectangle(outer.X, outer.Y, dx, outer.Height);
                        return;
                    case DragOperation.ResizeSE:
                        dx = Math.Min(Math.Max(p.X, sizeMin.Width), sizeMax.Width);
                        dy = Math.Min(Math.Max(p.Y, minHeight), sizeMax.Height);
                        Outer = new Rectangle(outer.X, outer.Y, dx, dy);
                        return;
                    case DragOperation.ResizeS:
                        dy = Math.Min(Math.Max(p.Y, minHeight), sizeMax.Height);
                        Outer = new Rectangle(outer.X, outer.Y, outer.Width, dy);
                        return;
                    case DragOperation.ResizeSW:
                        dx = outer.Width - Math.Min(Math.Max(outer.Width - dx, sizeMin.Width), sizeMax.Width);
                        dy = Math.Min(Math.Max(p.Y, minHeight), sizeMax.Height);
                        Outer = new Rectangle(outer.X + dx, outer.Y, outer.Width - dx, dy);
                        return;
                    case DragOperation.ResizeW:
                        dx = outer.Width - Math.Min(Math.Max(outer.Width - dx, sizeMin.Width), sizeMax.Width);
                        Outer = new Rectangle(outer.X + dx, outer.Y, outer.Width - dx, outer.Height);
                        return;
                }
            }

            if (!AutoSize && !maximized)
            {
                if (e.X < skin.Border.Left)
                {
                    if (e.Y < skin.Border.Top)
                        Container.Cursor = GLCursor.SizeNWSE;
                    else if (e.Y >= outer.Height - skin.Border.Bottom)
                        Container.Cursor = GLCursor.SizeNESW;
                    else
                        Container.Cursor = GLCursor.SizeWE;
                }
                else if (e.X >= Outer.Width - skin.Border.Right)
                {
                    if (e.Y < skin.Border.Top)
                        Container.Cursor = GLCursor.SizeNESW;
                    else if (e.Y >= outer.Height - skin.Border.Bottom)
                        Container.Cursor = GLCursor.SizeNWSE;
                    else
                        Container.Cursor = GLCursor.SizeWE;
                }
                else if (e.Y < skin.Border.Top || e.Y >= outer.Height - skin.Border.Bottom)
                    Container.Cursor = GLCursor.SizeNS;
                else
                    Container.Cursor = GLCursor.Default;
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            Container.Cursor = GLCursor.Default;
        }

        private bool justDoubleClicked = false;
        private bool maximized = false;
        private Rectangle restoreOuter;
        private GLAnchorStyles restoreAnchor;
        private void Maximize(bool maximize)
        {
            if (maximized == maximize)
                return;
            if (!maximize) // restore
            {
                maximized = false;
                anchor = restoreAnchor;
                outer = restoreOuter;
                Invalidate();
            }
            else // maximize
            {
                maximized = true;
                restoreOuter = outer;
                restoreAnchor = anchor;
                outer = Parent.Inner;
                anchor = GLAnchorStyles.Left | GLAnchorStyles.Top | GLAnchorStyles.Right | GLAnchorStyles.Bottom;
                Invalidate();
            }
        }

        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!AutoSize && Parent != null && moveClickRegion.Contains(e.Position))
                Maximize(!maximized);

            Container.Cursor = GLCursor.Default; // hack to avoid move operation cursor
            justDoubleClicked = true;
        }
    }
}

