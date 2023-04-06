using UnityEngine;

namespace Battlehub.Utils
{   
    public class FieldDisplayNameAttribute : PropertyAttribute
    {
        public string NewName { get; private set; }
        public FieldDisplayNameAttribute(string name)
        {
            NewName = name;
        }
    }
}
