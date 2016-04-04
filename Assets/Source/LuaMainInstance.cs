using UnityEngine;
using System.Collections;
using LuaInterface;

//该类负责初始化和释放主LuaState
//这里与tolua原本的设计不一样。由于采用了绑定脚本模式，所以一个节点脚本的初始化可能要早于LuaClient，
//因此，需要提前初始化LuaState。
public class LuaMainInstance
{
	static LuaMainInstance s_instance = new LuaMainInstance();

	public static LuaMainInstance Instance
	{
		get{ return s_instance; }
	}

	public LuaState luaState
	{
		get;
		private set;
	}

	LuaMainInstance()
	{
		luaState = new LuaState();
		LuaBinder.Bind(luaState);

		luaState.Start();
		luaState.DoFile("Main.lua");
	}

	public void destroy()
	{
		luaState.Dispose();
		luaState = null;
	}

	//自己实现一个lua require函数，可以得到require的返回值。
	public object require(string fileName)
	{
		int top = luaState.LuaGetTop();
		string error = null;
		object result = null;
		
		if (luaState.LuaRequire(fileName) != 0)
		{
			error = luaState.LuaToString(-1);
		}
		else
		{
			if(luaState.LuaGetTop() > top)
			{
				result = luaState.ToVariant(-1);
			}
		}

		luaState.LuaSetTop(top);
		
		if (error != null)
		{
			throw new LuaException(error);
		}
		return result;
	}
}
