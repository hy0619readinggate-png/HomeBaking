using DoDoEng.Common;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class SROptions
{
    //[Category("Addressables"), Sort(300)]
    //[DisplayName("HostType")]
    //public HostServerType Addr_ServerType { get; set; } = HostServerType.Production;
    //[Category("Addressables"), Sort(301)]
    //[DisplayName("Change Server")]
    //public void Addr_ChangeServer()
    //{
    //    AddressableMGR.One.SwitchTo(Addr_ServerType);
    //}
    [Category("Test"), Sort(100), DisplayName("TestMenu")]
    public void TEST_TestMenu()
    {
        SceneManager.LoadScene("Z10_TestMenu");
    }
    [Category("Test"), Sort(101), DisplayName("Clear Cache Now")]
    public void TEST_Clear_Cache_Now()
    {
        DataLoader.One.ReleaseHandles();
        AddressableMGR.One.ClearCache();
    }



    [Category("GamePlay"), Sort(200), DisplayName("1x")]
    public void Timescale_1x()
    {
        LOG.Info($"Timescale_1x()", this);
        Time.timeScale = 1;
        AudioMGR.One.Pitch = 1;
    }
    [Category("GamePlay"), Sort(201), DisplayName("1.2x")]
    public void Timescale_1dot2x()
    {
        LOG.Info($"Timescale_1point2x()", this);
        Time.timeScale = 1.2f;
        AudioMGR.One.Pitch = 1.2f;
    }
    [Category("GamePlay"), Sort(202), DisplayName("1.5x")]
    public void Timescale_1dot5x()
    {
        LOG.Info($"Timescale_1point5x()", this);
        Time.timeScale = 1.5f;
        AudioMGR.One.Pitch = 1.5f;
    }
    [Category("GamePlay"), Sort(202), DisplayName("2x")]
    public void Timescale_2x()
    {
        LOG.Info($"Timescale_2x()", this);
        Time.timeScale = 2;
        AudioMGR.One.Pitch = 2;
    }
    [Category("GamePlay"), Sort(203), DisplayName("4x")]
    public void Timescale_4x()
    {
        LOG.Info($"Timescale_4x()", this);
        Time.timeScale = 4;
        AudioMGR.One.Pitch = 4;
    }
    [Category("GamePlay"), Sort(204), DisplayName("10x")]
    public void Timescale_10x()
    {
        LOG.Info($"Timescale_10x()", this);
        Time.timeScale = 10;
        AudioMGR.One.Pitch = 10;
    }



    [Category("Aff"), Sort(300)]
    public bool ShowDebugInfo
    {
        get => AffordanceMGR.ShowDebugInfo;
        set => AffordanceMGR.ShowDebugInfo = value;
    }
    [Category("Aff"), Sort(301)]
    public bool DisableMonitor
    {
        get => AffordanceMGR.DisableMonitor;
        set => AffordanceMGR.DisableMonitor = value;
    }
    [Category("Aff"), Sort(302)]
    public void TimeLeap()
    {
        AffordanceMGR.One.DEV_timeleap();
    }



    [Category("LMS"), Sort(400), DisplayName("Force Reward Coin")]
    public bool ForceGetCoin
    {
        get => LMS.DEV_ForceRewardCoin;
        set => LMS.DEV_ForceRewardCoin = value;
    }
    [Category("LMS"), Sort(401), DisplayName("Force Reward Coin Count")]
    public int ForceRewardCoinCount
    {
        get => LMS.DEV_ForceRewardCoinCount;
        set => LMS.DEV_ForceRewardCoinCount = value;
    }



    [Category("DEBUG"), Sort(900), DisplayName("Exception")]
    public void DEBUG_ErrorScene()
    {
        SceneManager.LoadScene("Z90_TestError");
    }
    [Category("DEBUG"), Sort(901), DisplayName("Permission")]
    public void DEBUG_PermissionScene()
    {
        SceneManager.LoadScene("Z91_TestPermission");
    }
    [Category("DEBUG"), Sort(902), DisplayName("Orientation")]
    public void DEBUG_Orientation()
    {
        SceneManager.LoadScene("Z92_TestOrientation");
    }
}
