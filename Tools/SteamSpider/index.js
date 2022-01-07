const $get = require('request-promise');
const {JSDOM} = require('jsdom');
const url = 'https://store.steampowered.com/news/?appids=838350';
const fs = require('fs');
const dirName = 'D:\\太吾绘卷\\wiki\\上传\\更新日志';

function Wait(ms){
    return new Promise(resolve=>setTimeout(resolve, ms));
}

function Write(fileName, content, dir){
    let parsedFileName = fileName.replace('/','_1_').replace(':','_2_').replace('*','_3_').replace('"','_4_');
    let path = dir ? dir + '/' + parsedFileName : parsedFileName;
    if (fs.existsSync(path)){
        console.log('文件已存在：' + path);
        return true;
    }
    else{
        fs.writeFile(path, content, err=>{
            if (err){
                console.warn(path + '：' + err);
            }
            else{
                console.log(path + '：' + 'success');
            }
            fs.appendFileSync('日志列表.txt', 'Data:' + fileName + '\n');
        });
    }
}

$get(url).then(s=>{
    console.log('start');
    let order = Date.now();
    if (!fs.existsSync(dirName)){
        fs.mkdirSync(dirName);
    }
    const {window} = new JSDOM(s);
    const document = window.document;
    function ParseElement(elem){
        for (let node of elem.querySelectorAll('.newsPostBlock')){
            let title = node.querySelector('.posttitle>a').childNodes[0].textContent; // 不知为啥无法获取任何元素的innerText
            let body = node.querySelector('.body').innerHTML;
            let id = node.parentElement.id.match(/\d+/)[0];
            let json = JSON.stringify({id,title,body,order:order--});
            let fileName = '更新日志/' + id + '.json';
            if (Write(fileName, json, dirName)){
                return;
            }
        }
        let next = elem.querySelector('#more_posts_url');
        next = next && next.getAttribute('href');
        return next;
    }
    
    async function Work(){
        let next = ParseElement(document);
        let container = document.createElement('div');
        while(next){
            console.log(next);
            container.innerHTML = await $get(next);
            await Wait(2000);
            next = ParseElement(container);
        }
        
        console.log('done');
    }
    Work();
});