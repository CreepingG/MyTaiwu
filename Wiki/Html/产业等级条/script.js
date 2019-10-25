"use strict";
if (typeof(window.LoadScriptList)==='undefined') window.LoadScriptList = [];
window.LoadScriptList.push(function($){
	$('#home-place-level').each(function(){
		$(this).find('table.home-place-level').find('td').each(function(){
			var td = $(this);
			$.each(this.attributes, function(){
				var name = this.name;
				if(this.specified && name.startsWith('data-v-')){
					td.attr(name.slice(5, name.length), this.value);
				}
			});
		});
		var dataList = [];
		$(this).children('ol.data-list').children('li').each(function(){
			var data = {};
			$(this).children('div').each(function(){
				var key = $(this).attr('data-key');
				var content = $(this).html();
				data[key] = content;
			});
			dataList.push(data);
		});
		var origin = {
			min:1,
			max:dataList.length,
			value:1
		};
		Object.keys(dataList[0]).forEach(
			function(key){
				origin[key] = dataList[0][key];
			}
		);
	
		function Clamp(value, min, max){
			value = Math.max(min, value);
			value = Math.min(max, value);
			return value;
		}
		var vm = new Vue({
			el: '#home-place-level',
			data: origin,
			methods: {
				change: function(step) {
					this.value = Clamp(this.value + step, this.min, this.max);
				},
				getInput: function(event){
					var value = event.target.value;
					if (value=="") return;
					value = Number(value);
					if(isNaN(value)){
						event.target.value = this.value;
						return;
					}
					value = Clamp(value, this.min, this.max);
					value = Math.floor(value);
					event.target.value = this.value = value;
				}
			},
			watch: {
				value: function (newValue, oldValue) {
					if (newValue!=oldValue){
						var newData = dataList[newValue-1];
						var data = this;
						Object.keys(newData).forEach(
							function(key){
								data[key] = newData[key];
							}
						);
					}
				}
			},
		});
	});
});

/*在别处调用*/
window.$(function(){
	var list = window.LoadScriptList;
	if (typeof(list)!=="undefined" && list.constructor === Array){
		list.forEach(function(action){
			action(window.$);
		});
	}
});