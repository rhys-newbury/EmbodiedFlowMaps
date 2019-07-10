using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using VRTK;
using System;


public class PointableObject : MonoBehaviour
{

    private string name;
    private GameObject objToSpawn;
    private Vector3[] vertices3D;
    private Triangulator T;
    private Color color;
    private MeshRenderer meshRenderer;
    private float[] bounds;

    public int gridX;
    public int gridY;
    private float centroidX;

    private float centroidY;
    private float ZShift;

    private bool selected = false;

    public string getName()
    {
        return this.name;
     }


    public void onPointEnter()
    {
        if (this.selected)
        {
            this.color.a = 0.8F;
            this.meshRenderer.material.color = this.color;
        }

        else
        {
            this.color.a = 0.5F;
            this.meshRenderer.material.color = this.color;
        }
    }
   

    public void onPointLeave()
    {
        if (this.selected)
        {
            this.color.a = 1F;
            this.meshRenderer.material.color = this.color;
        }

        else
        {
            this.color.a = 0.75F;
            this.meshRenderer.material.color = this.color;
        }

    }

    internal bool onClick()
    {
        //if (this.color.r != 0)
        //{
        //    this.color.r = 0F;
        //    this.color.b = 255F;
        //    this.meshRenderer.material.color = this.color;
        //    return true;
        //}
        //else
        //{
        //    this.color.r = 255F;
        //    this.color.b = 0F;
        //    this.meshRenderer.material.color = this.color;
        //    return false;

        //}

        if (this.selected)
        {
            this.color.a = 1F;
            this.meshRenderer.material.color = this.color;
        }

        else
        {
            this.color.a = 0.75F;
            this.meshRenderer.material.color = this.color;
        }
        return this.selected;
    }

    private void drawObject()
    {
        
        // Use the triangulator to get indices for creating triangles
        var indices = T.Triangulate();

        Color meshColor = UnityEngine.Random.ColorHSV();
        // Generate a color for each vertex
        var colors = Enumerable.Range(0, vertices3D.Length)
            .Select(i => meshColor)
            .ToArray();

        // Create the mesh
        var mesh = new Mesh
        {
            vertices = vertices3D,
            triangles = indices
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();



        // Set up game object with mesh;
        meshRenderer = objToSpawn.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        this.color = Color.red;
        meshRenderer.material.color = this.color;

        var filter = objToSpawn.AddComponent<MeshFilter>();
        filter.mesh = mesh;

        var collider = objToSpawn.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;

        objToSpawn.transform.SetPositionAndRotation(new Vector3(-this.centroidX+1.5F, -this.centroidY+2.5F, ZShift), new Quaternion(0, 0, 0, 1));

    }

    internal void constructor(Vector2[] points, string name, GameObject objToSpawn, float[] bounds)
    {
        
        T = new Triangulator(points);
        vertices3D = System.Array.ConvertAll<Vector2, Vector3>(points, v => v);
        var vertices3D2 = points.Select(x => new Vector3(x.x, x.y, 5F));
        var y = vertices3D.ToList();
        y.AddRange(vertices3D2);
        vertices3D = y.ToArray();



        this.name = name;
        this.bounds = bounds;
        this.objToSpawn = objToSpawn;
        this.objToSpawn.name = name;
 

        this.drawObject();
    }

    public void setParent(Transform parent)
    {
        objToSpawn.transform.SetParent(parent);
    }

    public float getMinX()
    {
        return bounds[2];
    }
    public float getMaxX()
    {
        return bounds[0];
    }

    internal void setOrigin(float centroidX, float centroidY, float ZShift)
    {
        this.centroidX = centroidX;
        this.centroidY = centroidY;
        this.ZShift = ZShift;
    }

    public float getMaxY()
    {
        return bounds[1];
    }
    public float getMinY()
    {
        return bounds[3];
    }

}




public class Triangulator
{
    protected List<Vector2> m_points = new List<Vector2>();

    public Triangulator(Vector2[] points)
    {
        m_points = new List<Vector2>(points);
    }

    public int[] Triangulate()
    {
        List<int> indices = new List<int>();

        int n = m_points.Count;
        if (n < 3)
            return indices.ToArray();

        int[] V = new int[n];
        if (Area() > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;
        for (int v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                return indices.ToArray();

            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;

            if (Snip(u, v, w, nv, V))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices.ToArray();
    }

    private float Area()
    {
        int n = m_points.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = m_points[p];
            Vector2 qval = m_points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return (A * 0.5f);
    }

    private bool Snip(int u, int v, int w, int n, int[] V)
    {
        int p;
        Vector2 A = m_points[V[u]];
        Vector2 B = m_points[V[v]];
        Vector2 C = m_points[V[w]];
        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;
        for (p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector2 P = m_points[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }

    private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
}
