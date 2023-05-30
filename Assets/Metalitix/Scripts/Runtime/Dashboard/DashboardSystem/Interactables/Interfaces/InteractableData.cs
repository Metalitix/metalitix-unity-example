using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Interfaces
{
    public class InteractableData
    {
        public Vector2 uv { get; }
        public Vector2 mousePos { get; }
        public float width { get; }
        public float height { get; }

        public InteractableData(Vector2 uv, Vector2 mousePos, float width, float height)
        {
            this.uv = uv;
            this.width = width;
            this.height = height;
            this.mousePos = mousePos;
        }
    }
}