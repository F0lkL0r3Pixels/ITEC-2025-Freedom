using UnityEngine;

public class Grappling : MonoBehaviour
{
    public float spring = 6f;
    public float damper = 5f;
    public float massScale = 8f;
    public bool canGrapple;
    public LayerMask whatIsGrappleable;
    public Transform gunTip, player;

    private LineRenderer lr;
    private Vector3 grapplePoint;
    private Transform playerCam;
    private float maxDistance = 20f;
    private SpringJoint joint;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        playerCam = Camera.main.transform;
    }

    void Update()
    {
        GameManager.UIManager.crosshairController.ApplyState(Physics.Raycast(playerCam.position, playerCam.forward, maxDistance, whatIsGrappleable));

        if (GameManager.InputMaster.Player.Attack.WasPerformedThisFrame())
        {
            StartGrapple();
        }
        else if (GameManager.InputMaster.Player.Attack.WasReleasedThisFrame())
        {
            StopGrapple();
        }
    }

    //Called after Update
    void LateUpdate()
    {
        DrawRope();
    }

    /// <summary>
    /// Call whenever we want to start a grapple
    /// </summary>
    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, maxDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            //The distance grapple will try to keep from grapple point. 
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            //Adjust these values to fit your game.
            joint.spring = spring;
            joint.damper = damper;
            joint.massScale = massScale;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        }
    }


    /// <summary>
    /// Call whenever we want to stop a grapple
    /// </summary>
    void StopGrapple()
    {
        lr.positionCount = 0;
        Destroy(joint);
        canGrapple = false;
    }

    private Vector3 currentGrapplePosition;

    void DrawRope()
    {
        //If not grappling, don't draw rope
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }

    public bool IsGrappling()
    {
        return joint != null;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }

    public void SetCanGrapple(bool value)
    {
        canGrapple = value;
    }
}