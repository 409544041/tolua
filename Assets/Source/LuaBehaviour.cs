using UnityEngine;
using System;
using System.Collections;
using LuaInterface;

public class LuaBehaviour : MonoBehaviour
{
	public string  			ScriptName;

	protected LuaState 		luaState_;
	protected LuaTable 		self_;

	public LuaTable script
	{
		get{ return self_; }
	}

    protected void Awake()
    {
        // 这一步会触发LuaClient实例化。
        luaState_ = GetMainLuaState();

        if (ScriptName != null && ScriptName != null)
        {
            loadScript(ScriptName);

            //尝试调用脚本对象的Awake函数
            CallMethod("Awake");
        }
    }

    protected void Start ()
	{
		//尝试调用脚本对象的Start函数
		CallMethod("Start");
	}

    // 这里实现Update函数仅用于测试目的。项目中使用的话需要移除这段代码，
    // 使用ToLua提供的Update事件系统替代：`UpdateBeat:Add(YourUpdateMethod, self)`
    protected void Update()
    {
        CallMethod("Update");
    }
	
	protected void OnDestroy()
	{
		CallMethod("OnDestroy");

		//销毁脚本对象
		if(self_ != null)
		{
			self_.Dispose();
			self_ = null;
		}
	}

    // 根据Lua模块名，加载Lua脚本
    public bool loadScript(string scriptName)
    {
        if (scriptName == null)
        {
            Debug.LogError("The ScriptName must be set.");
            return false;
        }
        ScriptName = scriptName;

        // require lua文件，得到返回的类
        LuaTable metatable = (LuaTable)require(ScriptName);
        if (metatable == null)
        {
            Debug.LogError("Invalid script file '" + ScriptName + "', metatable needed as a result.");
            return false;
        }

        // 从类中找到New函数
        LuaFunction lnew = (LuaFunction)metatable["New"];
        if (lnew == null)
        {
            Debug.LogError("Invalid metatable of script '" + ScriptName + "', function 'New' needed.");
            return false;
        }

        //执行New函数生成脚本对象
        object[] results = lnew.Call(metatable);
        if (results == null || results.Length == 0)
        {
            Debug.LogError("Invalid 'New' method of script '" + ScriptName + "', a return value needed.");
            return false;
        }

        //存贮脚本对象
        bindScript((LuaTable)results[0]);
        return true;
    }

    // 将一个已经存在的Lua对象绑定到当前组件中。
    // 用于在脚本中动态创建对象，并手动挂接Lua对象。
    public void bindScript(LuaTable script)
    {
        self_ = script;

        //给脚本对象设置上常用的属性
        self_["transform"] = transform;
        self_["gameObject"] = gameObject;
        self_["behaviour"] = this;
    }

    // 调用Lua端的成员方法。会自动将self作为第一个参数，传递到Lua。
    protected object[] CallMethod(string func, params object[] args)
    {
        if (self_ == null)
        {
            return null;
        }

        LuaFunction lfunc = (LuaFunction)self_[func];
        if (lfunc == null)
        {
            return null;
        }

        //等价于lua语句: self:func(...)
        int oldTop = lfunc.BeginPCall();
        lfunc.Push(self_);
        lfunc.PushArgs(args);
        lfunc.PCall();
        object[] objs = luaState_.CheckObjects(oldTop);
        lfunc.EndPCall();
        return objs;
    }

    // 自己实现一个lua require函数，可以得到require的返回值。
    public object require(string fileName)
	{
		int top = luaState_.LuaGetTop();
		string error = null;
		object result = null;

		if (luaState_.LuaRequire(fileName) != 0)
		{
			error = luaState_.LuaToString(-1);
		}
		else
		{
			if(luaState_.LuaGetTop() > top)
			{
				result = luaState_.ToVariant(-1);
			}
		}

		luaState_.LuaSetTop(top);

		if (error != null)
		{
			throw new LuaException(error);
		}
		return result;
	}

    // 这个函数是实例化LuaClient的入口。代码中获取LuaClient都调用这个静态函数。
    public static LuaClient GetLuaClient()
    {
        if (LuaClient.Instance == null)
        {
            Debug.Log("LuaBehaviour create LuaClient");

            GameObject obj = new GameObject();
            obj.name = "LuaClient";

            // 绑定LuaClient组件
            obj.AddComponent<LuaClient>();

            // 让obj常驻内存，不自动卸载
            DontDestroyOnLoad(obj);
        }
        return LuaClient.Instance;
    }

    public static LuaState GetMainLuaState()
    {
        GetLuaClient();
        return LuaClient.GetMainState();
    }
}
