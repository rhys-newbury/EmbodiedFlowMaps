using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRope : MonoBehaviour
{

    private ObiRope rope;
    private ObiRopeBlueprint blueprint;
    private ObiRopeExtrudedRenderer ropeRenderer;
    private void Start()
    {
        CreateRope(GameObject.Find("Cube (5)").transform, GameObject.Find("Cube (4)").transform, GameObject.Find("Obi Solver").GetComponent<ObiSolver>());

    }

    private void CreateRope(Transform left, Transform right, ObiSolver solver)
    {
        StartCoroutine(MakeRope(left, right, solver));
    }
    // Start is called before the first frame update
    IEnumerator MakeRope(Transform left, Transform right, ObiSolver solver)
    {
        // Create both the rope and the solver:	
        rope = gameObject.AddComponent<ObiRope>();
        ropeRenderer = gameObject.AddComponent<ObiRopeExtrudedRenderer>();
        ropeRenderer.section = (ObiRopeSection)Resources.Load("DefaultRopeSection");
        ropeRenderer.uvScale = new Vector2(1, 5);
        ropeRenderer.normalizeV = false;
        ropeRenderer.uvAnchor = 1;
        rope.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Green");

        gameObject.AddComponent<VRTK_ObiInteractableActor>();
        rope.maxCompression = 1;


        // Setup a blueprint for the rope:
        blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
        blueprint.resolution = 0.4f;
        blueprint.thickness = 0.025f;
               
        // Procedurally generate the rope path (a simple straight line):
        blueprint.path.Clear();
        blueprint.path.AddControlPoint(left.position, left.TransformDirection(Vector3.down), left.TransformDirection(Vector3.up), left.TransformDirection(Vector3.right), 0.1f, 0.1f, 1, 1, Color.white, "Hook start");
        blueprint.path.AddControlPoint(right.position, right.TransformDirection(Vector3.up), right.TransformDirection(Vector3.down), right.TransformDirection(Vector3.left), 0.1f, 0.1f, 1, 1, Color.white, "Hook start");
        blueprint.path.FlushEvents();
               
        // Generate the particle representation of the rope (wait until it has finished):
        yield return blueprint.Generate();

        // Set the blueprint (this adds particles/constraints to the solver and starts simulating them).
        rope.ropeBlueprint = blueprint;
        rope.GetComponent<MeshRenderer>().enabled = true;


        var attach1 = gameObject.AddComponent<ObiParticleAttachment>();

        attach1.particleGroup = rope.ropeBlueprint.groups[0];
        attach1.target = left;

        var attach2 = gameObject.AddComponent<ObiParticleAttachment>();

        attach2.particleGroup = rope.ropeBlueprint.groups[1];
        attach2.target = right;


        rope.transform.parent = solver.transform;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
