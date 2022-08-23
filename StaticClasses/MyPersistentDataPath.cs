using System.Collections;
using System.Collections.Generic;

public static class MyPersistentDataPath
{
    public const string CONFIG = "Persistent.data";
    
    public const string LEFT_00 = "Save.bak0";
    public const string LEFT_01 = "Save.bak1";
    public const string LEFT_02 = "Save.bak2";
    public const string LEFT_03 = "Save.bak3";
    public const string LEFT_04 = "Save.bak4";
    public const string LEFT_05 = "Save.bak5";
    public const string LEFT_06 = "Save.bak6";
    public const string LEFT_07 = "Save.bak7";

    /// <summary>
    /// 내가 소유한 카드의 인덱스 저장 {0,1,2,3,4}
    /// </summary>
    public const string RIGHT_00 = "AutoSave.bak0";
    public const string RIGHT_01 = "AutoSave.bak1";
    public const string RIGHT_02 = "AutoSave.bak2";
    public const string RIGHT_03 = "AutoSave.bak3";
    public const string RIGHT_04 = "AutoSave.bak4";
    public const string RIGHT_05 = "AutoSave.bak5";
    public const string RIGHT_06 = "AutoSave.bak6";
    public const string RIGHT_07 = "AutoSave.bak7";
}
