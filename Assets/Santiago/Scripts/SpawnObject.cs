using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class SpawnObject : NetworkBehaviour
{
    //private XRGrabInteractable xrGrabInteractable;
    //[SerializeField]
    //private GameObject spawnObject;

    //[SerializeField]
    //private GameObject targetSpawn;

    //[SerializeField]
    //private NetworkManager networkManager;

    //private bool spawn = false;
    //// Start is called before the first frame update

    //private void Awake()
    //{

    //}
    //public override void OnNetworkSpawn()
    //{
    //    if (IsServer)
    //    {
    //        ObjectSpawnServerRpc();
    //    }
    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    //[ServerRpc]
    //private void ObjectSpawnServerRpc()
    //{
    //    GameObject obj = Instantiate(spawnObject, targetSpawn.transform.position, Quaternion.identity);
    //    obj.GetComponent<NetworkObject>().Spawn(true);
    //}

    private XRGrabInteractable m_GrabInteractable;

    private Rigidbody m_RigidBody;
    private NetworkObject m_NetObject;

    private Vector3 m_Position;
    private Quaternion m_Rotation;
    private Vector3 m_Scale;
    private Vector3 m_Velocity;

    private bool m_NeedsMoving = false;

    private ulong m_ClientID;

    //private const double m_IdleTime = 1000; //milliseconds
    //private double m_TimeMove = 0;

    public override void OnNetworkSpawn()
    {
        m_ClientID = NetworkManager.Singleton.LocalClientId;
        m_NetObject = gameObject.GetComponent<NetworkObject>();
        m_RigidBody = gameObject.GetComponent<Rigidbody>();
        m_GrabInteractable = gameObject.GetComponent<XRGrabInteractable>();

        if (m_GrabInteractable != null)
        {
            //Debug.Log("Grabbing");
            m_GrabInteractable.selectEntered.AddListener(OnGrabbed);
            m_GrabInteractable.selectExited.AddListener(OnLetGo);

        }
        else
        {
            Debug.Log("Cannot Grab");
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        SendMoveToServerRpc(transform.position, transform.rotation, transform.localScale, m_RigidBody.velocity, m_ClientID);
    }

    private void OnLetGo(SelectExitEventArgs args)
    {
        //Debug.Log("Sending info from OnLetGo");
        SendMoveToServerRpc(transform.position, transform.rotation, transform.localScale, m_RigidBody.velocity, m_ClientID);
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Sending info from OnCollisionEnter");
        SendMoveToServerRpc(transform.position, transform.rotation, transform.localScale, m_RigidBody.velocity, m_ClientID);
    }

    private void GenerateMove()
    {
        if (IsOwner && transform.hasChanged)
        {
            //Debug.Log("Sending move info from server");
            SendMoveToServerRpc(transform.position, transform.rotation, transform.localScale, m_RigidBody.velocity, m_ClientID);
            transform.hasChanged = false;
        }
    }

    private void ClientMove()
    {
        if (!IsOwner && m_NeedsMoving)
        {
            //m_RigidBody.velocity = m_Velocity;
            //Debug.Log("Client move on client" + m_Position.ToString() + m_Rotation.ToString());
            m_RigidBody.Move(m_Position, m_Rotation);
            m_NeedsMoving = false;
            // transform.localScale = m_Scale;
        }
    }

    private void FixedUpdate()
    {
        GenerateMove();
        ClientMove();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendMoveToServerRpc(Vector3 position, Quaternion rotation, Vector3 scale, Vector3 velocity, ulong clientID)
    {
        // var networkTime = NetworkManager.Singleton.ServerTime.TimeAsFloat;
        Debug.Log("Client move from server" + position.ToString() + rotation.ToString() + scale.ToString());
        m_NetObject.ChangeOwnership(clientID);
        SendMoveToClientRpc(position, rotation, scale, velocity);
    }

    [ClientRpc]
    private void SendMoveToClientRpc(Vector3 position, Quaternion rotation, Vector3 scale, Vector3 velocity)
    {
        m_Position = position;
        m_Rotation = rotation;
        m_Scale = scale;
        m_Velocity = velocity;
        m_NeedsMoving = true;
    }
}
