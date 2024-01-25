/// <summary>
/// Project : Easy Build System
/// Class : RenderExtension.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Extensions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

namespace EasyBuildSystem.Features.Runtime.Extensions
{
    public static class RenderExtension
    {
        public static void ChangeMaterialColorRecursively(Renderer[] renderers, Color color, Renderer[] ignoreRenderers)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    if (!ignoreRenderers.Contains(renderers[i]))
                    {
                        for (int x = 0; x < renderers[i].materials.Length; x++)
                        {
                            if (GraphicsSettings.currentRenderPipeline)
                            {
                                renderers[i].materials[x].SetColor("_BaseColor", color);
                            }
                            else
                            {
                                renderers[i].materials[x].SetColor("_Color", color);
                            }
                        }
                    }
                }
            }
        }

        public static void ChangeMaterialRecursively(Renderer[] renderers, Material material, Renderer[] ignoreRenderers)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].enabled)
                {
                    if (!ignoreRenderers.Contains(renderers[i]))
                    {
                        Material[] materials = new Material[renderers[i].sharedMaterials.Length];

                        for (int x = 0; x < renderers[i].sharedMaterials.Length; x++)
                        {
                            materials[x] = material;
                        }

                        renderers[i].sharedMaterials = materials;
                    }
                }
            }
        }

        public static void ChangeMaterialRecursively(Renderer[] renderers, Dictionary<Renderer, Material[]> materials, Renderer[] ignoreRenderers)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].enabled)
                {
                    if (!ignoreRenderers.Contains(renderers[i]))
                    {
                        Material[] copySharedMaterials = renderers[i].materials;

                        for (int x = 0; x < copySharedMaterials.Length; x++)
                        {
                            copySharedMaterials[x] = materials[renderers[i]][x];
                        }

                        renderers[i].materials = copySharedMaterials;
                    }
                }
            }
        }
    }
}