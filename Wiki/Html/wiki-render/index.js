const fs = require("fs");
const config = require('./config.json');
const template = fs.readFileSync('./bin/template.html').toString();
const content = [
    config.style.map(path => `<link rel="stylesheet" href="${path}">`),
    config.content.map(path => fs.readFileSync(path).toString()),
    config.script.map(path => `<script src="${path}"></script>`),
].flat().join('\n');
const args = {
    TITLE : config.title,
    CONTENT : content,
};
fs.writeFileSync('./index.html', template.replace(/{{([A-Z0-9]+)}}/g, (match, name, index) => args[name]));