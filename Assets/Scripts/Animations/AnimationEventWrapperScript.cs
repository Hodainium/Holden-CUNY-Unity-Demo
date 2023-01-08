using System;
using UnityEngine;

[Serializable]
class AnimationEventWrapperScript : MonoBehaviour 
{
    public event Action OnFreezePlayer;
    public event Action OnUnFreezePlayer;

    public event Action<float> OnModifySpeed;

    public event Action OnResetSpeed;
    

    public void FreezeMovement() => OnFreezePlayer?.Invoke();
    public void UnFreezeMovement() => OnUnFreezePlayer?.Invoke();

    public void SetSpeedModifier(float modifierValue) => ModifySpeed(modifierValue);
    public void UnSetSpeedModifier(float modifierValue) => ModifySpeed(1f / modifierValue);

    private void ModifySpeed(float modifierValue) => OnModifySpeed?.Invoke(modifierValue);
}

