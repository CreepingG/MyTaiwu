local Heap = {cmp = function(a,b) return a < b end}

setmetatable(Heap, {
	__call = function(self, ...)
		return Heap:New(...)
	end
})

function Heap:New(size, cmp)
    local this = {age = 0, cnt = 0, size = size, cmp = cmp}
    setmetatable(this, {__index = Heap})
    return this
end

function Heap:Push(elem)
    local top = self.top
    if self.cnt < self.size then -- 堆未满，将新元素加入堆
        if top == nil then
            self.top = {elem}
        else
            self:Insert({elem}, top)
        end
        self.cnt = self.cnt + 1
    else -- 堆已满，比较新元素与堆顶
        if self.cmp(elem, top[1]) then --新元素优先级更高，新元素取代堆顶，更新堆以保证堆顶为优先级最低的元素
            top[1] = elem
            self:UpdateValue(top)
        else --堆顶元素优先级更高，无视新元素
        end
    end
    self.age = self.age + 1
    return self
end

function Heap:Insert(new, node)
    if self.cmp(node[1], new[1]) then --
        new.children = {node}
        if node.parent == nil then --原节点无亲节点，新节点成为新的堆顶
            self.top = new
            node.parent = new
        else --新节点取代原节点在亲节点处的位置
            local silbings = node.parent.children
            silbings[(node == silbings[1]) and 1 or 2] = new
            new.parent, node.parent = node.parent, new
        end
        self:UpdateBranch(new)
    else --新节点的优先级更高，新节点成为原节点的后代
        if node.children==nil then --原节点无子节点
            node.children = {new}
            new.parent = node
        else
            local children = node.children
            if #children==1 then --原节点有1个子节点，新节点成为原节点的第2个子节点
                children[2] = new
                new.parent = node
            else --原节点的子节点已满，新节点与原节点的随机子节点进行比较
                self:Insert(new, children[self.age%2+1])
            end
        end
    end
end

function Heap:UpdateBranch(parent) --尽可能将分支点向顶部移动，提升搜索效率
    local children = parent.children
    if children==nil or #children==2 then --无需更新
        return
    end
    local child = children[1]
    local grandChildren = child.children
    if grandChildren==nil then --没有可晋升节点
        return
    end
    children[2] = grandChildren[#grandChildren]
    children[2].parent = parent
    if #grandChildren==1 then
        child.children = nil
    else
        grandChildren[2] = nil
        self:UpdateBranch(child)
    end
end


function Heap:UpdateValue(parent)
    local children = parent.children
    if children==nil then return end --堆底，无需比较
    local elder = children[1]
    if children[2]~=nil and self.cmp(elder[1], children[2][1]) then --存在第二个子节点，且优先级低于第一个
        elder = children[2]
    end
    if self.cmp(elder[1], parent[1]) then --子节点优先级均高于亲节点，更新完成
        return
    else --亲节点优先级高于子节点，交换值，进行下一步更新
        elder[1], parent[1] = parent[1], elder[1]
        self:UpdateValue(elder)
    end
end

local function Out(node, list)
    table.insert(list, node[1])
    if node.children then
        for _,child in ipairs(node.children) do
            Out(child, list)
        end
    end
end

function Heap:Done()
    local result = {}
    Out(self.top, result)
    table.sort(result, self.cmp)
    return result
end


function Heap:Print()
    mw.logObject(self)
end

return Heap
