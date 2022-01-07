import gulp from 'gulp';
const { task, src, dest } = gulp;
import buffer from 'vinyl-buffer';
import csso from 'gulp-csso';
import merge from 'merge-stream';
import spritesmith from 'gulp.spritesmith'; // https://github.com/twolfson/gulp.spritesmith
import webp from 'gulp-webp'; // https://github.com/sindresorhus/gulp-webp

function spriteTask(json = false) {
  const spriteData = src('images/ItemIcon/*.png').pipe(spritesmith({
    imgName: 'ItemIcon.png',
    cssName: 'ItemIcon.' + (json ? 'json' : 'css'),
    imgPath: 'dest/ItemIcon.png',
    cssTemplate: !json ? 'template.css.handlebars' : data => {
      const dict = {};
      const {width,height,image} = data.spritesheet;
      dict['_'] = {width,height,image};
      const list = data.sprites;
      list.forEach(({name,x,y,height,width})=>{
        dict[name] = {x,y,height,width};
      });
      return JSON.stringify(dict);
    }
  }));

  const imgStream = spriteData.img
    .pipe(buffer())
    .pipe(webp())
    .pipe(dest('dest/'));

  const cssStream = spriteData.css;
  if (!json) cssStream = cssStream.pipe(csso());
  cssStream = cssStream.pipe(dest('dest/'));

  // Return a merged stream to handle both `end` events
  return merge(imgStream, cssStream);
}

task('sprite-css', ()=>spriteTask());
task('sprite-json', ()=>spriteTask(true));