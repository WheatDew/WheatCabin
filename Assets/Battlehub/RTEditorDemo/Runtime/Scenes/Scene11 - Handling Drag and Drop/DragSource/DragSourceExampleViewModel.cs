using Battlehub.RTEditor.ViewModels;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Examples.Scene11.ViewModels
{
    [Binding]
    public class DragSourceExampleViewModel : ViewModel
    {
        private object[] m_dragItems;
        private int m_index;

        [Binding]
        public object[] DragItems
        {
            get { return m_dragItems; }
            set 
            {
                if (m_dragItems != value)
                {
                    m_dragItems = value;
                    RaisePropertyChanged(nameof(DragItems));
                }
            }
        }

        [Binding]
        public void OnBeginDrag()
        {
            DragItems = new[] { $"Drag Item {++m_index}"  };
        }

        [Binding]
        public void OnEndDrag()
        {
            DragItems = null;
        }
    }
}


