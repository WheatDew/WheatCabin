using Battlehub.Utils;
using System;
using System.Security.Cryptography;
using UnityEngine;

namespace Battlehub.Wireframe
{
    public class InstanceIDToColor : MonoBehaviour
    {
        private static readonly MD5 s_hashAlgo = MD5.Create();

        public Color Convert(int instanceId)
        {
            instanceId = Mathf.Abs(BitConverter.ToInt32(s_hashAlgo.ComputeHash(BitConverter.GetBytes(instanceId)), 0));

            Color32[] colors = Colors.Kellys;
            return colors[instanceId % colors.Length];
        }

    }

}
