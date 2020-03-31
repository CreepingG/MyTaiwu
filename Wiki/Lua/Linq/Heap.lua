local Heap = {cmp = function(a,b) return a < b end}

function Heap:Push(elem)
    if #self < self.size then -- 堆未满，将新元素加入堆
        self[#self+1] = elem
        if #self == self.size then table.sort(self, function(a,b) return not self.cmp(a,b) end) end --堆满刚时进行一次排序
    elseif self.cmp(elem, self[1]) then --堆已满，比较新元素与堆顶。若新元素更优：新元素替换堆顶，更新堆以保证堆顶为最劣元素
        self[1] = elem
        self:Update(1)
    end
    return self
end

function Heap:Update(index)
    local left = index * 2
    if left > self.size then return end
    local right = left + 1
    local childIndex = right <= self.size and self.cmp(self[left], self[right]) and right or left --较劣的子元素

    if self.cmp(self[index], self[childIndex]) then --新元素更优，交换
        self[index], self[childIndex] = self[childIndex], self[index]
        self:Update(childIndex)
    end
end

function Heap:Done()
    table.sort(self, self.cmp)
    return self
end

return function(size, cmp)
    return setmetatable({
        size = size,
        cmp = cmp
    }, {
        __index = Heap
    })
end
