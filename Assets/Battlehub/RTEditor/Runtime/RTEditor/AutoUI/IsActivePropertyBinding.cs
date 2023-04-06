using UnityWeld.Binding;

namespace Battlehub.UIControls
{
    public class IsActivePropertyBinding : OneWayPropertyBinding
    {
        public bool IsActive
        {
            get { return gameObject.activeSelf; }
            set { gameObject.SetActive(value); }
        }
    }
}


