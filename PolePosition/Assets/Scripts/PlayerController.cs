using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class PlayerController : NetworkBehaviour
{
    #region Variables publicas
    [Header("Movement")]
    [Tooltip("Lista con la información de los ejes")] public List<AxleInfo> axleInfos;
    [Tooltip("Potencia del motor hacia delante")] public float forwardMotorTorque = 100000;
    [Tooltip("Potencia del motor hacia atras")] public float backwardMotorTorque = 50000;
    [Tooltip("Maximo angulo de giro")] public float maxSteeringAngle = 15;
    [Tooltip("Freno de motor")] public float engineBrake = 1e+12f;
    [Tooltip("Freno de pie")] public float footBrake = 1e+24f;
    [Tooltip("Velocidad maxima")] public float topSpeed = 200f;
    [Tooltip("Fuerza de traccion hacia abajo")] public float downForce = 100f;
    [Tooltip("Limite de deslizamiento")] public float slipLimit = 0.2f;

    [HideInInspector] public GameObject nameTag;        // Name Tag del jugador (se encuentra encima del coche)
    #endregion

    #region Variables privadas

    private float CurrentRotation { get; set; }     // Rotacion actual del coche
    private float InputAcceleration { get; set; }   // Input de aceleracion
    private float InputSteering { get; set; }       // Input de giro
    private float InputBrake { get; set; }          // Input de freno

    private float m_SteerHelper = 0.8f;             // Ayuda al giro
    private float m_CurrentSpeed = 0;               // Velocidad actual

    private Transform transformCamera;              // Transform de la cámara que sigue al vehículo

    private PlayerInfo m_PlayerInfo;                // Referencia al PlayerInfo
    private Rigidbody m_Rigidbody;                  // Referencia al rigidody
    
    // Velocidad del coche
    private float Speed
    {
        get { return m_CurrentSpeed; }
        set
        {
            if (Math.Abs(m_CurrentSpeed - value) < float.Epsilon) return;
            m_CurrentSpeed = value;
            if (OnSpeedChangeEvent != null)
                OnSpeedChangeEvent(m_CurrentSpeed);
        }
    }

    // Delegado que actualiza la velocidad
    public delegate void OnSpeedChangeDelegate(float newVal);

    // Evento del delegado (mas bien handler)
    public event OnSpeedChangeDelegate OnSpeedChangeEvent;
    #endregion Variables

    #region Unity Callbacks

    /// <summary>
    /// Función Awake, que inicializa las siguientes variables
    /// </summary>
    public void Awake()
    {
        // Se obtienen las referencias
        m_Rigidbody = GetComponent<Rigidbody>();
        m_PlayerInfo = GetComponent<PlayerInfo>();
    }

    /// <summary>
    /// Función Start, que obtiene el transform de la camara
    /// </summary>
    private void Start()
    {
        transformCamera = GameObject.FindGameObjectWithTag("MainCamera").transform; // Se busca la cámara que sigue al jugador
    }

    /// <summary>
    /// Función Update, que se ejecuta cada frame
    /// </summary>
    public void Update()
    {
        // Si el jugador puede moverse, se obtienen sus inputs
        if (m_PlayerInfo.canMove)
        {
            InputAcceleration = Input.GetAxis("Vertical");
            InputSteering = Input.GetAxis(("Horizontal"));
            InputBrake = Input.GetAxis("Jump");
            Speed = m_Rigidbody.velocity.magnitude;
        }
        // Si no, y ha terminado la carrera, sus inputs valen 0
        else if (m_PlayerInfo.hasFinished)
        {
            InputAcceleration = 0;
            InputSteering = 0;
            InputBrake = 0;
            Speed = 0;
            m_Rigidbody.Sleep();
        }

        // Se actualiza el nameTag
        nameTag.transform.LookAt(transformCamera); // El nametag del jugador mira a la cámara
        nameTag.transform.rotation = Quaternion.LookRotation(transform.position - transformCamera.position); // Se rota el nametag para que no se vea al revés
    }

    /// <summary>
    /// Función FixedUpdate, que se ejecuta constantemente con velocidad fija de fotograma
    /// </summary>
    public void FixedUpdate()
    {
        // Si el jugador no ha terminado
        if (!m_PlayerInfo.hasFinished)
        {
            // Se capan los inputs entre 0 y 1
            InputSteering = Mathf.Clamp(InputSteering, -1, 1);
            InputAcceleration = Mathf.Clamp(InputAcceleration, -1, 1);
            InputBrake = Mathf.Clamp(InputBrake, 0, 1);

            // Se calcula el giro
            float steering = maxSteeringAngle * InputSteering;

            // Por cada eje
            foreach (AxleInfo axleInfo in axleInfos)
            {
                // Si esta activado el giro, giramos el eje
                if (axleInfo.steering)
                {
                    axleInfo.leftWheel.steerAngle = steering;
                    axleInfo.rightWheel.steerAngle = steering;
                }

                // Si esta activado el motor
                if (axleInfo.motor)
                {
                    // Si esta acelerando, asignamos la potencia del motor
                    if (InputAcceleration > float.Epsilon)
                    {
                        axleInfo.leftWheel.motorTorque = forwardMotorTorque;
                        axleInfo.leftWheel.brakeTorque = 0;
                        axleInfo.rightWheel.motorTorque = forwardMotorTorque;
                        axleInfo.rightWheel.brakeTorque = 0;
                    }

                    // Si esta decelerando, asignamos la potencia hacia atras del motor
                    if (InputAcceleration < -float.Epsilon)
                    {
                        axleInfo.leftWheel.motorTorque = -backwardMotorTorque;
                        axleInfo.leftWheel.brakeTorque = 0;
                        axleInfo.rightWheel.motorTorque = -backwardMotorTorque;
                        axleInfo.rightWheel.brakeTorque = 0;
                    }

                    // Si no hay aceleracion, lo establecemos todo a 0
                    if (Math.Abs(InputAcceleration) < float.Epsilon)
                    {
                        axleInfo.leftWheel.motorTorque = 0;
                        axleInfo.leftWheel.brakeTorque = engineBrake;
                        axleInfo.rightWheel.motorTorque = 0;
                        axleInfo.rightWheel.brakeTorque = engineBrake;
                    }
                    
                    // Si hay input de freno, se actualiza el freno de las ruedas
                    if (InputBrake > 0)
                    {
                        axleInfo.leftWheel.brakeTorque = footBrake;
                        axleInfo.rightWheel.brakeTorque = footBrake;
                    }
                }

                // Se aplican las posiciones
                ApplyLocalPositionToVisuals(axleInfo.leftWheel);
                ApplyLocalPositionToVisuals(axleInfo.rightWheel);
            }

            // Se llaman a los callbacks correspondientes
            SteerHelper();
            SpeedLimiter();
            AddDownForce();
            TractionControl();
        }
    }

    #endregion

    #region Methods

    // crude traction control that reduces the power to wheel if the car is wheel spinning too much
    private void TractionControl()
    {
        foreach (var axleInfo in axleInfos)
        {
            WheelHit wheelHitLeft;
            WheelHit wheelHitRight;
            axleInfo.leftWheel.GetGroundHit(out wheelHitLeft);
            axleInfo.rightWheel.GetGroundHit(out wheelHitRight);

            if (wheelHitLeft.forwardSlip >= slipLimit)
            {
                var howMuchSlip = (wheelHitLeft.forwardSlip - slipLimit) / (1 - slipLimit);
                axleInfo.leftWheel.motorTorque -= axleInfo.leftWheel.motorTorque * howMuchSlip * slipLimit;
            }

            if (wheelHitRight.forwardSlip >= slipLimit)
            {
                var howMuchSlip = (wheelHitRight.forwardSlip - slipLimit) / (1 - slipLimit);
                axleInfo.rightWheel.motorTorque -= axleInfo.rightWheel.motorTorque * howMuchSlip * slipLimit;
            }

            // Corrección de la deriva
            // Se guarda en una curva de fricción la información de la fricción lateral
            WheelFrictionCurve wheelCurve = axleInfo.leftWheel.sidewaysFriction;

            // Se modifica el límite de fricción dependiendo de si el coche está o no en movimiento
            wheelCurve.extremumSlip = (m_Rigidbody.velocity.magnitude > 0.2f) ? 0.2f : 0.3f;

            // Asignamos la nueva curva a los valores de fricción lateral
            axleInfo.leftWheel.sidewaysFriction = wheelCurve;
            axleInfo.rightWheel.sidewaysFriction = wheelCurve;
        }
    }

// this is used to add more grip in relation to speed
    private void AddDownForce()
    {
        foreach (var axleInfo in axleInfos)
        {
            axleInfo.leftWheel.attachedRigidbody.AddForce(
                -transform.up * (downForce * axleInfo.leftWheel.attachedRigidbody.velocity.magnitude));
        }
    }

    private void SpeedLimiter()
    {
        float speed = m_Rigidbody.velocity.magnitude;
        if (speed > topSpeed)
            m_Rigidbody.velocity = topSpeed * m_Rigidbody.velocity.normalized;
    }

// finds the corresponding visual wheel
// correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider col)
    {
        if (col.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = col.transform.GetChild(0);
        Vector3 position;
        Quaternion rotation;
        col.GetWorldPose(out position, out rotation);
        var myTransform = visualWheel.transform;
        myTransform.position = position;
        myTransform.rotation = rotation;
    }

    private void SteerHelper()
    {
        foreach (var axleInfo in axleInfos)
        {
            WheelHit[] wheelHit = new WheelHit[2];
            axleInfo.leftWheel.GetGroundHit(out wheelHit[0]);
            axleInfo.rightWheel.GetGroundHit(out wheelHit[1]);
            foreach (var wh in wheelHit)
            {
                if (wh.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }
        }

// this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
        if (Mathf.Abs(CurrentRotation - transform.eulerAngles.y) < 10f)
        {
            var turnAdjust = (transform.eulerAngles.y - CurrentRotation) * m_SteerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnAdjust, Vector3.up);
            m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
        }

        CurrentRotation = transform.eulerAngles.y;
    }

    #endregion
}