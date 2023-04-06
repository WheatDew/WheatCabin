using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [Adapter(typeof(int), typeof(string))]
    public class ConsoleCounterToStringAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            int value = (int)valueIn;
            if(value >= 100)
            {
                return "99+";
            }
            return value.ToString();
        }
    }
}

