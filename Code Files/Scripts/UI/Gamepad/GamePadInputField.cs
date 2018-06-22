using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePadInputField : MonoBehaviour
{
    private InputField field;
	void Start ()
    {
        field = GetComponent<InputField>();
	}
	
	void Update ()
    {
		if (Input.GetButtonUp("Submit") || Input.GetButtonUp("Cancel"))
        {
            field.DeactivateInputField();
        }
	}
}
