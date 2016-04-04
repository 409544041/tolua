using UnityEngine;
using System.Collections;
using LuaInterface;

public class LuaBehaviourScript : MonoBehaviour {

	public string  			ScriptName;

	protected LuaState 		luaState_;
	protected LuaTable 		self_;
	protected LuaFunction 	lupdate_;

	public LuaTable script
	{
		get{ return self_; }
	}

	protected void Awake()
	{
		luaState_ = LuaMainInstance.Instance.luaState;
		if(ScriptName == null)
		{
			Debug.LogError("The ScriptName must be set.");
			return;
		}

		//require lua文件，得到返回的类
		LuaTable metatable = (LuaTable)LuaMainInstance.Instance.require(ScriptName);
		if(metatable == null)
		{
			Debug.LogError("Invalid script file '" + ScriptName + "', metatable needed as a result.");
			return;
		}

		//从类中找到New函数
		LuaFunction lnew = (LuaFunction)metatable["New"];
		if(lnew == null)
		{
			Debug.LogError("Invalid metatable of script '" + ScriptName + "', function 'New' needed.");
			return;
		}

		//执行New函数生成脚本对象
		object[] results = lnew.Call(metatable, this);
		if(results == null || results.Length == 0)
		{
			Debug.LogError("Invalid 'New' method of script '" + ScriptName + "', a return value needed.");
			return;
		}

		//存贮脚本对象
		self_ = (LuaTable)results[0];

		//给脚本对象设置上常用的属性
		self_["transform"] = transform;
		self_["gameObject"] = gameObject;
		self_["behaviour"] = this;

		lupdate_ = (LuaFunction)self_["Update"];

		//尝试调用脚本对象的Awake函数
		CallMethod("Awake");
	}
	
	// Use this for initialization
	protected void Start ()
	{
		//尝试调用脚本对象的Start函数
		CallMethod("Start");
	}
	
	// Update is called once per frame
	protected void Update ()
	{
		if(lupdate_ != null)
		{
			lupdate_.Call(self_);
		}
	}

	protected void OnDestroy()
	{
		CallMethod("OnDestroy");

		if(lupdate_ != null)
		{
			lupdate_.Dispose();
			lupdate_ = null;
		}

		//销毁脚本对象
		if(self_ != null)
		{
			self_.Dispose();
			self_ = null;
		}
	}

	protected object[] CallMethod(string func, params object[] args)
	{
		if (self_ == null)
		{
			return null;
		}

		LuaFunction lfunc = (LuaFunction)self_[func];
		if(lfunc == null)
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
}
