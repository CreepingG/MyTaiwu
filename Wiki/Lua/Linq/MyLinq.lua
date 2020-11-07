local Linq = {__isLinq = true}

local END = {} --用于iter的返回值，标志迭代结束
local Heap = require('Heap') --用于TakeByOrder中存放并比较最值
local unpack = unpack or table.unpack --兼容lua新旧版本

local mt = {
	__index = Linq, 
	__pairs = function(this) return this:Iter() end
}
mt.__ipairs = mt.__pairs

local function IsArray(t)
	return t[1] ~= nil
	--[[local i = 1
	for k,v in pairs(t) do
		if k~=i then
			return false
		end
		i = i + 1
	end
	return true]]
end

local function Dict2Arr(t)
	local arr = {}
	local index = 1
	for k,v in pairs(t) do
		local kvp = {k, v}
		arr[index] = kvp
		index = index + 1
	end
	return arr
end

local function MakeLinq(source)
	if source.iter then
		return setmetatable(source, mt)
	end
	return setmetatable({source}, mt)
end

local function Compare(a, b) return a < b end

local function Pairs(source)
	local f,t,k = pairs(source)
	return function()
		local v
		k,v = f(t,k)
		if v==nil then return END end
		return v
	end
end

local p = {
	New = function(source, flag)
		local asArray = true
		if flag==nil then
			asArray = nil
		elseif type(flag)=='string' then
			flag = flag:sub(1,1)
			if flag=='d' or flag=='m' then
				asArray = false
			elseif flag=='a' or flag=='l' then
				asArray = true
			elseif flag=='i' or flag=='p' then
				return MakeLinq{iter = function() return Pairs(source) end}
			end
		else
		   asArray = flag
		end
		if source.__isLinq then return source end
		if asArray==false or (asArray==nil and not IsArray(source)) then
			source = Dict2Arr(source)
		end
		return MakeLinq(source)
	end,
	-- args: [from, ]to
	Range = function(...)
		local args = {...}
		local from, to
		if #args==1 then
			from, to = 1, args[1]
		else
			from, to = unpack(args)
		end
		local result = {changable = true}
		for i=from,to do
			table.insert(result, i)
		end
		return MakeLinq(result)
	end,
	
	-- generator: function(index) or value
	Repeat = function(generator, cnt)
		if type(generator)~='function' then
			local value = generator
			generator = function() return value end
		end
		local result = {changable = true}
		for i=1,cnt do
			result[i] = generator(i)
		end
		return MakeLinq(result)
	end,
}

setmetatable(p, {
	__call = function(self, ...)
		return self.New(...)
	end
})

function Linq:Iter()
	local iters = {}
	local i = 0 --总迭代计数
	local index = 1 --iters目录
	if self.iter then
		table.insert(iters, self.iter()) --迭代以函数形式传递的数据
	end

	local j,k = 1,0 --一级目录，二级目录
	local function rawIter() --迭代以数组形式存放的数据
		k = k + 1
		while self[j] do
			local v = self[j][k]
			if v~=nil then return v end
			j = j + 1
			k = 1
		end
		return END
	end

	table.insert(iters, rawIter)

	return function()
		i = i + 1
		while iters[index] do
			local v = iters[index]()
			if v~=END then return i,v end
			index = index + 1
		end
	end
end

function Linq:ToDictionary(getKey, getValue)
	if getKey==nil then getKey = function(kvp) return kvp[1] end end
	if getValue==nil then getValue = function(kvp) return kvp[2] end end
	local result = {}
	for i,v in self:Iter() do
		local key = getKey(v,i)
		local value = getValue(v, i)
		assert(result[key]==nil, ('duplicate keys: key = %s, value1 = %s, value2 = %s').format(key, result[key], value))
		result[key] = value
	end
	return result
end

function Linq:Done()
	local result = {}
	for i,v in self:Iter() do
		result[i] = v
	end
	return result
end

Linq.ToArray = Linq.Done

