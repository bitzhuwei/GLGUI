namespace GLGUI
{
	public class GLContextMenuEntry : GLButton
	{
		public GLContextMenuEntry(GLControlControlContainer gui) : base(gui)
		{
			SkinEnabled = Container.Skin.ContextMenuEntryEnabled;
			SkinPressed = Container.Skin.ContextMenuEntryPressed;
			SkinHover = Container.Skin.ContextMenuEntryHover;
			SkinDisabled = Container.Skin.ContextMenuEntryDisabled;

			Click += (s, e) => Container.CloseContextMenu();
		}
	}
}
