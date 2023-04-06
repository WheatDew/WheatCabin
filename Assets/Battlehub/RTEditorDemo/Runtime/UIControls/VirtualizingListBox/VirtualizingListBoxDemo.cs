using UnityEngine;
using UnityWeld.Binding;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Battlehub.UIControls
{
    [Binding]
    public class VirtualizingListBoxDemo : MonoBehaviour, INotifyPropertyChanged
    {
        [Adapter(typeof(IEnumerable), typeof(IEnumerable<Item>))]
        public class IEnumerableToIEnumerableOfItemAdapter : IAdapter
        {
            public object Convert(object valueIn, AdapterOptions options)
            {
                if (valueIn == null)
                {
                    return null;
                }

                IEnumerable enumerable = (IEnumerable)valueIn;
                return enumerable.Cast<Item>();
            }
        }

        [Binding]
        public class Item : INotifyPropertyChanged
        {
            private int m_index;
            [Binding]
            public int Index
            {
                get { return m_index; }
                set
                {
                    m_index = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Index)));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public override string ToString()
            {
                return Index.ToString();
            }
        }

        private IEnumerable<Item> m_items;

        [Binding]
        public IEnumerable<Item> Items
        {
            get { return m_items; }
            set 
            {
                m_items = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));

                if(m_items != null)
                {
                    StringBuilder order = new StringBuilder();

                    foreach (Item item in Items)
                    {
                        order.Append(item.Index).Append(" ");
                    }

                    Debug.Log(order);
                }
            }
        }

        private IEnumerable<Item> m_selectedItems;

        [Binding]
        public IEnumerable<Item> SelectedItems
        {
            get { return m_selectedItems; }
            set
            {
                m_selectedItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItems)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void Start()
        {
            List<Item> items = new List<Item>();
            for (int i = 0; i < 21; ++i)
            {
                Item item = new Item
                {
                    Index = i,
                };

                items.Add(item);
            }

            Items = items;
        }
    }
}


