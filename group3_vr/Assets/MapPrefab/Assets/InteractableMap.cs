using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VRTK;
using System;

/// <summary>
/// An Interactable Object, specifically an InteractableMap. This class provides a general interface for differnet map instances and lev
/// </summary>
public abstract class InteractableMap : InteractableObject
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
    private float alpha;
    private MeshRenderer meshRenderer;
    private float centerX, centerY;
    protected List<InteractableMap> children = new List<InteractableMap>();
    private float centroidX;
    private float centroidY;
    private float zShift;
    private bool selected;
    private readonly List<GameObject> lines = new List<GameObject>();


    public Mesh mesh;
    internal InteractableMap parent;
    public Dictionary<string, InteractableMap> siblings = new Dictionary<string, InteractableMap>();
    private MapController mapController;

    /// <summary>
    /// Draws a flow between given origin and the siblings, which are currently selected.
    /// </summary>
    /// <param name="origin_map">Origin of the flows</param>
    /// <returns></returns>
    /// 
    public virtual void GetInternalFlows(MapContainer origin_map)
    {

        Dictionary<String, InteractableMap> lchild = new Dictionary<string, InteractableMap>();
        foreach (var ma2p in origin_map.GetComponentsInChildren<InteractableMap>())
        {
            lchild[ma2p.name] = ma2p;
        }

        foreach (var pair in this.getMapController().flattenedList[origin_map.parentName].Take(20))
        {

            try
            {
                var origin = lchild[pair.Item1];
                var destination = lchild[pair.Item2];
                var flowData = pair.Item3;

                Bezier b = new Bezier(origin_map.transform, origin, destination, 0.1F * (0.00000699192F * flowData + 0.05F));
            }
            catch { }

            //b.line.startWidth = 0.1F * (0.00000699192F * flowData + 0.05F);
            //Debug.Log("line width = " + b.line.startWidth.ToString());
            //b.line.endWidth = b.line.startWidth;

            //b.line.material = new Material(Shader.Find("Sprites/Default"));

            //b.line.startColor = Color.green; //new Color(253, 187, 45, 255);
            //b.line.endColor = Color.blue; // new Color(34, 193,195, 255);

        }


    }

    /// <summary>
    /// Return the MapController, which controlles the map instance
    /// <returns></returns>
    ///
    public MapController getMapController()
    {
        this.mapController = this.transform.root.GetComponent<MapController>() ?? this.mapController;
        return this.mapController;
    }

    /// <summary>
    /// When the user grips the map, remove from stack.
    /// <returns></returns>
    ///
    public override void OnGripPressed()
    {
        this.reportGrabbed(true);
    }

    /// <summary>
    /// Behaviour, when the trigger button is pressed.
    /// If the map is currently selected, consider this a delete
    /// If the map is not selected, draw a new map
    /// <returns></returns>
    ///
    public override GameObject OnTriggerPressed(Transform direction)
    {

        if (!this.IsSelected())
        {

            //Create the gameObject for the map and then Draw it.
            GameObject mapGameObject = new GameObject();
            mapGameObject.transform.parent = this.transform.root;
            MapContainer main = mapGameObject.AddComponent(typeof(MapContainer)) as MapContainer;
            main?.Draw(this, this.GetLevel() + 1, this.transform.parent.transform, direction);


            this.selected = true;
            CreateLine();
            this.GetInternalFlows(main);
            return main.gameObject;
        }
        else
        {
            //On Deselect Remove it from the current list and Delete the object and its children 
            this.selected = false;
            DestoryLine();


            this.DeleteChildren();
            return null;
        }

    }

    public MapContainer draw_map(Transform direction)
    {
        //Create the gameObject for the map and then Draw it.
        GameObject mapGameObject = new GameObject();
        mapGameObject.transform.parent = this.transform.root;
        MapContainer main = mapGameObject.AddComponent(typeof(MapContainer)) as MapContainer;
        main?.Draw(this, this.GetLevel() + 1, this.transform.parent.transform, direction);


        this.selected = true;
        CreateLine();
        this.GetInternalFlows(main);
        return main;
    }

    /// <summary>
    /// Add a line to the current list.
    /// </summary>
    /// <param name="line">The line to add</param>
    /// <returns></returns>
    /// 
    public virtual void AddLine(GameObject line)
    {
        lines.Add(line);
    }
    /// <summary>
    /// Destory the game object for each line, then clear the list
    /// </summary>
    /// <returns></returns>
    /// 
    public virtual void RemoveLines()
    {
        lines.ToList().ForEach(Destroy);
        lines.Clear();
    }

    /// <summary>
    /// Return the name of the current state.
    /// </summary>
    /// <returns></returns>
    /// 
    public string GetName()
    {
        return this.name;
    }

    public abstract int GetLevel();

    /// <summary>
    /// Enable the tooltip OnPointerEnter.
    /// </summary>
    /// <param name="changeText">Delegate function to updae the tooltip text</param>
    /// <returns></returns>
    /// 
    public override void OnPointerEnter(Action<string> changeText)
    {
        this.color.a = 0.3F;
        this.meshRenderer.material.color = this.color;
        VRTK_ObjectTooltip tooltip = go.GetComponent<VRTK_ObjectTooltip>();

        var inc_flow = Mathf.Max(0,this.getMapController().getData(this.name, this.parentName, true));
        
        var out_flow = Mathf.Max(0,this.getMapController().getData(this.name, this.parentName, false));

        var text = this.name + ": \n" + inc_flow.ToString("N0") +  "Moved In \n" + out_flow.ToString("N0") + " Moved Out";


        changeText(text);
        tooltip.displayText = text;
        
        this.go.SetActive(true);


    }

    /// <summary>
    /// Disable the tooltip OnPointerLeave.
    /// </summary>
    /// <returns></returns>
    /// 
    public override void OnPointerLeave()
    {
        this.color.a = this.alpha;
        this.meshRenderer.material.color = this.color;
        this.go.SetActive(false);
    }

    /// <summary>
    /// Return a boolean, depicting whether the object is selected
    /// </summary>
    /// <returns></returns>
    /// 
    public bool IsSelected()
    {
        return this.selected;
    }

    /// <summary>
    /// Create the unity mesh from the triangualation result.
    /// </summary>
    /// <returns></returns>
    /// 
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
        Color meshColor = this.getMapController().getCountryColour(this.getMapController().getPopulationDensity(this.name, this.parentName));
        this.alpha = meshColor.a;
        this.color = meshColor;

        // Set up game object with mesh;
        meshRenderer = objToSpawn.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        meshRenderer.material.color = this.color;

        var filter = objToSpawn.AddComponent<MeshFilter>();
        filter.mesh = this.mesh;

        var mapcollider = objToSpawn.AddComponent<MeshCollider>();
        //mapcollider.convex = true;
        mapcollider.sharedMesh = this.mesh;
        

    }

    /// <summary>
    /// Update the colour of Objects, based on the toggling data.
    /// </summary>
    /// <returns></returns>
    /// 
    public void updateColour()
    {
        Color meshColor = this.getMapController().getCountryColour(this.getMapController().getPopulationDensity(this.name, this.parentName));
        this.alpha = meshColor.a;
        this.color = meshColor;
        meshRenderer.material.color = this.color;

    }

    /// <summary>
    /// On deselect, destory the line.
    /// </summary>
    /// <returns></returns>
    /// 
    internal void Deselect()
    {
        this.selected = false;
        DestoryLine();
    }

    /// <summary>
    /// Set the position and rotation of the shape and wrapper.
    /// </summary>
    /// <param name="pos">The new position the shape</param>
    /// <param name="angle">The new angle of the shape</param>
    /// <returns></returns>
    /// 
    public void SetPositionAndRotation(Vector3 pos, Quaternion angle)    
    {
        this.transform.SetPositionAndRotation(pos, angle);



        this.wrapper.transform.SetPositionAndRotation(this.transform.TransformPoint(new Vector3(centerX, centerY, -0.05F)), new Quaternion(0, 0, 0, 1));

        this.objToSpawn.transform.SetParent(this.wrapper.transform);

    }

    /// <summary>
    /// Construct the Interactable Map
    /// </summary>
    /// <param name="points">The results of the triangulator</param>
    /// <param name="name">The name of the map</param>
    /// <param name="objToSpawn">GameObject which is going to contain the map instance</param>
    /// <param name="bounds">Bounds of the shape</param>
    /// <param name="parentName">Name of the parent map</param>
    /// <param name="reportGrabbed">Delegate function, which the map instance uses to report grabbed</param>
    /// <returns></returns>
    /// 
    internal void Constructor(Vector2[] points, string name, GameObject objToSpawn, float[] bounds, string parentName, Action<bool> reportGrabbed)
    {
        T = new Triangulator(points);
        vertices3D = Array.ConvertAll<Vector2, Vector3>(points, v => v);

        this.reportGrabbed = reportGrabbed;
        this.name = name;
        this.parentName = parentName;
        this.centerX = (bounds[0] + bounds[2]) / 2F;
        this.centerY = (bounds[1] + bounds[3]) / 2F;


        this.go = Instantiate(Resources.Load("MapToolTip")) as GameObject;
        go.transform.parent = this.wrapper.transform;
        VRTK_ObjectTooltip tooltip = go.GetComponent<VRTK_ObjectTooltip>();
        tooltip.alwaysFaceHeadset = true;
        tooltip.displayText = this.name;
        tooltip.alwaysFaceHeadset = true;
        this.go.SetActive(false);


        this.objToSpawn = objToSpawn;
        this.objToSpawn.name = name;
        
        this.DrawObject();


        CreateLineAroundState();

        //this.AddToList(parentName, name);
    }


    /// <summary>
    /// Add the map instance to the list
    /// </summary>
    /// <param name="parentName">The name of the map parent</param>
    /// <param name="name">The name of the map</param>
    /// <returns></returns>
    /// 
    //internal virtual void AddToList(string parentName, string name)
    //{
    //    this.getMapController().addToList(parentName, name);
    //}


    /// <summary>
    /// Set the parent of wrapper GameObject to be the speicfied parent.
    /// </summary>
    /// <param name="parent">The parent of the wrapper</param>
    /// <returns></returns>
    /// 
    public void SetParent(Transform parent)
    {

        this.wrapper = new GameObject();
        this.wrapper.transform.SetParent(parent);
    }

    /// <summary>
    /// Delete children of the map instance
    /// </summary>
    /// <returns></returns>
    /// 
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

    public void CreateLineAroundState()
    {
        if (vertices3D.Length == 0)
        {
            Debug.Log("Empty: " + this.name);
            return;
        }

        var line = objToSpawn.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        var tempVertices3D = vertices3D.ToList();
        tempVertices3D.Add(vertices3D[0]);
        //Chuck her in front of the country so its visible
        tempVertices3D = tempVertices3D.Select(x => new Vector3(x.x, x.y, -0.005F)).ToList();
        var finaltempVertices3D = tempVertices3D.ToArray();
        line.positionCount = finaltempVertices3D.Length;
        line.SetPositions(finaltempVertices3D);

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(0.3F, 0.3F, 0.3F);//Color.grey;
        line.endColor = new Color(0.3F, 0.3F, 0.3F); ;
        line.startWidth = 0.004F;
        line.endWidth = 0.0004F;
        line.alignment = LineAlignment.TransformZ;
    }
    /// <

    /// <summary>
    /// Draw a line around the map
    /// </summary>
    /// <returns></returns>
    /// 
    public void CreateLine()
    {
        var line = objToSpawn.GetComponent<LineRenderer>();
        //line.useWorldSpace = false;
        //var tempVertices3D = vertices3D.ToList();
        //tempVertices3D.Add(vertices3D[0]);
        ////Chuck her in front of the country so its visible
        //tempVertices3D = tempVertices3D.Select(x => new Vector3(x.x, x.y, -0.01F)).ToList();
        //var finaltempVertices3D = tempVertices3D.ToArray();
        //line.positionCount = finaltempVertices3D.Length;
        //line.SetPositions(finaltempVertices3D);

        //line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(224,80,70);
        line.endColor = new Color(224, 80, 70);
        line.startWidth = 0.01F;
        line.endWidth = 0.01F;
        line.alignment = LineAlignment.TransformZ;
    }
    /// <summary>
    /// Destroy the line around the map.
    /// </summary>
    /// <returns></returns>
    /// 
    public void DestoryLine()
    {
        this.selected = false;
        MapContainer.update = true;
        this.RemoveLines();

        var line = objToSpawn.GetComponent<LineRenderer>();
        line.startColor = Color.grey;
        line.endColor = Color.gray;
        line.startWidth = 0.004F;
        line.endWidth = 0.0004F;
        line.alignment = LineAlignment.TransformZ;

        MapContainer.DeselectState();
      }
    /// <summary>
    /// Default angle of map
    /// </summary>
    /// <returns></returns>
    /// 
    public virtual Quaternion GetAngle()
    {
        return new Quaternion(0, 1, 0, 0);
    }
    /// <summary>
    /// Convert Translation in to correct coordinate system
    /// </summary>
    /// <returns></returns>
    /// 
    public virtual Vector3 GetTranslation(float x, float y)
    {
        return new Vector3(x, -y, 0);
    }
    /// <summary>
    /// Get Final angle of map.
    /// </summary>
    /// <returns></returns>
    /// 
    public virtual Quaternion GetFinalAngle()
    {
        return new Quaternion(0,0,0,1);
    }




    /// <summary>
    /// Add a 'child' to the map instance
    /// </summary>
    /// <returns></returns>
    /// 
    internal void AddChild(InteractableMap gameObject)
    {
        this.children.Add(gameObject);
    }
    /// <summary>
    /// Recursively delete the children and destroy the current object.
    /// </summary>
    /// <returns></returns>
    /// 
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
    /// <summary>
    /// Recursively delete the children and destroy the current object.
    /// </summary>
    /// <returns></returns>
    /// 
    internal void SetSiblings(List<InteractableMap> children)
    {

        foreach (var sibling in children)
        {
            this.siblings[sibling.name] = sibling;
        }
    }
}


