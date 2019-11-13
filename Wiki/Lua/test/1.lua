<!--ewiv info DO NOT edit 模块:item-->
local p = {
	main = function(...) return require('Module:Item/main')(...) end,
	mian = function(...) return require('Module:Item/main')(...) end,
}

local Debug = {}

local RichTab = require('Module:RichTab').meta
local NavBox = require('Module:Navbox').meta
local Name = mw.loadData("Module:Const/name")
local Make = mw.loadData("Module:Item/make")
local Data = mw.loadData("Module:Item/data")
local Util = require('Module:Util')

local MakeSkills = mw.text.split('制木|锻造|制石|织锦|厨艺|医毒', '|', true)
local Immortals = mw.text.split('莫女衣|伏邪铁|大玄凝|凤凰茧|焚神练|解龙魄|溶尘隐|囚魔木|鬼神霞|伏虞剑', '|', true)

local All = Data.raw

local function FindOne(key)
    key = Data.id[key] or Data.name[key]
	return All[key]
end

function p.link(t)
	if t==nil then error('Nil Argument') end
	local tuple
	if type(t)=='table' then
		if t.args then --{{#invoke
			local key = t.args[1]
			if key==nil or key=='' or key=='无' then return '无' end
			key = tonumber(key) or mw.ustring.gsub(key, '^.-·', '', 1) -- id || 大玄凝·化形·冰刃 → 化形·冰刃
			tuple = FindOne(key)
		else
			tuple = t
		end
	else
		tuple = FindOne(tonumber(t) or t)
	end
	
	local name = tuple.name
    local level = tuple.data8
    local color = Name.color[level]
    if color==nil then error(mw.dumpObject(tuple)) end
    local result = '[[文件:ItemIcon '..tuple.data98..' 20.png|link=]][['..'物品/'..name..'|<span style="color:'..color..'">'..name..'</span>]]'
    result = '<span class="huiji-tt" data-type="Item" data-name="'..name..'" style="display:inline-block">'..result..'</span>'
	
	return result
end
p.link2 = p.link

function p.tooltip(frame) --p.tooltip({'离离千世'})
	local args = frame.args or frame
	local name = args[1]
	if name==nil then error(mw.dumpObject(frame)) end
	local id = tonumber(name)
	if id then
		local item = id and FindOne(id)
		name = item.name
	end
	local result = mw.smw.ask({'[[物品/'..name..']]', '?Brief'})[1].Brief
	--mw.logObject(result)
	return result
end

local material = {"织造", "木制", "玉石", "金铁"}
local function CmpMakeName(a, b, cnt)
    cnt = cnt or 1
    if cnt >= 4 then --同为金铁，直接比较字符串
        return a < b
    end
    local flag1 = mw.ustring.find(a, material[cnt], 1, true)
    local flag2 = mw.ustring.find(b, material[cnt], 1, true)
    if flag1 and not flag2 then
        return true
    elseif not flag1 and flag2 then
        return false
    elseif flag1 and flag2 then --材质相同，直接比较字符串
        return a < b
    end
    return CmpMakeName(a, b, cnt + 1) --未找到材质，判断下一级
end

local function MakeLine(id,id2,sep)
    local list = {}
    local cnt = 0
    id2 = id2 or 9
    id2 = id2>id and id2 or id+id2-1
    for j=id,id2 do
        local item = FindOne(j)
        cnt = cnt + 1
        if item==nil then error('无此物品:'..j..'\n'..mw.dumpObject(Debug)) end
        list[cnt] = p.link(j)
    end
    local result = table.concat(list, sep or "·")
    return result
end

function p.series(frame)
	local args = frame.args
	local id = tonumber(args[1])
	local sep = args.sep or '·'
	if args[2] then return MakeLine(id, tonumber(args[2]), sep) end
	local first = id
	local last = id
	local tuple = FindOne(id)
	local oldLevel = tuple.data8
	while true do
		id = id + 1
		tuple = FindOne(id)
		if tuple==nil then break end
		local newLevel = tuple.data8
		if newLevel < oldLevel then break end
		oldLevel = newLevel
		last = id
	end
	return MakeLine(first, last, sep)
end
	
local function MakeLines(list,child)
    local oldLevel = 0
    local newLevel = 0
    local result = {}
    local head
    local tail
    for k,v in pairs(list) do
        local id = v[1]
        newLevel = v[2]
        head = head or id --获取第一个id
        if newLevel < oldLevel or (tail and id~=tail+1) then
        	Debug.head = head
        	Debug.tail = tail
        	Debug.list = list
            table.insert(result, MakeLine(head,tail))
            head = id
        end
        oldLevel = newLevel
        tail = id --记录上一个id
    end
    table.insert(result, MakeLine(head,tail))

    local nav = NavBox:new()
    if child then
        nav[1] = 'child'
    end
    for k,v in pairs(result) do
        nav:add(v)
    end
    return tostring(nav)
end

local function GetResult(rich, name)
	if name then
	    local cnt = rich.cnt
	    local tabBar = rich.tabBar
	    for i=1,cnt do
	    	if tabBar[i].tabName==name then
	    		local result = rich.tabContent[i].tabText
	    		--mw.log(result)
	    		return result
	    	end
	    end
	    return '无此标签'
	end
    return rich
end

function p.equip(frame)
    local args = frame.args or frame
    local name = args[1]
    
    local make = Make.data
    local makeObjs = {}
    for k, makeTyp in pairs(make) do
        if makeTyp.data97 > 0 then
            local makeObj = {}
            local makeName = makeTyp.data0
            makeObj.name = makeName
            local hard = makeTyp.hard
            local soft = makeTyp.soft
            local first = tonumber(hard[1])
            local itemTyp = FindOne(first).data5 -- 以首个产物的类型来设定该种生产的分类
            makeObj.nav = NavBox:new({above = makeName, [1] = 'child'})
            for i, id in ipairs(hard) do -- 九品~一品 为一行
                id = tonumber(id)
                makeObj.nav:add(MakeLine(id), "硬")
            end
            for i, id in ipairs(soft) do
                id = tonumber(id)
                makeObj.nav:add(MakeLine(id), "软")
            end
            makeObj.nav = tostring(makeObj.nav)
            if makeObjs[itemTyp] == nil then
                makeObjs[itemTyp] = {}
            end
            table.insert(makeObjs[itemTyp], makeObj)
        end
    end
    make = nil -- 清空物品制造数据
    
    local animal = {}
    animal.name = '动物代步'
    animal.nav = NavBox:new({above = animal.name, [1] = 'child'})
    animal.nav:add(MakeLine(83601))
    animal.nav = tostring(animal.nav)
    table.insert(makeObjs[18], animal)
    animal = nil
    
    local uniform = {}
    uniform.name = '门派衣着'
    uniform.nav = NavBox:new({['above'] = uniform.name, [1] = 'child'})
    uniform.nav:add(MakeLine(73801), '低阶')
    uniform.nav:add(MakeLine(73901), '高阶')
    uniform.nav = tostring(uniform.nav)
    table.insert(makeObjs[17], uniform)
    uniform = nil
    
    local rich = RichTab:new()
    local tabCnt = 0
    for itemTyp, v in pairs(makeObjs) do -- 每种物品类型为一个navBox，并作为richTab的一页
        --按制造类型的名称排序
        table.sort(v , function(a , b)
            return CmpMakeName(a.name, b.name)
        end)
        local page = NavBox:new({
            ['listclass'] = 'hlist',
            ['liststyle'] = 'color:rgb(155,135,115)',
        })
        for i, makeObj in pairs(v) do
            page:add(makeObj.nav)
        end
        makeObjs[itemTyp] = nil --清理
        rich:add(Name.item[itemTyp], page)
    end
    
    local page = NavBox:new({
        ['listclass'] = 'hlist',
        ['liststyle'] = 'color:rgb(155,135,115)',
    })
    local child = NavBox:new({[1]='child', above='机关'})
    child:add(MakeLine(40501), '锻造')
    child:add(MakeLine(40601), '制木')
    page:add(child)
    child = NavBox:new({[1]='child', above='令符'})
    child:add(MakeLine(40701), '制石')
    child:add(MakeLine(40801), '织锦')
    page:add(child)
    child = NavBox:new({[1]='child', above='毒器'})
    child:add(MakeLine(40301), '炼药')
    child:add(MakeLine(40401), '炼毒')
    page:add(child)
    
    rich:add('副产物', page)
    
    return GetResult(rich, name)
end

local function MedChild(above, id1, id2, name1, name2)
	local child = NavBox:new({[1] = 'child'})
    child.above = above
    child:add(MakeLine(id1, 6)..'<br/>'..MakeLine(id1+6, 6), name1)
    if id2 then
    	child:add(MakeLine(id2, 6)..'<br/>'..MakeLine(id2+6, 6), name2)
    end
    return child
end
local function MedicineNav()
    local nav = NavBox:new({
        ['listclass'] = 'hlist',
        ['liststyle'] = 'color:rgb(155,135,115)',
    })
    
    nav:add(MedChild('疗伤药',100101,100113,'外伤','内伤'))

    nav:add(MedChild('内息药',100125))

    nav:add(MedChild('健康药', 100137))

    local child = NavBox:new({[1] = 'child'})
    child.above = '解毒药'
    for i=0,5 do
        child:add(MakeLine(100173 + i*2*6,6)..'<br/>'..MakeLine(100173+(i*2+1)*6,6), Name.poison[i])
    end
    nav:add(child)

    local child = NavBox:new({[1] = 'child'})
    child.above = '战斗药'
    local itemId = 100149
    child:add(MakeLine(itemId,6)..'<br/>'..MakeLine(itemId+6,6), '外伤上限')
    itemId = itemId + 12
    child:add(MakeLine(itemId,6)..'<br/>'..MakeLine(itemId+6,6), '内伤上限')
    itemId = 100245
    for k,v in ipairs({'护体','御气','卸力','拆招','闪避','内息','力道%','提气速度','精妙%','架势速度','迅疾%','造成内外伤',}) do
    	child:add(MakeLine(itemId,6)..'<br/>'..MakeLine(itemId+6,6), v)
    	itemId = itemId + 12
    end
    nav:add(child)

    return nav
end
local function MaterialNav()
    local name1 = {'硬','软'}
    local name2 = {'木','铁','玉','布'}
    local nav = NavBox:new({
        ['listclass'] = 'hlist',
        ['liststyle'] = 'color:rgb(155,135,115)',
    })
    
    local child = NavBox:new({'child'})
    child.above = '引子'
    for i=1,4 do
        for j=1,2 do
            local itemId = 3001 + (i-1)*100 + (j-1)*7
            child:add(MakeLine(itemId, 7), name1[j]..name2[i])
        end
    end
    nav:add(child)

    child = NavBox:new({'child'})
    child.above = '精制'
    for i=1,4 do
        for j=1,2 do
            local itemId = 3501 + (i-1)*100 + (j-1)*7
            child:add(MakeLine(itemId, 7), name1[j]..name2[i])
        end
    end
    nav:add(child)
    
    return nav
end
local function DrinkNav()
    local nav = NavBox:new({
        ['listclass'] = 'hlist',
        ['liststyle'] = 'color:rgb(155,135,115)',
    })
    
    local child = NavBox:new({'child'})
    child.above = '茶'
    child:add(MakeLine(100601), '外伤')
    child:add(MakeLine(100610), '内伤')
    nav:add(child)
    
    child = NavBox:new({'child'})
    child.above = '酒'
    child:add(MakeLine(100501), '破体')
    child:add(MakeLine(100510), '破气')
    nav:add(child)
    
    return nav
end
local function ImmortalNav()
    local nav = NavBox:new()
    for i=1,10 do
        local itemId = 110001 + (i-1)
        local sep = '&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;'
        local list = p.link(itemId)..sep..p.link(itemId+10)..sep..p.link(itemId+20)..sep..p.link(20101+(i-1))
        nav:add(list, Immortals[i])
    end
    return nav
end
local function OtherNav()
    local nav = NavBox:new()
    nav:add(MakeLine(11, 15), '资源包')
    nav:add(MakeLine(21), '血露')
    nav:add(MakeLine(81, 99), '促织相关')
    nav:add(MakeLine(31, 35), '武林大会')
    return nav
end
local function SpNav()
	local nav = NavBox:new()
	nav:add(MakeLine(0, 2), '随机应变')
	nav:add(MakeLine(5001, 5005), '剧情物品')
	return nav
end
local function WestNav()
	local nav = NavBox:new()
	for i, xxName in ipairs(Name.xx) do
		nav:add(MakeLine(100701+(i-1)*5, 5), xxName)
	end
	return nav
end

local function Classify()
    local result = Util.groupBy(All, function(tuple) return tuple.data5 or 0 end)
    for k,v in pairs(result) do
    	result[k] = Util.map(
	    	v, 
	    	function(item) return {item.id or 0, item.data8 or 0} end
	    )
        table.sort(result[k] , function(a , b)
            return a[1] < b[1]
        end)
    end
    return result
end

function p.others(frame)
    local classified = Classify()
    local rich = RichTab:new()
    local args = frame.args or frame
    local name = args[1]
    
    local nav = NavBox:new()
    for i=1,6 do
    	nav:add(MakeLine(10001 + (i-1)*100), MakeSkills[i])
    end
    rich:add('工具', nav)
    
    rich:add('材料', MaterialNav())
    
    local nav = NavBox:new()
    for i=0,5 do
    	nav:add(MakeLine(4201+i*7,4207+i*7), Name.poison[i])
    end
    rich:add('毒物', nav)
    
    local nav = NavBox:new()
    for i=0,5 do
    	nav:add(MakeLine(100001+i*9), Name.poison[i])
    end
    rich:add('毒药', nav)
    
    rich:add('药材', MakeLines(classified[29]))
    rich:add('丹药', MedicineNav())
    rich:add('杂物', OtherNav())
    rich:add('蛰罐', MakeLines(classified[33]))
    rich:add('食材', MakeLines(classified[23]))
    rich:add('素食', MakeLines(classified[34]))
    rich:add('荤食', MakeLines(classified[35]))
    rich:add('神兵', ImmortalNav())
    rich:add('茶酒', DrinkNav())
    rich:add('宝典', MakeLines(classified[42]))
    rich:add('特殊', SpNav())
    rich:add('西域', WestNav())
    
    return GetResult(rich, name)
end


function p.t()
	local c = Classify()
	mw.logObject(c[34])
end

function p.s()
	local str = [[200001|200043|200085|200127
80201|80301|80401|80501|80601|80701|81001|81101|81601|81701|81801|81901|82401|82501|82601|82701|82801|82810|82901|82910|83001|83101|82001|82101|82201|82301|81201|81301|81401|81501|83201|83301|80801|80810|80819|80901|80910|80919|83401|83501
50201|50301|50401|50501|50601|50701|51001|51101|51601|51701|51801|51901|52001|52101|52201|52301|52401|52501|52601|52701|52801|52901|53001|53101|53201|53301|53401|53501|53601|53701|53801|53901|51201|51301|51401|51501|54001|54101|54201|54301|50801|50901
72001|72101|72201|72301|72401|72501|72601|72701|70401|70501|70601|70701|70801|70901|71001|71101|71201|71301|71401|71501|71601|71701|71801|71901|72801|72901|73001|73101|73201|73301|73401|73501|70201|70301|73601|73701
100101|100107|100113|100119|100125|100131|100137|100143|100149|100155|100161|100167|100173|100179|100185|100191|100197|100203|100209|100215|100221|100227|100233|100239|100245|100251|100257|100263|100269|100275|100281|100287|100293|100299|100305|100311|100317|100323|100329|100335|100341|100347|100353|100359|100365|100371|100377|100383
10001|10101|10201|10301|10401|10501|100001|100010|100019|100028|100037|100046|200001|200043|200085|200127|80201|80301|80401|80501|80601|80701|81001|81101|81601|81701|81801|81901|82401|82501|82601|82701|82801|82810|82901|82910|83001|83101|82001|82101|82201|82301|81201|81301|81401|81501|83201|83301|80801|80810|80819|80901|80910|80919|83401|83501|50201|50301|60201|60301|50401|50501|60401|60501|50601|50701|60601|60701|51001|51101|62201|62301|51601|51701|51801|51901|62801|62901|52001|52101|63001|63101|52201|52301|52401|52501|52601|52701|63201|63301|52801|52901|53001|53101|53201|53301|63401|63501|53401|53501|53601|53701|63601|63701|53801|53901|63801|63901|51201|51301|62401|62501|51401|51501|62601|62701|54001|54101|54201|54301|64001|64101|50801|50901|60801|60901|61001|61101|61201|61301|61401|61501|61601|61701|61801|61901|62001|62101|72001|72101|72201|72301|72401|72501|72601|72701|70401|70501|70601|70701|70801|70901|71001|71101|71201|71301|71401|71501|71601|71701|71801|71901|72801|72901|73001|73101|73201|73301|73401|73501|70201|70301|73601|73701|100101|100113|100125|100137|100149|100161|100173|100185|100197|100209|100221|100233|100245|100257|100269|100281|100293|100305|100317|100329|100341|100353|100365|100377|100501|100510|100601|100610|83401|83501|83601|4301
400001|400010|400019|400028|400037|400046|400055|400064|400073|400082|400091|400100|400109|400118|400127|400136|100501|100510|100601|100610|83401|83501|83601
60201|60301|60401|60501|60601|60701|62201|62301|62801|62901|63001|63101|63201|63301|63401|63501|63601|63701|63801|63901|62401|62501|62601|62701|64001|64101|60801|60901|61001|61101|61201|61301|61401|61501|61601|61701|61801|61901|62001|62101
100001|100010|100019|100028|100037|100046
]]
	for id in string.gmatch(str, '%d+') do
		id = tonumber(id)
		mw.log(FindOne(id).data0)
	end
end

return p