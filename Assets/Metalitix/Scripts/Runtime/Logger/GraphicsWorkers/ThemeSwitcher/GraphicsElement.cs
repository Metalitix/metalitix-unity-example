using UnityEngine;
using UnityEngine.UI;

namespace Metalitix.Scripts.Runtime.Logger.GraphicsWorkers.ThemeSwitcher
{
    public class GraphicsElement : MonoBehaviour
    {
        [SerializeField] private Graphic graphic;
        [SerializeField] private GraphicType type;

        public Graphic Graphic => graphic;
        public GraphicType Type => type;
    }
}