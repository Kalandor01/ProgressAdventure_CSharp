using System.Collections;

namespace PACommon
{
    public class Tree<T> : ITree<T, Tree<T>>, IEnumerable<Tree<T>>
    {
        #region Private fields
        private Tree<T>? _parrent;

        private readonly List<Tree<T>> _children;
        #endregion

        #region Public properties
        public Tree<T> Root => Parrent is null ? this : Parrent.Root;

        public int Level => Parrent is null ? 0 : Parrent.Level + 1;

        public Tree<T>? Parrent
        {
            get => _parrent;
            set
            {
                _parrent?.RemoveChild(this);
                _parrent = value;
            }
        }

        public IReadOnlyList<Tree<T>> Children => _children.AsReadOnly();

        public IEnumerable<Tree<T>> FlatList => GetFlatList();

        public T Value { get; set; }
        #endregion

        #region Constructors
        public Tree(T value)
        {
            _children = [];
            Value = value;
        }
        #endregion

        #region Public Methods
        public void AddChild(Tree<T> child)
        {
            child.Parrent = this;
            _children.Add(child);
        }

        public void AddChildren(IEnumerable<Tree<T>> children)
        {
            foreach (var child in children)
            {
                child.Parrent = this;
            }
            _children.AddRange(children);
        }

        public void RemoveAllChildren()
        {
            foreach (var child in _children)
            {
                child.Parrent = null;
            }
            _children.Clear();
        }

        public void RemoveChild(Tree<T> child)
        {
            child.Parrent = null;
            _children.Remove(child);
        }

        public void RemoveChildAt(int index)
        {
            _children[index].Parrent = null;
            _children.RemoveAt(index);
        }

        public override string? ToString()
        {
            return Value?.ToString();
        }

        public void Add(Tree<T> child)
        {
            AddChild(child);
        }

        public IEnumerator<Tree<T>> GetEnumerator()
        {
            return GetFlatList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetFlatList().GetEnumerator();
        }
        #endregion

        #region Private methods
        private IEnumerable<Tree<T>> GetFlatList()
        {
            yield return this;
            foreach (var child in _children)
            {
                foreach (var subTreeItem in child.GetFlatList())
                {
                    yield return subTreeItem;
                }
            }
        }
        #endregion
    }
}
