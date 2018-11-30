using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisableButton : MonoBehaviour {

    /*
     * This script disables the given button when ordered to do it.
     */

    public InputField inputField;
    private Button button;
	
	void Start () {
        button = GetComponent<Button>();
	}
	
	public void Disablebutton()
    {
        button.enabled = false;
        inputField.enabled = false;
    }
}
