using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public interface ITerrainCutoutMaskRenderer
    {
        int ObjectImageLayer
        {
            get;
            set;
        }

        Texture2D CreateMask(TerrainData terrainData, GameObject[] gameObjects, bool terrainScaleMask = true);
    }

    [DefaultExecutionOrder(-90)]
    public class TerrainCutoutMaskRenderer : MonoBehaviour, ITerrainCutoutMaskRenderer
    {
        // Use this for initialization
        public Camera Camera;
        public int m_objectImageLayer = 15;
        public int ObjectImageLayer
        {
            get { return m_objectImageLayer; }
            set
            {
                m_objectImageLayer = value;
                Camera.cullingMask = 1 << m_objectImageLayer;
            }
        }

        public bool DestroyScripts = true;
        public int snapshotTextureWidth = 513;
        public int snapshotTextureHeight = 513;
        public Vector3 defaultPosition = new Vector3(0, 0, 0);
        public Vector3 defaultRotation = new Vector3(-90, 0, 0);
        public Vector3 defaultScale = new Vector3(1, 1, 1);

        private void Awake()
        {
            if (Camera == null)
            {
                Camera = gameObject.AddComponent<Camera>();
                Camera.orthographic = true;
                Camera.clearFlags = CameraClearFlags.Depth;
                Camera.stereoTargetEye = StereoTargetEyeMask.None;
            }
          
            Camera.enabled = false;

            IOC.RegisterFallback<ITerrainCutoutMaskRenderer>(this);
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<ITerrainCutoutMaskRenderer>(this);
        }

        private void SetLayerRecursively(GameObject o, int layer)
        {
            foreach (Transform t in o.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = layer;
            }
        }

        public Texture2D CreateMask(TerrainData terrainData, GameObject[] gameObjects, bool terrainScaleMask = true)
        {
            return CreateMask(terrainData, gameObjects, defaultPosition, Quaternion.Euler(defaultRotation), defaultScale, terrainScaleMask);
        }

        public Texture2D CreateMask(TerrainData terrainData, GameObject[] gameObjects, Vector3 position, Quaternion rotation, Vector3 scale, bool terrainScaleMask)
        {
            if(gameObjects == null)
            {
                gameObjects = new GameObject[0];
            }

            // validate properties
            if (Camera == null)
            {
                throw new System.InvalidOperationException("Object Image Camera must be set");
            }

            if (m_objectImageLayer < 0 || m_objectImageLayer > 31)
            {
                throw new System.InvalidOperationException("Object Image Layer must specify a valid layer between 0 and 31");
            }

            Transform[] oldParents = new Transform[gameObjects.Length];
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject g = gameObjects[i];
                oldParents[i] = g.transform.parent;
            }

            GameObject root = new GameObject("Root");
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject g = gameObjects[i];
                g.transform.SetParent(root.transform, true);
            }

            bool isActive = root.activeSelf;
            root.SetActive(false);

            GameObject go = Instantiate(root, position, rotation * Quaternion.Inverse(root.transform.rotation)) as GameObject;
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject g = gameObjects[i];
                g.transform.SetParent(oldParents[i], true);
            }
            if (DestroyScripts)
            {
                MonoBehaviour[] scripts = go.GetComponentsInChildren<MonoBehaviour>(true);
                for (int i = 0; i < scripts.Length; ++i)
                {
                    if (scripts[i] == null)
                    {
                        continue;
                    }

                    if (scripts[i].GetType().FullName.StartsWith("UnityEngine"))
                    {
                        continue;
                    }
                    DestroyImmediate(scripts[i]);
                }
            }

            root.SetActive(isActive);
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            Texture2D texture = null;
            if (renderers.Length != 0)
            {
                
                float fov = Camera.fieldOfView * Mathf.Deg2Rad;
                float objSize;
                if(terrainScaleMask)
                {
                    objSize = Mathf.Max(terrainData.size.x / 2, terrainData.size.z / 2);
                }
                else
                {
                    Bounds bounds = go.CalculateBounds();
                    objSize = Mathf.Max(bounds.extents.y, bounds.extents.x, bounds.extents.z);
                    position += bounds.center;
                }

                float distance = Mathf.Abs(objSize / Mathf.Sin(fov / 2.0f));

                go.SetActive(true);
                for (int i = 0; i < renderers.Length; ++i)
                {
                    renderers[i].gameObject.SetActive(true);
                }

                Camera.transform.position = position - distance * Camera.transform.forward;
                Camera.orthographicSize = objSize;

                // set the layer so the render to texture camera will see the object 
                SetLayerRecursively(go, m_objectImageLayer);

                // get a temporary render texture and render the camera
                texture = RenderTexture();
            }
            else
            {
                texture = RenderTexture();
            }

            DestroyImmediate(go);
            DestroyImmediate(root);

            return texture;
        }

        private Texture2D RenderTexture()
        {
            Texture2D texture;
            Camera.targetTexture = UnityEngine.RenderTexture.GetTemporary(snapshotTextureWidth, snapshotTextureHeight, 24);
            Camera.enabled = true;
            Camera.Render();
            Camera.enabled = false;
            // activate the render texture and extract the image into a new texture
            RenderTexture saveActive = UnityEngine.RenderTexture.active;
            UnityEngine.RenderTexture.active = Camera.targetTexture;

            texture = new Texture2D(Camera.targetTexture.width, Camera.targetTexture.height);
            texture.ReadPixels(new Rect(0, 0, Camera.targetTexture.width, Camera.targetTexture.height), 0, 0);

            texture.Apply();
            UnityEngine.RenderTexture.active = saveActive;

            // clean up after ourselves
            UnityEngine.RenderTexture.ReleaseTemporary(Camera.targetTexture);
            return texture;
        }
    }
}

