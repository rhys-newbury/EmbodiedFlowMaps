using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using VRTK;
using System;
using UnityEngine.UI;

public class PointableObject : MonoBehaviour
{

    private string name;
    public string parentName;
    protected GameObject wrapper;
    private GameObject go;
    private GameObject objToSpawn;
    private Vector3[] vertices3D;
    private Triangulator T;
    private Color color;
    private MeshRenderer meshRenderer;
    private float[] bounds;

    protected List<PointableObject> children = new List<PointableObject>();

    private readonly float ANGLE = 1 / Mathf.Sqrt(2);

    private float centroidX;

    private float centroidY;
    private float ZShift;

    private bool selected = false;

    public Mesh mesh;
    internal PointableObject parent;

    public void Start()
    {

    }

    public string getName()
    {
        return this.name;
    }

    public virtual int getLevel() {
        return 0;
    }


    public void onPointEnter(Action<string> change_text)
    {
        this.color.a = 0.3F;
        this.meshRenderer.material.color = this.color;
        change_text(this.getName());
        this.go.SetActive(true);
    }
   

    public void onPointLeave()
    {
        this.color.a = 1F;
        this.meshRenderer.material.color = this.color;
        this.go.SetActive(false);
    }

    internal bool onClick()
    {
        this.selected = !this.selected;

        if (this.selected)
        {
            createLine();
            return true;
        }
        else
        {
            destoryLine();
            return false;
        }

    }

    public bool isSelected()
    {
        return this.selected;
    }

    private void drawObject()
    {

        List<Vector3> verticesList = new List<Vector3>(vertices3D);
        List<Vector3> verticesExtrudedList = new List<Vector3>();
        List<int> indices = new List<int>();

        var originalVertexCount = vertices3D.Count();

        for (int i = 0; i < verticesList.Count; i++)
        {
            verticesExtrudedList.Add(new Vector3(verticesList[i].x, verticesList[i].y, 0.1F));
        }

        //add the extruded parts to the end of verteceslist
        verticesList.AddRange(verticesExtrudedList);

        for (int i = 0; i < originalVertexCount; i++)
        {

            int N = originalVertexCount;
            int i1 = i;
            int i2 = (i1 + 1) % N;
            int i3 = i1 + N;
            int i4 = i2 + N;

            indices.Add(i1);
            indices.Add(i3);
            indices.Add(i4);

            indices.Add(i1);
            indices.Add(i4);
            indices.Add(i2);

        }

        // Use the triangulator to get indices for creating triangles
        var indices2 = T.Triangulate().ToList();
        indices2.AddRange(indices);

        //Mesh mesh = new Mesh();
        this.mesh = new Mesh();

        this.mesh.vertices = verticesList.ToArray();
        this.mesh.triangles = indices2.ToArray();

        this.mesh.RecalculateNormals();
        this.mesh.RecalculateBounds();

        //Color meshColor = UnityEngine.Random.ColorHSV();
        Color meshColor = dataAccessor.getColour(dataAccessor.getData(this.name));

        var colors = Enumerable.Range(0, verticesList.Count)
         .Select(i => meshColor)
         .ToArray();

        // Set up game object with mesh;
        meshRenderer = objToSpawn.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        this.color = meshColor;
        meshRenderer.material.color = this.color;

        var filter = objToSpawn.AddComponent<MeshFilter>();
        filter.mesh = this.mesh;

        var collider = objToSpawn.AddComponent<MeshCollider>();
        collider.sharedMesh = this.mesh;

    }

    internal void deselect()
    {
        this.selected = false;
        destoryLine();
    }

    internal virtual void destory()
    {
        GameObject.Destroy(this.wrapper);
    }

    public Mesh getMesh()
    {
        return this.mesh;
    }

    public void SetPositionAndRotation(Vector3 pos, Quaternion angle)
    {
        this.transform.SetPositionAndRotation(pos, angle);

        float centerX = (this.bounds[0] + this.bounds[2]) / 2F;
        float centerY = (this.bounds[1] + this.bounds[3]) / 2F;

        this.wrapper.transform.SetPositionAndRotation(this.transform.TransformPoint(new Vector3(centerX, centerY, -0.05F)), new Quaternion(0, 0, 0, 1));

        this.objToSpawn.transform.SetParent(this.wrapper.transform);

    }

    internal void constructor(Vector2[] points, string name, GameObject objToSpawn, float[] bounds, string parentName)
    {
        T = new Triangulator(points);
        vertices3D = System.Array.ConvertAll<Vector2, Vector3>(points, v => v);
   
   
        this.name = name;
        this.parentName = parentName;
        this.bounds = bounds;

        this.wrapper = new GameObject();

        this.go = Instantiate(Resources.Load("ObjectTooltip")) as GameObject;
        go.transform.parent = this.wrapper.transform;
        VRTK_ObjectTooltip tooltip = go.GetComponent<VRTK_ObjectTooltip>() as VRTK_ObjectTooltip;
        tooltip.alwaysFaceHeadset = true;
        tooltip.displayText = this.name;
        this.go.SetActive(false);



        this.objToSpawn = objToSpawn;
        this.objToSpawn.name = name;


        this.drawObject();
    }



    public void setParent(Transform parent)
    {


 
        this.wrapper.transform.SetParent(parent);

    }

    internal void deleteChildren()
    {
        if (this.children.Count > 0)
        {
            var filtered = this.children.Where(x => x != null).ToList();
            var parent = filtered[0].transform.parent.transform.parent.gameObject;
            filtered.ForEach(x => x.delete());
            GameObject.Destroy(parent);

        }
    }

    public void createLine()
    {
        var line = objToSpawn.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        var tempVertices3D = vertices3D.ToList();
        tempVertices3D.Add(vertices3D[0]);
        //Chuck her in front of the country so its visible
        tempVertices3D = tempVertices3D.Select(x => new Vector3(x.x, x.y, -0.01F)).ToList();
        var FinaltempVertices3D = tempVertices3D.ToArray();
        line.positionCount = FinaltempVertices3D.Count();
        line.SetPositions(FinaltempVertices3D);

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.red;
        line.endColor = Color.yellow;
        line.startWidth = 0.01F;
        line.endWidth = 0.01F;
        line.alignment = LineAlignment.TransformZ;

    }

    public virtual Quaternion getAngle()
    {
        return new Quaternion(0, 1, 0, 0);
    }

    public virtual Vector3 getTranslation(float x, float y)
    {
        return new Vector3(x, -y, 0);
    }

    public virtual Quaternion getFinalAngle()
    {
        return new Quaternion(0,0,0,1);
    }



    public void destoryLine()
    {
       
        this.selected = false;
        var line = objToSpawn.GetComponent<LineRenderer>();
        Destroy(line);


    }

    internal void addChild(PointableObject gameObject)
    {
        this.children.Add(gameObject);
            
    }

    internal virtual void delete()
    {
            this.children.ForEach(x => x.delete());
            this.children.Clear();
            GameObject.Destroy(this.gameObject);
            GameObject.Destroy(this.wrapper);
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
