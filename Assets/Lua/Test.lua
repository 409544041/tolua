
local Test = {}
Test.__index = Test

function Test.New(cls)
	print("Test:New", cls)

	local self = {}
	setmetatable(self, cls)
	return self
end

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

return Test
