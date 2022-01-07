import data from './dest/ItemIcon.json';

function makeImg(name, data){
    const {x,y,width,height} = data[name];
    const img = document.createElement('img');
    img.src = data._.image;
    img.style = `
    clip-path: polygon(${x}px ${y}px, ${x+width}px ${y}px, ${x+width}px ${y}px, ${x}px ${y+height}px);
    transform: translate(${-x}px, ${-y}px)
    `.trim().replaceAll('\n', ' ');
    return img;
}

console.log(makeImg('ItemIcon_1', data).outerHTML);