------------------
-- Array to Array
------------------

-- selector:func(value[, index]) => newValue
function Linq:Select(selector)
	local parent = self
	local childIter = function(this)
		local parentIter = parent:Iter()
		return function()
			local i, v = parentIter()
			if i==nil then return END end
			return selector(v, i)
		end
	end

	return MakeLinq{iter=childIter}
end

-- selector:func(value[, index]) => newValues:array
function Linq:SelectMany(selector)
	local parent = self
	local childIter = function(this)
		local parentIter = parent:Iter()
		local curIter = function() return end
		return function()
			local ii,vv = curIter()
			while ii==nil do
				local i, v = parentIter()
				if i==nil then return END end
				curIter = p(selector(v, i)):Iter()
				ii,vv = curIter()
			end
			return vv
		end
	end

	return MakeLinq{iter=childIter}
end

-- predicate:func(value[, index]) => want:bool
function Linq:Where(predicate)
	local parent = self
	local childIter = function(this)
		local parentIter = parent:Iter()
		return function()
			local i, v = parentIter()
			while i~=nil do
				if predicate(v, i) then return v end
				i, v = parentIter()
			end
			return END
		end
	end

	return MakeLinq{iter=childIter}
end

-- 返回前n个符合条件的元素
-- cnt:number, max size of subsequence
-- *predicate:func(value[, index]) => want:bool
function Linq:Take(cnt, predicate)
	if predicate==nil then predicate = function() return true end end
	local parent = self
	local childIter = function(this)
		local parentIter = parent:Iter()
		return function()
			cnt = cnt - 1
			if cnt<0 then return END end
			local i, v = parentIter()
			while i~=nil do
				if predicate(v,i) then return v end
				i, v = parentIter()
			end
			return END
		end
	end

	return MakeLinq{iter=childIter}
end

-- 排序并返回前n个元素，会生成新数组
-- 一般来说，n>50时耗时高于先全部OrderBy再Take，但好处在于占用内存小
function Linq:TakeByOrder(cnt, selector)
	selector = selector or function(v) return v end
	local heap = Heap(cnt, function(a, b) return a[2] < b[2] end)
	self:Select(function(t) return {t, selector(t)} end)
		:ForEach(function(v) heap:Push(v) end)
	return MakeLinq(heap:Done(), 'list'):Select(function(pair) return pair[1] end)
end

-- 将序列根据selector的返回值分组，key存在metatable中，会生成新数组
-- return [{1, 2, ..., 'key'}, ...]
function Linq:GroupBy(selector)
	local indexOf = {}
	local groups = {}
	for i,v in self:Iter() do
		local key = selector(v, i)
		if key~=nil then --映射结果为nil，则舍去
			if indexOf[key] == nil then
				table.insert(groups, setmetatable({}, {__index = {key = key}}))
				indexOf[key] = #groups
			end
			table.insert(groups[indexOf[key]], v)
		end
	end
	return MakeLinq(groups)
end

