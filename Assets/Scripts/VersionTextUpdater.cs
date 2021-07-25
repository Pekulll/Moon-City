using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VersionTextUpdater : MonoBehaviour
{
    void Start()
    {
        GetComponent<Text>().text = "Version " + Version.versionCode + "    Build " + Version.buildCode + "        <color=#FFA700>Pre-alpha access</color>";
    }
}
