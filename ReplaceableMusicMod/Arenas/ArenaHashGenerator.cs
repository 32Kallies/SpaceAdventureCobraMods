using System.Collections.Generic;
using MusicReplacer.Utilities;
using UnityEngine;

namespace MusicReplacer.Arenas;

public static class ArenaHashGenerator
{
    public static long Compute(GameObject root)
    {
        return unchecked((long)ComputeTransformRecursive(root.transform, true));
    }

    private static ulong ComputeTransformRecursive(Transform transform, bool root)
    {
        IHashWriter hash = new BasicHashWriter
        {
            FloatPrecision = 100
        };

        // root transform

        if (root)
        {
            hash.Write(transform.position);
            hash.Write(transform.rotation);
            hash.Write(transform.lossyScale);
        }
        else
        {
            hash.Write(transform.localPosition);
            hash.Write(transform.localRotation);
            hash.Write(transform.localScale);
        }

        // components on root

        foreach (Component component in transform.GetComponents<Component>())
        {
            if (component == null)
                continue;

            switch (component)
            {
                case MeshFilter mf:
                    HashMeshFilter(hash, mf);
                    break;

                case MeshRenderer mr:
                    HashRenderer(hash, mr);
                    break;

                case SkinnedMeshRenderer smr:
                    HashSkinnedRenderer(hash, smr);
                    break;

                case BoxCollider box:
                    HashBoxCollider(hash, box);
                    break;

                case SphereCollider sphere:
                    HashSphereCollider(hash, sphere);
                    break;

                case CapsuleCollider capsule:
                    HashCapsuleCollider(hash, capsule);
                    break;

                case MeshCollider mesh:
                    HashMeshCollider(hash, mesh);
                    break;

                default:
                    hash.Write(component.GetType().FullName);
                    break;
            }
        }

        // recursively for all children

        List<ulong> childHashes = new(transform.childCount);

        foreach (Transform child in transform)
            childHashes.Add(ComputeTransformRecursive(child, false));

        childHashes.Sort();

        foreach (ulong childHash in childHashes)
            hash.Write(childHash);

        return hash.ToUInt64();
    }
    
    // mesh components

    private static void HashMeshFilter(IHashWriter hash, MeshFilter filter)
    {
        var mesh = filter.sharedMesh;
        
        if (mesh == null)
            return;

        hash.Write(mesh.vertexCount);
        hash.Write(mesh.subMeshCount);

        var bounds = mesh.bounds;

        hash.Write(bounds.center);
        hash.Write(bounds.size);
    }

    private static void HashSkinnedRenderer(IHashWriter hash, SkinnedMeshRenderer renderer)
    {
        HashRenderer(hash, renderer);

        if (!renderer.sharedMesh)
            return;

        hash.Write(renderer.sharedMesh.vertexCount);
        hash.Write(renderer.sharedMesh.subMeshCount);
    }

    private static void HashRenderer(IHashWriter hash, Renderer renderer)
    {
        hash.Write(renderer.sharedMaterials.Length);

        foreach (Material mat in renderer.sharedMaterials)
        {
            if (!mat)
                continue;

            hash.Write(mat.shader ? mat.shader.name : "");
        }
    }

    // colliders

    private static void HashBoxCollider(IHashWriter hash, BoxCollider c)
    {
        hash.Write(c.center);
        hash.Write(c.size);
    }

    private static void HashSphereCollider(IHashWriter hash, SphereCollider c)
    {
        hash.Write(c.center);
        hash.Write(c.radius);
    }

    private static void HashCapsuleCollider(IHashWriter hash, CapsuleCollider c)
    {
        hash.Write(c.center);
        hash.Write(c.radius);
        hash.Write(c.height);
        hash.Write(c.direction);
    }

    private static void HashMeshCollider(IHashWriter hash, MeshCollider c)
    {
        if (!c.sharedMesh)
            return;

        hash.Write(c.sharedMesh.vertexCount);
        hash.Write(c.sharedMesh.subMeshCount);
    }
}