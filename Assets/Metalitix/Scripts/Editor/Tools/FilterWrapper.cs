namespace Metalitix.Scripts.Editor.Tools
{
    public class FilterWrapper
    {
        public int id { get; }
        public string filterName { get; }
        public bool isSelected { get; private set; }

        public FilterWrapper(int id, string filterName)
        {
            this.id = id;
            this.filterName = filterName;
        }

        public void Select()
        {
            isSelected = true;
        }

        public void UnSelect()
        {
            isSelected = false;
        }
    }
}