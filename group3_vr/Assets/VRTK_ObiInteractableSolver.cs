using System.Collections;
using UnityEngine;
using Obi;
using VRTK;

[System.Serializable]
public class InteractingInfo
{
    [Header("Grab Options")]
    public bool grabbingEnabled = true;

    [Header("Pin Options")]
    public Vector3 pinOffset;
    [Range(0f, 1f)]
    public float stiffness = 1;

    [HideInInspector]
    public bool isTouchingParticle;
    [HideInInspector]
    public int touchingParticle;
    [HideInInspector]
    public int? grabbedParticle;
    [HideInInspector]
    public int? pinIndex;
    [HideInInspector]
    public VRTK_InteractGrab interactGrab;
    [HideInInspector]
    public Collider collider;
    [HideInInspector]
    public ObiCollider obiCollider;
    [HideInInspector]
    public VRTK_ObiInteractableActor currentInteraction;

    public void Clear()
    {
        isTouchingParticle = false;
    }
}

[RequireComponent(typeof(ObiSolver))]
public class VRTK_ObiInteractableSolver : MonoBehaviour
{
    #region Fields


    public Vector3 colliderSize;
    public Vector3 colliderOffset;
    public Vector3 colliderRotation;


    [Header("Controller Info")]

    public InteractingInfo leftControllerInfo = new InteractingInfo();
    public InteractingInfo rightControllerInfo = new InteractingInfo();

    private ObiSolver solver;

    private GameObject leftController;
    private GameObject rightController;

    #endregion


    #region Mono Events

    private void Awake()
    {
        solver = GetComponent<ObiSolver>();

        VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange(this);
    }

    private void OnDestroy()
    {
        VRTK_SDKManager.instance.RemoveBehaviourToToggleOnLoadedSetupChange(this);
    }

    private void Reset()
    {
        colliderSize = new Vector3(0.06f, 0.06f, 0.06f);
        colliderOffset = new Vector3(0, 0, .02f);
        colliderRotation = new Vector3(45, 0, 0);
    }

    private void OnEnable()
    {
        leftController = VRTK.VRTK_DeviceFinder.GetControllerLeftHand(false);
        rightController = VRTK.VRTK_DeviceFinder.GetControllerRightHand(false);

        leftController.GetComponent<VRTK_ControllerEvents>().ControllerModelAvailable += ControllerModelAvailiable;
        rightController.GetComponent<VRTK_ControllerEvents>().ControllerModelAvailable += ControllerModelAvailiable;

        if (!ReferenceEquals(solver, null))
        {
            solver.OnCollision += HandleSolverCollision;
        }
    }

    private void OnDisable()
    {
        if (!ReferenceEquals(solver, null))
        {
            solver.OnCollision -= HandleSolverCollision;
        }
    }

    #endregion


    #region Private Methods


    private bool SetupController(GameObject controller, bool isLeft)
    {
        if (controller.transform.childCount == 0) return false;

        GameObject colliderObject = new GameObject();
        colliderObject.name = "ObiCollider";
        colliderObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        colliderObject.transform.SetParent(controller.transform);
        colliderObject.transform.localPosition = colliderOffset;
        colliderObject.transform.localRotation = Quaternion.Euler(colliderRotation);

        var collider = colliderObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = colliderSize;

        var obiCollider = colliderObject.AddComponent<ObiCollider>();

        if (isLeft)
        {
            if (ReferenceEquals(obiCollider, null))
            {
                leftControllerInfo.obiCollider = colliderObject.gameObject.AddComponent<ObiCollider>();
            }
            else
            {
                leftControllerInfo.obiCollider = obiCollider;
            }

            leftControllerInfo.collider = colliderObject.GetComponent<Collider>();

            leftControllerInfo.interactGrab = controller.GetComponent<VRTK_InteractGrab>();
            leftControllerInfo.interactGrab.GrabButtonPressed += HandleGrabButtonPressed;
            leftControllerInfo.interactGrab.GrabButtonReleased += HandleGrabButtonReleased;
        }
        else
        {
            if (ReferenceEquals(obiCollider, null))
            {
                rightControllerInfo.obiCollider = colliderObject.gameObject.AddComponent<ObiCollider>();
            }
            else
            {
                rightControllerInfo.obiCollider = obiCollider;
            }

            rightControllerInfo.collider = colliderObject.GetComponent<Collider>();

            rightControllerInfo.interactGrab = controller.GetComponent<VRTK_InteractGrab>();
            rightControllerInfo.interactGrab.GrabButtonPressed += HandleGrabButtonPressed;
            rightControllerInfo.interactGrab.GrabButtonReleased += HandleGrabButtonReleased;
        }

        return true;
    }

