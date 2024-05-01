using System.Collections;
using System.Collections.Generic;
using GlobalSettings;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GlobalSettingsTests {
    private string _settingsPath = "GlobalSettings/GlobalSettingsContainer";
    
    [Test]
    public void GlobalSettingsFloatTest() {
        var container = Resources.Load<GlobalSettingContainer>(_settingsPath);
        var testSetting = Settings.TEST_SETTING;
        Assert.That(testSetting, Is.EqualTo(2.5f));
        var setting = container.Settings["TestSetting"];
        container.Settings["TestSetting"] = setting with { FloatValue = 3.5f };
        Assert.That(Settings.TEST_SETTING, Is.EqualTo(3.5f));
    }
    
    [Test]
    public void GlobalSettingsIntTest() {
        var testSetting = Settings.TEST_INT_SETTING;
        Assert.That(testSetting, Is.EqualTo(5));
        var container = Resources.Load<GlobalSettingContainer>(_settingsPath);
        var setting = container.Settings["TestIntSetting"];
        container.Settings["TestIntSetting"] = setting with { IntValue = 10 };
        Assert.That(Settings.TEST_INT_SETTING, Is.EqualTo(10));
    }
    
    [Test]
    public void GlobalSettingsStringTest() {
        var testSetting = Settings.TEST_STRING_SETTING;
        Assert.That(testSetting, Is.EqualTo("Test"));
        var container = Resources.Load<GlobalSettingContainer>(_settingsPath);
        var setting = container.Settings["TestStringSetting"];
        container.Settings["TestStringSetting"] = setting with { StringValue = "TestString" };
        Assert.That(Settings.TEST_STRING_SETTING, Is.EqualTo("TestString"));
    }
    
    [Test]
    public void GlobalSettingsBoolTest() {
        var testSetting = Settings.TEST_BOOL_SETTING;
        Assert.That(testSetting, Is.EqualTo(true));
        var container = Resources.Load<GlobalSettingContainer>(_settingsPath);
        var setting = container.Settings["TestBoolSetting"];
        container.Settings["TestBoolSetting"] = setting with { BoolValue = false };
        Assert.That(Settings.TEST_BOOL_SETTING, Is.EqualTo(false));
    }
    
}