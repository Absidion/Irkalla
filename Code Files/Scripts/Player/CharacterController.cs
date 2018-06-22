using UnityEngine;
using System.Collections;
using XInputDotNetPure;

//Author: Liam
//Last Edited : Liam 1/18/2018

//http://wiki.unity3d.com/index.php?title=Xbox360Controller controller support information for unity input system
//https://github.com/speps/XInputDotNet github information on xinput and controllers

public static class CharacterController
{
    public static bool Inverted = false;
    private static float m_Sensitivety = 1.0f;                     //The sensitivety of the controller / keyboard and mouse 

    private static PlayerIndex m_PlayerIndex = PlayerIndex.One;
    [SerializeField]
    private static bool m_IsUsingController = false;

    #region Controller Vibration Values
    private static Vector2 m_VibrationPower = Vector2.zero;
    private static bool m_IsVibrating = false;
    private static float m_VibrationDuration = 0.0f;
    private static float m_VibrationTimer = 0.0f;
    #endregion

    #region Const Input Strings
    private const string m_Horizontal = "Horizontal";
    private const string m_Vertical = "Vertical";
    private const string m_Fire1 = "Fire1";
    private const string m_Fire2 = "Fire2";
    private const string m_Fire3 = "Fire3";
    private const string m_Jump = "Jump";
    private const string m_Interact = "Interact";
    private const string m_Pause = "Pause";
    private const string m_RotateAroundY = "LookAxisX";
    private const string m_RotateAroundX = "LookAxisY";
    private const string m_UseItem = "UseItem";
    private const string m_OpenJournal = "Journal";
    private const string m_Sprint = "Sprint";
    private const string m_Reload = "Reload";
    private const string m_Switch = "SwitchWeapons";
    private const string m_NextLevel = "NextLevel";
    private const string m_WeaponOne = "Weapon1";
    private const string m_WeaponTwo = "Weapon2";
    #endregion

    static CharacterController()
    {
        GamePadState detectionState = GamePad.GetState(m_PlayerIndex);
        if (detectionState.IsConnected)
            m_IsUsingController = true;
    }

    //Use to get the movement from the keyboard or the controller during this frame
    public static Vector3 GetMovementThisFrame()
    {
        Vector3 movement;
        movement.x = Input.GetAxis(m_Horizontal);
        movement.y = 0.0f;
        movement.z = Input.GetAxis(m_Vertical);

        return movement;
    }

    //Use this to get the rotation of the player this frame
    public static Quaternion GetRotationThisFrame()
    {
        int invert = 1;

        if (Inverted)
            invert = -1;

        if (IsUsingController)
        {
            return Quaternion.Euler(Input.GetAxis(m_RotateAroundX) * Time.deltaTime * 100.0f * m_Sensitivety * invert, Input.GetAxis(m_RotateAroundY) * Time.deltaTime * 100.0f * m_Sensitivety , 0.0f);
        }

        return Quaternion.Euler(Input.GetAxis(m_RotateAroundX) * m_Sensitivety  *invert, Input.GetAxis(m_RotateAroundY) * m_Sensitivety, 0.0f);
    }

    #region Controller Vibration
    public static void UpdateController()
    {
        if (GamePad.GetState(m_PlayerIndex).IsConnected && !m_IsUsingController)
        {
            m_IsUsingController = true;
        }
        else if (!GamePad.GetState(m_PlayerIndex).IsConnected && m_IsUsingController)
        {
            m_IsUsingController = false;
        }

        if (m_IsVibrating)
        {
            VibrateController(m_VibrationPower.x, m_VibrationPower.y);
            m_VibrationTimer += Time.deltaTime;
            if (m_VibrationTimer >= m_VibrationDuration)
            {
                ResetVibrationValues();
                VibrateController(0.0f, 0.0f);
            }
        }
    }

    //vibrates the controller by x amount in each motor
    public static void VibrateController(float leftMotor, float rightMotor)
    {
        if (!m_IsUsingController)
            return;
        GamePad.SetVibration(m_PlayerIndex, leftMotor, rightMotor);
    }

    //Sets values that allows the controller to vibrate overtime
    public static void VibrateController(float leftMotor, float rightMotor, float duration)
    {
        if (!m_IsUsingController)
            return;

        m_VibrationPower.x = leftMotor;
        m_VibrationPower.y = rightMotor;
        m_VibrationDuration = duration;
        m_IsVibrating = true;
    }

    //Resets timed vibration values back to zero
    private static void ResetVibrationValues()
    {
        m_VibrationDuration = 0.0f;
        m_VibrationPower = Vector2.zero;
        m_VibrationTimer = 0.0f;
        m_IsVibrating = false;
    }
    #endregion

    #region  Fire1 (Basic Shot)        
    public static bool GetFire1Down()
    {
        return Input.GetButtonDown(m_Fire1);
    }

    public static bool GetFire1Up()
    {
        return Input.GetButtonUp(m_Fire1);
    }

    public static bool GetFire1Held()
    {
        return Input.GetButton(m_Fire1);
    }

    public static float GetFire1PercentHeld()
    {
        return Input.GetAxis(m_Fire1);
    }

    #endregion

    #region Fire2 (Ability 1)
    public static bool GetFire2Down()
    {
        return Input.GetButtonDown(m_Fire2);
    }

