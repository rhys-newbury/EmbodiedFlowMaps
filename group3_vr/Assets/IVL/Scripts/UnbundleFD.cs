using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UnbundleFD : MonoBehaviour {
       
    // Visual Object Data
    public List<GameObject> sensorToTakeIntoAccount = new List<GameObject>();
    public List<KeyValuePair<GameObject, GameObject>> pointsList = new List<KeyValuePair<GameObject, GameObject>>();
    public List<GameObject> gameObjectToAvoid = new List<GameObject>();
    public List<GameObject> attractivePlane = new List<GameObject>();
    //public Dictionary<int, TubeRenderer> tubeList = new Dictionary<int, TubeRenderer>();
    public Dictionary<int, LineRenderer> tubeList = new Dictionary<int, LineRenderer>();

    #region Debug Params
    private List<GameObject> sphereNodes = new List<GameObject>();
    private List<GameObject> normals = new List<GameObject>();
    #endregion

    public Camera mainCam;
    public List<GameObject> otherClients;
    public bool isMaster;
    public bool SyncOtherClient = true;

    // View
    //public Text myText;
    public float radiusTube = 0.01f;

    // Private CPU vars.
    private int lineNb;
    private int lineLenght = 32;
    // CPU lines
    Vector3[] lineTab; // ActualLine
    List<Vector3[]> previousPositions;
    //CPU Normals
    Vector3[] beginLineNormal;
    Vector3[] endLineNormal;
    // Position of objects to avoid
    Vector3[] pointsToAvoid;
    // Bundle Id list    
    int[] bundleId;
    public bool drawSphere = false;
    Dictionary<int, int> bundleDict = new Dictionary<int, int>();

    //Position of attractive plane
    Vector3[] planeCoord;

    public Color colorT;

    #region Shaders Parts & Vars
    public ComputeShader shader;
    // GPU Buffer
    ComputeBuffer myLineBuffer;
    ComputeBuffer myDisplacementBuffer;
    ComputeBuffer normSrcVecBuffer;
    ComputeBuffer normDestVecBuffer;
    ComputeBuffer myAvoidPointBuffer;
    ComputeBuffer myAttractivePlane;
    ComputeBuffer myBundleIdBuffer;
    // GPU kernels
    private int fwdTransfKernel;
    private int bckTransfKernel;
    private int dispKernel;
    private int FDEUKernel;
    private int fwdTransfNormalKern;
    private int bckTransfNormalKern;
    private int fwdTransfObsKern;
    private int bckTransfObsKern;
    private int fwdTransPlanKern;
    private int bckTransPlanKern;

    // Shaders Uniforms
    // Simulation steps
    public float deltaSim;
    public float maxDeltaSim;   
    // Spring constant
    [Range(0, 5000.0f)]
    public float springConstant;
    // Repulsion constants
    [Range(-0.005f,0)]
    public float repulsionConstant;
    [Range(0, 0.005f)]
    public float attractionConstant;
    //[Range(0, 0.1f)]
    public float obstacleRepulsionConstant;
    public float obstacleDMin;
    //Normal Attraction
    public float distanceMaxNormal;
    public float forceMaxNormal;
    //Plane Attraction
    public float distancePlane;
    public float forcePlane;
    // Init bool
    private bool initDone = false;
    #endregion

    private Dictionary<GameObject, GameObject> debugPlane = new Dictionary<GameObject, GameObject>();

    public bool LineRendererDrawing;
    public Material linkMat;



    // Use this for initialization
    void Start () {
        
    }

    public void initUnbundling(List<KeyValuePair<GameObject, GameObject>> listGO)
    {
        initUnbundling(listGO, true);
    }

    public void initUnbundling(List<KeyValuePair<GameObject, GameObject>> listGO, bool drawing)
    {
        LineRendererDrawing = drawing;
        pointsList = listGO;
        Vector3 sum = Vector3.zero;
        int count = 0;
        //Find Center of the "Projection space"
        List<List<Vector3>> lineList = new List<List<Vector3>>();
        lineNb = pointsList.Count;
        lineTab = new Vector3[lineLenght * lineNb];
        beginLineNormal = new Vector3[lineNb];
        endLineNormal = new Vector3[lineNb];
        int posPointer = 0;
        int lineCounter = 0;

        Color32[] colorTube = new Color32[lineLenght];

        foreach (KeyValuePair<GameObject, GameObject> kv in pointsList)
        {
            GameObject go = kv.Key;
            GameObject sensorVisu = kv.Value;

            List<Vector3> line = new List<Vector3>();
            //Debug.Log("Use of the KeyValue Pair");

            Vector3 goP = go.transform.position;
            Vector3 goVP = sensorVisu.transform.position;

            Vector3 step = (goVP - goP) / (lineLenght - 1);

            lineTab[posPointer] = goP;
            posPointer++;

            for (int i = 1; i < lineLenght - 1; i++)
            {
                Vector3 interPoint = goP + i * step;
                lineTab[posPointer] = interPoint;
                posPointer++;
            }

            lineTab[posPointer] = goVP;
            posPointer++;
            sum += goP;
            sum += goVP;
            count += 2;

            //Normal to Parent Game Object
            //Try 1: Use a Ray
            /*RaycastHit hit;
            GameObject parent1 = go.transform.parent.gameObject;
            Vector3 originOfRay = parent1.transform.position + 0.1f * parent1.transform.forward;
            Vector3 directionOfRay = goP - originOfRay;
            Physics.Raycast(originOfRay, directionOfRay, out hit, Mathf.Infinity);
            Vector3 normal = hit.normal;*/

            //Try 2: Just use th forward vector of the point
            Vector3 normal1 = go.transform.forward;
            Vector3 normal2 = sensorVisu.transform.forward;
            //beginLineNormal[2 * lineCounter] = go.transform.position;
            beginLineNormal[lineCounter] = normal1;
            //endLineNormal[2 * lineCounter] = sensorVisu.transform.position;
            endLineNormal[lineCounter] = normal2;

            #region DEBUG - Create a cube to show the normal
            //GameObject goReal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //goReal.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            //goReal.transform.position = beginLineNormal[lineCounter];
            //GameObject goReal2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //goReal2.transform.localScale = new Vector3(0.0005f, 0.0005f, 0.0005f);
            //goReal2.transform.position = endLineNormal[lineCounter];
            #endregion

            lineCounter++;
        }


        #region Tube Creation
        drawTube();
        /*int countTube = 0;
        int idVec = 0;
        foreach (KeyValuePair<GameObject, GameObject> kv in pointsList)
        {
            GameObject tubeGO = new GameObject("Tube-" + countTube);
            tubeGO.transform.SetParent(this.transform);
            //tubeGO.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            TubeRenderer tube = tubeGO.AddComponent<TubeRenderer>();
            Vector3[] pointsTube = new Vector3[lineLenght];
            int idBegining = idVec;
            for (int i = idVec; i < idBegining + lineLenght; i++)
            {
                //Debug.Log(i + ";" + idVec + ";" + idBegining);
                pointsTube[i - idBegining] = lineTab[idVec];
                colorTube[i - idBegining] = colorT;
                idVec++;
            }
            tube.points = pointsTube;
            tube.radius = radiusTube;
            tube.colors = colorTube;
            tubeList.Add(idBegining, tube);

            tubeGO.GetComponent<Renderer>().material.color = colorT;

            //tubeGO.AddComponent<MeshCollider>();

            tubeGO.tag = "Tube";
            countTube++;
        }*/

        #endregion

        CreateAvoidPointsArray();
        initBundleID();
        CreateAttractivePlaneArray();
        //CreateTestBundles();

        previousPositions = new List<Vector3[]>();
        for (int i = 0; i < 5; i++)
        {
            Vector3[] tmp = new Vector3[lineTab.Length];
            lineTab.CopyTo(tmp, 0);
            previousPositions.Add(tmp);
        }

        #region DEBUG - SPHERE
        // DEBUG -- Creates array of spheres for node position
        /*Debug.Log("Array: ");
        sphereNodes = new List<GameObject>();
        foreach (Vector3 v in lineTab)
        {
            GameObject goReal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            goReal.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
            goReal.transform.SetParent(this.transform);
            goReal.transform.position = v;
            sphereNodes.Add(goReal);
        }*/
        //END DEBUG
        #endregion
        InitShaders();
        initDone = true;
    }

    // Create the private array of points to avoid.
    // Should put here the method to handle the decomposition of complex obstacles into point based skeletons.
    private void CreateAvoidPointsArray()
    {
        pointsToAvoid = new Vector3[gameObjectToAvoid.Count];
        for (int i = 0; i < gameObjectToAvoid.Count; i++)
        {
            pointsToAvoid[i] = gameObjectToAvoid[i].transform.position;
        }
    }

    private void updateAvoidPointArray()
    {
        for (int i = 0; i < gameObjectToAvoid.Count; i++)
        {
            pointsToAvoid[i] = gameObjectToAvoid[i].transform.position;
        }
    }


    //Attractive Plane creation
    private void CreateAttractivePlaneArray()
    {
        planeCoord = new Vector3[attractivePlane.Count * 4];
        int i = 0;

        
        foreach(GameObject plane in attractivePlane)
        {
            planeCoord[i] = plane.transform.position;
            i++;
            planeCoord[i] = plane.transform.right;
            i++;
            planeCoord[i] = plane.transform.forward;
            i++;
            planeCoord[i] = plane.transform.up;
            i++;
            //Debug.Log("Normal Vector: " +plane.transform.up);
            /*GameObject goReal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            goReal.name = "Normal Plane";
            goReal.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            goReal.transform.SetParent(this.transform);
            goReal.transform.position = plane.transform.position + 0.1f*plane.transform.up;
            debugPlane[plane] = goReal;*/
        }
    }
    
    private void updatePlaneCoord() {
        int i = 0;
        foreach (GameObject plane in attractivePlane)
        {
            planeCoord[i] = plane.transform.position;
            i++;
            planeCoord[i] = plane.transform.right;
            i++;
            planeCoord[i] = plane.transform.forward;
            i++;
            planeCoord[i] = plane.transform.up;
            i++;
            //Debug.Log("Normal Vector: " + plane.transform.up);
            //debugPlane[plane].transform.position = plane.transform.position + plane.transform.up;
        }
    }

    private void CreateTestBundles()
    {
        // DEBUG - TEST
        bundleId = new int[lineNb];
        // END DEBUG - TEST
        for (int i = 0; i < lineNb; i++)
        {
            bundleId[i] = i % 4;
        }// lineCounter % 4;
    }

    private void initBundleID()
    {
        bundleId = new int[lineNb];
        for (int i = 0; i < lineNb; i++)
        {
            bundleId[i] = i;
            bundleDict[i] = 1;

            //bundleId[i] = 0;
            

        }
        //bundleDict[0] = 3;
    }

    void InitShaders()
    {
        // Link kernels with shader
        fwdTransfKernel = shader.FindKernel("MultiplyWToL");
        bckTransfKernel = shader.FindKernel("MultiplyLToW");
        dispKernel = shader.FindKernel("movePoints");
        FDEUKernel = shader.FindKernel("executeFDEUB");
        fwdTransfNormalKern = shader.FindKernel("MultiplyWToLNorm");
        bckTransfNormalKern = shader.FindKernel("MultiplyLToWNorm");
        fwdTransfObsKern = shader.FindKernel("MultiplyWToLObs");
        bckTransfObsKern = shader.FindKernel("MultiplyLToWObs");
        fwdTransPlanKern = shader.FindKernel("MultiplyWToLPla");
        bckTransPlanKern = shader.FindKernel("MultiplyLToWPla");

        // Link buffer with RWTextures in shader
        myLineBuffer = new ComputeBuffer(lineTab.Length, 3 * sizeof(float));
        myLineBuffer.SetData(lineTab);
        myDisplacementBuffer = new ComputeBuffer(lineTab.Length, 3 * sizeof(float));
        normSrcVecBuffer = new ComputeBuffer(beginLineNormal.Length, 3 * sizeof(float));
        normSrcVecBuffer.SetData(beginLineNormal);
        normDestVecBuffer = new ComputeBuffer(endLineNormal.Length, 3 * sizeof(float));
        normDestVecBuffer.SetData(endLineNormal);
        if (pointsToAvoid.Length > 0)
        {
            myAvoidPointBuffer = new ComputeBuffer(pointsToAvoid.Length, 3 * sizeof(float));
        
            myAvoidPointBuffer.SetData(pointsToAvoid);
        }
        else
        {
            myAvoidPointBuffer = new ComputeBuffer(1, 3 * sizeof(float));
            Vector3[] dummyPoint = new Vector3[1];
            dummyPoint[0] = new Vector3(0, 0, 0);
            myAvoidPointBuffer.SetData(dummyPoint);
        }
        myBundleIdBuffer = new ComputeBuffer(bundleId.Length, sizeof(int));
        myBundleIdBuffer.SetData(bundleId);
        int nbPlane = (int) (planeCoord.Length / 4.0);
        //Debug.Log("Nb Plane: "+nbPlane);
        if(nbPlane > 0)
        {
            myAttractivePlane = new ComputeBuffer(planeCoord.Length, 3 * sizeof(float));
            myAttractivePlane.SetData(planeCoord);
        }
        else
        {
            myAttractivePlane = new ComputeBuffer(1, 3 * sizeof(float));
            Vector3[] dummyPoint = new Vector3[1];
            dummyPoint[0] = new Vector3(0, 0, 0);
            myAttractivePlane.SetData(dummyPoint);
        }


        // Coord Transform Buffers
        shader.SetBuffer(fwdTransfKernel, "pos", myLineBuffer);
        shader.SetBuffer(bckTransfKernel, "pos", myLineBuffer);
        shader.SetBuffer(bckTransfKernel, "dispVec", myDisplacementBuffer);
        //Normal coordinate change
        shader.SetBuffer(fwdTransfNormalKern, "normSrcVec", normSrcVecBuffer);
        shader.SetBuffer(fwdTransfNormalKern, "normDestVec", normDestVecBuffer);
        shader.SetBuffer(bckTransfNormalKern, "normSrcVec", normSrcVecBuffer);
        shader.SetBuffer(bckTransfNormalKern, "normDestVec", normDestVecBuffer);
        // Obstacle coordinate change
        if (pointsToAvoid.Length > 0)
        {
            shader.SetBuffer(fwdTransfObsKern, "avoidPoints", myAvoidPointBuffer);
            shader.SetBuffer(bckTransfObsKern, "avoidPoints", myAvoidPointBuffer);
        }
        //Plane Attraction Coordinate change
        if(planeCoord.Length > 0)
        {
            shader.SetBuffer(fwdTransPlanKern, "attracPlane", myAttractivePlane);
            shader.SetBuffer(bckTransPlanKern, "attracPlane", myAttractivePlane);
        }
        // FD Kernel Buffers
        shader.SetBuffer(FDEUKernel, "pos", myLineBuffer);
        shader.SetBuffer(FDEUKernel, "dispVec", myDisplacementBuffer);
        shader.SetBuffer(FDEUKernel, "normSrcVec", normSrcVecBuffer);
        shader.SetBuffer(FDEUKernel, "normDestVec", normDestVecBuffer);
        //if (pointsToAvoid.Length > 0)
        //{
            shader.SetBuffer(FDEUKernel, "avoidPoints", myAvoidPointBuffer);
        //}
        //if(planeCoord.Length > 0)
        //if(nbPlane > 0)
        //{
            //Debug.Log("Set AttractPlane property...");
            shader.SetBuffer(FDEUKernel, "attracPlane", myAttractivePlane);
        //}
        shader.SetBuffer(FDEUKernel, "bundleId", myBundleIdBuffer);
        // Displacement kernel Buffers
        shader.SetBuffer(dispKernel, "pos", myLineBuffer);
        shader.SetBuffer(dispKernel, "dispVec", myDisplacementBuffer);

        // Link shader uniforms
        shader.SetInt("lineNb", lineNb);
        shader.SetInt("obsNb", pointsToAvoid.Length);
        shader.SetFloat("k", springConstant);
        shader.SetFloat("kr", repulsionConstant);
        shader.SetFloat("ka", attractionConstant);
        shader.SetFloat("kor", obstacleRepulsionConstant);
        shader.SetFloat("dmin", obstacleDMin);
        shader.SetFloat("dmax", distanceMaxNormal);
        shader.SetFloat("fmax", forceMaxNormal);
        shader.SetInt("nbPlane", planeCoord.Length);
        shader.SetFloat("dmaxP", distancePlane);
        shader.SetFloat("fmaxP", forcePlane);
    }

    public void ResetBundling()
    {
        initDone = false;
        foreach (KeyValuePair<int, LineRenderer> entry in tubeList)
        //foreach (KeyValuePair<int, TubeRenderer> entry in tubeList)
        {
            // do something with entry.Value or entry.Key
            Destroy(entry.Value.gameObject);
        }
        tubeList.Clear();
        /*foreach(GameObject go in sphereNodes)
        {
            Destroy(go);
        }*/
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (initDone && isMaster)
        {
            //myText.text = " Link Repulsion : " + (int)Math.Round(-repulsionConstant / 0.00000001) + " \nNormal Repulsion : " + (int)Math.Round(forceMaxNormal / 0.005);
            // Compute displacement for each user..
            List<Vector3[]> listOfDispVec = new List<Vector3[]>();
            // Viewport User
            Vector3[] dispVec = ComputeDisplacementFromViewport(mainCam.worldToCameraMatrix);
            listOfDispVec.Add(dispVec);
            // Viewport client
            if (SyncOtherClient)
            {
                for (int i = 0; i < otherClients.Count; i++)
                {
                    //Vector3[] dispVecClient = new Vector3[lineLenght * lineNb];
                    Vector3[] dispVecClient = ComputeDisplacementFromViewport(otherClients[i].transform.worldToLocalMatrix);
                    listOfDispVec.Add(dispVecClient);
                }
            }

            // Apply displacement for each computed viewport : Try to do an average
            /*foreach (Vector3[] dispVectUser in listOfDispVec)
            {
                myDisplacementBuffer.SetData(dispVectUser);
                applyDisplacement();
            }*/

            Vector3[] averageDispl = new Vector3[lineLenght * lineNb];
            int count = 0;
            foreach (Vector3[] dispVectUser in listOfDispVec)
            {
                for (int i = 0; i < dispVectUser.Length; i++) {
                    averageDispl[i] = averageDispl[i] + dispVectUser[i];
                }
                count++;
            }

            for (int i = 0; i < averageDispl.Length; i++)
            {
                averageDispl[i] = averageDispl[i] / count;
            }
            myDisplacementBuffer.SetData(averageDispl);
            applyDisplacement();
            redrawTube();
        }

        if (Input.GetKey(KeyCode.T))
        {
            this.repulsionConstant = -0.00005f;
            this.obstacleDMin = 0.2f;
        }
    }

    Matrix4x4 rotation_between_two_vectora(Vector3 v1, Vector3 v2)
    {
        var angle = Vector3.Angle(v1, v2);
        var axis = Vector3.Cross(v1, v2);

        var q = Quaternion.AngleAxis(angle, axis);


        return Matrix4x4.TRS(Vector3.zero, q, Vector3.one);


    }

    private Vector3[] ComputeDisplacementFromViewport(Matrix4x4 trans)
    {
        //Updating points to link positions
        int count = 0;
        foreach (KeyValuePair<GameObject, GameObject> kv in pointsList)
        {
            GameObject go = kv.Key;
            GameObject sensorVisu = kv.Value;

            lineTab[lineLenght * count] = go.transform.position;
            lineTab[lineLenght * count + (lineLenght-1)] = sensorVisu.transform.position;

            

            //Change of the normal
            Vector3 normal1 = go.transform.forward;
            Vector3 normal2 = sensorVisu.transform.forward;
            //beginLineNormal[2 * lineCounter] = go.transform.position;
            beginLineNormal[count] = normal1;
            //endLineNormal[2 * lineCounter] = sensorVisu.transform.position;
            endLineNormal[count] = normal2;

            count++;

        }
        var mf = GameObject.Find("Screen2");
        //Top Left
        var vl = mf.GetComponent<MeshFilter>().sharedMesh.vertices;

        var s = mf.transform.TransformPoint(vl[0]);
        var r = mf.transform.TransformPoint(vl[1]);
        var b = mf.transform.TransformPoint(vl[4]);

        var QR = r - b;
        var QS = s - b;

        var perp = -Vector3.Cross(QR, QS);
        Debug.Log(perp);

        //var lol = 0;
        //if (drawSphere)
        //{
        //    drawSphere = false;
        //    foreach (var p in vl)
        //    {
        //        var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //        g.name = lol++.ToString();
        //        g.transform.position = mf.transform.TransformPoint(p);
        //        g.transform.localScale = new Vector3(0.05F, 0.05F, 0.05F);
        //    }
        //}


        //var t = mf.transform.TransformDirection(Vector3.up);

        var m = rotation_between_two_vectora(perp, Vector3.up);

        Debug.Log(m);

        shader.SetFloat("m00", m.m00);
        shader.SetFloat("m01", m.m01);
        shader.SetFloat("m02", m.m02);

        shader.SetFloat("m10", m.m10);
        shader.SetFloat("m11", m.m11);
        shader.SetFloat("m12", m.m12);

        shader.SetFloat("m20", m.m10);
        shader.SetFloat("m21", m.m11);
        shader.SetFloat("m22", m.m12);

        var inv = m.inverse;

        Vector3 test = new Vector3(0.5F, 0.4F, 0.6F);
        Debug.Log(inv.MultiplyVector(m.MultiplyVector(test)));


        shader.SetFloat("im00", inv.m00);
        shader.SetFloat("im01", inv.m01);
        shader.SetFloat("im02", inv.m02);

        shader.SetFloat("im10", inv.m10);
        shader.SetFloat("im11", inv.m11);
        shader.SetFloat("im12", inv.m12);

        shader.SetFloat("im20", inv.m10);
        shader.SetFloat("im21", inv.m11);
        shader.SetFloat("im22", inv.m12);







        myLineBuffer.SetData(lineTab);
        shader.SetBuffer(fwdTransfKernel, "pos", myLineBuffer);
        shader.SetBuffer(bckTransfKernel, "pos", myLineBuffer);
        shader.SetBuffer(FDEUKernel, "pos", myLineBuffer);
        shader.SetBuffer(dispKernel, "pos", myLineBuffer);


        normSrcVecBuffer.SetData(beginLineNormal);
        normDestVecBuffer.SetData(endLineNormal);
        shader.SetBuffer(fwdTransfNormalKern, "normSrcVec", normSrcVecBuffer);
        shader.SetBuffer(fwdTransfNormalKern, "normDestVec", normDestVecBuffer);
        shader.SetBuffer(bckTransfNormalKern, "normSrcVec", normSrcVecBuffer);
        shader.SetBuffer(bckTransfNormalKern, "normDestVec", normDestVecBuffer);
        shader.SetBuffer(FDEUKernel, "normSrcVec", normSrcVecBuffer);
        shader.SetBuffer(FDEUKernel, "normDestVec", normDestVecBuffer);



        //Updating Obstacles position
        updateAvoidPointArray();
        if (pointsToAvoid.Length > 0)
        {
            myAvoidPointBuffer.SetData(pointsToAvoid);
            shader.SetBuffer(fwdTransfObsKern, "avoidPoints", myAvoidPointBuffer);
            shader.SetBuffer(bckTransfObsKern, "avoidPoints", myAvoidPointBuffer);
            
        }

        shader.SetBuffer(FDEUKernel, "avoidPoints", myAvoidPointBuffer);


        //updating Plane position
        int nbPlane = (int)(planeCoord.Length / 4.0);
        //shader.SetInt("nbPlane", planeCoord.Length);
        shader.SetInt("nbPlane", nbPlane);
        //Debug.Log("Nb Plane: " + nbPlane);
        updatePlaneCoord();
        if (planeCoord.Length > 0)
        {
            myAttractivePlane.SetData(planeCoord);
            shader.SetBuffer(fwdTransPlanKern, "attracPlane", myAttractivePlane);
            shader.SetBuffer(bckTransPlanKern, "attracPlane", myAttractivePlane);
            shader.SetBuffer(FDEUKernel, "attracPlane", myAttractivePlane);
        }
        shader.SetBuffer(FDEUKernel, "attracPlane", myAttractivePlane);

        // FWD Coord Transf
        shader.SetMatrix("matwToL", trans);
        shader.SetMatrix("matlToW", trans.inverse);
        shader.SetInt("lineNb", lineNb);
        shader.SetInt("obsNb", pointsToAvoid.Length);
        shader.Dispatch(fwdTransfKernel, lineNb, 1, 1);
        shader.Dispatch(fwdTransfNormalKern, 1, 1, 1);
        if (planeCoord.Length > 0)
        {
            shader.Dispatch(fwdTransPlanKern, 1, 1, 1);
        }
        if (pointsToAvoid.Length > 0)
        {
            shader.Dispatch(fwdTransfObsKern, pointsToAvoid.Length, 1, 1);
        }
        // Apply FDEUB



        //shader.SetFloat()
        shader.SetInt("lineNb", lineNb);
        shader.SetInt("obsNb", pointsToAvoid.Length);
        shader.SetFloat("k", springConstant);
        shader.SetFloat("kr", repulsionConstant);
        shader.SetFloat("ka", attractionConstant);
        shader.SetFloat("kor", obstacleRepulsionConstant);
        shader.SetFloat("dmin", obstacleDMin);
        shader.SetFloat("dmax", distanceMaxNormal);
        shader.SetFloat("fmax", forceMaxNormal);
        shader.SetFloat("dmaxP", distancePlane);
        shader.SetFloat("fmaxP", forcePlane);
        shader.Dispatch(FDEUKernel, lineNb, 1, 1);
        #region DEBUG parts
        //Vector3[] tmpArray = new Vector3[lineTab.Length];
        //myDisplacementBuffer.GetData(tmpArray);
        // END DEBUG
        // DEBUG
        //Vector4[] tmpArrayNormOrig = new Vector4[lineNb];
        //Vector4[] tmpArrayNormDest = new Vector4[lineNb];
        //normSrcVecBuffer.GetData(tmpArrayNormOrig);
        //normDestVecBuffer.GetData(tmpArrayNormDest);
        //for (int i = 0; i < tmpArrayNormOrig.Length; i++)
        //{
        //    Debug.Log("NormalOrig " + i + ": (" + tmpArrayNormOrig[i].x + "," + tmpArrayNormOrig[i].y + "," + tmpArrayNormOrig[i].z + ")");
        //    Debug.Log("NormalDest " + i + ": (" + tmpArrayNormDest[i].x + "," + tmpArrayNormDest[i].y + "," + tmpArrayNormDest[i].z + ")");
        //}
        #endregion
        // BCK Coord Transf
        shader.Dispatch(bckTransfKernel, lineNb, 1, 1);
        shader.Dispatch(bckTransfNormalKern, 1, 1, 1);
        if (pointsToAvoid.Length > 0)
        {
            shader.Dispatch(bckTransfObsKern, pointsToAvoid.Length, 1, 1);
        }
        if (planeCoord.Length > 0)
        {
            shader.Dispatch(bckTransPlanKern, 1, 1, 1);
        }
        // Getting disp Vec back to CPU
        Vector3[] dispVec = new Vector3[lineLenght * lineNb];
        myDisplacementBuffer.GetData(dispVec);


        //DEBUG
        //////vector3[] planecoordvec = new vector3[attractiveplane.count*4];
        //////myattractiveplane.getdata(planecoordvec);
        //////int i = 0;
        //////foreach (gameobject plane in attractiveplane)
        //////{
        //////    debugplane[plane].transform.position = planecoordvec[i] + planecoordvec[i+3];
        //////    i++;
        //////}
        //for(int i=0; i< dispVec.Length; i = i + 4)
        //{
        //    Debug.Log();
        //}


        return dispVec;
    }

    private void applyDisplacement()
    {
        shader.SetFloat("delta", deltaSim);
        shader.SetFloat("maxDelta", maxDeltaSim);
        shader.Dispatch(dispKernel, lineNb, 1, 1);
    }


    public void changeOtherClient(List<GameObject> l)
    {
        otherClients = l;
    }

    public void sendUnbundle(List<int> lineToBundle)
    {
        if (lineToBundle.Count == 0)
        {
            return;
        }
        foreach(int b in lineToBundle)
        {
            int idLink = b / lineLenght;
            if (bundleDict[idLink] == 0)
            {
                bundleDict[bundleId[idLink]] -= 1;
                bundleDict[idLink] = 1;
                bundleId[idLink] = idLink;
            }
            else if (bundleDict[idLink] > 1)
            {
                List<int> otherLink = new List<int>();
                for(int i =0; i<bundleId.Length; i++)
                {
                    if((i != idLink) && (bundleId[i] == idLink))
                    {
                        otherLink.Add(i);
                    }
                }
                if (otherLink.Count > 0) {
                    int newIdLink = otherLink[0];
                    foreach(int otherL in otherLink)
                    {
                        bundleId[otherL] = newIdLink;
                    }
                    bundleDict[newIdLink] = otherLink.Count;
                    bundleDict[idLink] = 1;
                    bundleId[idLink] = idLink;
                }
            }
        }
        myBundleIdBuffer.SetData(bundleId);
        myBundleIdBuffer.SetData(bundleId);
        shader.SetBuffer(FDEUKernel, "bundleId", myBundleIdBuffer);
    }

    public void sendBundle(List<int> lineToBundle)
    {
        if (lineToBundle.Count == 0)
        {
            return;
        }

        int idBestBundle = -1;
        int nbLinkInBundle = 0;

        foreach(int b in lineToBundle)
        {
            int idLink = b / lineLenght;
            int nbLinkinB = bundleDict[bundleId[idLink]];
            if (nbLinkinB>nbLinkInBundle)
            {
                idBestBundle = bundleId[idLink];
                nbLinkInBundle = nbLinkinB;
            }
        }

        //int idFirstLink = lineToBundle[0] / lineLenght;

        Debug.Log("Index: "+ idBestBundle);

        //int idToBundle = bundleId[idFirstLink];
        for(int i = 0; i< lineToBundle.Count; i++)
        {
            int idLink = lineToBundle[i] / lineLenght;
            bundleDict[bundleId[idLink]] = 0;
            bundleId[idLink] = idBestBundle;

        }

        int nbInBestBundle = 0;
        foreach (int b in bundleId)
        {
            if(b == idBestBundle)
            {
                nbInBestBundle++;
            }
        }

        bundleDict[idBestBundle] = nbInBestBundle;

        //bundleDict[idBestBundle]



        myBundleIdBuffer.SetData(bundleId);
        myBundleIdBuffer.SetData(bundleId);
        shader.SetBuffer(FDEUKernel, "bundleId", myBundleIdBuffer);
    }
       
    #region Drawing Methods

    void drawTube()
    {
        if (LineRendererDrawing)
        {
            Debug.Log("Tube Creation");
            int countTube = 0;
            int idVec = 0;
            foreach (KeyValuePair<GameObject, GameObject> kv in pointsList)
            {
                GameObject tubeGO = new GameObject("Tube-" + countTube);
                //tubeGO.transform.SetParent(this.transform);
                LineRenderer lr = tubeGO.AddComponent<LineRenderer>();
                Vector3[] pointsTube = new Vector3[lineLenght];
                int idBegining = idVec;
                for (int i = idVec; i < idBegining + lineLenght; i++)
                {
                    Debug.Log(i + ";" + idVec + ";" + idBegining);
                    pointsTube[i - idBegining] = lineTab[idVec];
                    idVec++;
                }
                lr.positionCount = pointsTube.Length;
                lr.SetPositions(pointsTube);
                lr.startWidth = 0.015f;
                lr.endWidth = 0.015f;
                Material[] matArray = new Material[1];
                linkMat.color = colorT;
                matArray[0] = linkMat;
                lr.materials = matArray;
                tubeList.Add(idBegining, lr);

                //tubeGO.tag = "Tube";
                countTube++;

            }
        }
    }


    void redrawTube()
    {
        if (LineRendererDrawing) {
            myLineBuffer.GetData(lineTab);
            previousPositions.RemoveAt(0);
            Vector3[] tmp = new Vector3[lineTab.Length];
            lineTab.CopyTo(tmp, 0);
            previousPositions.Add(tmp);


            foreach (KeyValuePair<int, LineRenderer> entry in tubeList)
            {
                Vector3[] pointsTube = new Vector3[lineLenght];
                int idBegining = entry.Key;
                for (int i = idBegining; i < idBegining + lineLenght; i++)
                {
                    pointsTube[i - idBegining] = getAvgPos(i);

                }
                entry.Value.SetPositions(pointsTube);
                //entry.Value.points = pointsTube;
            }

        }
    }

    private Vector3 getAvgPos(int i)
    {
        Vector3 avgPos = new Vector3();
        avgPos.x = (previousPositions[0][i].x + previousPositions[1][i].x + previousPositions[2][i].x + previousPositions[3][i].x + previousPositions[4][i].x) / 5.0f;
        avgPos.y = (previousPositions[0][i].y + previousPositions[1][i].y + previousPositions[2][i].y + previousPositions[3][i].y + previousPositions[4][i].y) / 5.0f;
        avgPos.z = (previousPositions[0][i].z + previousPositions[1][i].z + previousPositions[2][i].z + previousPositions[3][i].z + previousPositions[4][i].z) / 5.0f;
        return avgPos;
    }

    #endregion
}
