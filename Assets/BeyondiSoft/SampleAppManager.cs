using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleAppManager : MonoBehaviour
{
    void Start()
    {
        string countryCode = BeyondiUtil.CountryCode();
        Debug.LogFormat("COUNTRY CODE: {0}", countryCode);
    }
}
