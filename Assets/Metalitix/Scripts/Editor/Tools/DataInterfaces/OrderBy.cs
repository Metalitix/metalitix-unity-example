using System;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    public class OrderBy
    {
        private OrderByType orderByType;
        private bool isDescending;

        public OrderBy(OrderByType orderByType, bool isDescending = false)
        {
            this.orderByType = orderByType;
            this.isDescending = isDescending;
        }

        public string GetOrder()
        {
            var sign = isDescending ? "-" : String.Empty;
            var value = sign + Enum.GetName(typeof(OrderByType), orderByType);
            return value;
        }
    }
}