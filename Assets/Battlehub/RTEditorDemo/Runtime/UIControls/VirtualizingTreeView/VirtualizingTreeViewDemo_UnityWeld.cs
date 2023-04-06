using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;
using System;
using Battlehub.UIControls.Binding;

namespace Battlehub.UIControls
{
    [Adapter(typeof(IEnumerable), typeof(IEnumerable<DataItemViewModel>))]
    public class IEnumerableToIEnumerableOfDataItemViewModelAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            IEnumerable enumerable = (IEnumerable)valueIn;
            return enumerable.OfType<DataItemViewModel>();
        }
    }


    [Binding]
    public class DataItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string m_name;

        [Binding]
        public string Name
        {
            get { return m_name; }
            set
            {
                if (m_name != value)
                {
                    if(!string.IsNullOrEmpty(m_name))
                    {
                        Debug.Log(value);
                    }
                    
                    m_name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }

        private bool m_isIconVisible;
        [Binding]
        public bool IsIconVisible
        {
            get { return m_isIconVisible; }
            set
            {
                if(m_isIconVisible != value)
                {
                    m_isIconVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsIconVisible)));
                }
            }
        }

        public DataItemViewModel Parent
        {
            get;
            set;
        }

        public List<DataItemViewModel> Children
        {
            get;
            private set;
        }

        public DataItemViewModel()
        {
            Children = new List<DataItemViewModel>();
        }

        public DataItemViewModel(string name, bool isIconVisible = true)
        {
            Name = name;
            IsIconVisible = isIconVisible;
            Children = new List<DataItemViewModel>();
        }

        public override string ToString()
        {
            return Name;
        }
    }


    [Binding]
    public class VirtualizingTreeViewDemo_UnityWeld : MonoBehaviour, IHierarchicalData<DataItemViewModel>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<HierarchicalDataChangedEventArgs> HierarchicalDataChanged;

        private List<DataItemViewModel> m_items = new List<DataItemViewModel>();
        [Binding]
        public IHierarchicalData<DataItemViewModel> HierarchicalData
        {
            get { return this; }
        }

        private IEnumerable<DataItemViewModel> m_selectedItems;

        [Binding]
        public IEnumerable<DataItemViewModel> SelectedItems
        { 
            get { return m_selectedItems; }
            set
            {
                if(m_selectedItems != value)
                {
                    m_selectedItems = value;
                    RaisePropertyChanged(nameof(SelectedItems));
                }
            }
        }

        private void Start()
        {
            var items = new List<DataItemViewModel>();
            for (int i = 0; i < 100; ++i)
            {
                DataItemViewModel dataItem = new DataItemViewModel("DataItem " + i, i % 2 == 0);
                items.Add(dataItem);
            }

            m_items = items;
            RaisePropertyChanged(nameof(HierarchicalData));
        }

        private void OnDestroy()
        {
           
        }

        private void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs args)
        {
            HierarchicalDataChanged?.Invoke(this, args);
        }

        public HierarchicalDataItemFlags GetItemFlags(DataItemViewModel item)
        {
            return HierarchicalDataItemFlags.Default;
        }

        public DataItemViewModel GetParent(DataItemViewModel item)
        {
            return item.Parent;
        }

        public bool HasChildren(DataItemViewModel parent)
        {
            return parent.Children.Count > 0;
        }

        public IEnumerable<DataItemViewModel> GetChildren(DataItemViewModel parent)
        {
            if(parent == null)
            {
                return m_items;
            }
            return parent.Children;
        }

        public int IndexOf(DataItemViewModel parent, DataItemViewModel item)
        {
            if(parent == null)
            {
                return m_items.IndexOf(item);
            }
            return parent.Children.IndexOf(item);
        }

        public void SetParent(DataItemViewModel item, DataItemViewModel parent, int index)
        {
            m_items.Remove(item);
            
            item.Parent = parent;
            if (item.Parent == null)
            {
                m_items.Insert(index, item);
            }
            else
            {
                item.Parent.Children.Insert(index, item);
            }
        }

        public void SetParent(DataItemViewModel item, DataItemViewModel parent)
        {
            SetParent(item, parent, item.Children.Count);
            RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ParentChanged(parent, item));
        }

        public void Add(DataItemViewModel parent, DataItemViewModel item)
        {
            SetParent(item, parent, item.Children.Count);
            RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemAdded(parent, item));
        }

        private DataItemViewModel GetPrevSibling(DataItemViewModel item)
        {
            List<DataItemViewModel> siblings = m_items;
            if(item.Parent != null)
            {
                siblings = item.Parent.Children;
            }

            int index = siblings.IndexOf(item) - 1;
            return index >= 0 ? siblings[index] : null;
        }

        private DataItemViewModel GetNextSibling(DataItemViewModel item)
        {
            List<DataItemViewModel> siblings = m_items;
            if (item.Parent != null)
            {
                siblings = item.Parent.Children;
            }

            int index = siblings.IndexOf(item) + 1;
            return index < siblings.Count ? siblings[index] : null;
        }

        public void Insert(DataItemViewModel parent, DataItemViewModel item, int index)
        {
            SetParent(item, parent, index);
            RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemAdded(parent, item));

            DataItemViewModel sibling = GetNextSibling(item);
            if(sibling != null)
            {
                RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.NextSiblingsChanged(sibling, item));
            }
            else
            {
                sibling = GetPrevSibling(item);
                if(sibling != null)
                {
                    RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.PrevSiblingsChanged(sibling, item));
                }
            }
        }

        public void Remove(DataItemViewModel parent, DataItemViewModel item)
        {
            if (parent == null)
            {
                m_items.Remove(item);
            }
            else
            {
                parent.Children.Remove(item);
            }

            RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemRemoved(parent, item));
        }

        public void Expand(DataItemViewModel item)
        {
            
        }

        public void Collapse(DataItemViewModel item)
        {
            
        }

        public HierarchicalDataFlags GetFlags()
        {
            return HierarchicalDataFlags.Default;
        }

        public void Select(IEnumerable<DataItemViewModel> items)
        {
            SelectedItems = items;
        }
    }
}
