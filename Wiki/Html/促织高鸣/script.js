/* eslint-disable no-console */
class Util{
	static randwithWeight(pairs){
		let sum = 0;
		pairs.forEach(pair => {
			sum += pair[1];
			pair[1] = sum;
		});
		let rand = Math.random() * sum;
		for (let pair of pairs){
			if (rand < pair[1]) return pair[0];
		}
		console.warn(rand, pairs);
		console.trace();
	}
	static randInt(from, to){
		return Math.floor(from + Math.random() * (to - from));
	}
	static clamp(value, min, max){
		return Math.max(min, Math.min(max, value));
	}
}
class AutoNumber{
	/** @param {number} value */
	constructor(value){
		this._value = value;
		this.effect = [];
	}
	static get curTime(){
		return Game.status.maxProgressTime - Game.status.progressTime;
	}
	to(toValue, duration, delay = 0){
		let fromTime = AutoNumber.curTime + delay;
		let endTime = fromTime + duration;
		this.effect.push({
			pipeline(v, cur){
				let progress = (cur - this.fromTime) / (this.endTime - this.fromTime);
				if (progress <= 0) return v;
				if (progress >= 1) return this.toValue;
				return v + (this.toValue - v) * progress;
			},
			dead(cur){
				return cur > this.endTime;
			},
			fromTime,
			endTime,
			toValue
		});
	}
	set value(v){
		this._value = v;
		this.effect = [];
	}
	/** 结算并移除已经停止变化的效果 */
	removeDead(){
		let cur = AutoNumber.curTime;
		while(this.effect.length > 0){
			let head = this.effect[0];
			if (head.dead(cur)){
				this._value = head.pipeline(this._value, cur);
				this.effect.shift();
			}
			else{
				break;
			}
		}
	}
	get value(){
		this.removeDead();
		return this.valueAtTime(0);
	}
	valueAtTime(timeOffset){
		let time = AutoNumber.curTime + timeOffset;
		let v = this._value;
		this.effect.forEach(e=>{
			v = e.pipeline(v, time);
		});
		return v;
	}
	/** 存在未停止的效果 */
	get active(){
		this.removeDead();
		return this.effect.length > 0;
	}
}
class Resource{
	static _imageMaps = {
		place : 'CricketPlaceImage-resources.assets-47.png',
		net : 'GetQuquIcon-resources.assets-344.png',
		wave : 'QuquCall-resources.assets-157.png',
	}
	static image = {}
	static _audioMaps = {
		call : 'CricketCall-resources.assets-1097.wav',
	}
	static audio = {}
	static _csvMaps = {
		cricket : 'Cricket_Date',
		place : 'CricketPlace_Date',
	}
	static csv = {}
	static loading
	static load(){
		if (this.loading) return this.loading;

		let li = [];

		function LoadImage(name, key){
			const img = new Image();
			img.src = 'resources/' + name;
			li.push(new Promise((resolve, reject)=>{
				img.onload = resolve;
				img.onerror = reject;
			}));
			Resource.image[key] = img;
		}
		Object.entries(this._imageMaps).forEach(([k,v])=>LoadImage(v, k));

		function LoadAudio(name, key){
			const filePath = './resources/' + name;
			const request = new XMLHttpRequest();
			request.open('GET', filePath, true);
			request.responseType = 'arraybuffer';
			request.onload = function() {
				Resource.audio[key] = request.response;
			};
			request.send();

			li.push(new Promise((resolve, reject)=>{
				request.onload = () => {
					Resource.audio[key] = request.response;
					resolve();
				};
				request.onerror = reject;
			}));
		}
		Object.entries(this._audioMaps).forEach(([k,v])=>LoadAudio(v, k));

		function LoadCsv(name, key){
			const filePath = './resources/' + name + '.txt';
			const request = new XMLHttpRequest();
			request.open('GET', filePath, true);
			request.send();
			li.push(new Promise((resolve, reject)=>{
				request.onload = ()=>{
					Resource.csv[key] = ParseCsv(request.responseText);
					resolve();
				};
				request.onerror = reject;
			}));
		}
		function ParseCsv(text){
			let data = {};
			let lines = text.split('\n');
			let keys = lines.shift().split(',');
			lines.forEach(line=>{
				if (line.length <= 1) return;
				let datum = {};
				let id;
				line.split(',').forEach((v,i)=>{
					if (i === 0) {
						id = Number(v);
					}
					else{
						datum[Number(keys[i])] = v;
					}
				});
				if (!isNaN(id)) data[id] = datum;
			});
			return data;
		}
		Object.entries(this._csvMaps).forEach(([k,v])=>LoadCsv(v, k));

		let p = Promise.all(li);
		this.loading = p;
		return p;
	}
}
class Size{
	static mainCanvas = {w:500, h:500}
	static backDisc = {r:0}
	static place = {s:300, d:300}
	static net = {s:100, d:0}
	static wave = {s:500, d:0}
	static plan(){
		this.backDisc.r = Math.min(this.mainCanvas.w, this.mainCanvas.h) * 0.45;
		this.place.d = this.backDisc.r * 2 / 5 * 0.8;
		this.net.d = this.place.d * 0.8;
		this.wave.d = this.place.d * 3;
	}
}
class CanvasObj{
	constructor(width, height, parent){
		height = height || width;
		this.elem = document.createElement('canvas');
		this.width = width;
		this.height = height;
		this.origin = {x: width / 2, y: height / 2};
		this.elem.width = width;
		this.elem.height = height;
		(parent || document.body).appendChild(this.elem);
		this.ctx = this.elem.getContext('2d');
	}
	drawDisc(color, x, y, r){
		this.ctx.fillStyle = color;
		this.ctx.beginPath();
		this.ctx.arc(x, y, r, Math.PI * 2, 0, true);
		this.ctx.closePath();
		this.ctx.fill();
	}
	drawCircle(color, x, y, r, width, rate){
		this.ctx.strokeStyle = color;
		this.ctx.lineWidth = width;
		this.ctx.beginPath();
		this.ctx.arc(x, y, r, Math.PI * -0.5, Math.PI * (-0.5 + 2 * rate), false);
		this.ctx.stroke();
	}
	fillBackground(color){
		this.ctx.fillStyle = color;
		this.ctx.fillRect(0, 0, this.width, this.height);
	}
}
class Cricket{
	/** 
	 * @param {CricketPlace} place
	 */
	constructor(place){
		this.place = place;
	}
	color = 0
	part = 0
	level = 0
	name = ''
	init(color, part){
		this.color = color;
		this.part = part;
		let data = Resource.csv['cricket'];
		let colorDatum = data[color];
		let data9 = Number(colorDatum[9]);
		let data10 = Number(colorDatum[10]);
		if (part === 0){
			this.level = Number(colorDatum[1]);
			this.name = colorDatum[0];
		}
		else{
			let partDatum = data[part];
			this.level = Math.floor((Number(colorDatum[1]) + Number(partDatum[1])) / 2);
			let colorName = colorDatum[0].split('|');
			if (colorDatum[2] >= partDatum[2]){
				this.name = colorName[0] + partDatum[0];
			}
			else{
				this.name = partDatum[0] + colorName[1];
			}
			data9 += Number(partDatum[9]);
			data10 += Number(partDatum[10]);
		}
		this.place.call.setData(data10 / 100, 1.25 - 0.65 * data9 / 100);
		console.log(this.color, this.part, this.level, this.name);
	}
}
class CricketCall{
	/** 
	 * @param {CricketPlace} place
	 */
	constructor(place){
		this.place = place;
		this.volume = new AutoNumber(0);
		this.opacity = new AutoNumber(1);
		this.scale = new AutoNumber(0);

		const audioCtx = new window.AudioContext();
		const source = audioCtx.createBufferSource();
		const gainNode = audioCtx.createGain();
		this.audio = {
			audioCtx,
			source,
			gainNode,
			promise : audioCtx.decodeAudioData(Resource.audio['call'].slice(0)/* 浅拷贝 */)
		};
	}
	setData(scale, pitch){
		this.scaleArg = scale;
		this.audio.promise.then(buffer=>{
			this.audio.source.buffer = buffer;
			this.audio.source.playbackRate.value = pitch;
			this.audio.source.connect(this.audio.gainNode);
			this.audio.gainNode.connect(this.audio.audioCtx.destination);
			this.audio.source.loop = false;
		});
	}
	/** @constant 80 */
	static highLevel = 80
	/** 全局音量设置 */
	static settingVolume = 0.5
	
