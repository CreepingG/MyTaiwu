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
    if self.cnt < self.size then -- ��δ��������Ԫ�ؼ����
        if top == nil then
            self.top = {elem}
        else
            self:Insert({elem}, top)
        end
        self.cnt = self.cnt + 1
    else -- ���������Ƚ���Ԫ����Ѷ�
        if self.cmp(elem, top[1]) then --��Ԫ�����ȼ����ߣ���Ԫ��ȡ���Ѷ������¶��Ա�֤�Ѷ�Ϊ���ȼ���͵�Ԫ��
            top[1] = elem
            self:UpdateValue(top)
        else --�Ѷ�Ԫ�����ȼ����ߣ�������Ԫ��
        end
    end
    self.age = self.age + 1
    return self
end

function Heap:Insert(new, node)
    if self.cmp(node[1], new[1]) then --
        new.children = {node}
        if node.parent == nil then --ԭ�ڵ����׽ڵ㣬�½ڵ��Ϊ�µĶѶ�
            self.top = new
            node.parent = new
        else --�½ڵ�ȡ��ԭ�ڵ����׽ڵ㴦��λ��
            local silbings = node.parent.children
            silbings[(node == silbings[1]) and 1 or 2] = new
            new.parent, node.parent = node.parent, new
        end
        self:UpdateBranch(new)
    else --�½ڵ�����ȼ����ߣ��½ڵ��Ϊԭ�ڵ�ĺ��
        if node.children==nil then --ԭ�ڵ����ӽڵ�
            node.children = {new}
            new.parent = node
        else
            local children = node.children
            if #children==1 then --ԭ�ڵ���1���ӽڵ㣬�½ڵ��Ϊԭ�ڵ�ĵ�2���ӽڵ�
                children[2] = new
                new.parent = node
            else --ԭ�ڵ���ӽڵ��������½ڵ���ԭ�ڵ������ӽڵ���бȽ�
                self:Insert(new, children[self.age%2+1])
            end
        end
    end
end

function Heap:UpdateBranch(parent) --�����ܽ���֧���򶥲��ƶ�����������Ч��
    local children = parent.children
    if children==nil or #children==2 then --�������
        return
    end
    local child = children[1]
    local grandChildren = child.children
    if grandChildren==nil then --û�пɽ����ڵ�
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
    if children==nil then return end --�ѵף�����Ƚ�
    local elder = children[1]
    if children[2]~=nil and self.cmp(elder[1], children[2][1]) then --���ڵڶ����ӽڵ㣬�����ȼ����ڵ�һ��
        elder = children[2]
    end
    if self.cmp(elder[1], parent[1]) then --�ӽڵ����ȼ��������׽ڵ㣬�������
        return
    else --�׽ڵ����ȼ������ӽڵ㣬����ֵ��������һ������
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
