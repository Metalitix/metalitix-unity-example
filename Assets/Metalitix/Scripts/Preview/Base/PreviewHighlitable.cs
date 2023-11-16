using Metalitix.Core.Base;
using UnityEngine;

namespace Metalitix.Scripts.Preview.Base
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider))]
    public abstract class PreviewHighlitable : MonoBehaviour, IScenePreviewPointerMove
    {
        public virtual void PointerEnter()
        {

        }

        public virtual void PointerExit()
        {

        }
    }
}