-- 连接两个序列（改变原序列，不改变来源）
function Linq:Concat(other)
	if other.__isLinq then
		other = other:Done()
	end
	self[#self+1] = other
	return self
end

-- 将两个序列映射至一个序列
-- selector: function(value1, value2, index) => newValue
function Linq:Zip(other, selector)
	if selector==nil then selector = function(v1, v2) return {v1, v2} end end

	local parent = self
	local other = p(other)
	local childIter = function(this)
		local iter1 = parent:Iter()
		local iter2 = other:Iter()
		return function()
			local i1, v1 = iter1()
			local i2, v2 = iter2()
			if i1==nil or i2==nil then return END end
			return selector(v1, v2, i1)
		end
	end

	return MakeLinq{iter=childIter}
end


-- 返回序列中不重复的值
function Linq:Distinct(selector)
	if selector==nil then selector = function(v) return v end end

	local parent = self
	local set = {}
	local childIter = function(this)
		local parentIter = parent:Iter()
		return function()
			local i, v = parentIter()
			while i do
				local key = selector(v, i)
				if set[key] then --已出现过该值，读取下一个
					i, v = parentIter()
				else
					set[key] = 1 --记录该值，返回
					return v
				end
			end
			return END
		end
	end

	return MakeLinq{iter=childIter}
end

-- 根据映射后的值排序，不改变原序列，会生成新数组
-- cmp:function(a,b) => a排在b前面
function Linq:OrderBy(selector, cmp)
	if cmp==nil then cmp = Compare end
	local kvps = self:Select(function(v,i) return {selector(v,i), v} end):Done()
	table.sort(kvps, function(a,b) return cmp(a[1], b[1]) end)
	return p(kvps):Select(function(kvp) return kvp[2] end)
end

-- 将序列逆序排列，会生成新数组
function Linq:Reverse()
	local cnt = self:Count()
	local result = {}
	for i,v in self:Iter() do
		result[cnt + 1 - i] = v
	end
	return MakeLinq(result)
end

------------------
-- Array to One
------------------

-- 使用'+'汇总序列映射后的值
-- *seed: 初始值
function Linq:Sum(selector, seed)
	if selector==nil then selector = function(v) return v end end
	for i,v in self:Iter() do
		v = selector(v, i)
		if seed==nil then
			seed = v
		else
			seed = seed + v
		end
	end
	return seed
end

-- 使用累加器汇总序列的值
-- *seed: 初始值
function Linq:Aggregate(accumulator, seed)
	for i,v in self:Iter() do
		if seed==nil then
			seed = v
		else
			seed = accumulator(seed, v, i)
		end
	end
	return seed
end

-- cmp:function(a, b) => a is less than b
function Linq:Max(cmp)
	if cmp == nil then cmp = Compare end
	local result, index
	for i,v in self:Iter() do
		if result == nil then
			result, index = v, i
		elseif cmp(result, v) then
			result, index = v, i
		end
	end
	return result, index
end

-- cmp:function(a, b) => a is less than b
function Linq:Min(cmp)
	if cmp == nil then cmp = Compare end
	local result, index
	for i,v in self:Iter() do
		if result == nil then
			result, index = v, i
		elseif cmp(v, result) then
			result, index = v, i
		end
	end
	return result, index
end

function Linq:All(predicate)
	for i,v in self:Iter() do
		if not predicate(v, i) then
			return false
		end
	end
	return true
end

function Linq:Any(predicate)
	for i,v in self:Iter() do
		if predicate(v, i) then
			return true
		end
	end
	return false
end

-- *predicate:func(value[, index]) => want:bool
function Linq:Count(predicate)
	if predicate==nil then predicate = function() return true end end
	local result = 0
	for i,v in self:Iter() do
		if predicate(v, i) then
			result = result + 1
		end
	end
	return result
end

function Linq:Average()
	return self:Sum() / self:Count()
end

function Linq:IndexOf(value)
	for i, v in self:Iter() do
		if v==value then
			return i
		end
	end
	return -1
end

function Linq:Contains(value)
	return self:IndexOf(value) ~= -1
end

function Linq:First(predicate)
	predicate = predicate or function() return true end
	for i,v in self:Iter() do
		if predicate(v, i) then
			return v, i
		end
	end
end

function Linq:Last(predicate)
	predicate = predicate or function() return true end
	local value, index
	for i,v in self:Iter() do
		if predicate(v, i) then
			value, index = v, i
		end
	end
	return value, index
end

------------------
-- Action
------------------

--break when return false
function Linq:ForEach(action)
	for i,v in self:Iter() do
		if action(v, i)==false then break end
	end
	return self
end

--在序列末尾加入新元素
function Linq:Append(value)
	local tail = self[#self]
	if tail and tail.changable then
		tail[#tail + 1] = value
	else
		self[#self + 1] = {value, changable = true}
	end
	return self
end


return p
