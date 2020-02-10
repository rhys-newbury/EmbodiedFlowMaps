using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;

namespace Obi
{
    [AddComponentMenu("Physics/Obi/Obi Rope Line Renderer", 884)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(ObiPathSmoother))]
    public class ObiRopeLineRenderer : MonoBehaviour
    {
        static ProfilerMarker m_UpdateLineRopeRendererChunksPerfMarker = new ProfilerMarker("UpdateLineRopeRenderer");

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();
        private List<Vector4> tangents = new List<Vector4>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<Color> vertColors = new List<Color>();
        private List<int> tris = new List<int>();

        ObiRopeBase rope;
        ObiPathSmoother smoother;

        [HideInInspector] [NonSerialized] public Mesh lineMesh;

        [Range(0, 1)]
        public float uvAnchor = 0;                  /**< Normalized position of texture coordinate origin along rope.*/

        public Vector2 uvScale = Vector2.one;       /**< Scaling of uvs along rope.*/

        public bool normalizeV = true;

        public float thicknessScale = 0.8f;  /**< Scales section thickness.*/

        void OnEnable()
        {

            CreateMeshIfNeeded();

            Camera.onPreCull += UpdateRenderer;

            rope = GetComponent<ObiRopeBase>();
            smoother = GetComponent<ObiPathSmoother>();
        }

        void OnDisable()
        {
            Camera.onPreCull -= UpdateRenderer;
            GameObject.DestroyImmediate(lineMesh);
        }

        private void CreateMeshIfNeeded()
        {
            if (lineMesh == null)
            {
                lineMesh = new Mesh();
                lineMesh.name = "extrudedMesh";
                lineMesh.MarkDynamic();
                GetComponent<MeshFilter>().mesh = lineMesh;
            }
        }

        public void UpdateRenderer(Camera camera)
        {
            using (m_UpdateLineRopeRendererChunksPerfMarker.Auto())
            {

                if (camera == null || !rope.gameObject.activeInHierarchy)
                    return;

                CreateMeshIfNeeded();
                ClearMeshData();

                float actualToRestLengthRatio = smoother.SmoothLength / rope.restLength;

                float vCoord = -uvScale.y * rope.restLength * uvAnchor; // v texture coordinate.
                int sectionIndex = 0;

                Vector3 localSpaceCamera = rope.transform.InverseTransformPoint(camera.transform.position);

                // for closed curves, last frame of the last curve must be equal to first frame of first curve.
                Vector3 firstTangent = Vector3.forward;

                Vector4 texTangent = Vector4.zero;
                Vector2 uv = Vector2.zero;

                for (int c = 0; c < smoother.smoothChunks.Count; ++c)
                {

                    ObiList<ObiPathFrame> curve = smoother.smoothChunks[c];

                    for (int i = 0; i < curve.Count; ++i)
                    {

                        // Calculate previous and next curve indices:
                        int prevIndex = Mathf.Max(i - 1, 0);

                        // update start/end prefabs:
                        if (c == 0 && i == 0)
                        {
                            // store first tangent of the first curve (for closed ropes):
                            firstTangent = curve[i].tangent;
                        }

                        // advance v texcoord:
                        vCoord += uvScale.y * (Vector3.Distance(curve[i].position, curve[prevIndex].position) /
                                               (normalizeV ? smoother.SmoothLength : actualToRestLengthRatio));

                        // calculate section thickness (either constant, or particle radius based):
                        float sectionThickness = curve[i].thickness * thicknessScale;

                        Vector3 normal = curve[i].position - localSpaceCamera;
                        normal.Normalize();

                        Vector3 bitangent = Vector3.Cross(normal, curve[i].tangent);
                        bitangent.Normalize();

                        vertices.Add(curve[i].position + bitangent * sectionThickness);
                        vertices.Add(curve[i].position - bitangent * sectionThickness);

                        normals.Add(-normal);
                        normals.Add(-normal);

                        texTangent = -bitangent;
                        texTangent.w = 1;
                        tangents.Add(texTangent);
                        tangents.Add(texTangent);

                        vertColors.Add(curve[i].color);
                        vertColors.Add(curve[i].color);

                        uv.Set(0, vCoord);
                        uvs.Add(uv);
                        uv.Set(1, vCoord);
                        uvs.Add(uv);

                        if (i < curve.Count - 1)
                        {
                            tris.Add(sectionIndex * 2);
                            tris.Add((sectionIndex + 1) * 2);
                            tris.Add(sectionIndex * 2 + 1);

                            tris.Add(sectionIndex * 2 + 1);
                            tris.Add((sectionIndex + 1) * 2);
                            tris.Add((sectionIndex + 1) * 2 + 1);
                        }

                        sectionIndex++;
                    }

                }

                CommitMeshData();
            }
        }

        private void ClearMeshData()
        {
            lineMesh.Clear();
            vertices.Clear();
            normals.Clear();
            tangents.Clear();
            uvs.Clear();
            vertColors.Clear();
            tris.Clear();
        }

        private void CommitMeshData()
        {
            lineMesh.SetVertices(vertices);
            lineMesh.SetNormals(normals);
            lineMesh.SetTangents(tangents);
            lineMesh.SetColors(vertColors);
            lineMesh.SetUVs(0, uvs);
            lineMesh.SetTriangles(tris, 0, true);
        }
    }
}


