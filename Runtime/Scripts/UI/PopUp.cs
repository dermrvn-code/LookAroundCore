using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    public TMP_Text messageText;


    public void SetMessage(string message)
    {
        messageText.text = message;
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
