using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRope : MonoBehaviour
{
    private void Start()
    {
        MakeRope();
    }
    // Start is called before the first frame update
    void MakeRope()
    {

        // create an object containing both the solver and the updater:
        GameObject solverObject = GameObject.Find("Obi Solver");
        ObiSolver solver = solverObject.GetComponent<ObiSolver>();
        ObiFixedUpdater updater = solverObject.GetComponent<ObiFixedUpdater>();

        // add the solver to the updater:
        updater.solvers.Add(solver);
        // create the blueprint: (ltObiRopeBlueprint, ObiRodBlueprint)
        var blueprint = (Obi.ObiRopeBlueprint)Resources.Load("rope blueprint 1");

        // create a rope:
        GameObject ropeObject = new GameObject("rope", typeof(ObiRope), typeof(ObiRopeExtrudedRenderer));

        // get component references:
        ObiRope rope = ropeObject.GetComponent<ObiRope>();
        ObiRopeExtrudedRenderer ropeRenderer = ropeObject.GetComponent<ObiRopeExtrudedRenderer>();

        // load the default rope section:
        ropeRenderer.section = Resources.Load<ObiRopeSection>("DefaultRopeSection");

        // instantiate and set the blueprint:
        rope.ropeBlueprint = blueprint;

        //ropeObject.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/RedRope");

        // parent the cloth under a solver to start simulation:
        rope.transform.parent = solver.transform;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
