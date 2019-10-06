using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VRTK;
using System;

public class PointableObject : Pointable
{

    private string name;
    public string parentName;
    protected GameObject wrapper;
    protected GameObject go;
    private GameObject objToSpawn;
    private Vector3[] vertices3D;
    public Action<bool> reportGrabbed;
    private Triangulator T;
    private Color color;
    private MeshRenderer meshRenderer;
    private float centerX, centerY;

    protected List<PointableObject> children = new List<PointableObject>();


    private float centroidX;

    private float centroidY;
    private float zShift;

    private bool selected;

    public Mesh mesh;
    internal PointableObject parent;
    public Dictionary<string, PointableObject> siblings = new Dictionary<string, PointableObject>();

    public void Start()
    {

    }

    public virtual void GetInternalFlows(PointableObject origin)
    {

    }

    public MapController getMapContainer()
    {
        return this.transform.root.GetComponent<MapController>();
              
    }

    public override void OnThrow()
    {

        int level = -1;
        //On Throw Delete each item in children
        //Deselect the parent
        foreach (var item in this.GetComponentsInChildren<PointableObject>())
        {
            item.Delete();
            item.parent.Deselect();
            level = level == -1 ? item.GetLevel() : level;
        }
        //Do not destroy country
        if (level > 0) Destroy(this);
    }


 

    public override void OnGripPressed()
    {
        this.reportGrabbed(true);
    }

    public override void OnTriggerPressed()
    {

        if (!this.IsSelected())
        {

            //Create the gameObject for the map and then Draw it.
            GameObject mapGameObject = new GameObject();
            mapGameObject.transform.parent = this.transform.root;
            MapContainer main = mapGameObject.AddComponent(typeof(MapContainer)) as MapContainer;
            main?.Draw(this, this.GetLevel() + 1);

            this.selected = true;
            CreateLine();
            this.GetInternalFlows(this);
        }
        else
        {
            //On Deselect Remove it from the current list and Delete the object and its children 
            this.selected = false;
            DestoryLine();


            this.DeleteChildren();
        }

    }


    public virtual void RemoveLines()
    {

    }

    public virtual void AddLine(GameObject line)
    {

    }

    public string GetName()
    {
        return this.name;
    }

    public virtual int GetLevel() {
        return 0;
    }


    public override void OnPointerEnter(Action<string> changeText)
    {
        this.color.a = 0.3F;
        this.meshRenderer.material.color = this.color;
        changeText(this.GetName());
        this.go.SetActive(true);
    }
   

    public override void OnPointerLeave()
    {
        this.color.a = 1F;
        this.meshRenderer.material.color = this.color;
        this.go.SetActive(false);
    }

    public bool IsSelected()
    {
        return this.selected;
    }

