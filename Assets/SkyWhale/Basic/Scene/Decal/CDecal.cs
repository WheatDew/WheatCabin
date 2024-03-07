using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class CDecal : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
        //  GetComponent<Camera>().depthTextureMode |= DepthTextureMode.DepthNormals;
    }
}
