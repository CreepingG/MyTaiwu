<script lang="ts">
	import Row from './Row.svelte';
	import List from './List.svelte';
	import Picker from './Picker.svelte';
	import Range from './Range.svelte';
	const data:any = {};
	function submit(){
		let args = Object.entries(data)
			.map(([k,v])=>[k,v&&v.toString()])
			.filter(pair=>pair[1])
			.map(([k,v])=>'|'+k+'='+v.replaceAll('=','{{=}}').replaceAll('|','{{!}}'))
			.join('');
		console.log(args);
	}
	function key(ev:KeyboardEvent){
		if (ev.key === "Enter"){
			submit();
		}
	}
</script>

<table on:keydown={key} tabindex="-1"> <!--tabindex="-1"才能让table触发键盘事件-->
	<Row name=名称 row={true}>
		<input id="iname" type="text" placeholder="例如：&quot;.*碎片&quot;" />
		<span>*支持<a target="_blank" href="https://taiwu.huijiwiki.com/p/21366">正则表达式</a></span>
	</Row>
	<Row name=品级>
		<Range bind:value={data['level']}></Range>
	</Row>
	<Row name=类型>
		<List bind:value={data['category']}
			multiple={false}
			items={['全部','装备']}></List>
		<List bind:value={data['equip']}
			multiple={false} visible={data['category']?.has('装备')}
			items={['全部','武器','护具','宝物','代步','衣着']}></List>
		<List bind:value={data['weapon']}
			multiple={true} visible={data['equip']?.has('武器')} 
			items={['针匣','对刺','暗器','箫笛','掌套','短杵','拂尘','长鞭','剑','刀','长兵','瑶琴','神兵','随机应变']}></List>
		<List bind:value={data['armor']}
			multiple={false} visible={data['equip']?.has('护具')} 
			items={['全部','头部','护体','足部']}></List>
	</Row>
	<Row name=材质 visible={['武器','护具','宝物'].some(s=>data['equip']?.has(s))}>
		<List bind:value={data['made-by']}
			multiple={true}
			items={['金铁','玉石','竹木','布帛','其他']}></List>
		<List bind:value={data['hardness']}
			multiple={true}
			items={['软','硬','其他']}></List>
	</Row>
	<Row name=攻速 visible={data['equip']?.has('武器')}>
		<List bind:value={data['attack-speed']}
			multiple={true}
			items={['极快','较快','普通','较慢','极慢']}></List>
	</Row>
	<Row name=招式 visible={data['equip']?.has('武器')}>
		<Picker bind:value = {data['attack-type']}
			items={['掷','弹','御','劈','刺','撩','崩','点','拿','音','缠','咒','机','毒','扫']}></Picker>
	</Row>
	<Row name=攻击范围 visible={data['equip']?.has('武器')}>
		<List bind:value={data['attack-range']}
			multiple={true}
			items={['2.0','2.5','3.0','3.5','4.0','4.5','5.0','5.5','6.0','6.5','7.0','7.5','8.0','8.5','9.0']}></List>
		<span>*查找攻击范围能覆盖以上所有被选距离的武器</span>
	</Row>
	<Row name=排序 row={true}>
		<input bind:value={data['排序']} type="text" placeholder="例如：&quot;伤害/攻击间隔&quot;"/>
		<span>*根据此表达式，对结果排序。<a target="_blank" href="https://taiwu.huijiwiki.com/p/21373">可用的表达式</a></span>
	</Row>
	<Row name=筛选 row={true}>
		<input bind:value={data['筛选']} type="text" placeholder="例如：&quot;伤害>20&quot;" />
		<span>*根据此表达式，过滤掉不符合条件的结果。</span>
	</Row>
	<Row>
		<button on:click={submit} class="btn mw-ui-button mw-ui-progressive fa fa-search" type="submit">查询</button>
	</Row>
</table>

<style>
	table{
		display:flex;
		flex-direction:column;
		align-items:flex-start;
	}
	input{
		margin-right:10px;
	}
</style>