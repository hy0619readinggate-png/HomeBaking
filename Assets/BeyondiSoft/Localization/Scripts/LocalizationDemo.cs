using System.Collections.Generic;
using UnityEngine;

public class LocalizationDemo : MonoBehaviour
{
    public void Change()
    {
        LocalizationManager.Instance.Next();
    }
}
