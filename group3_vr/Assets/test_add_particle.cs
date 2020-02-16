using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_add_particle : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        GameObject rope = GameObject.Find("Obi Rope");
        Obi.ObiParticleAttachment part = rope.AddComponent<Obi.ObiParticleAttachment>();
        Obi.ObiRope rope2 = rope.GetComponent<Obi.ObiRope>();
        //rope2.solver.part



        part.target = transform;
        part.particleGroup = rope2.blueprint.groups[0];
        Debug.Log(part.particleGroup);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
