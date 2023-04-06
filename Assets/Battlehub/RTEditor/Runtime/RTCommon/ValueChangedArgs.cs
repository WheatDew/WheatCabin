using System;
namespace Battlehub.RTCommon
{
    public class ValueChangedArgs<T> : EventArgs
    {
        public T OldValue
        {
            get;
            set;
        }

        public T NewValue
        {
            get;
            set;
        }

        public ValueChangedArgs()
        {
        }

        public ValueChangedArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public void Reset()
        {
            OldValue = default;
            NewValue = default;
        }
    }
}
