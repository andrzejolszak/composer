using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Composer.Editor
{
    abstract class Element
    {
        public ViewManager manager;
        public List<InteractableRegion> interactableRegions;

        public bool Highlighted { get; protected set; }

        public Element(ViewManager manager)
        {
            this.manager = manager;
        }


        public virtual void SetHighlighted(bool isSelected)
        {
            this.Highlighted = isSelected;
        }

        public virtual void RefreshLayout()
        {

        }


        public virtual void BeginModify()
        {

        }


        public virtual bool EndModify()
        {
            return false;
        }

        public virtual void OnPressKeyPreview(Keys keyData)
        {
            var ctrlKey = (keyData & Keys.Control) != 0;
            var shiftKey = (keyData & Keys.Shift) != 0;

            keyData = (keyData & ~(Keys.Control | Keys.Shift));
        }

        public virtual void OnPressUp(bool ctrlKey, bool shiftKey)
        {

        }


        public virtual void OnPressDown(bool ctrlKey, bool shiftKey)
        {

        }


        public virtual void OnPressRight(bool ctrlKey, bool shiftKey)
        {

        }


        public virtual void OnPressLeft(bool ctrlKey, bool shiftKey)
        {

        }


        public virtual void Drag()
        {

        }


        public virtual void Draw(Graphics g, bool hovering)
        {

        }
    }
}
