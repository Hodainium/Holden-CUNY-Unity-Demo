using UnityEngine;


public class PlayerMovementScript : MonoBehaviour //slerp is next but refactoring and cleaning code.
{
    [SerializeField] Camera playerCamera;
    [SerializeField] Transform _playerTransform;
    [SerializeField] AnimationEventWrapperScript _animationEventWrapperScript;
    [SerializeField] AnimationParameterWrapperScript _animationParameterWrapperScript;
    [SerializeField] float movementMultiplier3D = 1.0f;
    [SerializeField] float smoothMovementSpeed = 0.2f;
    [SerializeField] float rotationSpeed = 20f;

    Vector3 _movementVector = Vector3.zero;
    Vector3 _mousePlayerPlanePosition;
    Vector3 _temporaryMovementVector;
    Vector2 _currentMovementInput;
    Vector2 _smoothDampVelocity;
    
    private bool _isFreezePlayer = false;
    private bool _isLockedOn = false;
    float _velocityX, _velocityZ;
    float _movementModifierFloat = 1;
    int freezeCount = 0;

    //private void OnEnable()
    //{
    //    _animationEventWrapperScript.OnFreezePlayer += FreezePlayer;
    //    _animationEventWrapperScript.OnUnFreezePlayer += UnFreezePlayer;
    //}

    //private void OnDisable()
    //{
    //    _animationEventWrapperScript.OnFreezePlayer -= FreezePlayer;
    //    _animationEventWrapperScript.OnUnFreezePlayer -= UnFreezePlayer;
    //}

    #region Handling Input

    public void HandleMovementInput(Vector2 rawMovementInput)
    {
        ProcessMovementInput3D(rawMovementInput); //Dirty code regarding private var need to show more whats happening

        if (!_isFreezePlayer) //Eventually have two seperate variables for rotate and move
        {
            SetAnimatorVelocity();
            MovePlayer3D();
        }
    }

    public void HandleMouseScreenPosition(Vector2 unFilteredMousePosition)
    {
        _mousePlayerPlanePosition = GetMousePositionPlayerPlaneUp(unFilteredMousePosition);

        if (_isLockedOn) //Eventually have two seperate variables for rotate and move
        {
            //Pass lock on target, shouldn't be mousepos
            RotatePlayerToPoint(_mousePlayerPlanePosition);
        }
        else if (!_isFreezePlayer)
        {
            //Rotate towards position walking. Shouldn't be mousePos
            RotatePlayerToPoint(_mousePlayerPlanePosition);
        }
    }

    #endregion

    #region Input Processing

    private Vector3 GetMousePositionPlayerPlaneUp(Vector2 unFilteredMousePosition)
    {
        Ray mouseRay = playerCamera.ScreenPointToRay(unFilteredMousePosition);
        Plane p = new Plane(Vector3.up, _playerTransform.position);
        if (p.Raycast(mouseRay, out float hitDist))
        {
            Vector3 mousePos = mouseRay.GetPoint(hitDist);
            return mousePos;
        }
        else
        {
            //Ray was parallel to plane. Return XY of camera
            return mouseRay.GetPoint(0f);
        }
    }

    private void ProcessMovementInput3D(Vector2 unfilteredMovementInput)
    {
        unfilteredMovementInput.Normalize();

        _currentMovementInput = Vector2.SmoothDamp(_currentMovementInput, unfilteredMovementInput, ref _smoothDampVelocity, smoothMovementSpeed);


        if (_currentMovementInput.magnitude > 0)
        {
            _movementVector.Set(_currentMovementInput.x, 0, _currentMovementInput.y);

            _movementVector.Normalize();
            _movementVector *= movementMultiplier3D * Time.deltaTime * _movementModifierFloat;
        }
        else //Movement Vector is zero
        {

        }
    }

    #endregion

    private void SetProjectedMovementPosition(Vector2 projectedPosition)
    {

    }

    private void RotatePlayerToPointSimple(Vector3 point3D)
    {

        _playerTransform.LookAt(point3D);
    }

    private void RotatePlayerToPoint(Vector3 point3D)
    {
        Quaternion tr = Quaternion.LookRotation(point3D - _playerTransform.position);
        Quaternion tr2 = Quaternion.Slerp(_playerTransform.rotation, tr, rotationSpeed * Time.deltaTime);

        _playerTransform.rotation = tr2;
        //_playerTransform.LookAt(point3D);
    }

    private void MovePlayer3D() //Add freeze player to moveplayer method instead maybe. It's the freeze bool that's cuasing the error
    {
        //NEED TO IMPLEMENT DESIRED POSITION BOUNDARIES
        _playerTransform.Translate(_movementVector, Space.World);
    }

    private void FreezePlayer()
    {
        freezeCount++; //In case multiple freezes are set
        _isFreezePlayer = true;
    }

    private void UnFreezePlayer()
    {
        freezeCount--;
        if (freezeCount <= 0)
        {
            _isFreezePlayer = false;
        }
    }

    private void SetSpeedModifier(float movementValue)
    {
        _movementModifierFloat = movementValue;
    }

    private void ResetSpeedModifier()
    {
        _movementModifierFloat = 1f;
    }

    private void SetAnimatorVelocity()
    {
        _velocityX = Vector3.Dot(_movementVector.normalized, _playerTransform.right);
        _velocityZ = Vector3.Dot(_movementVector.normalized, _playerTransform.forward);

        #region Horizontal clamping

        if (0f < _velocityX && _velocityX < 0.55f)
        {
            _velocityX = 0.5f;
        }
        else if (0.55f < _velocityX)
        {
            _velocityX = 1f;
        }
        else if (0f > _velocityX && _velocityX > -0.55f)
        {
            _velocityX = -0.5f;
        }
        else if (-0.55f > _velocityX)
        {
            _velocityX = -1f;
        }
        else
        {
            _velocityX = 0f;
        }

        #endregion

        #region Vertical clamping

        if (0f < _velocityZ && _velocityZ < 0.55f)
        {
            _velocityZ = 0.5f;
        }
        else if (0.55f < _velocityZ)
        {
            _velocityZ = 1f;
        }
        else if (0f > _velocityZ && _velocityZ > -0.55f)
        {
            _velocityZ = -0.5f;
        }
        else if (-0.55f > _velocityZ)
        {
            _velocityZ = -1f;
        }
        else
        {
            _velocityZ = 0f;
        }

        #endregion

        _animationParameterWrapperScript.SetVelocityX(_velocityX);
        _animationParameterWrapperScript.SetVelocityZ(_velocityZ);
    }
}
