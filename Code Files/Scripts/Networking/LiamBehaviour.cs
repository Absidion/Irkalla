using UnityEngine;

//Writer: Guess
//Modified: 11/19/2017

public class LiamBehaviour : SyncBehaviour
{
    [HideInInspector]
    new public Rigidbody rigidbody;                             //Override the behaviours rigidbody value and make it a useable variable
    [HideInInspector]
    new public MeshRenderer renderer;                           //Override the behaviours renderer value and make it a useable variable

    public enum UpdateMode { None, Set, Lerp }                  //Enum used for determinning what states will be synced

    [SerializeField]private UpdateMode Position = UpdateMode.Lerp;               //Determines how position should be synced over the network
    [SerializeField]private UpdateMode Rotation = UpdateMode.Lerp;               //Determines how the rotation should be synced over the network
    [SerializeField]private UpdateMode Scale = UpdateMode.None;                  //Determines how the scale shold be synced over the network
    [SerializeField]private UpdateMode Velocity = UpdateMode.Set;                //Determines how the velocity should be synced over the network
    [SerializeField]private UpdateMode AngularVelocity = UpdateMode.Set;         //Determines how the angular velocity should be synced over the network
                    
    [SerializeField]private bool UseLocalVariables = true;                       //Determines if the syncing should be done using local variables or world position varaibles
    [SerializeField]private bool OnChangeOnly = true;                            //Determines if obejcts should be synced when they're changed or when just all the time
    [SerializeField]private bool InterpolationEnabled = true;                    //Determines if objects should use interpolation or extrapolation

    private ComponentInterpolator[] m_ComponentInterpolators;   //The components that are registered as sycnable (currently only supports Transform and Rigidbody)

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        rigidbody = GetComponent<Rigidbody>();

