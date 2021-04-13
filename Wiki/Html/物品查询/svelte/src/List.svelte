<script lang="ts">
	import { onMount } from 'svelte';

	export let items: string[];
	export let multiple: boolean;
	export let visible: boolean = true;
	export let value: any;
	const SELECTED = 'selected';
	const selection = new Set<number>();
	function parsedSelection(){
		let s = new Set([...selection.keys()].map(i => items[i]));
		s.toString = function(){
			return [...(<Set<string>>this).keys()].join('|');
		}
		return s;
	}

	function update(){
		value = parsedSelection();
		return '';
	}
	function remove(){
		value = null;
		return '';
	}
	function multiple_click(this:HTMLElement){
		const index = Number(this.getAttribute('data-index'));
		this.classList.toggle(SELECTED);
		if (selection.has(index)){
			selection.delete(index)
		}
		else{
			selection.add(index);
		}
		update();
	}
	function single_click(this:HTMLElement){
		const index = Number(this.getAttribute('data-index'));
		for (let c of this.parentElement.children){
			c.classList.remove(SELECTED);
		}
		this.classList.add(SELECTED);
		selection.clear();
		selection.add(index);
		update();
	}

	onMount(()=>{
		if (!multiple){
			selection.add(0);
		}
		update();
	});
</script>

{#if visible}
	<ul class:single={!multiple}>
		{#each items as item,i}
			<li class:selected={!multiple && i===0} on:click={(multiple ? multiple_click : single_click)} data-index={i}>{item}</li>
		{/each}
	</ul>
	{update()}
{:else}
	{remove()}
{/if}

<style>
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
ul.single > li{
    border-radius: 8px;
}
li.selected{
    border-color: #cfee1d; 
    color: #cfee1d; 
    background-color: #3c421a;
}
</style>