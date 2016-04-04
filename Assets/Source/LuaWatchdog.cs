using UnityEngine;
using System.Collections.Generic;
using LuaInterface;
using System.Collections;
using System.IO;

//由于不使用LuaClient，这里需要自己处理LuaLooper
public class LuaWatchdog : LuaLooper 
{
    void Awake()
    {
        luaState = LuaMainInstance.Instance.luaState;
    }
    
    void OnApplicationQuit()
    {
        LuaMainInstance.Instance.destroy();
    }
}
