using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody _rb;
    private Camera _mainCam;
    private Vector3 _aimPoint;
    private float _currentSpeed;

    [SerializeField] private InputReader _input;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _runSpeed = 5f;
    [SerializeField] private float _groundPlaneHeight;
    [SerializeField] private Transform _aimPivot;
    [SerializeField] private float _aimSmoothing = 10f;
    [SerializeField] private PlayerDash _dash;
    [SerializeField] private Animator _animator;

    public Vector3 AimPoint => _aimPoint;
    public Vector3 AimDirection => (_aimPoint - transform.position).normalized;
    public Vector3 MoveDirection {get; private set;} //da a entender q esto solo se puede cambiar en el script, no afuera
    public float CurrentSpeed => _currentSpeed;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        _rb.interpolation = RigidbodyInterpolation.Interpolate; //es pa q la compu sepa q onda jaja, tu posicion
        _mainCam = Camera.main;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateAiming();
    }

    private void UpdateAiming()
    {
        Vector2 mousePos = Input.mousePosition;
        Ray ray = _mainCam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0f));

        Plane ground = new Plane(Vector3.up, new Vector3(0f, _groundPlaneHeight, 0f));
        if (ground.Raycast(ray, out float distance)) //hace q esa variable creada solo se pueda usar ahi
        {
            _aimPoint = ray.GetPoint(distance);
        }

        Vector3 lookDir = _aimPoint - _aimPivot.position; //si restas vectores ej. a-b, b se dirigira a a
        lookDir.y = 0f;

        if (lookDir.magnitude > 0.1f) //hace q cuando el mouse este directamente en el personaje no de vueltas sin parar
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir); //q no haya ambiguedad, ej.si posicion es x/y/z, si c repite bleh so hace q hayan 4
            _aimPivot.rotation = Quaternion.Slerp(_aimPivot.rotation, targetRotation, _aimSmoothing* Time.deltaTime); //lerp es ej. (a=1,b=2,alpha=1.5) so slerp es eso pero en esfera
            //time.deltatime hace q se promedie y regula el tiempo (pq estamos en slo update, el cual varia sus fps)
        }
    }
    private void FixedUpdate() 
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        if (_dash != null && _dash.IsDashing) return;

        Vector2 rawInput = _input.Move; //wasd 2d
        Vector3 inputDir = new Vector3(rawInput.x, 0f, rawInput.y); //der,izq,arriba,abajo,frente,atraz

        if (_mainCam != null)
        {
            Vector3 camFoward = _mainCam.transform.forward;
            Vector3 camRight = _mainCam.transform.right;
            camFoward.y = 0f;
            camRight.y = 0f;

            camFoward.Normalize(); //hace q se haga lenght 0-1, for direction not speed. osea q sea always d same
            camRight.Normalize();

            inputDir = camRight * rawInput.x + camFoward * rawInput.y; //hace q se hagan esas cosas when used..
        }

        MoveDirection = inputDir;

        float currentSpeed = _moveSpeed;
        if (_input.Sprint) currentSpeed = _runSpeed;
        _animator.SetBool("isRunning", _input.Sprint);

        _rb.linearVelocity = inputDir * currentSpeed;
        _animator.SetFloat("Blend", _rb.linearVelocity.magnitude / _moveSpeed);
    }
}