	/** 最早鸣叫时间 */
	v4 = 0
	/** 剩余鸣叫次数 */
	v5 = 0
	/** 控制音量 */
	v6 = 0
	/** 限制音量稳定帧数 */
	v7 = 0
	/** 记录音量稳定帧数 */
	v8 = 0
	/** 控制高音量鸣叫间隔 */
	quick = 0
	/** 蛐蛐对波纹大小的影响 */
	scaleArg = 0

	start(duration, scale){
		if (this.scale.active || this.opacity.active) return;
		duration *= 1000;
		this.scale.value = 0;
		this.scale.to(scale * 0.3 + scale * 0.7 * this.scaleArg, duration);
		this.scale.to(0, 0, duration);
		this.opacity.value = 1;
		this.opacity.to(0, duration / 2, duration / 2);
		this.playSound(2.2, 0.6);
	}
	playSound(duration, fadeout){
		if (this.audio.audioCtx.currentTime > 0) this.audio.source.stop();
		this.audio.source.start(0, 0, duration + 0.5);
		setTimeout(()=>{
			this.audio.gainNode.gain.linearRampToValueAtTime(0, this.audio.audioCtx.currentTime + fadeout);
		}, (duration - fadeout) * 1000);
	}
	render(){
		let scale = this.scale.value;
		let opacity = this.opacity.value;
		if (scale <= 0 || opacity <= 0) return;
		let size = Size.wave.d * scale;
		Game.mainCanvas.ctx.globalAlpha = opacity;
		Game.mainCanvas.ctx.drawImage(Resource.image['wave'],
			this.place.origin.x - 0.5 * size, this.place.origin.y - 0.5 * size, size, size);
		Game.mainCanvas.ctx.globalAlpha = 1;
	}
}
class CricketPlace{
	constructor(col, row){
		this.col = col;
		this.row = row;
		this.origin = {
			x: Game.mainCanvas.origin.x + this.col * Size.place.d,
			y: Game.mainCanvas.origin.y + this.row * Size.place.d
		};
		this.shakeArgs = {
			startTime : 0,
			duration : 0,
			maxRotation : 0
		};
		this.hover = false;
		this.index = Util.randwithWeight(Object.entries(Resource.csv['place']).map(([k,v])=>[k,Number(v[11])]));
		this.cricket = new Cricket(this);
		this.call = new CricketCall(this);
		this.rotation = new AutoNumber(0);
	}
	shake(size, power){
		if (this.rotation.active) return false;
		let duration = size * 1000;
		let maxRotation = power / 100 * (1 + this.cricket.level / 10) * (Math.random() < 0.5 ? 1 : -1);
		this.rotation.to(maxRotation, duration / 2);
		this.rotation.to(0, duration / 2, duration / 2);
		return true;
	}
	render(){
		const imgRow = Math.floor(this.index / 5);
		const imgCol = this.index % 5;
		const rotation = this.rotation.value;
		if (rotation !== 0){
			Game.mainCanvas.ctx.save();
			Game.mainCanvas.ctx.translate(this.origin.x, this.origin.y);
			Game.mainCanvas.ctx.rotate(rotation);
			Game.mainCanvas.ctx.drawImage(Resource.image['place'],
				imgCol * Size.place.s, imgRow * Size.place.s, Size.place.s, Size.place.s,
				-0.5 * Size.place.d, -0.5 * Size.place.d, Size.place.d, Size.place.d);
			Game.mainCanvas.ctx.restore();
		}
		else{
			Game.mainCanvas.ctx.drawImage(Resource.image['place'],
				imgCol * Size.place.s, imgRow * Size.place.s, Size.place.s, Size.place.s,
				this.origin.x - 0.5 * Size.place.d, this.origin.y - 0.5 * Size.place.d, Size.place.d, Size.place.d);
		}
		if (this.hover){
			if (Game.status.stage === Game.stages.catch){
				Game.mainCanvas.ctx.drawImage(Resource.image['net'],
					Size.net.s * 2, 0,
					Size.net.s, Size.net.s,
					this.origin.x - 0.5 * Size.net.d, this.origin.y - 0.5 * Size.net.d,
					Size.net.d, Size.net.d);
			}
			else{
				Game.mainCanvas.ctx.drawImage(Resource.image['net'],
					Size.net.s, 0,
					Size.net.s, Size.net.s,
					this.origin.x - 0.5 * Size.net.d, this.origin.y - 0.5 * Size.net.d,
					Size.net.d, Size.net.d);
			}
		}
	}
	changeVolume(){
		let v6 = this.call.v6;
		let v5 = this.call.v5;
		let level = this.cricket.level;
		let step = 10 + level * 2;
		if (v6 >= CricketCall.highLevel){ // 大音量时
			if (Util.randInt(0,100) >= level){
				if (Util.randInt(0,100) >= (v5 <= 0 ? 40 : 30)){
					v6 -= step; // 大概率减
				}
				else{
					v6 += step; // 小概率增
				}
			}
			else{
				v6 = 0; // 极小概率清零
			}
		}
		else{
			if (Util.randInt(0,100) >= level){ // 小音量时
				if (Util.randInt(0,100) >= (v5 <= 0 ? 75 : 65)){
					v6 -= step; // 小概率减
				}
				else{
					v6 += step; // 大概率增
				}
			}
			else{
				v6 = CricketCall.highLevel; // 极小概率增至中音量
			}
		}
		return v6;
	}
	update(){
		let call = this.call;
		let v6 = call.v6;
		let level = this.cricket.level;
		if (v6 > 0){
			let limit = call.v7 + (v6 >= CricketCall.highLevel ? 40 : 0);
			call.v8 += 1;
			if (call.v8 >= limit){
				this.call.v8 = 0;
				v6 = this.changeVolume();
			}
			if (v6 > CricketCall.highLevel || v6 > call.v6){
				call.volume.value = Util.clamp(
					CricketCall.settingVolume * 0.4 + CricketCall.settingVolume * 0.6 * v6 / 100 - (9 - level) * 0.025,
					0, 1);
			}
			else if (call.volume.valueAtTime(0.3) >= 0){
				call.volume.to(0, 0.1, 0.2);
			}
			if (v6 > CricketCall.highLevel){ // 高音量期间，每2/3秒一次
				if (call.quick++ % 40 === 0){
					call.start(0.6, 0.6);
					this.shake(0.1, 20);
				}
			}
			else{
				if (v6 > call.v6){
					call.start(1.2, 0.3);
					this.shake(0.2, 10);
				}
				call.quick = 0;
			}
		}
		else if (Game.status.progressTime <= call.v4 && call.v5 > 0 && v6 <= 0){
			call.v5 -= 1;
			v6 = Util.randInt(level * 2, level * 4);
			call.v7 = Util.randInt(40, 60) + level * 5;
		}
		call.v6 = Util.clamp(v6, 0, 100);
	}
	static map = new Map()
	/** @type {CricketPlace} */
	static hoverPlace = null
	static init(){
		for (let col = -2; col <= 2; col++){
			let maxOffset = Math.abs(col) === 2 ? 1 : 2;
			for (let row = -maxOffset; row <= maxOffset; row++){
				CricketPlace.map.set(col * 100 + row, new CricketPlace(col, row));
			}
		}

		let list = CricketPlace.values();
		let cricketData = Resource.csv['cricket'];
		let placeData = Resource.csv['place'];
		let allCricket = Object.entries(cricketData);
		let allColor = [];
		allCricket.forEach(pair => {
			let color = Number(pair[1][3]);
			if (color === 0) return;
			if (!allColor[color]) allColor[color] = [];
			allColor[color].push([pair[0], Number(pair[1][6])]);
		});
		let allPart = allCricket.filter(pair => pair[1][4] === '1').map(pair => [pair[0], Number(pair[1][6])]);
		let theOne = Util.randInt(0, list.length);
		for (let index = 0; index < list.length; index++){
			let place = list[index];
			let placeDatum = placeData[place.index];
			let color = Util.randwithWeight(Array.from({length: index === theOne ? 6 : 7} /* 让天命窝不出呆物 */)
				.map((_, i)=>[i + 1, Number(placeDatum[i + 1])]));
			let colorId, partId;
			if (color === 7){ // 呆物
				colorId = 0;
				partId = 0;
			}
			else{
				if (index !== theOne){ // 普通蛐蛐
					colorId = Util.randwithWeight(allColor[color]);
					partId = Util.randwithWeight(allPart);
				}
				else if (Math.random() < 0.5){
					colorId = Util.randwithWeight(allCricket
						.filter(pair => pair[1][5] === '1') // 异品
						.map(pair => [pair[0], Number(pair[1][6])]));
					partId = 0;
				}
				else{
					colorId = Util.randwithWeight(allCricket
						.filter(pair => Number(pair[1][7]) === color) // 真色
						.map(pair => [pair[0], Number(pair[1][6])]));
					partId = 0;
				}
			}
			place.cricket.init(colorId, partId);
			place.call.v4 = Util.randInt(557, 1859) / 60 * 1000;
			place.call.v5 = Util.randInt(1, 4);
		}
		let cnt = Util.randInt(2, 7);
		for (let i = 0; i < cnt; i++){
			let index = Util.randInt(0, list.length);
			list[index].call.v4 = Util.randInt(1301, 1859) / 60 * 1000;
		}
		cnt = Util.randInt(6, 15);
		for (let i = 0; i < cnt; i++){
			let index = Util.randInt(0, list.length);
			if (index !== theOne) list[index].call.v5 = 0;
		}

	}
	static event(event, x, y){
		x /= Size.place.d;
		y /= Size.place.d;
		const col = Math.round(x);
		const row = Math.round(y);
		x -= col;
		y -= row;
		const onBorder = Math.abs(x) > 0.45 || Math.abs(y) > 0.4;
		const place = (!onBorder && CricketPlace.map.get(col * 100 + row)) || null;
		switch(event){
			case 'move':
				if (CricketPlace.hoverPlace === place) break;
				if (CricketPlace.hoverPlace) CricketPlace.hoverPlace.hover = false;
				if (place){
					place.hover = true;
				}
				CricketPlace.hoverPlace = place;
				break;
			case 'click':
				if (!place) return;
				console.log(place);
				Game.status.stage = Game.stages.catch;
				Game.render();
				break;
			default:
				console.warn(event);
		}
	}
	/**
	 * @returns {Array<CricketPlace>}
	 */
	static values(){
		return Array.from(CricketPlace.map.values());
	}
	static forEach(action){
		CricketPlace.map.forEach(action);
	}
	static random(){
		let itor = CricketPlace.map.values();
		let randIndex = Math.floor(Math.random() * CricketPlace.map.size);
		for(let i = 0; i < randIndex; i++){
			itor.next();
		}
		return itor.next().value;
	}
}
class Game{
	/**
	 * @type {CanvasObj}
	 */
	static mainCanvas
	static backDisc
	static text
	static stages = {
		'init' : -1,
		'progress' : 0,
		'timeout' : 1,
		'catch' : 2,
		'get' : 3,
		'nothing' : 4,
	}
	static status = {
		interval : 1000 / 60,
		maxStartTime : 2 * 1000,
		startTime : 0,
		maxProgressTime : 31 * 1000,
		progressTime : 0,
		stage : this.stages.init,
	}
	static async init(){
		await Resource.load();
		Size.plan();
		Game.mainCanvas = new CanvasObj(Size.mainCanvas.w, Size.mainCanvas.h);
		Game.backDisc = {
			r : Size.backDisc.r,
			x : Game.mainCanvas.origin.x,
			y : Game.mainCanvas.origin.y,
			render(){
				Game.mainCanvas.drawDisc('#000', this.x, this.y, this.r);
				Game.mainCanvas.drawCircle('#fff', this.x, this.y, this.r, 2, Math.max(0, Game.status.progressTime / Game.status.maxProgressTime));
			}
		};
		Game.text = document.createElement('div');
		Game.text.style.whiteSpace = 'pre';
		document.body.appendChild(Game.text);
		CricketPlace.init();
		Game.mainCanvas.elem.onclick = function(e){
			if (Game.status.stage !== Game.stages.progress) return;
			const x = e.offsetX - Game.mainCanvas.origin.x;
			const y = e.offsetY - Game.mainCanvas.origin.y;
			CricketPlace.event('click', x, y);
		};
		Game.mainCanvas.elem.onmousemove = function(e){
			if (Game.status.stage !== Game.stages.progress) return;
			const x = e.offsetX - Game.mainCanvas.origin.x;
			const y = e.offsetY - Game.mainCanvas.origin.y;
			CricketPlace.event('move', x, y);
		};

		Game.start();
	}
	static timer
	static start(){
		Game.status.progressTime = Game.status.maxProgressTime;
		Game.status.stage = Game.stages.progress;
		Game.render();
		Game.timer = setInterval(Game.update, Game.status.interval);
	}
	static update(){
		Game.status.progressTime -= Game.status.interval;
		if (Game.status.stage === Game.stages.progress){
			CricketPlace.values().forEach(p => p.update());
			Game.render();
			let place = CricketPlace.hoverPlace;
			if (place){
				let call = place.call;
				Game.text.innerText = JSON.stringify({
					name: place.cricket.name,
					rotate: place.rotation.value,
					scale: call.scale.value,
					opacity: call.opacity.value
				}, null, 4);
			}
		}
		if (Game.status.progressTime < 0) {
			Game.status.stage = Game.stages.timeout;
		}
		if (Game.status.stage !== Game.stages.progress){
			clearInterval(Game.timer);
		}
	}
	static render(){
		Game.mainCanvas.fillBackground('#123');
		Game.backDisc.render();
		let ps = CricketPlace.values();
		ps.forEach(p=>p.render());
		ps.forEach(p=>p.call.render());
	}
}
Game.init();