    #endregion


    #region Controller Events

    private void ControllerModelAvailiable(object sender, ControllerInteractionEventArgs e)
    {
        StartCoroutine(ModelLoaded(sender as VRTK_ControllerEvents));
    }

    private IEnumerator ModelLoaded(VRTK_ControllerEvents events)
    {
        yield return new WaitForEndOfFrame();

        bool succeeded = true;

        if (ReferenceEquals(events.gameObject, leftController))
        {
            succeeded = SetupController(events.gameObject as GameObject, true);
        }
        else if (ReferenceEquals(events.gameObject, rightController))
        {
            succeeded = SetupController(events.gameObject as GameObject, false);
        }

        if (!succeeded)
            StartCoroutine(ModelLoaded(events as VRTK_ControllerEvents));
    }

    private void HandleGrabButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (ReferenceEquals(sender, leftControllerInfo.interactGrab) && leftControllerInfo.grabbingEnabled)
        {
            if (leftControllerInfo.isTouchingParticle)
            {
                // Get actor and apply constraint
                leftControllerInfo.currentInteraction.GrabActor(leftControllerInfo);
            }
        }
        else if (ReferenceEquals(sender, rightControllerInfo.interactGrab) && rightControllerInfo.grabbingEnabled)
        {
            if (rightControllerInfo.isTouchingParticle)
            {
                // Get actor and apply constraint
                rightControllerInfo.currentInteraction.GrabActor(rightControllerInfo);
            }
        }
    }

    private void HandleGrabButtonReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (rightControllerInfo.grabbedParticle != null && ReferenceEquals(sender, rightControllerInfo.interactGrab) && rightControllerInfo.grabbingEnabled)
        {
            rightControllerInfo.currentInteraction.ReleaseActor(rightControllerInfo);

            if (leftControllerInfo.pinIndex.HasValue && leftControllerInfo.isTouchingParticle && leftControllerInfo.pinIndex > rightControllerInfo.pinIndex)
                leftControllerInfo.pinIndex--;

            rightControllerInfo.grabbedParticle = null;
            rightControllerInfo.pinIndex = null;

        }
        else if (leftControllerInfo.grabbedParticle != null && ReferenceEquals(sender, leftControllerInfo.interactGrab) && leftControllerInfo.grabbingEnabled)
        {
            leftControllerInfo.currentInteraction.ReleaseActor(leftControllerInfo);

            if (rightControllerInfo.pinIndex.HasValue && rightControllerInfo.isTouchingParticle && rightControllerInfo.pinIndex > leftControllerInfo.pinIndex)
                rightControllerInfo.pinIndex--;

            leftControllerInfo.grabbedParticle = null;
            leftControllerInfo.pinIndex = null;
        }
    }

    #endregion


    #region Obi Solver Events

    private void HandleSolverCollision(object sender, Obi.ObiSolver.ObiCollisionEventArgs e)
    {
        if (e == null || e.contacts == null || !rightControllerInfo.grabbingEnabled && !leftControllerInfo.grabbingEnabled) return;

        // Reset every frame
        leftControllerInfo.Clear();
        rightControllerInfo.Clear();

        for (int i = 0; i < e.contacts.Count; ++i)
        {
            var contact = e.contacts[i];
            var pia = solver.particleToActor[e.contacts[i].particle];
            var actor = pia.actor;
            var particleIndex = pia.indexInActor;

            // make sure this is an actual contact
            if (contact.distance < 1F)
            {
                Collider collider = ObiColliderBase.idToCollider[contact.other] as Collider;

                if (collider.ToString().Contains("Left"))
                {
                    leftControllerInfo.isTouchingParticle = true;
                    leftControllerInfo.touchingParticle = particleIndex;
                    leftControllerInfo.currentInteraction = actor.gameObject.GetComponent<VRTK_ObiInteractableActor>();
                }
                else if (collider.ToString().Contains("Right"))
                {
                    rightControllerInfo.isTouchingParticle = true;
                    rightControllerInfo.touchingParticle = particleIndex;
                    rightControllerInfo.currentInteraction = actor.gameObject.GetComponent<VRTK_ObiInteractableActor>();
                }
            }
        }
    }

    #endregion

}