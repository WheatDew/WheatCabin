using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NyaTerrain : WDEntity
{
    [HideInInspector]public NavMeshSurface meshSurface;

    private void Start()
    {
        meshSurface = gameObject.AddComponent<NavMeshSurface>();
        BuildNavMesh();
    }

    public void BuildNavMesh()
    {
        meshSurface.BuildNavMesh();
    }

    public void SetMeshSurface()
    {
        meshSurface.agentTypeID=0;
    }

}
