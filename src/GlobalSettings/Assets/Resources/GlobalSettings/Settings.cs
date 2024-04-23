
using System;
using UnityEngine;

[GlobalSettings("GlobalSettingsContainer.asset")]
public static partial class Settings {
    public static void Test() {
        Debug.Log(TEST_SETTING);
    }
}
