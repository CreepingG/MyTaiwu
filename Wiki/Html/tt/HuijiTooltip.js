/* eslint-disable max-nested-callbacks */
/* eslint-disable no-inner-declarations */
/* eslint-disable no-var */
// 说明：
// 当添加新类型浮层时，需要对应在“★”注释之后的部分添加代码

// 现加载浮层代码
$(function (){
    var TOOLTIP_CACHE_BLOCK_ID = 'huiji-tt-cache-block';
    var TOOLTIP_CACHE_BLOCK = $();
    var Link_CLASS_NAME = 'huiji-tt';
    var CACHE_TIME = 3600; // 解析内容保存在服务器和本地的缓存时间，单位是秒，如果内容不经常改的话，可以设长一些减少服务器压力，增加读取速度
    var isSmall = $(window).width() < 768;
    
    var mode = {};
    if (!isSmall){
        mode.listen = function(link){
            link = $(link);
            link.find('a').removeAttr('title'); // 去掉链接上的title，避免鼠标指上去时有自带浮层显示
            link.mouseenter(function(ev){
                var div = getDiv(link);
                if (div.length === 0){
                    div = new_div(link);
                }
                link.data('event',ev); // 记录鼠标位置，用于在load_tooltip_lua后重新set_tt_position
                recordBoth(link, div);

                set_position(div);
                div.show();
            });
            link.mouseleave(function(){
                getDiv(link).hide();
            });
        };
        mode.init = function(range){
            // 添加缓存浮层的区域
            if (TOOLTIP_CACHE_BLOCK.length === 0){
                $("body").append('<div id="' + TOOLTIP_CACHE_BLOCK_ID + '"></div>');
                TOOLTIP_CACHE_BLOCK = $('#' + TOOLTIP_CACHE_BLOCK_ID);
            }
    
            // 为所有浮层添加事件
            $(range).find('.' + Link_CLASS_NAME).addBack('.' + Link_CLASS_NAME).each(function(){
                mode.listen($(this));
            });
        };
    }
    else {
        mode.listen = function(link) {
            link = $(link);
            link.find('a').removeAttr('title'); // 去掉链接上的title，避免鼠标指上去时有自带浮层显示
            link.click(function(){
                var div = getDiv(link);
                if (div.length === 0){
                    div = new_div(link);
                }
                recordBoth(link, div);

                div.show();// 显示对应的内容
                $('#tt-modal').modal('show');// 显示模态框
                $('#tt-modal').children('.modal-dialog').children('a').attr('href', link.find('a').last().attr('href'));
            });
        };

        mode.init = function(range){
            if (TOOLTIP_CACHE_BLOCK.length === 0){
                $("body").append('<div class="modal fade" id="tt-modal" tabindex="-1" role="dialog">'
                    + '<div class="modal-dialog">'
                        + '<div id="' + TOOLTIP_CACHE_BLOCK_ID + '">'
                        + '</div>'
                        + '<a class="btn btn-primary">前往详情页</a>'
                    + '</div>'
                + '</div>');
                TOOLTIP_CACHE_BLOCK = $('#' + TOOLTIP_CACHE_BLOCK_ID);
                $('#tt-modal').click(function(){
                    $('#tt-modal').modal('hide'); // 任意位置点击后隐藏模态框
                    TOOLTIP_CACHE_BLOCK.children().hide();
                });
            }
            
            $(range).find('.' + Link_CLASS_NAME).addBack('.' + Link_CLASS_NAME).each(function(){
                mode.listen($(this));
                return false; // 阻止链接跳转
            });
        };
    }
    mode.init($('#mw-content-text'));

    // 监听dom变动，为新插入的tt注册事件
    new MutationObserver(function(mutationsList){
        mutationsList.forEach(function(mutation){
            var typ = mutation.type;
            if (typ === 'childList'){
                mutation.addedNodes.forEach(mode.init);
                mutation.removedNodes.forEach(function(node){
                    $(node).find('.' + Link_CLASS_NAME).addBack('.' + Link_CLASS_NAME).each(function(){
                        var div = getDiv(this);
                        if (div.length === 0) return;
                        var linkList = div.data('linkList');
                        linkList = linkList.filter(function(link){
                            return link[0] !== this;
                        });
                        if (linkList.length === 0){ // 不再有调用该div的link了，将其移除
                            div.remove();
                        }
                        else {
                            div.data('linkList', linkList);
                        }
                    });
                });
            }
        });
    }).observe($('#mw-content-text')[0], {
        childList: true,
        subtree: true,
    });

    function recordBoth(link, div){
        link.data('div', div);
        div.data('link', link);
        var linkList = div.data('link') || [];
        if (!linkList.find(link)) linkList.push(link);
        div.data('linkList', linkList);
    }
    
    function getDiv(tt_type, tt_name){
        if (arguments.length === 1){
            var node = tt_type;
            tt_type = get_data(node, 'type');
            tt_name = get_data(node, 'name').replace(' ', '_');
        }
        return TOOLTIP_CACHE_BLOCK.find('#tt-' + tt_type + '-' + tt_name);
    }
    
    function get_data(obj, name){
        var text = $(obj).attr("data-" + name);
        if (typeof text === "undefined") text = "";
        return text;
    }

    // 根据浮层大小和当前元素的位置，决定浮层的位置（尽量让浮层显示在屏幕中）
    function set_position(div) {
        var window_h = $(window).height();
        var window_w = $(window).width();
        var link = div.data('link');
        var top = link.offset().top - $(document).scrollTop();
        var left = link.offset().left; // 元素左侧绝对位置，若发生换行则为第二行的开始位置
        var width = link.outerWidth();
        var parent = link.parent();
        var relativeLeft = link[0].offsetLeft; // 元素第一行左侧相对于容器的位置
        var ttWidth = div.outerWidth();
        var ttHeight = div.outerHeight();
        
        div.css("left","unset");
        div.css("right","unset");
        if (relativeLeft + width > parent.innerWidth()){ // 元素左侧相对偏移+元素外侧宽度>容器内侧宽度，发生换行
            var mouseEvent = link.data('event');
            if (mouseEvent.clientX > window_w / 2){ // 鼠标在左侧
                div.css("right", window_w - (left + relativeLeft) + 10); // 元素第一行的左侧位置
            }
			else {
                div.css("left", mouseEvent.clientX + 10); // 鼠标滑入位置
            }
		}
		else if (left > window_w / 2){
            div.css("right", window_w - left + 10);
        }
        else {
            div.css("left", left + width + 10);
        }

        if (ttWidth > window_h){
            top = 10;
        }
        else if (top + ttHeight > window_h){
            top = window_h - ttHeight - 10;
        }

        div.css("top", top);
        
    }

    // 生成浮层
    function new_div(link){
        var tt_type = get_data(link, 'type');
        var tt_name = get_data(link, 'name').replace(' ', '_');
        var div = $('<div></div>');
        div.attr('id', 'tt-' + tt_type + '-' + tt_name).addClass('huiji-tt-cache');
        // 可以为每种type的浮层，写不同的占位文本
        // 如果浮层内容简单，也可以直接将其内容使用js生成
        switch (tt_type){
            // 可以写多种类型
            // ★添加类型时，需要在此处添加新的case来为不同类型指定未加载前的占位代码
            // case 'test':
            //     ...
            //     break;
            default:
                $('<div></div>').append('Loading...').appendTo(div);
                break;
        }
        div.appendTo(TOOLTIP_CACHE_BLOCK);

        // 异步读取模板解析结果
        // 如果提示框内容比较简单，则可以在上面用js直接生成，不需要在此处配置
        switch (tt_type){
            case 'test':
                load_wikitext('{{#if:1|' + tt_type + '|' + tt_name + '}}', div);
                break;
            case 'GongFa':
            case 'Item':
            case 'Home':
                load_wikitext('{{#invoke:' + tt_type + '|tooltip|' + tt_name + '}}', div);
                break;
            default:
                break;
        }

        return div;
    }
    
    // 根据wikitext生成浮层内容
    function load_wikitext(wikitext, div) {
        // eslint-disable-next-line no-undef
        var url = encodeURI('https://cdn.huijiwiki.com/' + mw.config.get('wgHuijiPrefix') + '/api.php?format=json&action=parse&disablelimitreport=true&prop=text&title=首页&smaxage=' + CACHE_TIME + '&maxage=' + CACHE_TIME + '&text=')
            + encodeURIComponent(wikitext);
        $.get(url, function(result){
            div.html(result.parse.text['*']);
            if (!isSmall) set_position(div); // 内容改变后，尺寸变化，需重新调整位置
        });
    }
});