    private void DrawObject()
    {

        List<Vector3> verticesList = new List<Vector3>(vertices3D);
        List<Vector3> verticesExtrudedList = new List<Vector3>();
        List<int> indices = new List<int>();

        var originalVertexCount = vertices3D.Length;

        for (int i = 0; i < verticesList.Count; i++)
        {
            verticesExtrudedList.Add(new Vector3(verticesList[i].x, verticesList[i].y, 0.1F));
        }

        //add the extruded parts to the end of vertices list
        verticesList.AddRange(verticesExtrudedList);

        for (int i = 0; i < originalVertexCount; i++)
        {

            int n = originalVertexCount;
            int i1 = i;
            int i2 = (i1 + 1) % n;
            int i3 = i1 + n;
            int i4 = i2 + n;

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
        this.mesh = new Mesh
        {
            vertices = verticesList.ToArray(),
            triangles = indices2.ToArray()
        };

        this.mesh.RecalculateNormals();
        this.mesh.RecalculateBounds();

        //Color meshColor = UnityEngine.Random.ColorHSV();
        Color meshColor = this.getMapContainer().getCountryColour(this.getMapContainer().getData(this.name, this.parentName));

        // Set up game object with mesh;
        meshRenderer = objToSpawn.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        this.color = meshColor;
        meshRenderer.material.color = this.color;

        var filter = objToSpawn.AddComponent<MeshFilter>();
        filter.mesh = this.mesh;

        var mapcollider = objToSpawn.AddComponent<MeshCollider>();
        mapcollider.sharedMesh = this.mesh;

    }

    public void updateColour()
    {
        Color meshColor = this.getMapContainer().getCountryColour(this.getMapContainer().getData(this.name, this.parentName));
        this.color = meshColor;
        meshRenderer.material.color = this.color;

    }

    internal void Deselect()
    {
        this.selected = false;
        DestoryLine();
    }

    public void SetPositionAndRotation(Vector3 pos, Quaternion angle)    
    {
        this.transform.SetPositionAndRotation(pos, angle);



        this.wrapper.transform.SetPositionAndRotation(this.transform.TransformPoint(new Vector3(centerX, centerY, -0.05F)), new Quaternion(0, 0, 0, 1));

        this.objToSpawn.transform.SetParent(this.wrapper.transform);

    }

    internal void Constructor(Vector2[] points, string name, GameObject objToSpawn, float[] bounds, string parentName, Action<bool> reportGrabbed)
    {
        T = new Triangulator(points);
        vertices3D = Array.ConvertAll<Vector2, Vector3>(points, v => v);

        this.reportGrabbed = reportGrabbed;
        this.name = name;
        this.parentName = parentName;
        this.centerX = (bounds[0] + bounds[2]) / 2F;
        this.centerY = (bounds[1] + bounds[3]) / 2F;


        this.go = Instantiate(Resources.Load("ObjectTooltip")) as GameObject;
        go.transform.parent = this.wrapper.transform;
        VRTK_ObjectTooltip tooltip = go.GetComponent<VRTK_ObjectTooltip>();
        tooltip.alwaysFaceHeadset = true;
        tooltip.displayText = this.name;
        tooltip.alwaysFaceHeadset = true;
        this.go.SetActive(false);


        this.objToSpawn = objToSpawn;
        this.objToSpawn.name = name;
        
        this.DrawObject();

        this.AddToList(parentName, name);
    }

   internal virtual void AddToList(string parentName, string name)
    {

    }


    public void SetParent(Transform parent)
    {

        this.wrapper = new GameObject();
        this.wrapper.transform.SetParent(parent);
    }

    internal void DeleteChildren()
    {
        if (this.children.Count > 0)
        {
            var filtered = this.children.Where(x => x != null).ToList();
            var parent = filtered[0].transform.parent.transform.parent.gameObject;
            filtered.ForEach(x => x.Delete());
            Destroy(parent);

        }
    }

    public void CreateLine()
    {
        var line = objToSpawn.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        var tempVertices3D = vertices3D.ToList();
        tempVertices3D.Add(vertices3D[0]);
        //Chuck her in front of the country so its visible
        tempVertices3D = tempVertices3D.Select(x => new Vector3(x.x, x.y, -0.01F)).ToList();
        var finaltempVertices3D = tempVertices3D.ToArray();
        line.positionCount = finaltempVertices3D.Length;
        line.SetPositions(finaltempVertices3D);

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.red;
        line.endColor = Color.yellow;
        line.startWidth = 0.01F;
        line.endWidth = 0.01F;
        line.alignment = LineAlignment.TransformZ;

    }

    public virtual Quaternion GetAngle()
    {
        return new Quaternion(0, 1, 0, 0);
    }

    public virtual Vector3 GetTranslation(float x, float y)
    {
        return new Vector3(x, -y, 0);
    }

    public virtual Quaternion GetFinalAngle()
    {
        return new Quaternion(0,0,0,1);
    }



    public void DestoryLine()
    {
       
        this.selected = false;
        MapContainer.update = true;
        this.RemoveLines();
        var line = objToSpawn.GetComponent<LineRenderer>();
        MapContainer.DeselectState();
        Destroy(line);


    }

    internal void AddChild(PointableObject gameObject)
    {
        this.children.Add(gameObject);
            
    }

    internal virtual void Delete()
    {
            this.children.ForEach(x => x.Delete());
            this.children.Clear();

        try
        {
            Destroy(this.gameObject);
            Destroy(this.wrapper);
        }
        catch { }
    }

    internal void SetSiblings(List<PointableObject> children)
    {

        foreach (var sibling in children)
        {
            this.siblings[sibling.name] = sibling;
        }
    }
}


