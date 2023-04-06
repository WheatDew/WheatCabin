using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Binding.Adapters
{
    [Adapter(typeof(LogType), typeof(Color), typeof(LogTypeAdapterOptions))]
    public class LogTypeToColorAdapter : IAdapter
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
                    return adapterOptions.ErrorColor;
                case LogType.Warning:
                    return adapterOptions.WarningColor;
                default:
                    return adapterOptions.InfoColor;
            }
        }
    }
}
