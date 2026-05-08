using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class RaycastTarget : Graphic
    {
        // https://discussions.unity.com/t/ui-panel-without-image-component-as-raycast-target-it-is-possible/152401
        // https://younitystudy.tistory.com/m/75

        public override void SetMaterialDirty() { }
        public override void SetVerticesDirty() { }
        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            return true;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}