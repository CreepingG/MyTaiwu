if ('LoadScriptList' in window === false) window.LoadScriptList = [];
window.LoadScriptList.push(function(){
	/* 计算得分 */
	function GetScore(problems, choice){
		return choice.reduce((pre,cur) => (pre + (cur > 0 ? 1 : 0)), 0);
	}
	/* 清理模板字符串 */
	function T(s){
		return s.replace(/([>}])\s*\n\s*/g,"$1").replace(/\s*\n\s*([<{}])/g,"$1");
	}
	/* 用jQuery预处理输入 */
	const startTemplate = (function(){
		const page = $('#page-0');
		const btn = page.find('#start-btn');
		btn.attr('v-on:click', 'next');
		const result = page[0].outerHTML;
		page.remove();
		return result;
	})();
	$("#exam-body").each(function(){
		$(this).height(Math.max($(this).height(), window.innerHeight * 0.8));
		$(this).append(`<component
			v-bind:is="curPage"
			v-bind:src="src"
			v-on:next="onNext($event)"
			v-on:post="onPost($event)"
		></component>`);
	});
	class Problem{
		constructor(ul, ol, index, list) {
			const ulChildren = ul.children('li');
			this.question = ulChildren.first().html();
			this.answer = Number(ulChildren.eq(1).html());
			this.explain = ulChildren.eq(2).html();

			this.options = ol.children('li').map(function(i){
				return {
					text: $(this).html(),
					id: i + 1
				};
			}).get().sort(() => 0.5 - Math.random()); // 乱序

			if (isNaN(this.answer) || this.answer <= 0 || this.answer > this.options.length){
				// eslint-disable-next-line no-alert
				alert(`问题“${this.question.substr(0,20)}...”的答案“${this.answer}”输入有误，应为从1开始的序号`);
			}

			this.index = index;
			this.list = list;
		}
	}
	const problems = [];
	$('#exam-outer-body').find('#questions').children('ul').each(function(){
		const ul = $(this);
		const ol = ul.next();
		problems.push(new Problem(ul, ol, problems.length, problems));
	});
	/* 注册组件 */
	Vue.component('start', {
		methods: {
			next(){
				this.$emit('next');
			}
		},
		template:startTemplate,
	});
	Vue.component('problem', {
		props:['src'],
		data() {
			return {choice: 0};
		},
		methods: {
			onChoose(id){
				if (this.choice === 0){
					const bingo = this.src.answer === id;
					this.choice = bingo ? id : -id;
					this.$emit('post', this.choice);
					return bingo;
				}
				return null;
			},
			next(){
				this.$emit('next');
				this.choice = 0;
			}
		},
		template: T(`<div id="problem">
			<div class="process">进度{{src.index + 1}}/{{src.list.length}}</div>
			<div v-html="src.question"></div>
			<div class="option-list">
				<opt 
					v-for="option in src.options"
					v-bind:text="option.text"
					v-bind:key="option.id"
					v-bind:result="Math.abs(choice) === option.id ? choice > 0 : null"
					v-bind:onChoose="onChoose.bind(null, option.id)"
				></opt>
			</div>
			<template v-if="choice !== 0">
				<template v-if="choice < 0">
					<div>
						回答错误，正确答案为
						"<span v-html="src.options.find((v) => v.id === src.answer).text"></span>"
					</div>
					<div>
						解析：
						<span v-html="src.explain"></span>
					</div>
				</template>
				<div v-else>回答正确</div>
				<button class="btn btn-primary" v-on:click="next">
					{{src.list.length - 1 === src.index ? '提交' : '下一题'}}
				</button>
			</template>
		</div>`),
		components: {
			'opt': {
				props:['text', 'onChoose', 'result'],
				template: T(`<button 
					v-html="text"
					v-bind:class="{
						'btn': true,
						'btn-primary': result === null,
						'btn-success': result === true,
						'btn-danger': result === false,
						'btn-choosed': result !== null
					}"
					v-on:click="onChoose"
				></button>`)
			}
		},
	});
	Vue.component('score', {
		props:['src'],
		computed: {
			value(){
				return GetScore(this.src.problems, this.src.choice);
			}
		},
		template: T(`<div id="score">
			<div>你的得分为{{value}}</div>
			<div>答题情况：
				<div v-for="(problem,index) in src.problems">
					<div>{{index + 1}}. {{problem.question}}</div>
					<div v-if="src.choice[index] < 0">
						{{problem.options.find((v)=>(v.id === -src.choice[index])).text}}
						<no />
						<span style="display:inline-block;width:10px" />
						{{problem.options.find((v)=>(v.id === problem.answer)).text}}
						<yes />
					</div>
					<div v-else>
						{{problem.options.find((v)=>(v.id === problem.answer)).text}}
						<yes />
					</div>
				</div>
			</div>
		</div>`),
		components: {
			'yes': {
				template: '<i class="fa fa-circle-o" aria-hidden="true" style="color:lightgreen"></i>'
			},
			'no': {
				template: '<i class="fa fa-times" aria-hidden="true" style="color:red"></i>'
			}
		},
	});
	/* 生成vue元素 */
	// eslint-disable-next-line no-new
	new Vue({
		el: '#exam-body',
		data: {
			index: -1,
			choice:[],
		},
		computed: {
			curPage(){
				if (this.index === -1) return 'start';
				if (this.index >= problems.length) return 'score';
				return 'problem';
			},
			src(){
				if (this.index === -1) return null;
				if (this.index >= problems.length){
					return {
						choice:this.choice,
						problems:problems,
					};
				}
				return problems[this.index];
			}
		},
		methods: {
			onNext(){
				this.index = this.index + 1;
			},
			onPost(value){
				this.choice.push(value); // 记录选择的id及对错
				if (this.choice.length < problems.length) return;
				try {
					let box_url = 'https://jsonbox.io/box_1d9e89b589cdbe185439';
					if (false){
						box_url = 'https://jsonbox.io/TaiwuWikiProblemDatabase<%id%>';
						console.log(box_url);
					}
					const payload = JSON.stringify({
						choice: this.choice,
						score: GetScore(problems, this.choice),
					});
					if ('mw' in window){
						payload.pageName = window.mw.config.get('wgPageName');
						payload.userName = window.mw.config.get('wgUserName');
					}
					console.log(payload);
					const options = {
						method: 'POST',
						body: payload,
						headers: {
							"Content-Type": "application/json",
						},
					};
		
					// Fetch to make API call
					fetch(box_url, options)
						.then(async response => response.json())
						.then(console.log);
				} catch (error) {
					console.log(error);
				}
			}
		}
	});
});

/* 在别处调用 */
window.$(function(){
	if ('LoadScriptList' in window){
		const list = window.LoadScriptList;
		if (list.constructor === Array){
			list.forEach(function(action){
				action(window.$);
			});
		}
	}
});