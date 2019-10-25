local Linq = {__isLinq = true}

local END = {} --����iter�ķ���ֵ����־��������
local Heap = require('Heap') --����TakeByOrder�д�Ų��Ƚ���ֵ

setmetatable(Linq, {
	__call = function(self, ...)
		return Linq:New(...)
	end
})

local mt = {__index = Linq, __ipairs = function(this) return this:Iter() end}

local function IsArray(t)
    --return t[1] ~= nil
    local i = 1
    for k,v in pairs(t) do
        if k~=i then
            return false
        end
        i = i + 1
    end
    return true
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

function Linq:New(...)
    local source = {...}
    local asArr = false
    if type(source[1])=='table' and type(source[2])~='table' then
        source, asArr = unpack(source)
    end
    if type(source)~='table' then
        error('expect table, get '..type(source))
    end
    if source.__isLinq then return source end
    if not asArr and not IsArray(source) then
        source = Dict2Arr(source)
    end
    return MakeLinq(source)
end

function Linq:Iter()
	local iters = {}
	local i = 0 --�ܵ�������
	local index = 1 --itersĿ¼
	if self.iter then
		table.insert(iters, self.iter()) --�����Ժ�����ʽ���ݵ�����
	end

    local j,k = 1,0 --һ��Ŀ¼������Ŀ¼
    local function rawIter() --������������ʽ��ŵ�����
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
-- make Array
------------------

-- args: [from, ]to
function Linq:Range(...)
	local args = (type(self)=='table' and self.__isLinq) and {...} or {self, ...} --��:��ʽ���� �� ��.��ʽ����
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
end

-- generator: function([index]) or value
function Linq:Repeat(...)
	local args = (type(self)=='table' and self.__isLinq) and {...} or {self, ...} --��:��ʽ���� �� ��.��ʽ����
	local generator, cnt = unpack(args)
    if type(generator)~='function' then
        local value = generator
        generator = function() return value end
    end
    local result = {changable = true}
    for i=1,cnt do
        result[i] = generator(i)
    end
    return MakeLinq(result)
end

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
	    local curList = {}
	    local curIndex = 0
    	return function()
	    	curIndex = curIndex + 1
    		while curList[curIndex]==nil do
	    		local i, v = parentIter()
	    		if i==nil then return END end
	    		curList = selector(v, i)
	    		curIndex = 1
	    	end
    		return curList[curIndex]
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

-- ����ǰn������������Ԫ��
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

-- ���򲢷���ǰn��Ԫ�أ�������������
-- һ����˵��n>50ʱ��ʱ������ȫ��OrderBy��Take�����ô�����ռ���ڴ�С
function Linq:TakeByOrder(cnt, selector)
	selector = selector or function(v) return v end
    local heap = Heap(cnt, function(a, b) return a[2] < b[2] end)
    self:Select(function(t) return {t, selector(t)} end)
        :ForEach(function(v) heap:Push(v) end)
    return MakeLinq(heap:Done()):Select(function(pair) return pair[1] end)
end

-- �����и���selector�ķ���ֵ���飬key����metatable�У�������������
-- return [{1, 2, ..., 'key'}, ...]
function Linq:GroupBy(selector)
    local indexOf = {}
    local groups = {}
    for i,v in self:Iter() do
        local key = selector(v, i)
        if indexOf[key] == nil then
            table.insert(groups, setmetatable({}, {__index = {key = key}}))
            indexOf[key] = #groups
        end
        table.insert(groups[indexOf[key]], v)
    end
    return MakeLinq(groups)
end

-- �����������У��ı�ԭ���У����ı���Դ��
function Linq:Concat(other)
    if other.__isLinq then
        other = other:Done()
    end
    self[#self+1] = other
    return self
end

-- ����������ӳ����һ������
-- selector: function(value1, value2, index) => newValue
function Linq:Zip(other, selector)
    if selector==nil then selector = function(v1, v2) return {v1, v2} end end

    local parent = self
    local other = Linq(other)
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


-- ���������в��ظ���ֵ
function Linq:Distinct(selector)
    if selector==nil then selector = function(v) return v end end

    local parent = self
    local set = {}
    local childIter = function(this)
    	local parentIter = parent:Iter()
    	return function()
    		local i, v = parentIter()
    		while i do
    			if set[v] then --�ѳ��ֹ���ֵ����ȡ��һ��
    				i, v = parentIter()
    			else
		    		set[v] = 1 --��¼��ֵ������
		    		return selector(v, i)
		    	end
	    	end
	    	return END
    	end
    end

    return MakeLinq{iter=childIter}
end

-- ����ӳ����ֵ���򣬲��ı�ԭ���У�������������
-- cmp:function(a,b) => a����bǰ��
function Linq:OrderBy(selector, cmp)
    if cmp==nil then cmp = Compare end
    local kvps = self:Select(function(v,i) return {selector(v,i), v} end):Done()
    table.sort(kvps, function(a,b) return cmp(a[1], b[1]) end)
    return Linq(kvps):Select(function(kvp) return kvp[2] end)
end

-- �������������У�������������
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

-- ʹ��'+'��������ӳ����ֵ
-- *seed: ��ʼֵ
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

-- ʹ���ۼ����������е�ֵ
-- *seed: ��ʼֵ
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

--������ĩβ������Ԫ��
function Linq:Append(value)
    local tail = self[#self]
    if tail and tail.changable then
        tail[#tail + 1] = value
    else
        self[#self + 1] = {value, changable = true}
    end
    return self
end


return Linq
