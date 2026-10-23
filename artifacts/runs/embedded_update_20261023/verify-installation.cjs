const fs = require('node:fs/promises');
const path = require('node:path');
const http = require('node:http');
const crypto = require('node:crypto');
const {execFile} = require('node:child_process');
const {promisify} = require('node:util');
const exec = promisify(execFile);
const run = __dirname;
const repo = path.resolve(run, '../../..');
const mainName = '沙河批量打开器.exe';
const old = path.join(repo,'artifacts/runs/csharp_gitcode_release_20261020/public-package-4.0.46-gitcode-final');
const hash = data => crypto.createHash('sha256').update(data).digest('hex').toUpperCase();
const fileHash = async p => hash(await fs.readFile(p));
const record = async (name,data) => {
  await fs.writeFile(path.join(run,name+'.json'), JSON.stringify(data,null,2)+'\n');
  await fs.appendFile(path.join(repo,'PROJECT_STRUCTURE.md'), '\n- UPDATE-EMBED-01 '+name+'：返回码 '+data.returnCode+'；真实 helper/安装结果见 artifacts/runs/embedded_update_20261023/'+name+'.json。只操作本轮 real-install 目录，未改用户旧版目录或公开源。\n');
};
(async () => {
  const finalOnly=process.argv.includes('--final');
  const inventory = JSON.parse(await fs.readFile(path.join(run,finalOnly?'install-payloads-final.json':'install-payloads.json'),'utf8'));
  const bytes = {};
  for (const k of ['default','legacy']) bytes[k] = await fs.readFile(inventory[k].zip);
  const originalOldHashes = await Promise.all(['updater.exe','ShaheLauncher.Core.dll'].map(n=>fileHash(path.join(old,n))));
  const requests=[];
  const server = http.createServer((req,res)=>{const k=req.url==='/legacy.zip'?'legacy':req.url==='/default.zip'?'default':null;if(!k){res.writeHead(404);return res.end();}requests.push(req.url);res.writeHead(200,{'Content-Type':'application/zip','Content-Length':bytes[k].length});res.end(bytes[k]);});
  await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
  const results=[];
  const remaining=process.argv.includes("--remaining");
  try {
    for (const scenario of [{name:'embedded-self',kind:'default',source:'publish-default',helper:mainName,mode:true,restart:true},{name:'legacy-protocol-alias',kind:'legacy',source:'publish-legacy',helper:'updater.exe',mode:false,restart:false},{name:'published46-to-embedded',kind:'legacy',source:old,helper:'updater.exe',mode:false,restart:false}]) {
      if(remaining&&scenario.name==='embedded-self')continue;
      if(finalOnly&&scenario.name!=='embedded-self')continue;
      const location=path.join(run,'real-install',scenario.name+(finalOnly?'-final':'')), app=path.join(location,'app'), helper=path.join(location,'helper');
      await fs.mkdir(location,{recursive:true});
      await fs.mkdir(app);await fs.mkdir(helper);
      const source=path.isAbsolute(scenario.source)?scenario.source:path.join(run,scenario.source+(finalOnly?'-final':''));
      for(const file of await fs.readdir(source)) if(file===scenario.helper||file===scenario.helper+'.config'||file.endsWith('.dll'))await fs.copyFile(path.join(source,file),path.join(helper,file));
      const configDir=path.join(app,'Configurations'),fake=path.join(app,'fake-sandboxie');
      await fs.mkdir(configDir);await fs.mkdir(fake);await fs.mkdir(path.join(app,'sandbox-root'));
      await fs.writeFile(path.join(fake,'Start.exe'),'');
      const config=path.join(configDir,'shahe_config.ini');
      await fs.writeFile(config,'[config]\n\n[settings]\nshow_tray=false\nauto_run=false\nhotkeys_enabled=false\nhotkeys={}\nmax_history_items=17\napp_history=[]\nsandboxie_path='+JSON.stringify(fake)+'\nsandbox_root='+JSON.stringify(path.join(app,'sandbox-root'))+'\nspeech_interface_dll="nvda"\nspeech_assist_hints={"enabled":false}\n');
      const before=await fileHash(config);
      await fs.writeFile(path.join(app,mainName),'old fixture main');
      const request=path.join(location,'request.json');
      await fs.writeFile(request,JSON.stringify({url:'http://127.0.0.1:'+server.address().port+'/'+scenario.kind+'.zip',package_kind:'full',current_version:'4.0.46',target_version:'4.0.47',size:bytes[scenario.kind].length,sha256:hash(bytes[scenario.kind]),restart:scenario.restart}));
      const args=[...(scenario.mode?['--install-update']:[]),'--app-dir',app,'--exe',path.join(app,mainName),'--pid','0','--request-file',request];
      let detail={scenario:scenario.name,command:[path.join(helper,scenario.helper),...args],returnCode:0};
      try {
        const child=execFile(path.join(helper,scenario.helper),args,{windowsHide:true,timeout:25000,encoding:'utf8'},()=>{});
        detail.helperPid=child.pid;
        const code=await new Promise((resolve,reject)=>{child.once('error',reject);child.once('exit',(c,signal)=>{detail.signal=signal;resolve(c);});});
        detail.helperExitCode=code;if(code!==0)throw new Error('helper exit: '+code);
        detail.mainHashMatches=await fileHash(path.join(app,mainName))===inventory[scenario.kind].mainSha256;
        detail.newDllHashMatches=await fileHash(path.join(app,'lib/new-runtime.dll'))===inventory[scenario.kind].newDllSha256;
        detail.configUnchanged=await fileHash(config)===before;
        if(!detail.mainHashMatches||!detail.newDllHashMatches||!detail.configUnchanged)throw new Error('Installed payload or configuration mismatch');
        if(scenario.restart){const observed=await exec('powershell.exe',['-NoProfile','-File',path.join(run,'observe-restart.ps1'),'-Executable',path.join(app,mainName)],{windowsHide:true,timeout:20000,encoding:'utf8'});detail.restart=JSON.parse(observed.stdout);if(detail.restart.exitCode!==0)throw new Error('Restarted main did not exit normally');detail.configUnchangedAfterRestart=await fileHash(config)===before;if(!detail.configUnchangedAfterRestart)throw new Error('Restart changed user settings');}
      } catch(error){detail.returnCode=1;detail.error=String(error);}
      await record(scenario.name+(finalOnly?'-final':''),detail);results.push(detail);
      if(detail.returnCode)break;
    }
  } finally {await new Promise(resolve=>server.close(resolve));}
  const oldUnchanged=JSON.stringify(originalOldHashes)===JSON.stringify(await Promise.all(['updater.exe','ShaheLauncher.Core.dll'].map(n=>fileHash(path.join(old,n)))));
  const result={returnCode:results.length===(finalOnly?1:remaining?2:3)&&results.every(r=>r.returnCode===0)&&oldUnchanged?0:1,passed:results.filter(r=>r.returnCode===0).length,failed:results.filter(r=>r.returnCode!==0).length,skipped:(finalOnly?1:remaining?2:3)-results.length,oldDirectoryUntouched:oldUnchanged,downloadRequests:requests,scenarios:results.map(r=>r.scenario)};
  await record(finalOnly?'installation-update-only-result':remaining?'installation-remaining-result':'installation-proof-result',result);console.log(JSON.stringify(result,null,2));process.exitCode=result.returnCode;
})().catch(async error=>{await record('installation-proof-result',{returnCode:2,error:String(error)});console.error(error);process.exitCode=2;});
