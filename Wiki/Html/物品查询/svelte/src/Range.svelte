<script lang="ts">
	export let value: string = '';
	const default_value = '1,9';

	const LEVEL = [
        '<span style="color:#8E8E8E">下·九品</span>',
        '<span style="color:#FFFFFF">中·八品</span>',
        '<span style="color:#6DB75F">上·七品</span>',
        '<span style="color:#8FBAE7">奇·六品</span>',
        '<span style="color:#63CED0">秘·五品</span>',
        '<span style="color:#AE5AC8">极·四品</span>',
        '<span style="color:#E3C66D">超·三品</span>',
        '<span style="color:#F28234">绝·二品</span>',
        '<span style="color:#E4504D">神·一品</span>',
    ];
    
	let wait = new Promise<void>((resolve,reject)=>{
		let handle:number;
		handle = setInterval(()=>{
			if (window['jQuery']?.jRange){
				clearInterval(handle);
				window['jQuery']('#level').jRange({
					from: 1,
					to: 9,
					step: 1,
					scale: [9,8,7,6,5,4,3,2,1],
					format: function(n: number){
						return LEVEL[n - 1];
					},
					width: Math.min(300, document.body.clientWidth - 100),
					showLabels: true,
					isRange : true,
					onstatechange: (v:string) => {value = (v === default_value ? '' : v)}
				});
				resolve();
			}
		}, 200);
	});
</script>

<div style="padding:20px 0;">
	<input id="level" class="jrange" type="hidden" value={default_value}/>
	{#await wait}
		<div>加载中...</div>
	{/await}
</div>

<style>
</style>