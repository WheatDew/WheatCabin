using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [Adapter(typeof(LogType), typeof(Sprite), typeof(LogTypeAdapterOptions))]
    public class LogTypeToSpriteAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            LogTypeAdapterOptions adapterOptions = (LogTypeAdapterOptions)options;
            LogType logType = (LogType)valueIn;
            switch (logType)
            {
                case LogType.Assert:
                case LogType.Error:
                case LogType.Exception:
                    return adapterOptions.ErrorIcon;
                case LogType.Warning:
                    return adapterOptions.WarningIcon;
                default:
                    return adapterOptions.InfoIcon;
            }
        }
    }
}
