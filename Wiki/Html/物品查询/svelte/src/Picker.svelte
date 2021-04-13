<script lang="ts">
	export let items: string[];
	export let value: any;
	let cnt:number[] = [];
	let text = '';

	function update(){
		text = items.map((v,i) => [v,(cnt[i] || 0)]).filter(pair=>pair[1]>0).map(pair => pair[0] + '×' + pair[1]).join(',');

		let data = Object.fromEntries(items.map((v,i) => [v, cnt[i] || 0]));
		Object.setPrototypeOf(data, {toString: function(){
			return Object.entries(this).filter(pair=>pair[1]>0).map(([k,v])=>k+v).join('')
		}});
		value = data;
	}

	function click(this:HTMLElement){
		const index = Number(this.getAttribute('data-index'));
		cnt[index] = ((cnt[index] || 0) + 1) % 7;
		update();
	}
	function reset(){
		cnt = [];
		update();
	}
</script>

<form>
	<input type="text" readonly={true} bind:value={text} placeholder="点击下方按钮以添加" />
	<button on:click={reset} class="btn mw-ui-button mw-ui-progressive fa fa-refresh" type="reset">清除</button>
</form>
<ul>
	{#each items as item,i}
		<li on:click={click} data-index={i}>{item}</li>
	{/each}
</ul>

<style>
	form{
		display: flex;
		align-items: center;
		margin-top: 12px;
	}
	form > input{
		margin-right: 5px;
	}
	ul{
		display:flex;
		flex-wrap: wrap;
		margin: 0;
	}
	li{
		display:inline-block;
		min-width: 50px; 
		margin: 2px 2px; 
		padding: 0 5px; 
		text-align: center; 
		border: 1px solid #565353; 
		color: #fff; 
		cursor: pointer;
		user-select: none;
	}
</style>