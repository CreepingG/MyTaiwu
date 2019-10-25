/* eslint-disable no-new */
if (typeof (window.LoadScriptList) === 'undefined') window.LoadScriptList = [];
window.LoadScriptList.push(function(){
	const NAN = '??';
	function Range(n){
		return Math.max(0, Math.min(100, n));
	}
	function RoundToInt(float){
		const floor = Math.floor(float);
		if (floor % 2 === 0 && Math.floor((float - floor) * 10) === 5){ // 五取偶
			return floor;
		}
		return Math.round(float);
	}
	function GongfaValue(基础提气比, 内功发挥){
		if (isNaN(基础提气比) || isNaN(内功发挥)) return [NAN, NAN];
		const 转化难度 = (内功发挥 < 0) ?
			((基础提气比 <= 50) ?
				[1, '1'] : [1 + (基础提气比 - 50) / 50, `[1 + (基础提气比${基础提气比} - 50) / 50]`]
			) :
			((基础提气比 >= 50) ?
				[1, '1'] : [1 + (50 - 基础提气比) / 50, `[1 + (50 - 基础提气比${基础提气比}) / 50]`]
		);
		const 内伤比 = 基础提气比 + 内功发挥 / 转化难度[0];
		return [Range(RoundToInt(内伤比)), `提气比${基础提气比} + 内功发挥${内功发挥} / 转化难度${转化难度[1]}`];
	}
	function WeaponValue(基础内伤比, 内功传导率, 内功发挥){
		if (isNaN(基础内伤比) || isNaN(内功传导率) || isNaN(内功发挥)) return [NAN, NAN];
		内功发挥 = [内功发挥 * 内功传导率 / 100, `(内功发挥${内功发挥} × 内功传导率${内功传导率}%)`];
		const 转化难度 = (内功发挥[0] < 0) ?
			((基础内伤比 <= 50) ?
				[1, '1'] : [1 + (基础内伤比 - 50) / 50, `[1 + (基础内伤比${基础内伤比} - 50) / 50]`]
			) :
			((基础内伤比 >= 50) ?
				[1, '1'] : [1 + (50 - 基础内伤比) / 50, `[1 + (50 - 基础内伤比${基础内伤比}) / 50]`]
		);
		const 内伤比 = 基础内伤比 + 内功发挥[0] / 转化难度[0];
		return [Range(RoundToInt(内伤比)), `基础内伤比${基础内伤比} + ${内功发挥[1]} / 转化难度${转化难度[1]}`];
	}
	function Obj2Arr(o){
		const arr = Object.values(o);
		arr.find = pattern =>
			arr.filter(v => v.data0.search(pattern) > -1);
		return arr;
	}
	function Average(a, b){
		if (isNaN(a) || isNaN(b)) return NAN;
		return (a + b) / 2;
	}
	let data = {
		weapon : {
			'1':{data0:'空手', data10:0, data17:100}
		},
		gongfa : {
			'1':{data0:'太祖长拳', data8:0},
			'2':{data0:'太乙玄门剑', data8:80},
		}
	};
	data.weapon = Obj2Arr(data.weapon);
	data.gongfa = Obj2Arr(data.gongfa);
	let onceFlag = false; // 防止死循环
	function Watch(key, newValue, oldValue){
		if (onceFlag) return;
		if (oldValue.search(newValue) > -1) { // 退格中
			this[key] = null;
			return;
		}
		const matches = data[key].find(newValue);
		const len = matches.length;
		if (len === 1){
			this[key] = matches[0];
			onceFlag = true;
			this[key + 'Name'] = this[key].data0;
			onceFlag = false;
		}
		else if (len === 0){
			this[key] = null;
		}
		else {
			this[key + 'List'] = matches;
		}
	}
	new Vue({
		el: '#gongfa-magic',
		data: {
			data1105: 0,
			weapon: null,
			weaponName: '',
			gongfa: null,
			gongfaName: '',
			weaponText: NAN,
			gongfaText: NAN,
			weaponList:[],
			gongfaList:[],
			weapon10: NAN,
			weapon17: NAN,
			gongfa8: NAN
		},
		computed: {
			finalText: function() {
				this.weapon10 = this.weapon == null ? NAN : this.weapon.data10;
				this.weapon17 = this.weapon == null ? NAN : this.weapon.data17;
				this.gongfa8 = this.gongfa == null ? NAN : this.gongfa.data8;
				const weaponValue = WeaponValue(this.weapon10, this.weapon17, this.data1105);
				const gongfaValue = GongfaValue(this.gongfa8, this.data1105);
				this.weaponText = weaponValue[0] === NAN ? NAN : `${weaponValue[1]} = ${weaponValue[0]}`;
				this.gongfaText = gongfaValue[0] === NAN ? NAN : `${gongfaValue[1]} = ${gongfaValue[0]}`;
				return `(武器内伤比${weaponValue[0]} + 功法内伤比${gongfaValue[0]}) / 2 = ${Average(weaponValue[0], gongfaValue[0])}`;
			}
		},
		watch: {
			weaponName: function(newValue, oldValue){
				Watch.call(this, 'weapon', newValue, oldValue);
			},
			gongfaName: function (newValue, oldValue) {
				Watch.call(this, 'gongfa', newValue, oldValue);
			},
		},
	});
});

/* 在别处调用 */
window.$(function(){
	const list = window.LoadScriptList;
	if (typeof (list) !== 'undefined' && list.constructor === Array){
		list.forEach(function(action){
			action(window.$);
		});
	}
});