const fs = require('node:fs/promises');
const path = require('node:path');
const http = require('node:http');
const crypto = require('node:crypto');
const {execFile} = require('node:child_process');
const {promisify} = require('node:util');
const exec = promisify(execFile);
const run = __dirname;
const delivery=process.argv.includes('--delivery');
const repo = path.resolve(run, '../../..');
const mainName = '沙河批量打开器.exe';
const hash = data => crypto.createHash('sha256').update(data).digest('hex').toUpperCase();
const fileHash = async p => hash(await fs.readFile(p));
const json = async p => JSON.parse((await fs.readFile(p,'utf8')).replace(/^\uFEFF/,''));
async function record(name,data) {
 await fs.writeFile(path.join(run,name+'.json'), JSON.stringify(data,null,2)+'\n');
 await fs.appendFile(path.join(repo,'PROJECT_STRUCTURE.md'),'\n- LOG-01 '+name+'：返回码 '+data.returnCode+'；正式发布包的隔离安装/启动和日志开关证据见 artifacts/runs/diagnostic_logging_release_20261023/'+name+'.json。只操作本轮 real-install 目录。\n');
}
(async()=>{
 const before=await json(path.join(run,'environment-before.json'));
 if(before.autoRun || before.processes.some(p=>['沙河批量打开器.exe','updater.exe'].includes(p.Name)))throw new Error('Unsafe smoke baseline');
 const inventory=await json(path.join(run,delivery?'delivery-package-result.json':'package-result.json'));
 const bytes=await fs.readFile(inventory.package);
 if(hash(bytes)!==inventory.sha256)throw new Error('Package hash mismatch');
 const requests=[];
 const server=http.createServer((req,res)=>{requests.push(req.url);res.writeHead(200,{'Content-Type':'application/zip','Content-Length':bytes.length});res.end(bytes);});
 await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
 const results=[];
 try {
  for (const enabled of (delivery?[true]:[true,false])) {
   const name=(enabled?'release-install-logging-on':'release-install-logging-off')+(delivery?'-delivery':'');
   const location=path.join(run,'real-install',name),app=path.join(location,'app'),helper=path.join(location,'helper');
   await fs.mkdir(location,{recursive:true});await fs.mkdir(app);await fs.mkdir(helper);
   const source=path.join(run,delivery?'publish-default-delivery':'publish-default-final');
   for(const file of await fs.readdir(source))if(file===mainName||file===mainName+'.config'||file.endsWith('.dll'))await fs.copyFile(path.join(source,file),path.join(helper,file));
   const configDir=path.join(app,'Configurations'),fake=path.join(app,'fake-sandboxie');
   await fs.mkdir(configDir);await fs.mkdir(fake);await fs.mkdir(path.join(app,'sandbox-root'));
   await fs.writeFile(path.join(fake,'Start.exe'),'');
   const config=path.join(configDir,'shahe_config.ini');
   await fs.writeFile(config,'[config]\n\n[settings]\nshow_tray=false\nauto_run=false\nhotkeys_enabled=false\nhotkeys={}\nmax_history_items=17\napp_history=[]\nlog_output_mode='+JSON.stringify(enabled?'detailed':'off')+'\nsandboxie_path='+JSON.stringify(fake)+'\nsandbox_root='+JSON.stringify(path.join(app,'sandbox-root'))+'\nspeech_interface_dll="nvda"\nspeech_assist_hints={"enabled":false}\n');
   const configHash=await fileHash(config);
   await fs.writeFile(path.join(app,mainName),'old fixture main');
   const request=path.join(location,'request.json');
   await fs.writeFile(request,JSON.stringify({url:'http://127.0.0.1:'+server.address().port+'/package.zip',package_kind:'full',current_version:'4.0.47',target_version:'4.0.48',size:bytes.length,sha256:hash(bytes),restart:true}));
   const args=['--install-update','--app-dir',app,'--exe',path.join(app,mainName),'--pid','0','--request-file',request];
   const detail={returnCode:0,scenario:name,command:[path.join(helper,mainName),...args],enabled};
   try {
    const child=execFile(path.join(helper,mainName),args,{windowsHide:true,timeout:30000,encoding:'utf8'},()=>{});
    detail.helperPid=child.pid;
    detail.helperExitCode=await new Promise((resolve,reject)=>{child.once('error',reject);child.once('exit',(code,signal)=>{detail.signal=signal;resolve(code);});});
    if(detail.helperExitCode!==0)throw new Error('Helper failed');
    const observed=await exec('powershell.exe',['-NoProfile','-File',path.join(run,'observe-restart.ps1'),'-Executable',path.join(app,mainName)],{windowsHide:true,timeout:20000,encoding:'utf8'});
    detail.restart=JSON.parse(observed.stdout);
    if(detail.restart.exitCode!==0)throw new Error('Restart/exit failed');
    detail.installedFilesMatch=true;
    for(const [file,expected]of Object.entries(inventory.inventory))if(await fileHash(path.join(app,file))!==expected)throw new Error('Installed file mismatch: '+file);
    detail.configurationUnchanged=(await fileHash(config))===configHash;
    if(!detail.configurationUnchanged)throw new Error('User configuration changed');
    const logs=path.join(app,'logs');
    if(enabled){
     const names=await fs.readdir(logs);
     if(!names.length||names.some(n=>!/^\d{4}-\d{2}-\d{2}\.log$/.test(n)))throw new Error('Unexpected log filename');
     const text=(await Promise.all(names.map(n=>fs.readFile(path.join(logs,n),'utf8')))).join('\n');
     detail.logFiles=names.map(n=>path.join(logs,n));
     detail.stages=['更新安装开始','下载更新包完成','校验更新包通过','解压更新包完成','备份安装目录完成','替换文件: ShaheLauncher.Core.dll','更新安装完成','清理更新临时文件完成','程序入口: 版本=4.0.48','主窗口进入消息循环','主程序退出'];
     if(delivery)detail.stages.push('窗口关闭请求:');
     for(const stage of detail.stages)if(!text.includes(stage))throw new Error('Missing log stage: '+stage);
     detail.logStagesPresent=true;
    }else{
     detail.noLogsDirectory=!(await fs.stat(logs).catch(()=>null));
     if(!detail.noLogsDirectory)throw new Error('Logging disabled but files created');
    }
    detail.noLegacyLogDirectory=!(await fs.stat(path.join(app,'log')).catch(()=>null));
    if(!detail.noLegacyLogDirectory)throw new Error('Legacy log directory created');
   }catch(error){detail.returnCode=1;detail.error=String(error);}
   await record(name,detail);results.push(detail);
   if(detail.returnCode)break;
  }
 }finally{await new Promise(resolve=>server.close(resolve));}
 const result={returnCode:results.length===(delivery?1:2)&&results.every(r=>r.returnCode===0)?0:1,passed:results.filter(r=>r.returnCode===0).length,failed:results.filter(r=>r.returnCode!==0).length,skipped:(delivery?1:2)-results.length,downloadRequests:requests,scenarios:results.map(r=>r.scenario)};
 await record(delivery?'release-install-delivery-result':'release-install-result',result);console.log(JSON.stringify(result,null,2));process.exitCode=result.returnCode;
})().catch(async error=>{await record('release-install-result',{returnCode:2,error:String(error)});console.error(error);process.exitCode=2;});
