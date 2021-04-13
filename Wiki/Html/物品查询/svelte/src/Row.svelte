<script lang="ts">
	export let name: string = '';
	export let visible: boolean = true;
	export let row: boolean = false;
</script>

<tr class:hidden={!visible}>
	{#if name}
	<th><div>{name}</div></th>
	{/if}
	<td class:row={row}><slot></slot></td>
</tr>

<style>
	.hidden{
		display: none !important;
	}
	tr{
		display: flex;
		flex-direction: row;
		min-height: 50px;
	}
	th{
		display: flex;
		align-items: flex-start;
		flex-shrink: 0;
		width: 50px;
	}
	th > div{
		padding-top: 15px;
		display: flex;
		width: 2em;
	}
	td{
		display: flex;
		flex-direction: column;
		justify-content: center;
	}
	td.row{
		flex-direction: row;
		align-items: center;
		flex-wrap: wrap;
		margin: 12px 0;
	}
</style>