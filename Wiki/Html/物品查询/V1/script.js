/* eslint-disable no-undef */
/* eslint-disable guard-for-in */
/* eslint-disable no-redeclare */
/* eslint-disable block-scoped-var */
/* eslint-disable no-console */
/* eslint-disable no-shadow */
/* eslint-disable no-undefined */
/* eslint-disable no-inner-declarations */
/* eslint-disable spaced-comment */
/* eslint-disable no-var */
window.addEventListener('load', function(){
    var $ = window.$;
    var CACHE_TIME = 3600;
    var LEVEL = [
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
    /*双滑块滑动条*/
    /*品级*/
    $('#level').jRange({
        from: 1,
        to: 9,
        step: 1,
        scale: [9,8,7,6,5,4,3,2,1],
        format: function(n){
            return LEVEL[n - 1];
        },
        width: Math.min(300,$(window).width() - 100),
        showLabels: true,
        isRange : true
    });
    /** @param {HTMLUListElement} ul */
    function get_selection(ul){
        return [...ul.children].filter(li=>li.classList.contains('selected'));
    }
    /** @param {Element} elem */
    function get_key(elem){
        return elem.id.replace('IQ_','');
    }
    const form = document.getElementById('form');
    /* 生成初始数据 */
    /** @type {{[k:string]: string[]}} */
    const data = {};
    form.querySelectorAll('.filter').forEach(filter=>{
        data[get_key(filter)] = get_selection(filter).map(e=>e.innerText);
    });
    console.log(data);
    /* 记录依赖关系 */
    /** @type {{[k:string]: [Element, string[]]}} */
    const listeners = {};
    Object.keys(data).forEach(key=>{
        listeners[key] = [];
    });
    form.querySelectorAll('tr[data-visible]').forEach(target=>{
        const attr = target.getAttribute('data-visible');
        const [k,v] = attr.split(':');
        const list = v.split('|');
        listeners[k].push([target, list]);
    });
    /** 
     * @param {string|Element} key 
     * @param {string[]} value 
     */
    function update(key, value){
        if (typeof key === 'object'){
            key = get_key(key);
            value = get_selection(key).map(e=>e.innerText);
        }
        data[key] = value;
        listeners[key].forEach(pair=>{
            let [elem, list] = pair;
            let visible = value.some(v=>list.includes(v));
            if (visible) show(elem);
            else hide(elem);
        });
    }
    /** @param {Element} elem */
    function hide(elem){
        if (!elem) return;
        if (elem.style.display === 'none') return;
        for (let filter of elem.getElementsByClassName('filter')){
            update(get_key(filter), []);
        }
    }
    /** @param {Element} elem */
    function show(elem){
        if (!elem) return;
        if (elem.style.display !== 'none') return;
        for (let filter of elem.getElementsByClassName('filter')){
            update(get_key(filter);
        }
    }
    /* 初始化显隐 */
    for (let key in data){
        update(key, data[key]);
    }
    /* 点击事件 */
    for (let elem of form.getElementsByClassName('.filter')){
        if (elem.classList.contains('single')){
            for (let li of elem.children){
                li.addEventListener('click', /**@this {Element}*/function(){
                    let selection = get_selection(this.parentElement);
                    selection.forEach(e=>e.classList.remove('selected'));
                    this.classList.add('selected');
                    update(this.parentElement);
                });
            }
        }
        else if (elem.classList.contains('multiple')){
            for (let li of elem.children){
                li.addEventListener('click', /**@this {Element}*/function(){
                    this.classList.toggle('selected');
                    update(this.parentElement);
                });
            }
        }
    }
    /* picker */
    for (let picker of form.getElementsByClassName('picker')){
        let cnt = {};
        let children = [...picker.children];
        let target = document.getElementById(picker.getAttribute('target'));
        let input = target.getElementsByTagName('input')[0];
        let reset = target.getElementsByTagName('button')[0];
        Object.setPrototypeOf(cnt, {show(){
            let pairs = [];
            for (let key in this){
                let val = this[key];
                pairs.push([key, val]);
            }
            input.value = pairs.map(p => p[0] + '×' + p[1]).join(' ');

            picker.setAttribute('data-pairs', pairs.filter(p => p[1] > 0).map(p => p[0] + p[1]).join(''));
        }})
        children.forEach(c=>{
            cnt[c.innerText] = 0;
            c.addEventListener('click', /**@this {Element}*/function(){
                let name = this.innerText;
                let value = cnt[name];
                if (value > 6) value = 0;
                cnt[name] = value;
                
                cnt.show();
            })
        });
        reset.addEventListener('click', function(){
            for(let key in cnt){
                cnt[key] = 0;
            }
            cnt.show();
        });
        
    }
    
    /* submit */
    document.getElementById('submit').addEventListener('click', submit);
    form.addEventListener('keydown', function(e){
        if (e.key === 13) submit();
    })
    $('#submit').click(function(){
        submit($);
    });
    $('#form').keydown(function(event){
        if(event.which === 13) submit($);
    });
    function submit(){
        /** @type {HTMLButtonElement} */
        const btn = document.getElementById('submit');
        if (btn.disabled) return;
        btn.disabled = true;
        btn.innerText = '查询中...';

        const args = {};
        for(let elem of form.getElementsByClassName('text-input')){
            const input = elem.getElementsByTagName('input')[0];
            const id = input.id;
            args[id] = input.value;
        }
        for(let picker of form.getElementsByClassName('picker')){
            const id = picker.getAttribute('target');
            args[id] = picker.getAttribute('data-pairs');
        }
        $('.jrange').each(function(){
            var input = $(this);
            var key = input.prop('id');
            if(key === undefined) console.log(input.html());
            var val = input.val();
            if (val && val !== '1,9') args[key] = val;
        });
        $('.filter').each(function(){
            var ul = $(this);
            var key = ul.prop('id');
            if(key === undefined) console.log(ul.html());
            var val = ul.data('selection').map(function(li){return li.text()}).join('、');
            if (val && val !== '全部') args[key] = val;
        });
        console.log(args);
        args = Object.entries(args).map(pair=>'|' + pair[0] + '=' + String(pair[1]).replace('|','{{!}}')).join('');
        console.log(args);
        let wikitext = '{{#invoke:Item/query|d' + args + '}}';
        $.get('/api.php',{
            action: "parse", // 解析
            format: "json", // 返回内容的格式
            disablelimitreport: true, // 不返回使用内存、时间信息
            prop: "text", // 返回解析后的文本
            contentmodel: "wikitext", // 内容模型
            smaxage: CACHE_TIME,
            maxage: CACHE_TIME,
            text: wikitext, // 待解析文本
        }).then(function(result){
            btn.prop('disabled', false);
            btn.html('查询');
            result = result.parse.text['*'];
            var container = $('#result');
            container.html(result);
            if (container.find('.error').length > 0){
                _hmt && _hmt.push(['_trackEvent', '错误','物品查询', args + '\n@' + container.text(), 100]);
            }
        }).catch(err=>{
            err = JSON.stringify(err);
            $('#result').text('查询出错：' + err);
            _hmt && _hmt.push(['_trackEvent', '错误','物品查询', args + '\n@' + err, 100]);
        });
        _hmt && _hmt.push(['_trackEvent', '查询','物品查询', args]);
    }
});