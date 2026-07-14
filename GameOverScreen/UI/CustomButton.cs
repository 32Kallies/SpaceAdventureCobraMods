using System;
using UnityEngine;

namespace GameOverScreen.UI;

public class CustomButton : MonoBehaviour
{
    public Action OnButtonPressed;

    public void Click()
    {
        OnButtonPressed.Invoke();
    }
}