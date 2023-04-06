using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class LayersInfo : ScriptableObject
    {
        [Serializable]
        public class Layer
        {
            public string Name;
            public int Index;
            public Layer(string name, int index)
            {
                Name = name;
                Index = index;
            }
        }

        public List<Layer> Layers;
    }
}
