using System;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class MenuItemViewModel 
    {
        public class ValidationArgs
        {
            public bool IsValid;
        }

        public Action<string> Action;
        public Action<ValidationArgs> Validate;

        public int TypeIndex;
        public string Path;

        [Binding]
        public string Text
        {
            get;
            set;
        }

        [Binding]
        public Sprite Icon
        {
            get;
            set;
        }

        [Binding]
        public string Command
        {
            get;
            set;
        }

    }
}
