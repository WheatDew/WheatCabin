using UnityEngine;

namespace Battlehub.RTEditor.Binding.Adapters
{
    public class IsActiveProperty : MonoBehaviour
    {
        public bool IsActive
        {
            get { return gameObject.activeSelf; }
            set { gameObject.SetActive(value); }
        }
    }

}
