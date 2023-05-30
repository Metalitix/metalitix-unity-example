using System.Collections.Generic;
using Metalitix.Scripts.Runtime.Logger.Core.Enums;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Containers
{
    public static class MetalitixPointStatesContainer
    {
        private const string Pressed = "state.pressed";
        private const string Updated = "state.updated";
        private const string Released = "state.released";
        private const string Stationary = "state.stationary";
        
        private static Dictionary<PointStates, string> _dictionary;
        
        /// <summary>
        /// Create and return dictionary with with an Enum + String pair for easy use
        /// </summary>
        /// <returns></returns>
        static MetalitixPointStatesContainer()
        {
            _dictionary = new Dictionary<PointStates, string>
            {
                { PointStates.Pressed, Pressed},
                { PointStates.Updated, Updated},
                { PointStates.Released, Released},
                { PointStates.Stationary, Stationary},
            };
        }
        
        /// <summary>
        /// Create and return dictionary with with an Enum + String pair for easy use
        /// </summary>
        /// <returns></returns>
        public static string GetConstant(PointStates key)
        {
            return !_dictionary.TryGetValue(key, out var value) ? null : value;
        }
    }
}