local p = {}

local E = require('Module:ECharts')
local Util = require('Module:Util')
local Const = require('Module:Const/name')
local Data = require('Module:GangGroupValue/data')
local GangValueData = require('Module:GangGroup/data')
local Linq = require('Module:Linq')
require('Module:no globals')

local function GetGangValueId(gangId,level)
	return GangValueData[gangId][level] or 0
end

local function FindOne(key,key2)
	if type(key)=='table' then return key end
	local id = tonumber(key)
	local index
	if id then
		index = Data.id[id]
		--if id>0 then error(index..','..type(Data.raw[index])) end
	else
		local gangGroup = Data.name[key]
		if gangGroup==nil then error('没有名为'..(key or '')..'的势力') end
		index = gangGroup[key2]
	end
	return Data.raw[index]
end

local function Clamp(value) return math.min(1,math.max(0,value)) end
local function GenerateOne(baseValue, upSize)
	local resultValue = 25
	if baseValue<0 then
		return resultValue, upSize + 20
	end
	if baseValue>0 then
		upSize = upSize - 20
		resultValue = 55

		local chance = Clamp(baseValue/2/100)
		resultValue = resultValue + chance*15
		upSize = upSize - chance*10

		local chance2 = Clamp(baseValue/3/100 * chance)
		resultValue = resultValue + chance2*15
		upSize = upSize - chance2*10

		return resultValue, upSize
	end
	local upSize1 = upSize - 20
	local result1 = 55
	local chance = Clamp(upSize1/100)
	result1 = result1 + chance*15
	upSize1 = upSize1 - chance*10
	local chance2 = Clamp((chance-0.1) * chance)
	result1 = result1 + chance2*15
	upSize1 = upSize1 - chance2*10

	local upSize2 = upSize + 20
	local result2 = resultValue

	local largerWeight = Clamp(upSize/100)
	return largerWeight*result1 + (1-largerWeight)*result2,
		largerWeight*upSize1 + (1-largerWeight)*upSize2
end

local function GenerateList(baseList, power)
	power = power or 0
	local upSize = 50
	local result = {}
	for i,v in ipairs(baseList) do
		result[i],upSize = GenerateOne(v,upSize)
		result[i] = Util.toInt(result[i])
		if power~=0 then
			result[i] = math.floor(result[i]*(100+power)/100) --外道资质调整
		end
	end
	return result
end

local function MakeChart(data, getText, title)
	data = Util.map(data, function(v,i)
		return {
			text = getText[i],
			value = v,
			max = 80
		}
	end)

	return E.build({
	    option = E.singleRadar(data,{
	    	title = title,
	    	name = '期望值',
	    	max = 100,
	    }),
	    style='max-width:300px',
	    height='90%'
	})
end

