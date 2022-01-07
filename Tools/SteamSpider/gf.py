# -*- coding:utf-8 -*-
import json
import csv
import re
import os
from functools import reduce

dir_name = r'D:\太吾绘卷\wiki\上传\更新日志'

alias = {
    "无瑕七绝剑": ["无暇七绝剑"],
    "百邪销骨香": ["百邪透骨香"],
    "三霄迷仙曲": ["三宵迷仙曲"],
    "蓬莱仙尺": ["九曲盘龙棍"],
    "降龙棍法": ["伏虎棍法"],
    "鬼头棒法": ["金睛玄虎杖法"],
    "金睛玄虎棒法": ["通天棍法"],
    "浑元铁棒法": ["浑元铁杖法"],
    "穿纵功": ["提纵术"],
    "透骨切剖法": ["腑脏切剖法"],
}


def get_names(name):
    if name in alias:
        li = alias[name][:]
        li.append(name)
        return li
    else:
        return [name]


with open('C:\\taiwu\\steamapps\\common\\The Scroll Of Taiwu\\Backup\\txt\\GongFa_Date.txt',
          mode='r', encoding='utf-8') as f:
    data = dict([(row['#'], {'name': row['0'], '*': []}) for row in csv.DictReader(f)])
    f.close()


def get_including(name):
    li = []
    for gf in data.values():
        li += filter(lambda s: (name in s) and (name != s), get_names(gf['name']))
    return li


page_names = []
info_list = []
all_file_name = filter(lambda name:re.match(r'更新日志_1_\d+\.json', name), os.listdir(dir_name))
all_file_name = list(all_file_name)
for file_name in all_file_name:
    info_list.append(json.loads(open(dir_name+'/'+file_name, mode='r', encoding='utf-8').read()))

info_list.sort(key=lambda info: info['order'], reverse=True)
for info in info_list:
    ID = info['id']
    title = info['title']
    print(title)
    parts = re.compile('<.*?>').sub('\n', info['body']).split('\n')
    for gf in data.values():
        matches = list()
        for part in parts:
            for name in get_names(gf['name']):
                if name in part:
                    if name in reduce(
                            lambda s, name: str.replace(s, name, ''),
                            get_including(name),
                            part
                    ).replace('功法', ''):
                        matches.append(part)
                        break
        if len(matches) > 0:
            gf['*'].append({'id': ID, 'title': title, 'list': matches})

for gf_id, gf in data.items():
    if len(gf['*']) > 0:
        page_names.append('Data:更新日志/功法/' + str(gf_id) + '.json')
        with open(dir_name+'/更新日志_1_功法_1_' + str(gf_id) + '.json', mode='w', encoding='utf-8') as f:
            f.write(json.dumps(gf, ensure_ascii=False))
            f.close()
with open('功法列表.txt', 'w') as f:
    f.writelines(map(lambda v: v + '\n', page_names))
    f.close()
