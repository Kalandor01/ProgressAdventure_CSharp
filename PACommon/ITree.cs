namespace PACommon
{
    public interface ITree<T, TTree>
        where TTree : ITree<T, TTree>
    {
        #region Properties
        public TTree Root { get; }

        public int Level { get; }

        public TTree? Parrent { get; }

        public IReadOnlyList<TTree> Children { get; }

        public IEnumerable<TTree> FlatList { get; }

        public T Value { get; set; }
        #endregion

        #region Methods
        public void AddChild(TTree child);

        public void AddChildren(IEnumerable<TTree> children);

        public void RemoveChild(TTree child);

        public void RemoveChildAt(int index);

        public void RemoveAllChildren();
        #endregion
    }
}