function p.gang(frame) --p.gang{args={'武当派','真传弟子'}}
	local gangName = frame.args[1]
	local levelName = frame.args[2]
	local actor = FindOne(gangName,levelName)
	local level = 10 - actor.data2
	local gangId = actor.gangId
	for _,v in ipairs{802,803,804,805} do --关系地位
		local key = 'data'..v
		local gangLevelId = GetGangValueId(gangId, actor[key])
		local social = FindOne(gangLevelId)
		if social==nil then
			actor[key] = nil
		else
			local socialLevel = 10 - social.data2
			actor[key] = Util.color(social.data1001, Const.color[socialLevel])
		end
	end
	for _,v in ipairs{1002,1003} do --配偶称呼
		local key = 'data'..v
		local value = actor[key]
		if value==nil or tonumber(value)==0 then
			actor[key] = nil
		else
			actor[key] = Util.color(value, Const.color[level])
		end
	end
	for _,v in ipairs{
		{'技艺资质',721,'skill'},
		{'武学资质',731,'gongfa'},
		{'主要属性',741,'attr'}
	} do
		local key = 'data'..v[2]
		local rawText = actor[key]
		local list = mw.text.split(rawText,'|',true)
		list = Util.map(list, function(v) return tonumber(v) end)
		local names = Util.map(Util.range(#list), function(i) return Const[v[3]][i-1] end)
		actor[v[1]] = MakeChart(GenerateList(list), names, v[1])
	end
	actor.data801 = require('Module:SurName/data')[actor.data801]
	local randItemLevel = (function()
		local times = math.floor((level+1)/2)
		local chance = math.min(level*5+60,100)/100
		return Linq:Range(
			Util.toInt(times*chance*chance), --脸黑
			Util.toInt(times*chance*math.min(chance*2,1)) --脸白
		):Done()
	end)()
	local weaponList
	actor['功法'],weaponList = require('Module:GongFa/nav').gangActor(actor.data601)
	actor['装备'] = (function()
		weaponList = Linq(weaponList)
			--根据其最大装备摧破数量，按品级取前列，大致模拟NPC运功情况
			:TakeByOrder(require('Module:Actor/maxGongFaCount')[level][2],function(pair) return -pair[2] end)
			--将id相同的武器合并
			:GroupBy(function(pair) return pair[1] end)
			--选出数量最多的3种武器
			:TakeByOrder(3, function(group) return -#group end)
			--获得其id
			:Select(function(group) return group.key end)
			:Done()
		local gangEquip = require('Module:GangEquip/data')[actor.data501] or {}
		local equip = {
			['兵器一'] = weaponList[1],
			['兵器二'] = weaponList[2],
			['兵器三'] = weaponList[3],
			['头部'] = gangEquip[304],
			['护甲'] = gangEquip[306],
			['足部'] = gangEquip[307],
			['宝物一'] = gangEquip[308],
			['宝物二'] = gangEquip[309],
			['宝物三'] = gangEquip[310],
			['代步'] = gangEquip[311],
			['衣着'] = actor.data751,
		}
		local equipLevel = Util.clamp(randItemLevel[#randItemLevel],1,9) --多次获取，留下最好的，故按较幸运的情况判断
		for k,v in pairs(equip) do
			if v==0 then
				equip[k] = nil
			elseif k~='衣着' then
				equip[k] = v + equipLevel - 1
			end
		end
		return mw.getCurrentFrame():expandTemplate{ title = '人物装备列表', args = equip }
	end)()
	actor['物品'] = require('Module:Item/nav').gangActor(actor.data502, randItemLevel)

	return mw.getCurrentFrame():expandTemplate{ title = '普通NPC页面', args = actor }
end

function p.creep(frame) --p.creep{args={'山大王'}}
	local actor = (function(key)
		local data = require('Module:PresetActor/data')
		return data.raw[data.name[key]]
	end)(frame.args[1])
	local enemyRand = require('Module:EnemyRand/data')[actor.data8]
	actor['内力'] = enemyRand.data3
	actor['造诣'] = enemyRand.data5
	actor['精纯'] = enemyRand.data6
	local gangList = Linq(mw.text.split(enemyRand.data2,'|',true))
		:Select(function(s) return tonumber(s) end)
		:Done()
	local gangGroupValueList = Linq(gangList)
		:Select(function(gangId) return FindOne(GetGangValueId(gangId,actor.data20)) end)
	--属性
	for _,option in ipairs{
		{'技艺资质',721,'skill', 'data7'},
		{'武学资质',731,'gongfa', 'data8'},
		{'主要属性',741,'attr'}
	} do
		local key = 'data'..option[2]
		local baseValueList = gangGroupValueList
			:Select(function(tuple) return tuple[key] end)
			:Select(function(rawText)
				return Util.map(
					mw.text.split(rawText,'|',true),
					function(v) return tonumber(v) end
				)
			end)
			:Aggregate(function(seed,list)
				if seed==0 then
					return Linq(list)
				else
					return seed:Zip(list, function(v1,v2)
						return v1 + v2
					end)
				end
			end, 0)
		baseValueList = Linq(baseValueList)
			:Select(function(v) return v/#gangList end)
			:Select(math.floor)
			:Done()
		local valueList = GenerateList(baseValueList, enemyRand[option[4]])
		local names = Util.map(Util.range(#valueList), function(i) return Const[option[3]][i-1] end)
		actor[option[1]] = MakeChart(valueList, names, option[1])
	end
	local FeatureLink = require('Module:Feature').show
	actor['特性'] = Linq(mw.text.split(tostring(actor.data101), '|', true))
		:Select(function(s) return tonumber(s) or 0 end)
		:Where(function(id) return id>0 end)
		:Select(function(id) return FeatureLink(id) end)
		:Aggregate(function(seed,link)
			return seed..'、'..link
		end)

end

return p
