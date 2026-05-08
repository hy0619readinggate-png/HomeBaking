using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Localization : MonoBehaviour
{
    protected string Hierarcy
    {
        get
        {
            Transform tf = transform;
            string hira = tf.name;
            while (tf.parent)
            {
                tf = tf.parent;
                hira = System.String.Format("{0}/{1}", tf.name, hira);
            }
            return hira;
        }
    }

    protected virtual void ChangeLocale()
    {
    }

    void OnEnable()
    {
        LocalizationManager.Instance.ChangeLocale += ChangeLocale;
        ChangeLocale();
    }

    void OnDisable()
    {
        LocalizationManager.Instance.ChangeLocale -= ChangeLocale;
    }
}
