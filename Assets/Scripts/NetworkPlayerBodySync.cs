using Photon.Pun;
using UnityEngine;


//NetworkAvatarBoneSync V2.0 for main and puppet communication
public class NetworkPlayerBodySync : MonoBehaviourPunCallbacks, IPunObservable
{

    [SerializeField, Range(1, 60)]
    private float recordFPS = 30f;
    float time = 0;
    float fraction;

    public GameObject mainPlayer, puppetPlayer;

    public Transform[] inputChildTransforms;
    public Transform[] outputChildTransforms;

    public GameObject rootPos;

    public bool calibPos;


    //public GameObject leftEye, rightEye;
    public Transform mainPlayerRoot;
    private void Start()
    {
        if(inputChildTransforms.Length == 0){
            inputChildTransforms = mainPlayerRoot.GetComponentsInChildren<Transform>();
        }
        fraction = 1000 / recordFPS;

    }


    // Method to retrieve the positions and rotations of all child objects
    private void GetChildPositionsAndRotations(out Vector3[] childPositions, out Quaternion[] childRotations)
    {
        int childCount = inputChildTransforms.Length;
        childPositions = new Vector3[childCount];
        childRotations = new Quaternion[childCount];

        for (int i = 0; i < childCount; i++)
        {
            childPositions[i] = inputChildTransforms[i].position;
            childRotations[i] = inputChildTransforms[i].rotation;
        }
    }

    // Method to apply positions and rotations to all child objects
    private void SetChildPositionsAndRotations(Vector3[] childPositions, Quaternion[] childRotations)
    {
        int childCount = outputChildTransforms.Length;
        for (int i = 0; i < childCount; i++)
        {
            outputChildTransforms[i].position = childPositions[i];
            outputChildTransforms[i].rotation = childRotations[i];
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting && time >= fraction && photonView.IsMine)
        {
            Vector3[] childPositions;
            Quaternion[] childRotations;
            GetChildPositionsAndRotations(out childPositions, out childRotations);

            // Send data for the parent and all child objects
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(childPositions);
            stream.SendNext(childRotations);

            time = 0;
        }

        if (stream.IsReading && time >= fraction && !photonView.IsMine)
        {
            Vector3 parentPosition = (Vector3)stream.ReceiveNext();
            Quaternion parentRotation = (Quaternion)stream.ReceiveNext();

            // Receive data for all child objects
            Vector3[] receivedChildPositions = (Vector3[])stream.ReceiveNext();
            Quaternion[] receivedChildRotations = (Quaternion[])stream.ReceiveNext();

            // Apply data to the parent
            transform.position = parentPosition;
            transform.rotation = parentRotation;

            // Apply data to child objects
            SetChildPositionsAndRotations(receivedChildPositions, receivedChildRotations);

            time = 0;
        }
    }
}
