using System.Collections.Generic;
using Metalitix.Scripts.Runtime.Logger.Survey.Enums;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.Extensions
{
    public static class AnchorExtension
    {
        private static Dictionary<AnchorPreset, Vector2> _anchorPresets;
        
        static AnchorExtension()
        {
            _anchorPresets = new Dictionary<AnchorPreset, Vector2>
            {
                { AnchorPreset.Middle, new Vector2(0.5f, 0.5f) },
                { AnchorPreset.MiddleLeft, new Vector2(0, 0.5f) },
                { AnchorPreset.MiddleRight, new Vector2(1, 0.5f) },
                { AnchorPreset.BottomLeft, new Vector2(0, 0) },
                { AnchorPreset.BottomMiddle, new Vector2(0.5f, 0) },
                { AnchorPreset.BottomRight, new Vector2(1, 0) },
                { AnchorPreset.TopLeft, new Vector2(0, 1) },
                { AnchorPreset.TopMiddle, new Vector2(0.5f, 1) },
                { AnchorPreset.TopRight, new Vector2(1, 1) }
            };
        }

        public static Vector2 GetAnchorPositions(AnchorPreset type)
        {
            return _anchorPresets.TryGetValue(type, out var value) ? value : new Vector2(0.5f, 0.5f);
        }
    }
}