
--构造Test类
local Test = {}
Test.__index = Test --让实例对象的__get方法指向Test类

--给Test类实例化一个对象
function Test.New(cls)
	print("Test:New", cls)

	local self = {}
	setmetatable(self, cls)
	return self
end

--Awake方法
function Test:Awake()
	print("Test:Awake", self)
end

function Test:Start()
	print("Test:Start", self)
end

function Test:Update()
	local pos = self.transform.position
	pos.y = math.sin(Time.time) * 4

	self.transform.position = pos
end

--将类Test返回。通过require函数的返回值就可以获取到此值了。
return Test
