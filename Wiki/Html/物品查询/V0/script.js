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
    /*标签筛选器filter*/
    function hide(ul){
        if(ul === undefined) return;
        var visible = ul.is(':visible');
        if(!visible) return false;
        ul.hide();
        var targets = ul.data('targets');
        if(targets === undefined) return;
        targets.forEach(function(target){
            hide(target);
        });
    }
    function show(ul){
        if(ul === undefined) return;
        var visible = ul.is(':visible');
        if(visible) return true;
        ul.show();
        var selection = ul.data('selection');
        if(selection === undefined) return;
        selection.forEach(function(li){
            var target = li.data('target');
            show(target);
        });
    }
    /*记录附属ul*/
    var filters = $('.filter');
    filters.each(function(){
        $(this).data('targets', []);
    });
    filters.children('li').each(function(){
        var target = $(this).attr('target');
        if(target !== undefined){
            target = $('.' + target);
            $(this).data('target', target);
            var parent = $(this).parent();
            var list = parent.data('targets');
            list.push(target);
        }
    });
    /*隐藏未被选中的*/
    filters.children('li').not('selected').each(function(){
        var target = $(this).data('target');
        hide($(target));
    });
    /*显示被选中的*/
    filters.children('li.selected:visible').each(function(){
        var target = $(this).data('target');
        show($(target));
    });
    /*点击单选*/
    $('.filter.single').each(function(){
        var ul = $(this);
        var selection = [ul.children('.selected').first()]; //数组记录第一个被选中的li
        ul.data('selection', selection);
        ul.children('li').click(function(){
            var li = $(this);
            li.addClass('selected');
            selection[0].removeClass('selected');
            hide(selection[0].data('target'));
            show(li.data('target'));
            selection[0] = li;
        });
    });
    /*点击多选*/
    $('.filter.multiple').each(function(){
        var ul = $(this);
        var selection = [];
        ul.data('selection',selection);
        ul.children('li').click(function(){
            var li = $(this);
            if(li.hasClass('selected')){ //取消选择
                li.removeClass('selected');
                var index = selection.findIndex(function(item){
                    return item.text() === li.text();
                });
                selection.splice(index, 1); //从数组中移除
                console.log(selection.map(function(li){
                    return li.text();
                }));
                
                var target = li.data('target');
                hide(target);
            }
            else{ //加入选择
                li.addClass('selected');
                selection.push(li);
                console.log(selection.map(function(li){
                    return li.text();
                }));
                
                var target = li.data('target');
                show(target);
            }
        });
    });
    
    /*picker*/
    $('.picker').each(function(){
        var ul = $(this);
        var target = $(this).attr('target');
        target = $('#' + target);
        var input = target.children('input');
        var reset = target.children('button');
        var table = {}; //以text()为key，数量为value
        var children = ul.children('li');
        children.each(function(){
            var key = $(this).text();
            table[key] = 0;
        });
        ul.data('cnt', table);
        children.click(function(){
            var key = $(this).text();
            var newValue = table[key] + 1;
            if(newValue > 6) newValue = 0;
            table[key] = newValue;
            
            var str = '';
            for (key in table){
                var val = table[key];
                if(val > 0){
                    str += key + '×' + val + ' ';
                }
            }
            input.val(str);
        });
        reset.click(function(){
            for (var key in table){
                table[key] = 0;
            };
            input.val('');
        });
    });
    
    /*submit*/
    $('#submit').click(function(){
        submit($);
    });
    $('#form').keydown(function(event){
        if(event.which === 13) submit($);
    });
    function submit(){
        var btn = $('#submit');
        if(btn.prop('disabled')) return;
        btn.prop('disabled', true);
        btn.html('查询中...');
        var args = {};
        $('.text-input').each(function(){
            var input = $(this).children('input');
            var key = input.prop('id');
            if(key === undefined) console.log(input.html());
            var val = input.val();
            if (val) args[key] = val;
        });
        $('.picker').each(function(){ //放在前面，防止出现'}}'
            var ul = $(this);
            var key = ul.attr('target');
            if(key === undefined) console.log(ul.html());
            var val = ul.data('cnt');
            val = Object.entries(val).filter(pair=>pair[1] > 0).map(pair=>pair[0] + pair[1]).join('');
            if (val) args[key] = val;
        });
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