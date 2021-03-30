local logger = {
	enabled = true,
	data = {},
	error = function(this, ...)
		local msg = ''
		for _,v in ipairs{...} do
			msg = msg .. mw.dumpObject(v) .. '\n'
		end
		msg = msg .. '\n== logs ==\n'
		for _,v in ipairs(this.data) do
			msg = msg .. mw.dumpObject(v) .. '\n'
		end
		error(msg)
	end,
	push = function(this, ...)
		if this.enabled then mw.log(...) end
		local list = {}
		for i,v in ipairs{...} do
			list[i] = tostring(v)
		end
		table.insert(this.data, table.concat(list, '\t'))
	end,
	clear = function(this)
		this.data = {}
	end,
}
local function map_by(list, getter)
	local result = {}
	for _,v in ipairs(list) do
		for _,k in ipairs{getter(v)} do
			result[k] = v
		end
	end
	return result
end
local function set_from(list)
	local result = {}
	for _,v in ipairs(list) do
		result[v] = 1
	end
	return result
end
local function get_keys(tbl)
	local list = {}
	for k,_ in pairs(tbl) do
		list[#list+1] = k
	end
	return list
end
local function reverse(list)
	local result = {}
	local len = #list
	for i,v in ipairs(list) do
		result[len - i + 1] = v
	end
	return result
end
local function merge(...)
	local result = {}
	for _,t in ipairs{...} do
		for k,v in pairs(t) do
			result[k] = v
		end
	end
	return result
end

local symbol = {
	Accept = {}, -- 算符匹配成功
	Operator = {}, -- 算符
	Parenthese = {}, -- 单个括号
	Separator = {}, -- 逗号
	Variable = {}, -- 变量名
	Literal = {}, -- 字面量
	Error = {}, -- 错误
	Nil = {},
}
do
	local _symbol_mt = {__tostring = function(this) return this[1] end}
	for k, v in pairs(symbol) do
		v[1] = k
		setmetatable(v, _symbol_mt)
	end
end

local function escape(s) -- 提取字符串，替换全角符号等
	local special_mark_map = {
		['！'] = '!',
		['＝'] = '=',
		['＞'] = '>',
		['＜'] = '<',
		['≥'] = '>=',
		['≤'] = '<=',
		['≠'] = '!=',
		['＋'] = '+',
		['－'] = '-',
		['×'] = '*',
		['÷'] = '/'
	}
	local quotation = {
		["'"] = "'",
		['"'] = '"',
		['“'] = '”',
		['‘'] = '’',
		['`'] = '`'
	}
	local result = {}
	local strings = {}
	local raw = mw.text.split(s, '')
	local len = #raw
	local i = 1
	while i <= len do
		local char = raw[i]
		local closing_quotation = quotation[char]
		if closing_quotation then
			local j = i + 1
			local ch = raw[j]
			local string_builder = {}
			while ch ~= closing_quotation do
				if ch == nil then
					logger:error('没有与['..char..']匹配的右引号')
				end
				table.insert(string_builder, ch)
				j = j + 1
			end
			i = j
			table.insert(strings, table.concat(string_builder))
			table.insert(result, #strings)
		else
			local v = special_mark_map[char]
			if v then
				for _,c in ipairs(mw.text.split(v, '')) do
					table.insert(result, c)
				end
			else
				table.insert(result, char)
			end
		end
		i = i + 1
	end
	return result, strings
end

local operators = {
	-- 乘方
	{name = '^',	level = 2,	func = function(self,a,b) return math.pow(self.num(a), self.num(b)) end},
	{name = '**',	level = 2,	func = function(self,a,b) return math.pow(self.num(a), self.num(b)) end},
	{name = '*',	level = 3,	func = function(self,a,b) return self.num(a) * self.num(b) end},
	{name = '/',	level = 3,	func = function(self,a,b) return self.num(a)==0 and 0 or self.num(a) / self.num(b) end},
	-- 整除
	{name = '//',	level = 3,	func = function(self,a,b) return self.num(a)==0 and 0 or math.floor(self.num(a) / self.num(b)) end},
	{name = '\\',	level = 3,	func = function(self,a,b) return self.num(a)==0 and 0 or math.floor(self.num(a) / self.num(b)) end},
	{name = '%',	level = 3,	func = function(self,a,b) return self.num(a) % self.num(b) end},
	{name = '+',	level = 4,	func = function(self,a,b) return self.num(a) + self.num(b) end},
	{name = '-',	level = 4,	func = function(self,a,b) return b==nil and -self.num(a) or self.num(a) - self.num(b) end},
	{name = '<',	level = 6,	func = function(self,a,b) return self.num(a) < self.num(b) end},
	{name = '>',	level = 6,	func = function(self,a,b) return self.num(a) > self.num(b) end},
	{name = '<=',	level = 6,	func = function(self,a,b) return self.num(a) <= self.num(b) end},
	{name = '>=',	level = 6,	func = function(self,a,b) return self.num(a) >= self.num(b) end},
	{name = 'in',	level = 6,	func = function(self,a,b) return mw.ustring.match(tostring(b), tostring(a))~=nil end},
	{name = '~=',	level = 7,	func = function(self,a,b) return a ~= b end},
	{name = '!=',	level = 7,	func = function(self,a,b) return a ~= b end},
	{name = '<>',	level = 7,	func = function(self,a,b) return a ~= b end},
	{name = '==',	level = 7,	func = function(self,a,b) return a == b end},
	{name = '=',	level = 7,	func = function(self,a,b) return a == b end},
	{name = '..',	level = 8,	func = function(self,a,b) return self.str(a)..self.str(b) end},
	{name = 'and',	level = 11,	func = function(self,a,b) return a and b end,	gap = 1},
	{name = '&&',	level = 11,	func = function(self,a,b) return a and b end},
	{name = 'or',	level = 12,	func = function(self,a,b) return a or b end,	gap = 1},
	{name = '||',	level = 12,	func = function(self,a,b) return a or b end},
}
do
	local _op_mt = {
		__index = {
			__isOp = 1,
			num = function(s) return tonumber(s) or 0 end,
			str = function(v) return v==nil and '' or tostring(v) end
		},
		__tostring = function(obj)
			return obj.name
		end,
		__call = function(this, ...)
			return this.func(this, ...)
		end
	}
	for _,v in ipairs(operators) do
		setmetatable(v, _op_mt)
	end
end

local parenthesis = map_by({
		{'(', ')'},
		{'[', ']'},
		{'{', '}'}
	},function(pair) return pair[1], pair[2] end)
local function is_parenthesis(token)
	if token.type == symbol.Parenthese then
		local value = token.value
		local pair = parenthesis[value]
		return pair[1]==value and 1 or 2
	end
end

local token_mt = {
	__tostring = function(this) return '<token type='..tostring(this.type)..' value='..tostring(this.value)..'>' end,
	__index = {__istoken = 1}
}

local function lexical_analysis(char_list, string_list) -- 生成词法单元[类型, 值]，算符以外的值都为string类型
	local is_whitespace = set_from{' ', '\t', '\n', '\r'}
	local op_tree = {}
	for _,v in ipairs(operators) do
		local node = op_tree
		for _, char in ipairs(mw.text.split(v.name,'')) do
			if not node[char] then
				node[char] = {}
			end
			node = node[char]
		end
		node[symbol.Accept] = v
	end

	local function token(typ, value)
		return setmetatable({type = typ, value = value}, token_mt)
	end
	local tokens = {
		error = false,
		data = {},
		push = function(this, typ, value)
			table.insert(this, token(typ, value))
			if typ == symbol.Error then
				this.error = true
			end
		end
	}
	local string_builder = {
		data = {},
		push = function(this, item)
			table.insert(this.data, item)
		end,
		clear = function(this)
			this.data = {}
		end,
		submit = function(this)
			local data = this.data
			if #data>0 then
				local s = table.concat(data)
				local v = s
				local t = symbol.Variable
				if s:match('^[Tt]rue$') then
					t = symbol.Literal
					v = true
				elseif s:match('^[Ff]alse$') then
					t = symbol.Literal
					v = false
				elseif tonumber(s) then
					t = symbol.Literal
					v = tonumber(s)
				end
				tokens:push(t, v)
				this:clear()
			end
		end,
		len = function (this)
			return #this.data
		end
	}
	local i = 1
	while i<=#char_list do
		local char = char_list[i]
		if type(char) == 'number' then -- 字符串
			tokens:push(symbol.Literal, string_list[char])
			string_builder:submit()
		elseif is_whitespace[char] then -- 空白符
			string_builder:submit()
		elseif char == ',' then -- 逗号
			string_builder:submit()
			tokens:push(symbol.Separator, char)
		elseif parenthesis[char] then -- 单个括号
			string_builder:submit()
			tokens:push(symbol.Parenthese, char)
		else
			-- 匹配算符
			local is_op = false
			local node = op_tree[char]
			if node then
				local j = i
				local last -- {end_index, op}
				while 1 do
					local acc_op = node[symbol.Accept]
					if acc_op and acc_op.gap then
						local gap_matched = false
						if string_builder:len() == 0 then -- gap前方内容已完成匹配
							local next_char = char_list[j + 1]
							if next_char==nil or is_whitespace[next_char] then -- gap后方为空白符或结尾
								gap_matched = true
							end
						end
						if not gap_matched then
							acc_op = nil
						end
					end
					if acc_op then
						last = {j, acc_op}
					end
					j = j + 1
					local next_node = node[char_list[j]]
					if not next_node then -- 贪婪模式，匹配尽可能长的操作符；匹配不到时，选择途中最长的
						if last then -- 途中有匹配结束标记
							i = last[1]
							string_builder:submit()
							tokens:push(symbol.Operator, last[2])
							is_op = true
						end
						break
					end
					node = next_node
				end
			end
			-- 变量名
			if not is_op then
				string_builder:push(char)
			end
		end
		i = i + 1
	end
	string_builder:submit()
	return tokens
end

local function syntax_analysis(tokens) -- 生成语法树  tree -> [func, tree[]] | token
	local _preset_func_mt = {
		__tostring = function(this) return this[2] end,
		__call = function(this, ...) return this[1](...) end
	}
	local preset_functions = setmetatable({{}}, {
		__index=function(this, key)
			local func = this[1][key] or math[key]
			if func then
				return setmetatable({func,key}, _preset_func_mt)
			end
		end
	})

	local stack = {__index = {
		push = function(this, item) -- 入栈
			if item == nil then item = symbol.Nil end
			table.insert(this, item)
		end,
		pop = function(this) -- 栈顶元素出栈并返回
			local len = #this
			if len <= 0 then logger:error('栈空') end
			local result = this:peek()
			this[len] = nil
			return result
		end,
		peek = function(this) -- 返回栈顶元素
			local result = this[#this]
			if result == symbol.Nil then result = nil end
			return result
		end
	}}
	local marks = setmetatable({}, stack) -- {type, value, cnt, func}
	local vals = setmetatable({}, stack) -- token
	local vars = {}
	function vals:pop_and_record()
		local v = self:pop()
		if v.type == symbol.Variable then
			vars[v.value] = 1
		end
		return v
	end
	local function combine_top() -- 取出运算，放回结果
		local mark = marks:pop()
		if mark.type ~= symbol.Operator then
			logger:error('异常算符', mark, 'marks:', marks, 'vals:', vals)
		end
		logger:push('combine', mark.value)
		local param_cnt = mark.cnt
		if param_cnt > #vals then
			logger:error('缺少参数', mark, 'marks:', marks, 'vals:', vals)
		end
		local params = {}
		for _ = 1,param_cnt do
			table.insert(params, vals:pop_and_record())
		end
		vals:push({mark.value, reverse(params)})
	end
	local i = 1
	while i <= #tokens do
		local token = tokens[i]
		local this_type = token.type
		local this_value = token.value
		if this_type == symbol.Operator then
			-- 根据左接token，判断是否为单目算符
			local single = false
			local left_token = tokens[i-1]
			if left_token == nil
				or left_token.type == symbol.Operator
				or left_token.type == symbol.Separator
				or is_parenthesis(left_token)==1
				then -- 左接 空|算符|逗号|左括号
				single = true
			end

			if single then
				marks:push{value=this_value, type=symbol.Operator, cnt=1}
				logger:push('进栈', this_value, 1)
			else
				local go_next = true
				while go_next do -- 检查栈中算符
					go_next = false
					local mark = marks:peek()
					if mark then
						local left_op = mark.value
						if mark.type == symbol.Operator then -- 是算符
							local left_level = left_op.level
							local this_level = this_value.level
							if left_level <= this_level then -- 左算符优先级更高
								combine_top()
								go_next = true
							end
						end
					end
				end
				marks:push{value=this_value, type=symbol.Operator, cnt=2}
				logger:push('进栈', this_value, 2)
			end
		elseif this_type == symbol.Parenthese then
			local left_parenthesis = parenthesis[this_value][1]
			if left_parenthesis == this_value then -- 左括号
				local mark = {value=this_value, type=symbol.Parenthese}
				local left_token = tokens[i-1]
				if left_token.type == symbol.Variable then -- 变量作为预设方法名
					vals:pop()
					mark.func = preset_functions[left_token.value]
					if mark.func == nil then
						logger:error('缺少预设方法'..left_token.value)
					end
				end
				marks:push(mark)
			else -- 右括号
				local args = {}
				while 1 do
					local top_mark = marks:peek()
					if top_mark == nil then
						logger:error('没有匹配的左括号', 'marks:', marks, 'vals:', vals)
					end
					if top_mark.value == left_parenthesis then
						break
					end
					if top_mark.type == symbol.Separator then
						marks:pop()
						table.insert(args, vals:pop_and_record())
					else
						combine_top()
					end
				end
				local left = marks:pop()
				if left.func then -- 逗号分隔的列表作为预设方法的参数
					table.insert(args, vals:pop_and_record())
					vals:push{left.func, reverse(args)}
				end
			end
		elseif this_type == symbol.Separator then
			local right_token = tokens[i+1]
			if not (right_token==nil or is_parenthesis(right_token)==2) then -- 右接 空|右括号 时 忽略
				marks:push{value=this_value, type=symbol.Separator}
			end
		else
			logger:push('进栈', token)
			vals:push(token)
		end
		i = i + 1
	end
	logger:push('收尾', marks, vals)
	while #marks > 0 do
		combine_top()
	end
	if #vals == 1 then
		local val = vals:pop_and_record()
		return val, vars
	end
	logger:error('参数过多', #vals, 'marks:', marks, 'vals:', vals)
end

local function set_callable(node) -- 通过setmetatable使其可作为方法调用
	local literal_mt = merge(token_mt, {
		__call = function(this) return this.value end,
		__tostring = function(this) return mw.dumpObject(this.value) end,
	})
	local variable_mt = merge(token_mt, {
		__call = function(this, t) return t[this.value] end,
		__tostring = function(this) return 't.'..this.value end,
	})
	local function_mt = {
		__call = function(this, t)
			local args = {}
			for _,v in ipairs(this[2]) do
				table.insert(args, v(t))
			end
			return this[1](unpack(args))
		end,
		__tostring = function (this)
			local args = {tostring(this[1])}
			for _,v in ipairs(this[2]) do
				table.insert(args, tostring(v))
			end
			return '[' .. table.concat(args, ', ') .. ']'
		end,
	}

	if node.__istoken then
		local typ = node.type
		if typ == symbol.Literal then
			return setmetatable(node, literal_mt)
		elseif typ == symbol.Variable then
			return setmetatable(node, variable_mt)
		else
			logger:error(mw.dumpObject(typ))
		end
	else
		for _,v in ipairs(node[2]) do
			set_callable(v)
		end
		return setmetatable(node, function_mt)
	end
	return node
end

return function (expr)
	logger.enabled = false

	local chars, strings = escape(expr)
	local tokens = lexical_analysis(chars, strings)

	for _,v in ipairs(tokens) do
		if v.type == symbol.Error then
			logger:error(v.value..'【'..table.concat(chars)..'】')
		end
		logger:push(v.type, v.value)
	end
	logger:clear()
	logger.enabled = false

	local tree,vars = syntax_analysis(tokens)

	--logger.enabled = true

	local function print_tree(node, depth)
		local indent = ''
		for _=1,depth do
			indent = indent..'\t'
		end
		if node.__istoken then
			logger:push(indent..tostring(node.value))
		else
			logger:push(indent..tostring(node[1]))
			for _,v in ipairs(node[2]) do
				print_tree(v, depth + 1)
			end
		end
	end
	print_tree(tree, 0)
	logger:push('vars: ['..table.concat(get_keys(vars), ', ')..']')
	logger:clear()

	return set_callable(tree)
end