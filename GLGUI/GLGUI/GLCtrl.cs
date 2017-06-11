using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Input;

namespace GLGUI
{
    public abstract partial class GLCtrl
    {
        public virtual string Name { get; set; }
        public GLCtrlContainer Container { get; internal set; }
        public GLCtrl Parent { get; internal set; }
        public IEnumerable<GLCtrl> Controls { get { return controls; } }

        public Rectangle Outer { get { return outer; } set { if (outer != value) { outer = value; Invalidate(); } } }
        public Rectangle Inner { get { return inner; } protected set { if (inner != value) { lastInner = inner; inner = value; DoResize(); /*Invalidate();*/ } } }
        public Size SizeMin { get { return sizeMin; } set { sizeMin = value; Invalidate(); } }
        public Size SizeMax { get { return sizeMax; } set { sizeMax = value; Invalidate(); } }
        public bool HasFocus { get; private set; }
        public bool IsDragged { get { return isDragged || controls.Any(c => c.IsDragged); } }
        public GLAnchorStyles Anchor { get { return anchor; } set { anchor = value; Invalidate(); } }
        public bool AutoSize { get { return autoSize; } set { autoSize = value; Invalidate(); } }
        public virtual GLContextMenu ContextMenu { get { return contextMenu; } set { contextMenu = value; } }
        public bool HandleMouseEvents { get { return handleMouseEvents; } set { handleMouseEvents = value; } }

        // derived from above properties:
        public Point Location { get { return outer.Location; } set { Outer = new Rectangle(value, outer.Size); } }
        public Size Size { get { return outer.Size; } set { Outer = new Rectangle(outer.Location, value); } }
        public int X { get { return outer.X; } }
        public int Y { get { return outer.Y; } }
        public int Width { get { return outer.Width; } }
        public int Height { get { return outer.Height; } }
        public Point InnerOffset { get { return inner.Location; } }
        public Size InnerSize { get { return inner.Size; } }
        public int InnerWidth { get { return inner.Width; } }
        public int InnerHeight { get { return inner.Height; } }

        protected Rectangle outer;
        protected Size sizeMin, sizeMax;
        protected bool isDragged = false;
        protected GLAnchorStyles anchor = GLAnchorStyles.Left | GLAnchorStyles.Top;

        private Rectangle inner;
        private Rectangle lastInner;
        private bool autoSize = false;
        private readonly List<GLCtrl> controls = new List<GLCtrl>();
        private static int idCounter = 0;
        private bool visited = false;
        private GLContextMenu contextMenu;
        private bool handleMouseEvents = true;

        private GLCtrl hoverChild;
        private GLCtrl focusedChild;

        protected GLCtrl(GLCtrlContainer container)
        {
            Container = container;
            Name = GetType().Name + (idCounter++);

            outer = new Rectangle(0, 0, 0, 0);
            inner = new Rectangle(0, 0, 0, 0);
            sizeMin = new Size(0, 0);
            sizeMax = new Size(int.MaxValue, int.MaxValue);
        }

        public void Invalidate()
        {
            if (visited || Container == null || Container.LayoutSuspended)
                return;
            visited = true;

            UpdateLayout();

            if (Parent != null && Parent.autoSize)
                Parent.Invalidate();

            visited = false;
        }

        protected virtual void UpdateLayout()
        {
            if (autoSize)
            {
                if (controls.Count > 0)
                {
                    outer.Width = Controls.Max(c => c.Outer.Right);
                    outer.Height = Controls.Max(c => c.Outer.Bottom);
                }
                else
                {
                    outer.Width = 0;
                    outer.Height = 0;
                }
            }
            outer.Width = Math.Min(Math.Max(outer.Width, sizeMin.Width), sizeMax.Width);
            outer.Height = Math.Min(Math.Max(outer.Height, sizeMin.Height), sizeMax.Height);
            Inner = new Rectangle(0, 0, outer.Width, outer.Height);
        }

        public virtual T Add<T>(T control) where T : GLCtrl
        {
            if (control.Parent != null)
            {
                string message = string.Format("{0} {1} is already a child of {2} {3}.",
                    control.GetType().Name, control.Name,
                    control.Parent.GetType().Name, control.Parent.Name);
                throw new ArgumentException(message);
            }
            if (control is GLForm || control is GLContextMenu)
                controls.Insert(0, control);
            else
                controls.Add(control);
            control.Container = Container;
            control.Parent = this;
            control.Invalidate();
            return control;
        }

        public virtual void Remove(GLCtrl control)
        {
            if (control.Parent == null)
                return;

            if (control.Parent != this)
            {
                string message = string.Format("{0} {1} is a child of {2} {3}.",
                    control.GetType().Name, control.Name,
                    control.Parent.GetType().Name, control.Parent.Name);
                throw new ArgumentException(message);
            }

            controls.Remove(control);
            control.Container = null;
            control.Parent = null;
        }

        public virtual void Clear()
        {
            Container.SuspendLayout();
            foreach (GLCtrl control in controls.ToArray())
                Remove(control);
            Container.ResumeLayout();
        }

        protected Point ToControl(Point p)
        {
            p.X -= outer.X;
            p.Y -= outer.Y;
            GLCtrl c = Parent;
            while (c != c.Parent)
            {
                p.X -= c.inner.X + c.outer.X;
                p.Y -= c.inner.Y + c.outer.Y;
                c = c.Parent;
            }
            return p;
        }

        protected Point ToViewport(Point p)
        {
            p.X += outer.X;
            p.Y += outer.Y;
            GLCtrl c = Parent;
            while (c != c.Parent)
            {
                p.X += c.inner.X + c.outer.X;
                p.Y += c.inner.Y + c.outer.Y;
                c = c.Parent;
            }
            return p;
        }

        protected Rectangle ToViewport(Rectangle r)
        {
            r.X += outer.X;
            r.Y += outer.Y;
            GLCtrl c = Parent;
            while (c != c.Parent)
            {
                r.X += c.inner.X + c.outer.X;
                r.Y += c.inner.Y + c.outer.Y;
                c = c.Parent;
            }
            return r;
        }

        internal void DoRender(Point absolutePosition, double timeDelta)
        {
            absolutePosition.X += outer.X;
            absolutePosition.Y += outer.Y;

            if (Render != null)
            {
                GLDraw.ControlRect = new Rectangle(absolutePosition, outer.Size);
                GLDraw.ScissorRect.Intersect(GLDraw.ControlRect);
                if (GLDraw.ScissorRect.Width != 0 && GLDraw.ScissorRect.Height != 0)
                    Render(this, timeDelta);
            }

            if (controls.Count > 0)
            {
                absolutePosition.X += inner.X;
                absolutePosition.Y += inner.Y;
                GLDraw.ControlRect = new Rectangle(absolutePosition, inner.Size);
                GLDraw.ScissorRect.Intersect(GLDraw.ControlRect);

                if (GLDraw.ScissorRect.Width != 0 && GLDraw.ScissorRect.Height != 0)
                {
                    Rectangle scissorRect = GLDraw.ScissorRect;
                    for (int i = controls.Count - 1; i >= 0; i--)
                    {
                        controls[i].DoRender(absolutePosition, timeDelta);
                        GLDraw.ScissorRect = scissorRect;
                    }
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
