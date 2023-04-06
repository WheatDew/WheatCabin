using Battlehub.Utils;
using System;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTCommon
{
    public interface IResourcePreviewUtility
    {
        Vector3 PreviewObjectScale
        {
            get;
            set;
        }

        float PreviewScale
        {
            get;
            set;
        }

        int PreviewWidth
        {
            get;
            set;
        }

        int PreviewHeight
        {
            get;
            set;
        }

        Camera Camera
        {
            get;
        }

        bool CanCreatePreview(UnityObject obj);
        byte[] CreatePreviewData(UnityObject obj, bool instantiate = true);
        Texture2D CreatePreview(UnityObject obj, bool instantiate = true);
        Texture2D CreatePreview(GameObject obj, bool instantiate = true);

        [Obsolete("Use CreatePreivew(GameObject) instead")]
        Texture2D TakeSnapshot(GameObject go);
    }

    public class ResourcePreviewUtility : MonoBehaviour, IResourcePreviewUtility
    {
        private Shader m_unlitTexShader;

        [SerializeField]
        private ObjectToTexture m_objectToTextureCamera = null;

        [SerializeField]
        private GameObject m_fallbackPrefab = null;

        [SerializeField]
        private Vector3 m_previewObjectScale = new Vector3(1.0f, 1.0f, 1.0f);
        public Vector3 PreviewObjectScale
        {
            get { return m_previewObjectScale; }
            set { m_previewObjectScale = value; }
        }

        [SerializeField]
        private float m_previewScale = 1;
        public float PreviewScale
        {
            get { return m_previewScale; }
            set { m_previewScale = value; }
        }

        public int PreviewWidth
        {
            get { return m_objectToTextureCamera.snapshotTextureWidth; }
            set { m_objectToTextureCamera.snapshotTextureWidth = value; }
        }

        public int PreviewHeight
        {
            get { return m_objectToTextureCamera.snapshotTextureHeight; }
            set { m_objectToTextureCamera.snapshotTextureHeight = value; }
        }

        public virtual Camera Camera
        {
            get
            {
                if (m_objectToTextureCamera != null)
                {
                    return m_objectToTextureCamera.GetComponent<Camera>();
                }
                return null;
            }
        }

        private GameObject m_materialPreviewSphere;
        private MeshRenderer m_materialPreviewRenderer;

        protected virtual void Awake()
        {
            IRTE rte = IOC.Resolve<IRTE>();

            m_unlitTexShader = Shader.Find("Unlit/Texture");
            if (m_objectToTextureCamera == null)
            {
                GameObject objectToTextureGO = new GameObject("Object To Texture");
                objectToTextureGO.SetActive(false);
                objectToTextureGO.transform.SetParent(transform, false);

                Camera camera = objectToTextureGO.AddComponent<Camera>();
                camera.nearClipPlane = 0.01f;
                camera.orthographic = true;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0, 0, 0, 0);
                camera.stereoTargetEye = StereoTargetEyeMask.None;
                camera.cullingMask = 1 << rte.CameraLayerSettings.ResourcePreviewLayer;

                m_objectToTextureCamera = objectToTextureGO.AddComponent<ObjectToTexture>();
                m_objectToTextureCamera.objectImageLayer = rte.CameraLayerSettings.ResourcePreviewLayer;

                Light[] lights = FindObjectsOfType<Light>();
                for (int i = 0; i < lights.Length; ++i)
                {
                    lights[i].cullingMask &= ~(1 << rte.CameraLayerSettings.ResourcePreviewLayer);
                }

                GameObject lightGO = new GameObject("Directional light");
                lightGO.transform.SetParent(objectToTextureGO.transform, false);
                lightGO.layer = rte.CameraLayerSettings.ResourcePreviewLayer;
                lightGO.transform.rotation = Quaternion.Euler(30, 0, 0);

                Light light = lightGO.AddComponent<Light>();
                light.type = LightType.Directional;
                light.cullingMask = 1 << rte.CameraLayerSettings.ResourcePreviewLayer;
            }

            m_materialPreviewSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            m_materialPreviewSphere.transform.SetParent(transform, false);
            m_materialPreviewSphere.transform.position = Vector3.zero;
            m_materialPreviewSphere.SetActive(false);

            m_materialPreviewRenderer = m_materialPreviewSphere.GetComponent<MeshRenderer>();
        }

        private void OnDestroy()
        {
            if (m_materialPreviewSphere != null)
            {
                Destroy(m_materialPreviewSphere);
            }
        }

        public virtual bool CanCreatePreview(UnityObject obj)
        {
            return obj is GameObject || obj is Material || obj is Texture2D || obj is Sprite;
        }

        public virtual Texture2D CreatePreview(UnityObject obj, bool instantiate)
        {
            Texture2D previewTexture = null;
            if (obj is GameObject)
            {
                GameObject go = (GameObject)obj;
                previewTexture = CreatePreview(go, instantiate);
            }
            else if (obj is Material)
            {
                Material material = (Material)obj;
                Shader shader = material.shader;
                bool replaceParticlesShader = shader != null && shader.name.StartsWith("Particles/");
                if (replaceParticlesShader)
                {
                    material.shader = m_unlitTexShader;
                }

                m_materialPreviewRenderer.sharedMaterial = material;
                m_materialPreviewSphere.transform.position = Vector3.zero;
                previewTexture = CreatePreview(m_materialPreviewSphere, false);

                if (replaceParticlesShader)
                {
                    material.shader = shader;
                }
            }
            else if (obj is Texture2D)
            {
                Texture2D texture = (Texture2D)obj;
                bool isReadable = texture.isReadable;
                bool isSupportedFormat = texture.format == TextureFormat.ARGB32 ||
                                      texture.format == TextureFormat.RGBA32 ||
                                      texture.format == TextureFormat.RGB24 ||
                                      texture.format == TextureFormat.Alpha8;

                if (isReadable && isSupportedFormat)
                {
                    texture = Instantiate(texture);
                }
                else
                {
                    texture = texture.DeCompress();
                }

#if !UNITY_WEBGL
                float textureAspect = texture.width * m_objectToTextureCamera.snapshotTextureHeight / (float)Mathf.Max(1, texture.height * m_objectToTextureCamera.snapshotTextureWidth);
                TextureScale.Bilinear(texture, Mathf.RoundToInt(m_objectToTextureCamera.snapshotTextureWidth * textureAspect), m_objectToTextureCamera.snapshotTextureHeight);
#endif
                previewTexture = texture;
            }
            else if (obj is Sprite)
            {
                Sprite sprite = (Sprite)obj;
                previewTexture = FromSprite(sprite);
            }

            return previewTexture;
        }

        public byte[] CreatePreviewData(UnityObject obj, bool instantiate = true)
        {
            Texture2D texture = CreatePreview(obj, instantiate);

            byte[] result;
            if (texture != null)
            {
                result = texture.EncodeToPNG();
                Destroy(texture);
            }
            else
            {
                result = new byte[0];
            }

            return result;
        }


        public Texture2D CreatePreview(GameObject obj, bool instantiate)
        {
            m_objectToTextureCamera.gameObject.SetActive(true);
            Texture2D texture = m_objectToTextureCamera.TakeObjectSnapshot(
                obj,
                m_fallbackPrefab,
                m_objectToTextureCamera.defaultPosition,
                Quaternion.Euler(m_objectToTextureCamera.defaultRotation),
                m_previewObjectScale,
                m_previewScale,
                instantiate);
            m_objectToTextureCamera.gameObject.SetActive(false);
            return texture;
        }

        private Texture2D FromSprite(Sprite sprite)
        {
            if (sprite.texture != null && sprite.texture.isReadable)
            {
                Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                             (int)sprite.textureRect.y,
                                                             (int)sprite.textureRect.width,
                                                             (int)sprite.textureRect.height);
                texture.SetPixels(newColors);
#if UNITY_2021_2_OR_NEWER
                texture.Reinitialize(m_objectToTextureCamera.snapshotTextureWidth, m_objectToTextureCamera.snapshotTextureHeight);
#else
                texture.Resize(m_objectToTextureCamera.snapshotTextureWidth, m_objectToTextureCamera.snapshotTextureHeight);
#endif
                return texture;
            }

            return null;
        }

        [Obsolete]
        public Texture2D TakeSnapshot(GameObject go)
        {
            return CreatePreview(go, true);
        }
    }
}



