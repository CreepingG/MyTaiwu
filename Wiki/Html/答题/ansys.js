/* eslint-disable max-nested-callbacks */
const url = 'https://jsonbox.io/TaiwuWikiProblemDatabase_1?limit=1000';
const options = {
    method: 'GET',
    headers: {
        "Content-Type": "application/json",
    },
};
let get;
fetch(url, options).then(async response => response.json()).then(result=>{
    get = result;
    let aggr = [];
    let rate = [];
    get.forEach(function(o){
        const choice = o.choice;
        choice.forEach(function(_,i){
            if (i in aggr === false) {
                aggr[i] = [];
                rate[i] = [];
            }
            const v = Math.abs(Number(choice[i]));
            aggr[i][v] = (aggr[i][v] || 0) + 1;
            rate[i][0] = (rate[i][0] || 0) + 1;
            rate[i][1] = (rate[i][1] || 0) + (Number(choice[i]) > 0 ? 1 : 0);
        });
    });
    aggr.forEach(function(q, index){
        let log = '';
        q.forEach(function(v,i){
            if (v !== null){
                let tuple = window.ProblemList[index].options.find((t)=>Number(t.id) === Number(i));
                if (typeof tuple !== "undefined"){
                    log += `${tuple.text}:${v}; `;
                }
            }
        });
        console.log(window.ProblemList[index].question);
        console.log(log);
        console.log(`正确率 ${rate[index][1]} / ${rate[index][0]}`);
    });
});
