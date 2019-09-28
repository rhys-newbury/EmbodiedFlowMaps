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

public class PointableObject : Pointable
{
    static readonly List<PointableObject> currentList = new List<PointableObject>();

    private string name;
    public string parentName;
    protected GameObject wrapper;
    private GameObject go;
    private GameObject objToSpawn;
    private Vector3[] vertices3D;
    public Action<bool> report_grabbed;
    private Triangulator T;
    private Color color;
    private MeshRenderer meshRenderer;
    private float[] bounds;

    protected List<PointableObject> children = new List<PointableObject>();


    private float centroidX;

    private float centroidY;
    private float ZShift;

    private bool selected = false;

    public Mesh mesh;
    internal PointableObject parent;
    public Dictionary<string, PointableObject> siblings = new Dictionary<string, PointableObject>();

    public void Start()
    {

    }

    public virtual void getInternalFlows(PointableObject origin)
    {

    }

    public override void OnThrow()
    {

        int level = -1;
        //On Throw delete each item in children
        //Deselect the parent
        foreach (var item in this.GetComponentsInChildren<PointableObject>())
        {
            item.delete();
            item.parent.deselect();
            level = level == -1 ? item.getLevel() : level;
        }
        //Do not destroy country
        if (level > 0) Destroy(this);
    }

    public override void OnUpdateTouchPadPressed(float touchpadAngle, Transform transformDirection)
    {
        //Left/Right -> Rotate
        //Up/Down -> Back and forth in direction of pointer.
        if (touchpadAngle< 72 || touchpadAngle> 288)
        {
            this.transform.position = this.transform.position + transformDirection.TransformDirection(new Vector3(0, 0, 0.01F));
        }
        else if (touchpadAngle< 144)
        {
            this.transform.Rotate(new Vector3(0, -1, 0), Space.Self);
        }
        else if (touchpadAngle< 216)
        {
            this.transform.position = this.transform.position + transformDirection.TransformDirection(new Vector3(0, 0, -0.01F));
        }
        else if (touchpadAngle< 288)
        {
            this.transform.Rotate(new Vector3(0, 1, 0), Space.Self);
        }

    }

    public override void OnGripPressed()
    {
        this.report_grabbed(true);
    }

    public override void OnTriggerPressed()
    {

        if (!this.isSelected())
        {
            //Add it to the list of current objects
            currentList.Add(this);

            //Create the gameObject for the map and then draw it.
            GameObject gameObject = new GameObject();
            draw_object main = gameObject.AddComponent(typeof(draw_object)) as draw_object;
            main.draw(this, this.getLevel() + 1);

            this.selected = true;
            createLine();
            this.getInternalFlows(this);
        }
        else
        {
            //On deselect Remove it from the current list and delete the object and its children 
            this.selected = false;
            destoryLine();

            currentList.Remove(this);
            this.deleteChildren();
        }

    }


    public virtual void removeLines()
    {

    }

    public virtual void addLine(GameObject line)
    {

    }

    public string getName()
    {
        return this.name;
    }

    public virtual int getLevel() {
        return 0;
    }


    public override void OnPointerEnter(Action<string> change_text)
    {
        this.color.a = 0.3F;
        this.meshRenderer.material.color = this.color;
        change_text(this.getName());
        this.go.SetActive(true);
    }
   

    public override void OnPointerLeave()
    {
        this.color.a = 1F;
        this.meshRenderer.material.color = this.color;
        this.go.SetActive(false);
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
        this.mesh = new Mesh
        {
            vertices = verticesList.ToArray(),
            triangles = indices2.ToArray()
        };

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

    public void SetPositionAndRotation(Vector3 pos, Quaternion angle)
    {
        this.transform.SetPositionAndRotation(pos, angle);

        float centerX = (this.bounds[0] + this.bounds[2]) / 2F;
        float centerY = (this.bounds[1] + this.bounds[3]) / 2F;

        this.wrapper.transform.SetPositionAndRotation(this.transform.TransformPoint(new Vector3(centerX, centerY, -0.05F)), new Quaternion(0, 0, 0, 1));

        this.objToSpawn.transform.SetParent(this.wrapper.transform);

    }

    internal void constructor(Vector2[] points, string name, GameObject objToSpawn, float[] bounds, string parentName, Action<bool> report_grabbed)
    {
        T = new Triangulator(points);
        vertices3D = Array.ConvertAll<Vector2, Vector3>(points, v => v);

        this.report_grabbed = report_grabbed;
        this.name = name;
        this.parentName = parentName;
        this.bounds = bounds;

        this.wrapper = new GameObject();

        this.go = Instantiate(Resources.Load("ObjectTooltip")) as GameObject;
        go.transform.parent = this.wrapper.transform;
        VRTK_ObjectTooltip tooltip = go.GetComponent<VRTK_ObjectTooltip>() as VRTK_ObjectTooltip;
        tooltip.alwaysFaceHeadset = true;
        tooltip.displayText = this.name;
        tooltip.alwaysFaceHeadset = true;
        this.go.SetActive(false);


        this.objToSpawn = objToSpawn;
        this.objToSpawn.name = name;
        
        this.drawObject();

        this.addToList(parentName, name);
    }

   internal virtual void addToList(string parentName, string name)
    {

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
            Destroy(parent);

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
        draw_object.update = true;
        this.removeLines();
        var line = objToSpawn.GetComponent<LineRenderer>();
        draw_object.DeselectState();
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

        try
        {
            Destroy(this.gameObject);
            Destroy(this.wrapper);
        }
        catch { }
    }

    internal void setSiblings(List<PointableObject> children)
    {

        foreach (var sibling in children)
        {
            this.siblings[sibling.name] = sibling;
        }
    }
}