        m_ComponentInterpolators = new ComponentInterpolator[rigidbody == null ? 1 : 2];
        m_ComponentInterpolators[0] = new ComponentInterpolator(this, transform);
        if (rigidbody != null)
            m_ComponentInterpolators[1] = new ComponentInterpolator(this, rigidbody);
    }

    protected virtual void Update()
    {
        if (!photonView.isMine)
        {
            double currentTime = PhotonNetwork.time;
            double interpolationTime = currentTime - 0.15;//GetInterpolationBackTime();
            foreach (ComponentInterpolator ci in m_ComponentInterpolators)
            {
                ci.Update(interpolationTime);
            }
        }
    }
    #endregion

    #region Basic RPC Functionality

    //set the name over the network
    public void SetName(string newName)
    {
        photonView.RPC("RPCSetNameOverNetwork", PhotonTargets.All, newName);
    }

    [PunRPC]
    protected void RPCSetNameOverNetwork(string newName)
    {
        name = newName;
    }
    #endregion

    #region Network Activation
    //sets the component to be active over the network
    public void SetActive(bool active)
    {
        photonView.RPC("RPCSetActiveOverNetwork", PhotonTargets.All, active);
    }

    [PunRPC]
    protected void RPCSetActiveOverNetwork(bool active)
    {
        gameObject.SetActive(active);
    }

    //sets the component to be active over the network at a specific position
    public void SetActive(bool active, Vector3 position)
    {
        photonView.RPC("RPCSetActiveOverNetworkAtPosition", PhotonTargets.All, active, position);
    }

    [PunRPC]
    protected void RPCSetActiveOverNetworkAtPosition(bool active, Vector3 position)
    {
        transform.position = position;

        gameObject.SetActive(active);
    }

    //sets the component to be active over the network at a specific position and rotation
    public void SetActive(bool active, Vector3 position, Vector3 rotation)
    {
        photonView.RPC("RPCSetActiveOverNetworkAtPositionAndRotation", PhotonTargets.All, active, position, rotation);
    }

    [PunRPC]
    protected void RPCSetActiveOverNetworkAtPositionAndRotation(bool active, Vector3 position, Vector3 rotation)
    {
        transform.position = position;
        transform.rotation = Quaternion.Euler(rotation);

        gameObject.SetActive(active);
    }

    //sets the component to be active over the network at a specific position and rotation with a velocity and direction. This is mainly useful for physics bodies
    public void SetActive(bool active, Vector3 position, Vector3 rotation, Vector3 direction, float speed)
    {
        if (rigidbody == null)
        {
            Debug.LogError("Attempting to sync data accross the network using a rigidbody that is null, please fix the issue and try again. " + this);
        }
        else
        {
            photonView.RPC("RPCSetActiveOverNetworkRigidBody", PhotonTargets.All, active, position, rotation, direction, speed);
        }
    }

    [PunRPC]
    protected void RPCSetActiveOverNetworkRigidBody(bool active, Vector3 position, Vector3 rotation, Vector3 direction, float speed)
    {
        transform.position = position;
        transform.rotation = Quaternion.Euler(rotation);

        gameObject.SetActive(active);

        //reset the velocity and then add a force in the normalized direction at a specfic speed
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(direction * speed, ForceMode.VelocityChange);
    }

    #endregion

    #region Transformation RPCS
    public void SetDirection(Vector3 direction)
    {
        photonView.RPC("RPCSetDirection", PhotonTargets.All, direction);
    }

    [PunRPC]
    protected void RPCSetDirection(Vector3 direction)
    {
        transform.forward = direction;
    }
    #endregion

    #region Physics RPCs
    //sets is kinematic to be true/false so a rigid body doesn't get affected by forces.
    public void SetIsKinematic(bool flag)
    {
        if (rigidbody != null)
        {
            photonView.RPC("RPCSetIsKinematic", PhotonTargets.All, flag);
        }
        else
        {
            Debug.Log("Object with no rigid body is trying to set isKinematic");
        }
    }

    [PunRPC]
    protected void RPCSetIsKinematic(bool flag)
    {
        rigidbody.isKinematic = flag;
    }

    public void ApplyForceOverNetwork(Vector3 force, ForceMode mode)
    {
        if (rigidbody == null)
        {
            return;
        }

        photonView.RPC("RPCApplyForceOverNetwork", PhotonTargets.All, force, mode);
    }

    [PunRPC]
    protected void RPCApplyForceOverNetwork(Vector3 force, ForceMode mode)
    {
        rigidbody.AddForce(force, mode);
    }

    public void SetUseGravity(bool trigger)
    {
        photonView.RPC("RPCSetUseGravity", PhotonTargets.All, trigger);
    }

    [PunRPC]
    protected void RPCSetUseGravity(bool trigger)
    {
        rigidbody.useGravity = false;
    }

    public void SetVelocity(Vector3 velocity)
    {
        photonView.RPC("RPCSetVelocity", PhotonTargets.All, velocity);
    }

    [PunRPC]
    protected void RPCSetVelocity(Vector3 velocity)
    {
        rigidbody.velocity = velocity;
    }
    #endregion

    #region Nav Mesh RPCs
    //set navmeshagent to be true/false. useful if you need to do alternative movement
    public void SetNavMeshAgent(bool flag)
    {
        photonView.RPC("RPCSetNavMeshAgent", PhotonTargets.All, flag);
    }

    [PunRPC]
    protected void RPCSetNavMeshAgent(bool flag)
    {
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (agent != null)
        {
            agent.enabled = flag;
        }
        else
        {
            Debug.Log("Gameobject with no NavMesh component is trying to disable/enable it.");
        }
    }
    #endregion

    #region Renderer RPCs

    public void SetRendererActive(bool active)
    {
        photonView.RPC("RPCSetRendererActive", PhotonTargets.All, active);
    }

    [PunRPC]
    protected void RPCSetRendererActive(bool active)
    {
        renderer.enabled = false;
    }

    #endregion

    #region Serialization
    protected override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);

        foreach (ComponentInterpolator ci in m_ComponentInterpolators)
        {
            ci.OnPhotonSerializeView(stream, info);
        }
    }

    private double GetInterpolationBackTime()
    {
        int interpolaitionBackTime;
        int ping = PhotonNetwork.GetPing();
        if (ping < 50)
        {
            interpolaitionBackTime = 100;
        }
        else if (ping < 100)
        {
            interpolaitionBackTime = 150;
        }
        else if (ping < 200)
        {
            interpolaitionBackTime = 250;
        }
        else if (ping < 400)
        {
            interpolaitionBackTime = 450;
        }
        else if (ping < 600)
        {
            interpolaitionBackTime = 650;
        }
        else
        {
            interpolaitionBackTime = 1050;
        }
        return interpolaitionBackTime / 1000d;
    }

    private class ComponentInterpolator
    {
        private LiamBehaviour m_NetObject;
        private Component m_Component;

        internal struct State
        {
            internal double timestamp;
            internal Vector3 position;
            internal Quaternion rotation;
            internal Vector3 scale;
            internal Vector3 velocity;
            internal Vector3 angVelocity;
        }

        private State[] m_States = new State[20];
        private int m_LastRecievedSlot = 0;
        private int m_NextFreeSlot = 0;
        private int m_SlotsUsed = 0;

        public ComponentInterpolator(LiamBehaviour netObject, Component component)
        {
            m_NetObject = netObject;
            m_Component = component;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;
            Vector3 scale = Vector3.zero;
            Vector3 velocity = Vector3.zero;
            Vector3 angularVelocity = Vector3.zero;

            //If the photon stream is currently writing
            if (stream.isWriting)
            {
                if (m_Component is Transform)
                {
                    Transform transform = m_Component.transform;
                    //if the transform is going to be sycning local values 
                    if (m_NetObject.UseLocalVariables)
                    {
                        pos = transform.localPosition;
                        rot = transform.localRotation;
                    }
                    //if the transform is going to be syncing world values
                    else
                    {
                        pos = transform.position;
                        rot = transform.rotation;
                    }
                    //only 1 form of scale to sync
                    scale = transform.localScale;

                    //If the component doesn't care about syncing on change OR
                    //the number of slots used is 0 meaning there is no stored data OR
                    //there has been a change in any of the relevant data
                    if (!m_NetObject.OnChangeOnly || m_SlotsUsed == 0 || HasChanged(pos, rot, scale, velocity, angularVelocity))
                    {
                        //if the transform values are set to be able to be synced then there data will be serialized
                        if (m_NetObject.Position != UpdateMode.None)
                        {
                            stream.Serialize(ref pos);
                        }
                        if (m_NetObject.Rotation != UpdateMode.None)
                        {
                            stream.Serialize(ref rot);
                        }
                        if (m_NetObject.Scale != UpdateMode.None)
                        {
                            stream.Serialize(ref scale);
                        }
                    }
                }
                else if (m_Component is Rigidbody)
                {
                    Rigidbody rbody = (Rigidbody)m_Component;
                    velocity = rbody.velocity;
                    angularVelocity = rbody.angularVelocity;
                    if (!m_NetObject.OnChangeOnly || m_SlotsUsed == 0 || HasChanged(pos, rot, scale, velocity, angularVelocity))
                    {
                        if (m_NetObject.Velocity != UpdateMode.None)
                        {
                            stream.Serialize(ref velocity);
                        }
                        if (m_NetObject.AngularVelocity != UpdateMode.None)
                        {
                            stream.Serialize(ref angularVelocity);
                        }
                    }
                }

                if (m_NetObject.OnChangeOnly)
                {
                    //keep a copy of the state
                    State state;
                    state.timestamp = info.timestamp;
                    state.position = pos;
                    state.rotation = rot;
                    state.scale = scale;
                    state.velocity = velocity;
                    state.angVelocity = angularVelocity;

                    m_States[0] = state;
                    m_SlotsUsed = 1;
                }
            }
            //If the photon stream is currently reading
            else
            {
                if (m_Component is Transform)
                {
                    //deserialize the required data for the transform component
                    if (m_NetObject.Position != UpdateMode.None)
                    {
                        stream.Serialize(ref pos);
                    }
                    if (m_NetObject.Rotation != UpdateMode.None)
                    {
                        stream.Serialize(ref rot);
                    }
                    if (m_NetObject.Scale != UpdateMode.None)
                    {
                        stream.Serialize(ref scale);
                    }
                }
                else if (m_Component is Rigidbody)
                {
                    //deserialize the required data for the rigidbody component
                    if (m_NetObject.Velocity != UpdateMode.None)
                    {
                        stream.Serialize(ref velocity);
                    }
                    if (m_NetObject.AngularVelocity != UpdateMode.None)
                    {
                        stream.Serialize(ref angularVelocity);
                    }
                }

                if (m_SlotsUsed == 0 || m_States[m_LastRecievedSlot].timestamp <= info.timestamp)
                {
                    State state;
                    state.timestamp = info.timestamp;
                    state.position = pos;
                    state.rotation = rot;
                    state.scale = scale;
                    state.velocity = velocity;
                    state.angVelocity = angularVelocity;

                    m_States[m_NextFreeSlot] = state;
                    m_LastRecievedSlot = m_NextFreeSlot;
                    m_NextFreeSlot = (m_NextFreeSlot + 1) % m_States.Length;
                    m_SlotsUsed = Mathf.Min(m_SlotsUsed + 1, m_States.Length);
                }
            }

        }

        private bool HasChanged(Vector3 pos, Quaternion rot, Vector3 scale, Vector3 velocity, Vector3 angularVelocity)
        {
            //TODO: Add actual checks to make sure that this method can work with the on change logic
            return true;
        }

        public void Update(double interpolaitionTime)
        {
            if (m_SlotsUsed > 0)
            {
                State latest = m_States[m_LastRecievedSlot];
                //This logic is what is refered to as interpolation
                if (m_NetObject.InterpolationEnabled && latest.timestamp > interpolaitionTime)
                {
                    for (int n = 0; n < m_SlotsUsed; n++)
                    {
                        int i = (m_LastRecievedSlot + m_States.Length - n) % m_States.Length;
                        if (m_States[i].timestamp <= interpolaitionTime || n == (m_States.Length - 1))
                        {
                            int previous = (n == 0) ? i : (i + 1) % m_States.Length;
                            State rhs = m_States[previous];
                            State lhs = m_States[i];

                            double length = rhs.timestamp - lhs.timestamp;

                            float time = 0.0f;

                            if (length > 0.0001)
                                time = (float)((interpolaitionTime - lhs.timestamp) / length);

                            UpdateStates(lhs, rhs, time);
                            break;
                        }
                    }
                }
                //This logic is what is extrapolation
                //TODO: do more logical extrapolation
                else
                {
                    // Use extrapolation. Here we do something really simple and just repeat the last
                    // received state. You can do clever stuff with predicting what should happen.
                    if (m_Component is Transform)
                    {
                        Transform transform = m_Component.transform;
                        if (m_NetObject.UseLocalVariables)
                        {
                            if (m_NetObject.Position != UpdateMode.None)
                                transform.localPosition = latest.position;
                            if (m_NetObject.Rotation != UpdateMode.None)
                                transform.localRotation = latest.rotation;
                        }
                        else
                        {
                            if (m_NetObject.Position != UpdateMode.None)
                                transform.position = latest.position;
                            if (m_NetObject.Rotation != UpdateMode.None)
                                transform.rotation = latest.rotation;
                        }
                        if (m_NetObject.Scale != UpdateMode.None)
                            transform.localScale = latest.scale;
                    }
                    else if (m_Component is Rigidbody)
                    {
                        Rigidbody rbody = (Rigidbody)m_Component;
                        if (m_NetObject.Velocity != UpdateMode.None)
                        {
                            rbody.velocity = latest.velocity;
                        }
                        if (m_NetObject.AngularVelocity != UpdateMode.None)
                        {
                            rbody.angularVelocity = latest.angVelocity;
                        }
                    }
                }
            }
        }

        protected void UpdateStates(State lhs, State rhs, float time)
        {
            if (m_Component is Transform)
            {
                //make a reference to the transform component so that its values may be set
                Transform transform = m_Component.transform;

                if(m_NetObject.UseLocalVariables)
                {
                    //Position
                    if(m_NetObject.Position == UpdateMode.Set)
                    {
                        transform.localPosition = lhs.position;
                    }
                    else if(m_NetObject.Position == UpdateMode.Lerp)
                    {
                        transform.localPosition = Vector3.Lerp(lhs.position, rhs.position, time);
                    }
                    //Rotation
                    if(m_NetObject.Rotation == UpdateMode.Set)
                    {
                        transform.localRotation = lhs.rotation;
                    }
                    else if(m_NetObject.Rotation == UpdateMode.Lerp)
                    {
                        transform.localRotation = Quaternion.Slerp(lhs.rotation, rhs.rotation, time);
                    }
                }
                else
                {
                    //Position
                    if (m_NetObject.Position == UpdateMode.Set)
                    {
                        transform.position = lhs.position;
                    }
                    else if (m_NetObject.Position == UpdateMode.Lerp)
                    {
                        transform.position = Vector3.Lerp(lhs.position, rhs.position, time);
                    }
                    //Rotation
                    if (m_NetObject.Rotation == UpdateMode.Set)
                    {
                        transform.rotation = lhs.rotation;
                    }
                    else if (m_NetObject.Rotation == UpdateMode.Lerp)
                    {
                        transform.rotation = Quaternion.Slerp(lhs.rotation, rhs.rotation, time);
                    }
                }
                //Scale
                if (m_NetObject.Scale == UpdateMode.Set)
                {
                    transform.localScale = lhs.scale;
                }
                else if (m_NetObject.Scale == UpdateMode.Lerp)
                {
                    transform.localScale = Vector3.Lerp(lhs.scale, rhs.scale, time);
                }
            }
            else if (m_Component is Rigidbody)
            {
                Rigidbody rbody = (Rigidbody)m_Component;

                if(m_NetObject.Velocity == UpdateMode.Set)
                {
                    rbody.velocity = lhs.velocity;
                }
                else if(m_NetObject.Velocity == UpdateMode.Lerp)
                {
                    rbody.velocity = Vector3.Lerp(lhs.velocity, rhs.velocity, time);
                }
                if(m_NetObject.AngularVelocity == UpdateMode.Set)
                {
                    rbody.angularVelocity = lhs.angVelocity;
                }
                else if(m_NetObject.AngularVelocity == UpdateMode.Lerp)
                {
                    rbody.angularVelocity = Vector3.Lerp(lhs.angVelocity, rhs.angVelocity, time);
                }
            }
        }
    }
    #endregion
}
