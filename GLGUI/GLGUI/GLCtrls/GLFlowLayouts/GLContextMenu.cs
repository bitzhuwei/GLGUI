using System;
using System.Drawing;
using System.Linq;

namespace GLGUI
{
    public class GLContextMenu : GLFlowLayout
    {
        public GLContextMenu(GLCtrlContainer container)
            : base(container)
        {
            FlowDirection = GLFlowDirection.TopDown;
            AutoSize = true;

            Skin = Container.Skin.ContextMenu;
        }

        protected override void UpdateLayout()
        {
            if (Controls.Count() > 0)
            {
                int maxWidth = 0;
                foreach (var entry in Controls)
                {
                    entry.AutoSize = true;
                    maxWidth = Math.Max(maxWidth, entry.Width);
                }
                foreach (var entry in Controls)
                {
                    entry.AutoSize = false;
                    entry.Size = new Size(maxWidth, entry.Height);
                }
            }

            base.UpdateLayout();
        }
    }
}