    public static bool GetFire2Up()
    {
        return Input.GetButtonUp(m_Fire2);
    }

    public static bool GetFire2Held()
    {
        return Input.GetButton(m_Fire2);
    }

    public static float GetFire2PercentHeld()
    {
        return Input.GetAxis(m_Fire2);
    }
    #endregion

    #region Fire3 (Ability 2, oh wait)
    public static bool GetFire3Down()
    {
        return Input.GetButtonDown(m_Fire3);
    }

    public static bool GetFire3Up()
    {
        return Input.GetButtonUp(m_Fire3);
    }

    public static bool GetFire3Held()
    {
        return Input.GetButton(m_Fire3);
    }

    public static float GetFire3PercentHeld()
    {
        return Input.GetAxis(m_Fire3);
    }
    #endregion

    #region Jump
    public static bool GetJumpDown()
    {
        return Input.GetButtonDown(m_Jump);
    }

    public static bool GetJumpActive()
    {
        return Input.GetButton(m_Jump);
    }

    public static bool GetJumpUp()
    {
        return Input.GetButtonUp(m_Jump);
    }

    public static bool GetJumpHeld()
    {
        return Input.GetButton(m_Jump);
    }
    #endregion

    #region Pause
    public static bool GetPauseDown()
    {
        return Input.GetButtonDown(m_Pause);
    }

    public static bool GetPauseUp()
    {
        return Input.GetButtonUp(m_Pause);
    }

    public static bool GetPauseHeld()
    {
        return Input.GetButton(m_Pause);
    }
    #endregion

    #region Journal
    public static bool GetJournalDown()
    {
        return Input.GetButtonDown(m_OpenJournal);
    }

    public static bool GetJournalUp()
    {
        return Input.GetButtonUp(m_OpenJournal);
    }

    public static bool GetJournalHeld()
    {
        return Input.GetButton(m_OpenJournal);
    }
    #endregion

    #region Interact
    //For Keyboard & Mouse: E
    //For Controller: X

    public static bool GetInteractDown()
    {
        return Input.GetButtonDown(m_Interact);
    }

    public static bool GetInteractUp()
    {
        return Input.GetButtonUp(m_Interact);
    }

    public static bool GetInteractHeld()
    {
        return Input.GetButton(m_Interact);
    }
    #endregion

    #region Use Item
    public static bool GetUseItemDown()
    {
        return Input.GetButtonDown(m_UseItem);
    }

    public static bool GetUseItemUp()
    {
        return Input.GetButtonUp(m_UseItem);
    }

    public static bool GetUseItemHeld()
    {
        return Input.GetButton(m_UseItem);
    }
    #endregion

    #region Sprint
    public static bool GetSprintDown()
    {
        return Input.GetButtonDown(m_Sprint);
    }

    public static bool GetSprintUp()
    {
        return Input.GetButtonUp(m_Sprint);
    }

    public static bool GetSprintHeld()
    {
        return Input.GetButton(m_Sprint);
    }
    #endregion

    #region Switch Weapons
    public static float GetSwitchPercentHeld()
    {
        return Input.GetAxis(m_Switch);
    }

    public static bool GetSwitchDown()
    {
        return Input.GetButtonDown(m_Switch);
    }

    public static bool GetSwitchUp()
    {
        return Input.GetButtonUp(m_Switch);
    }

    public static bool GetSwitchHeld()
    {
        return Input.GetButton(m_Switch);
    }
    #endregion

    #region Manual Weapon Switch
    public static bool GetWeaponOneDown()
    {
        return Input.GetButtonDown(m_WeaponOne);
    }

    public static bool GetWeaponOneUp()
    {
        return Input.GetButtonUp(m_WeaponOne);
    }

    public static bool GetWeaponOneHeld()
    {
        return Input.GetButton(m_WeaponOne);
    }

    public static bool GetWeaponTwoDown()
    {
        return Input.GetButtonDown(m_WeaponTwo);
    }

    public static bool GetWeaponTwoUp()
    {
        return Input.GetButtonUp(m_WeaponTwo);
    }

    public static bool GetWeaponTwoHeld()
    {
        return Input.GetButton(m_WeaponTwo);
    }
    #endregion

    #region Reload
    public static bool GetReloadDown()
    {
        return Input.GetButtonDown(m_Reload);
    }

    public static bool GetReloadUp()
    {
        return Input.GetButtonUp(m_Reload);
    }

    public static bool GetReloadHeld()
    {
        return Input.GetButton(m_Reload);
    }
    #endregion

    #region Direct Axis Control

    public static float GetHorizontalAxis()
    {
        return Input.GetAxis(m_Horizontal);
    }

    public static float GetVerticalAxis()
    {
        return Input.GetAxis(m_Vertical);
    }

    public static float GetLookAxisX()
    {
        return Input.GetAxis(m_RotateAroundX);
    }

    public static float GetLookAxisY()
    {
        return Input.GetAxis(m_RotateAroundY);
    }

    #endregion

    #region Test Cases    
    public static bool GetNextLevelDown()
    {
        return Input.GetButtonDown("NextLevel");
    }
    #endregion

    #region Properties
    public static bool IsUsingController { get { return m_IsUsingController; } }
    public static float Sensitivety { get { return m_Sensitivety / 0.014f; } set { m_Sensitivety = value * 0.014f; } }
    #endregion
}
