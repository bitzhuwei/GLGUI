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
        public delegate void RenderEventHandler(object sender, double timeDelta);
        public delegate bool KeyEventHandler(object sender, KeyboardKeyEventArgs e);
        public delegate bool KeyPressEventHandler(object sender, KeyPressEventArgs e);

        public event RenderEventHandler Render;
        public event EventHandler<MouseMoveEventArgs> MouseMove;
        public event EventHandler<MouseButtonEventArgs> MouseDown;
        public event EventHandler<MouseButtonEventArgs> MouseUp;
        public event EventHandler<MouseWheelEventArgs> MouseWheel;
        public event EventHandler MouseEnter;
        public event EventHandler MouseLeave;
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;
        public event KeyPressEventHandler KeyPress;
        public event EventHandler Focus;
        public event EventHandler FocusLost;
        public event EventHandler Resize;

        internal bool DoMouseMove(MouseMoveEventArgs e)
        {
            if (Parent == null)
                return false;

            if (!isDragged)
            {
                Point im = new Point(e.X - inner.X, e.Y - inner.Y);

                if (hoverChild != null && hoverChild.IsDragged)
                {
                    hoverChild.DoMouseMove(new MouseMoveEventArgs(im.X - hoverChild.Outer.X, im.Y - hoverChild.Outer.Y, e.XDelta, e.YDelta));
                    return true;
                }

                if (inner.Contains(e.Position))
                {
                    foreach (GLCtrl control in controls)
                    {
                        if (control.Outer.Contains(im))
                        {
                            if (control.DoMouseMove(new MouseMoveEventArgs(im.X - control.Outer.X, im.Y - control.Outer.Y, e.XDelta, e.YDelta)))
                            {
                                if (hoverChild != control)
                                {
                                    if (hoverChild != null)
                                        hoverChild.DoMouseLeave();
                                    hoverChild = control;
                                    hoverChild.DoMouseEnter();
                                }
                                return true;
                            }
                        }
                    }
                }

                if (hoverChild != null)
                {
                    hoverChild.DoMouseLeave();
                    hoverChild = null;
                }
            }

            if (MouseMove != null)
            {
                MouseMove(this, e);
                return true;
            }

            if (MouseEnter != null || MouseLeave != null) // force correct MouseEnter/Leave handling
                return true;

            return handleMouseEvents;
        }

        internal bool DoMouseDown(MouseButtonEventArgs e)
        {
            if (Parent == null)
                return false;

            if (!isDragged)
            {
                Point im = new Point(e.X - inner.X, e.Y - inner.Y);

                if (!HasFocus)
                {
                    HasFocus = true;
                    if (Focus != null)
                        Focus(this, EventArgs.Empty);
                }

                if (inner.Contains(e.Position))
                {
                    int i = 0;
                    foreach (GLCtrl control in controls)
                    {
                        if (control.Outer.Contains(im))
                        {
                            bool handled = control.DoMouseDown(new MouseButtonEventArgs(im.X - control.Outer.X, im.Y - control.Outer.Y, e.Button, e.IsPressed));
                            if (control is GLForm)
                            {
                                GLCtrl tmp = controls[i]; // move to front
                                controls.RemoveAt(i);
                                controls.Insert(0, tmp);
                            }
                            if (handled)
                            {
                                if (control != focusedChild)
                                {
                                    if (focusedChild != null)
                                        focusedChild.DoFocusLost();
                                    focusedChild = control;
                                }
                                return true;
                            }
                        }
                        i++;
                    }
                }
            }

            bool handledHere = false;

            if (MouseDown != null)
            {
                MouseDown(this, e);
                handledHere = true;
            }

            if (contextMenu != null && e.Button == MouseButton.Right)
            {
                Container.OpenContextMenu(contextMenu, ToViewport(e.Position));
                handledHere = true;
            }

            return handledHere || handleMouseEvents;
        }

        internal bool DoMouseUp(MouseButtonEventArgs e)
        {
            if (Parent == null)
                return false;

            if (!isDragged)
            {
                Point im = new Point(e.X - inner.X, e.Y - inner.Y);

                if (hoverChild != null && hoverChild.IsDragged)
                {
                    hoverChild.DoMouseUp(new MouseButtonEventArgs(im.X - hoverChild.Outer.X, im.Y - hoverChild.Outer.Y, e.Button, e.IsPressed));
                    return true;
                }

                if (inner.Contains(e.Position))
                {
                    foreach (GLCtrl control in controls)
                    {
                        if (control.Outer.Contains(im))
                        {
                            if (control.DoMouseUp(new MouseButtonEventArgs(im.X - control.Outer.X, im.Y - control.Outer.Y, e.Button, e.IsPressed)))
                                return true;
                        }
                    }
                }
            }

            if (MouseUp != null)
            {
                MouseUp(this, e);
                return true;
            }

            return handleMouseEvents;
        }

        internal bool DoMouseWheel(MouseWheelEventArgs e)
        {
            if (Parent == null)
                return false;

            if (!isDragged)
            {
                Point im = new Point(e.X - inner.X, e.Y - inner.Y);

                if (inner.Contains(e.Position))
                {
                    foreach (GLCtrl control in controls)
                    {
                        if (control.Outer.Contains(im))
                        {
                            if (control.DoMouseWheel(new MouseWheelEventArgs(im.X - control.Outer.X, im.Y - control.Outer.Y, e.Value, e.Delta)))
                                return true;
                        }
                    }
                }
            }

            if (MouseWheel != null)
            {
                MouseWheel(this, e);
                return true;
            }

            return handleMouseEvents;
        }

        internal void DoMouseEnter()
        {
            if (Parent == null)
                return;

            Container.Cursor = GLCursor.Default;

            if (MouseEnter != null)
                MouseEnter(this, EventArgs.Empty);
        }

        internal void DoMouseLeave()
        {
            if (Parent == null)
                return;

            if (hoverChild != null)
            {
                hoverChild.DoMouseLeave();
                hoverChild = null;
            }

            if (MouseLeave != null)
                MouseLeave(this, EventArgs.Empty);
        }

        internal bool DoKeyUp(KeyboardKeyEventArgs e)
        {
            if (Parent == null)
                return false;

            if (!isDragged)
            {
                foreach (GLCtrl control in controls)
                {
                    if (control.HasFocus)
                    {
                        if (control.DoKeyUp(e))
                            return true;
                    }
                }
            }

            if (KeyUp != null)
                return KeyUp(this, e);

            return false;
        }

        internal bool DoKeyDown(KeyboardKeyEventArgs e)
        {
            if (Parent == null)
                return false;

            if (!isDragged)
            {
                foreach (GLCtrl control in controls)
                {
                    if (control.HasFocus)
                    {
                        if (control.DoKeyDown(e))
                            return true;
                    }
                }
            }

            if (KeyDown != null)
                return KeyDown(this, e);

            return false;
        }

        internal bool DoKeyPress(KeyPressEventArgs e)
        {
            if (Parent == null)
                return false;

            if (!isDragged)
            {
                foreach (GLCtrl control in controls)
                {
                    if (control.HasFocus)
                    {
                        if (control.DoKeyPress(e))
                            return true;
                    }
                }
            }

            if (KeyPress != null)
                return KeyPress(this, e);

            return false;
        }

        internal void DoFocusLost()
        {
            if (Parent == null)
                return;

            if (HasFocus)
            {
                HasFocus = false;
                if (FocusLost != null)
                    FocusLost(this, EventArgs.Empty);
            }
            foreach (GLCtrl control in controls)
                if (control.HasFocus)
                    control.DoFocusLost();
        }

        private int subPixelDx = 0, subPixelDy = 0;
        internal void DoResize()
        {
            if (Parent == null)
                return;

            if (Resize != null)
                Resize(this, EventArgs.Empty);

            int dx = inner.Width - lastInner.Width;
            int dy = inner.Height - lastInner.Height;

            foreach (GLCtrl control in controls)
            {
                var a = control.Anchor;
                var o = control.Outer;
                int l = o.Left, r = o.Right, t = o.Top, b = o.Bottom;

                if ((a & (GLAnchorStyles.Left | GLAnchorStyles.Right)) == 0)
                {
                    dx += control.subPixelDx; control.subPixelDx = Math.Sign(dx) * (dx & 1);
                    l += dx / 2;
                    r += dx / 2;
                }
                else
                {
                    if ((a & GLAnchorStyles.Left) == 0)
                        l += dx;
                    if ((a & GLAnchorStyles.Right) != 0)
                        r += dx;
                }

                if ((a & (GLAnchorStyles.Top | GLAnchorStyles.Bottom)) == 0)
                {
                    dy += control.subPixelDy; control.subPixelDy = Math.Sign(dy) * (dy & 1);
                    t += dy / 2;
                    b += dy / 2;
                }
                else
                {
                    if ((a & GLAnchorStyles.Top) == 0)
                        t += dy;
                    if ((a & GLAnchorStyles.Bottom) != 0)
                        b += dy;
                }

                control.Outer = new Rectangle(l, t, r - l, b - t);
            }
        }

    }
}
