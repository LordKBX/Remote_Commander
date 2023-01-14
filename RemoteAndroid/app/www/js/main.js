function GGUID()
{
	var currentDateMilliseconds = new Date().getTime();
	var currentDateSeconds = parseInt(currentDateMilliseconds / 1000, 10);
	var currentDateMilli = currentDateMilliseconds - (currentDateSeconds * 1000);
	var str = "_" + currentDateSeconds + '-' + currentDateMilli + '-pxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (currentChar)
		{
			var randomChar = (currentDateMilliseconds + Math.random() * 16) % 16 | 0;
			currentDateMilliseconds = Math.floor(currentDateMilliseconds / 16);
			return (currentChar === 'x' ? randomChar : (randomChar & 0x7 | 0x8)).toString(16);
		}
		);
	return str.substring(1);
}
function DUID() { return device.uuid; }
function clone(src)
{
	let target = {};
	for (let prop in src) { if (src.hasOwnProperty(prop)) { target[prop] = src[prop]; } }
	return target;
}

// CSS TOOLS
function addNewStyle(newStyle,dest) {
	if(dest === undefined){dest = 'styles_js';}
	if(!document.getElementById(dest)){sc = document.createElement('style');sc.setAttribute('id',dest);document.getElementsByTagName('head')[0].appendChild(sc);}
    var styleElement = document.getElementById(dest);
    styleElement.appendChild(document.createTextNode(newStyle));
}
function rmStyle(StyleName,dest) {
	if(dest === undefined){dest = 'styles_js';}
    var styleElement = document.getElementById(dest);
    if (!styleElement) {return;}
	list = styleElement.childNodes;
	idrm = -1;
	for(var i=0; i< list.length; i++){
		if(list[i].data.indexOf(StyleName+'{') != -1 || list[i].data.indexOf(StyleName+' {') != -1){idrm = i;break;}
		}
	console.log('idrm = '+idrm);
	if(idrm == -1){return false;}
	else{
		styleElement.removeChild(styleElement.childNodes[idrm]);
		return true;
	}
}
function rmAllStyle(dest) {
	if(dest === undefined){dest = 'styles_js';}
    var styleElement = document.getElementById(dest);
    if (!styleElement) {return;}
	list = styleElement.childNodes;
	idrm = -1;
	for(var i=list.length-1; i>0; i--){
		styleElement.removeChild(styleElement.childNodes[i]);
		}
}

app =
{
	Params: {},
	Serverkey: null,
	servPort: 25000,
	servAddr: "192.168.1.20",
	servPass: "",
	servPassSaved: false,
	reconnectTry: 0,
	manualyClosed: false,
	waitRegister: false,
	waitLog: false,
	lastPing: 0,
	timerPing: null,
	timerCheckPing: null,
	storedGrids: null,
	storedMods: [],
	sockets: [],
	socketIndex: 0,
	storedActiveGrid: 1,
	listImages: [],
	listImagesObjects: [],
	listImagesToGet: [],
	arrayBlobs: {},
	arrayBlobsURL: {},
	modsVars: {},
	PasswordDialog: null,
	purgeFileName(path){ return path.replace(new RegExp('\\/', 'g'), '__').replace(new RegExp('\\\\', 'g'), '__').replace(new RegExp(' ', 'g'), '-');},
	isCachedImg(ref){
		for(var i=0; i<app.listImages.length; i++){ if(app.listImages[i].name == ref){ return true; } }
		return false;
		},
	getCachedImg(ref){
		for(var i=0; i<app.listImages.length; i++){ if(app.listImages[i].name == ref){ return app.listImages[i]; } }
		return null;
		},
	init: function ()
	{
		app.timerCheckPing = setInterval(app.loopDetectDisconnected, 1000);
		app.timerPing = setInterval(app.ping, 3000);
		screen.orientation.lock('portrait');
		var options = { name: 'config.db', location: 'default' };
		if (device.platform == "Android") { 
			options.androidDatabaseProvider = 'system'; 
			document.querySelector('.app').style.paddingTop = '20px';
			}
		if (device.platform == "iOS") { options.location = 'Library'; }
		app.db = window.sqlitePlugin.openDatabase(
			options,
			function (e)
			{
				console.log('Load DB Success');
				app.db.transaction(function (tx)
				{
					tx.executeSql("PRAGMA table_info('PARAMS')", [], function (tx, res)
					{
						if (res.rows.length === 0)
						{
							console.log('CREA TABLE PARAMS');
							tx.executeSql("CREATE TABLE 'PARAMS' ('NAME' TEXT PRIMARY KEY NOT NULL, 'VALUE' TEXT)");
							tx.executeSql("INSERT INTO 'PARAMS'('NAME','VALUE') VALUES('lang','fr')");
							tx.executeSql("INSERT INTO 'PARAMS'('NAME','VALUE') VALUES('multi_screen','0')");
							tx.executeSql("INSERT INTO 'PARAMS'('NAME','VALUE') VALUES('style','white')");
							tx.executeSql("INSERT INTO 'PARAMS'('NAME','VALUE') VALUES('last_purge','0')");
						}
						tx.executeSql("PRAGMA table_info('SERVERSv2')", [], function (tx, res)
						{
							if (res.rows.length === 0)
							{
								console.log('CREA TABLE SERVERSv2');
								tx.executeSql("CREATE TABLE 'SERVERSv2' (" +
									"'NAME' TEXT PRIMARY KEY NOT NULL, " +
									"'IP' TEXT NOT NULL, " +
									"'PORT' TEXT NOT NULL, " +
									"'PASS' TEXT NOT NULL, " +
									"'SAVEDPASS' INTEGER DEFAULT 0, " +
									"'REC_TIME' TEXT NOT NULL, " +
									"'LAST_USE' TEXT NOT NULL" +
									")");
								//tx.executeSql("INSERT INTO 'SERVERSv2'('NAME','IP','PORT','REC_TIME','LAST_USE', 'PASS', 'SAVEDPASS') VALUES('TEST EKATON','192.168.1.174','25000','0','0','',0)");
								//tx.executeSql("INSERT INTO 'SERVERSv2'('NAME','IP','PORT','REC_TIME','LAST_USE', 'PASS', 'SAVEDPASS') VALUES('TEST HOME','192.168.1.64','25000','0','0','',0)");
							}
							tx.executeSql("PRAGMA table_info('CACHE')", [], function (tx, res)
							{
								if (res.rows.length === 0)
								{
									console.log('CREA TABLE CACHE');
									tx.executeSql("CREATE TABLE 'CACHE' (" +
										"'NAME' TEXT PRIMARY KEY NOT NULL, " +
										"'VALUE' TEXT NOT NULL, " +
										"'LAST_UPDATE' REAL NOT NULL" +
										")");
								}
								//tx.executeSql("DELETE FROM 'CACHE';");
								//cleanDir(cordova.file.cacheDirectory, function(ret){});
							}, 
							function (e) { console.log("ERROR TEST DB 4:", e); }
							);
							tx.executeSql("SELECT 'NAME','VALUE' FROM PARAMS", [], function (tx, res)
								{
									if (res.rows.length !== 0)
									{
										for (i = 0; i < res.rows.length; i++) { app.Params[res.rows.item(i).NAME] = res.rows.item(i).VALUE; }
									}
									setTimeout(app.init_P2, 200);
								}
							);
						}, 
						function (e) { console.log("ERROR TEST DB 3:", e); }
						);
					}, 
					function (e) { console.log("ERROR TEST DB 2:", e); }
					);
				}, 
				function (e) { console.error("ERROR TEST DB 1:", e); }
				);
			},
			function (e) { console.error('Load DB Error', e); }
			);
	},
	init_P2: function ()
	{
		console.log("init_P2");
		
		listDir(cordova.file.cacheDirectory, function(tab){
			if(tab == false){ return; }
			window.resolveLocalFileSystemURL(cordova.file.cacheDirectory, function success(fileEntry) {
				console.log('cdvfile URI: ' + fileEntry.toInternalURL());
				console.log('tab: ', tab);
				dir = fileEntry.toInternalURL();
				var ins = Date.now();
				for(var i=0; i<tab.length; i++){
					if(tab[i].search('.jpg') != -1 || tab[i].search('.png') != -1){
						app.listImages.push({
							name: tab[i],
							data: fileEntry.toInternalURL()+tab[i],
							last_update: ins
						});
						app.listImagesObjects.push(new Image());
						app.listImagesObjects[app.listImagesObjects.length - 1].onload = function(){ 
							console.log("img loaded");
							try{
								//$('.control-grid-button[image="' + app.purgeFileName(tab[i]) + '"]')[0].style.backgroundImage="url('" + this.src + "')";
								}
							catch(err){ console.error(err); console.log(this.src); }
							
							}
						app.listImagesObjects[app.listImagesObjects.length - 1].src = fileEntry.toInternalURL()+tab[i];
						}
					}
			});
		});
		
		try{ Crytography.Init(); } catch(err){ console.error(err); }
		app.Serverkey = null;
		//app.timerImages = setInterval(app.getImages, 100);
		$('#side_menu').bind('click', function(e){
			$('#side_menu').css('left','-250px');
		});
		app.PasswordDialog = $( "#dialog-form" ).dialog({
			autoOpen: false,
			height: 300,
			width: 350,
			modal: true,
			buttons: {
				Ok: function() {
					app.servPass = $("#password").val().replace(new RegExp(" ", "g"), "");
					app.servPassSaved = $("#saved")[0].checked;
					if (app.servPass === "") { return; }

					obl =
					{
						"function": "Login",
						"password": app.servPass
					};
					app.SendToServerEncoded(JSON.stringify(obl));
				}
			},
			close: function() {
				$("#password").val('');
				$("#saved")[0].checked = false;
				}
		});

		//$('#PannelHomeIP').val(app.servAddr);
		//$('#PannelHomePORT').val(app.servPort);
		app.listServers();
		$("#toolbar_icon").bind('click', 
			function (e)
			{
			$('#side_menu').css('left','0px');
				
			//location.reload();
			//app.SendToServerEncoded(app.servAddr, app.servPort, "{\"function\":\"ForceReload\"}");
			//app.init_P2();
			}
		);
		app.gotoHome();

	},
	listServers: function(){
		$('#PannelHomeList').html('');
		app.db.transaction(
			function (tx)
			{
				tx.executeSql("SELECT NAME,IP,PORT,PASS,SAVEDPASS FROM SERVERSv2", [], function (tx, res)
				{
					if (res.rows.length !== 0)
					{
						for (i = 0; i < res.rows.length; i++)
						{
							doc = document.createElement('div');
							doc.setAttribute('class', 'QABlock');
							st = 'Name: ' + res.rows.item(i).NAME + '<br>';
							st = st + 'Address: ' + res.rows.item(i).IP + '<br>';
							st = st + 'Port: ' + res.rows.item(i).PORT;
							p = document.createElement('p');
							p.innerHTML = st;
							btn = document.createElement('button');
							btn.setAttribute('onclick', "app.TestServer('" + res.rows.item(i).IP + "'," + res.rows.item(i).PORT + ", '"+res.rows.item(i).PASS+"', "+((parseInt(res.rows.item(i).SAVEDPASS) == 1)?'true':'false')+");");
							btn.innerHTML = "Connect";
							doc.appendChild(p);
							doc.appendChild(btn);
							$('#PannelHomeList').append(doc);
						}
						console.log(res.rows);
					}
				}
				);
			}
		);
	},
	sockectOnMessage: function (event){
		try
		{
			if(event.data == ''){return;}
			//console.log(event);
			data = event.data;
			to = event.origin.split('/')[2].split(':');
			//console.log(to);
			host = to[0];
			port = to[1];
			tdata = JSON.parse(data);
			//console.log(tdata);
			if (tdata["type"] !== undefined)
			{
				if (tdata["type"] == "encoded")
				{
					decrypted = Crytography.Decrypt(tdata["data"], Crytography.GetPublicKeyString());
					//console.log(decrypted);
					try
					{
						tdata = JSON.parse(decrypted);
					}
					catch (error)
					{}
				}
				if (tdata["type"] == "error")
				{
					console.error(tdata);
					//if(tdata["cause"] == "not_loged"){ alert("Invalid session"); }
				}
			}
			if (tdata["function"] !== undefined && tdata["function"] !== null)
			{
				tdata["function"] = tdata["function"].trim();
				//console.log(tdata["function"]);
				if (tdata["function"] == "GetInfo")
				{
					console.log("TEST OK !");
					app.Serverkey = tdata["PublicKey"];
					if (app.waitLog == true)
					{
						if (app.waitRegister == true)
						{
							name = "";
							while (name === "")
							{
								name = prompt("Please enter the host name", "My PC").replace(new RegExp(" ", "g"), "");
							}

							app.db.transaction(
							function (tx)
								{
									tx.executeSql("SELECT NAME,IP,PORT FROM SERVERSv2 WHERE NAME = ? OR IP = ?", [name, host], function (tx, res)
									{
										if (res.rows.length !== 0)
										{
											st = "";
											if (res.rows.item(i).NAME == name)
											{
												st = "Host Name already recorded";
											}
											if (res.rows.item(i).IP == event.target.url)
											{
												st = "Host Address already recorded";
											}
											alert("Error: " + st);
										}
										else
										{
											tx.executeSql("INSERT INTO SERVERSv2(NAME,IP,PORT,REC_TIME,LAST_USE,PASS, SAVEDPASS) VALUES(?,?,?,?,?,?,?)", [name, host, port, 0, 0, (app.servPassSaved)?app.servPass:'', (app.servPassSaved)?1:0], function (tx, res)  { app.listServers(); });
										}
									}
									);
								}
							);
						}

						app.waitRegister = false;
						app.waitLog = false;
						app.servPort = port;
						app.servAddr = host;
						app.lastPing = Date.now();
						if(app.servPassSaved && app.servPass != ""){
							obl =
								{
									"function": "Login",
									"password": app.servPass
								};
							app.SendToServerEncoded(JSON.stringify(obl));
							}
						else{
							$('#password').val(app.servPass);
							app.PasswordDialog.dialog( "open" );
							}
						
					}
				}
				if (tdata["function"] == "Login")
				{
					if(tdata["status"] == "OK"){ 
						console.log("Session OK"); 
						app.gotoControls(); 
						app.db.transaction(function (tx){ tx.executeSql("UPDATE SERVERSv2 SET PASS=?, SAVEDPASS=? WHERE IP = ? AND PORT = ?", [(app.servPassSaved)?app.servPass:'',(app.servPassSaved)?1:0,app.servAddr, app.servPort], function (tx, res){}); });
						app.PasswordDialog.dialog( "close" );
						}
					else{ 
						if(app.servPassSaved){
							app.db.transaction(function (tx){ tx.executeSql("UPDATE SERVERSv2 SET PASS=?, SAVEDPASS=? WHERE IP = ? AND PORT = ?", ['',0,app.servAddr, app.servPort], function (tx, res){}); });
							}
						alert("invalid login"); 
						$('#password').val("");
						app.PasswordDialog.dialog( "open" );
						}
				}
				if (tdata["function"] == "Pong")
				{
					app.lastPing = Date.now();
					//app.SendToServerEncoded('{"function":"Pong"}');
				}
				if (tdata["function"] == "SendGrids")
				{
					app.orientation = tdata["orientation"];
					screen.orientation.lock((app.orientation == "horizontal")?'landscape':'portrait');
					
					app.storedGrids = tdata["grids"];
					
					app.storedMods = [];
					parser = new DOMParser();
					for(index in tdata["mods"]){
						app.storedMods.push({id: index, data: parser.parseFromString(tdata["mods"][index], "text/xml")});
						console.log(app.storedMods[app.storedMods.length - 1]);
						}
					
					app.storedActiveGrid = 1;
					setTimeout("app.drawGrids();", 100);
				}
				if (tdata["function"] == "RetGetImage")
				{
					if (tdata["result"] != "ERROR")
					{
						console.log("RetGetImage");
						tabi = tdata["result"].split(',');
						tt = tabi[0].split(';')[0].replace("data:", "");
						fpath = app.purgeFileName(tdata["reference"]);
						//console.log(tabi[1], tt);
						console.log(cordova.file.cacheDirectory, fpath);
						var ins = Date.now();
						if(app.isCachedImg(fpath) == false){
							try{
								savebase64AsFile(cordova.file.cacheDirectory, fpath, tabi[1], tt, function(retSave){
									console.log("retSave", retSave);
									window.resolveLocalFileSystemURL(cordova.file.cacheDirectory + retSave, function success(fileEntry) {
										app.listImages.push({
											name: retSave,
											data: fileEntry.toInternalURL(),
											last_update: ins
										});
										console.log('cdvfile URI: ' + fileEntry.toInternalURL());
										$('.control-grid-button[image="' + retSave + '"]').css('background-image',"url('" + fileEntry.toInternalURL() + "')");
										
										img = new Image();
										txtev = "function(){"
										+"try{ $('.control-grid-button[image=\"" + retSave + "\"]').css('background-image',\"url('" + fileEntry.toInternalURL() + "')\"); }"
										+"catch(err){ console.error(err); console.log(this.src); }}";
										console.log(txtev);
										img.onload = eval(txtev);
										app.listImagesObjects.push(img);
										img.src = fileEntry.toInternalURL();
									});
								});
							}
							catch(error){
								console.error(error);
								}
						}
						else{
							window.resolveLocalFileSystemURL(cordova.file.cacheDirectory + fpath, function success(fileEntry) {
								$('.control-grid-button[image="' + fpath + '"]')[0].style.backgroundImage="url('" + this.src + "')";
								});
							}
					}
				}
				if(tdata["function"].search('.') != -1){
					var ModData = tdata["data"];
					if(tdata["data"] == null){}
					else if(typeof(tdata["data"]) == "string"){ 
						if(tdata["data"].search('{') != -1 || tdata["data"].search('\\[') != -1){ ModData = JSON.parse(tdata["data"]+" "); }
						}
					
					for(index=0; index < app.storedMods.length; index++){
						var module = app.storedMods[index].data;
						var scripts = module.getElementsByTagName("Scriptxs")[0].childNodes;
						
						for(var nodei=0; nodei < scripts.length; nodei++){
							if(scripts[nodei].nodeName == "Scriptx"){
								try{ 
									var sources = scripts[nodei].getAttribute('sources').split('|');
									for(var j=0; j< sources.length; j++){ sources[j] = sources[j].trim(); }
									if(sources.indexOf(tdata["function"]) != -1){
										// Execution script pour module
										//eval(scripts[nodei].childNodes[0].nodeValue);
										var sc = scripts[nodei].childNodes[0].nodeValue;
										sc = sc.replace(new RegExp('%mid%', 'g'), app.storedMods[index].id);
										try{
											var vars = {};
											var varsNodes = module.getElementsByTagName("Vars")[0].childNodes;
											for(var nodev=0; nodev< varsNodes.length; nodev++){
												if(varsNodes[nodev].nodeName == "Var"){
													var vn = varsNodes[nodev].getAttribute('name');
													var isEval = false;
													try{ var val = varsNodes[nodev].getAttribute('eval'); if(val == "true"){ isEval = true; } } catch(erv2){ }
													if(!isEval){ vars[vn] = varsNodes[nodev].childNodes[0].nodeValue; }
													else{ try{ vars[vn] = eval(varsNodes[nodev].childNodes[0].nodeValue); } catch(erv3){ vars[vn] = ''; } }
													}
												}
											}
										catch(erv){ console.log(erv); }
										eval(sc);
										}
									} 
								catch(errr){ 
									console.log(errr);
									}
								}
							}
						//console.log(app.storedMods[index].data.getElementsByTagName("Id")[0].childNodes[0].nodeValue); 
						}
				}
			}
		}
		catch (err)
		{
			console.log((data.length>100)?data.slice(0, 100)+"...":"'"+data+"'");
			console.log(event.target);
			console.error(err);
		}
	},
	dispose: function ()
	{
		//alert('BABA');
		app.socket.close();
	},
	crypt: function ()  {},
	decrypt: function ()  {},
	TestAndRegister: function ()
	{
		console.log(">> TestAndRegister");
		app.waitLog = true;
		app.waitRegister = true;
		addr = $('#PannelHomeIP').val();
		port = $('#PannelHomePORT').val();
		app.GetServerInfos(addr, port);
	},
	TestServer: function (addr, port, pass, passSaved)
	{
		console.log(">> TestServer");
		app.waitLog = true;
		app.waitRegister = false;
		app.servPass = pass;
		app.servPassSaved = passSaved;
		app.GetServerInfos(addr, port);
	},
	GetServerInfos: function (addr, port)
	{
		app.lastHost = addr;
		app.lastPort = port;
		app.reconnectTry = 0;
		app.socketOpen(addr, port);
	},
	socketReOpen: function(){
		if (window.confirm("Session disconnected, retry ?")) { 
			app.TestServer(app.lastHost,app.lastPort, app.servPass, app.servPassSaved); 
			}
		else{  app.gotoHome(); }
		
		},
	socketSend: function(input){
		if(app.sockets[app.socketIndex].readyState != 1){ return false; }
		try{ app.sockets[app.socketIndex].send(input); return true; } catch(err){ return false; }
		},
	socketOpen: function(host, port){
		//app.sockets.forEach(function(socki){ socki.close(); });
		var sock = new WebSocket("wss://"+host+":"+port+"/Service");
		sock.onopen = app.sockectOnOpen;
		sock.onmessage = app.sockectOnMessage;
		sock.onclose = app.sockectOnClose;
		sock.onerror = app.sockectOnError;
		app.sockets.push(sock);
		},
	sockectOnOpen: function(e){
		console.log(e);
		console.log(app.sockets.indexOf(e.target));
		app.socketIndex = app.sockets.indexOf(e.target);
		app.manualyClosed = false;
		console.log("Connection established"); 
		console.log(">> GetServerInfos");
		app.reconnectTry = 0;
		app.manualyClosed = false;
		obl =
		{
			"function": "GetInfo",
			"keyPU": Crytography.GetPublicKeyString(),
			"time": Date.now
		};
		e.target.send(JSON.stringify(obl));
	},
	sockectOnClose: function(event){
		console.log("Connection ended");
		app.reconnectTry = app.reconnectTry + 1;
		if(app.manualyClosed == false && app.reconnectTry <= 5){
			setTimeout("app.TestServer(app.lastHost,app.lastPort, app.servPass, app.servPassSaved);", 1000);
		}
		else{ 
			app.gotoHome();
		}
		//setTimeout(app.socketReOpen, 1000);
		app.sockets[app.socketIndex] = null;
	},
	sockectOnError: function(error) {
		console.log(`[error]`, error);
	},
	SendToServerEncoded: function (st)
	{
		//st = JSON.stringify(modata);
		//console.log(st);
		mob =
		{
			type: "encoded",
			data: Crytography.Encrypt(st, app.Serverkey)
		};
		app.socketSend(JSON.stringify(mob));
	},
	gotoHome: function ()
	{
		app.lastPing = 0;
		app.servPort = null;
		app.servAddr = null;
		$("#PannelControl").css("display", "none");
		$("#PannelHome").css("display", "block");
		$("#toolbar_text").text("HOME");
		screen.orientation.lock('portrait');
	},
	gotoControls: function ()
	{
		$("#PannelHome").css("display", "none");
		$("#PannelControl").css("display", "block");
		$("#toolbar_text").text("CONTROLS");
		app.SendToServerEncoded("{\"function\":\"GetGrids\"}");
	},
	ping: function ()
	{
		try{
			console.log("ping: "+((app.socketSend('{"function":"Ping"}'))?"OK":"NOK")); 
			} 
		catch(err){}
	},
	loopDetectDisconnected: function ()
	{
		if (app.lastPing + 25000 < Date.now())
		{
			try{
				app.manualyClosed = false;
				app.socket.close();
			} 
			catch(err){}
		}
	},
	drawGrids: function ()
	{
		console.log("drawGrids");
		$("#ControlsListTabs").html('');
		$("#ControlsListGrids").html('');
		ww = window.innerWidth - 4;
		console.log(app.storedGrids);
		tdata = clone(app.storedGrids);
		app.storedActiveGrid = 1;

		//$("#ControlsListGrids").text(JSON.stringify(tdata));
		try
		{
			for (index in tdata)
			{
				ts = "";
				tts = "";
				gs = "";
				bs = "";
				tabStyle = tdata[index]["style"].replace(new RegExp(" ", "g"), "").split("}");
				for (li in tabStyle)
				{
					console.log(tabStyle[li]);
					if (tabStyle[li].search('.control-tab{') == 0)
					{
						ts = tabStyle[li].replace(".control-tab{", "");
					}
					if (tabStyle[li].search('.control-tab-style{') == 0)
					{
						tts = tabStyle[li].replace(".control-tab-style{", "");
					}
					if (tabStyle[li].search('.control-grid{') == 0)
					{
						gs = tabStyle[li].replace(".control-grid{", "");
					}
					if (tabStyle[li].search('.control-grid-button{') == 0)
					{
						bs = tabStyle[li].replace(".control-grid-button{", "");
					}
				}

				tab = document.createElement('div');
				tab.setAttribute('class', 'control-tab');
				tab.setAttribute('style', ts);
				tab.setAttribute('stype', tts);
				tab.setAttribute('activ', (index == 0) ? '1' : '0');
				tab.setAttribute('tid', escape(tdata[index]["name"]));
				tab.setAttribute('onclick', 'app.ExchangeTab("' + escape(tdata[index]["name"]) + '");');
				tab.innerText = tdata[index]["name"];
				$("#ControlsListTabs").append(tab);

				caseWidth = (ww / tdata[index]["width"]) - 8;
				
			
				grid = document.createElement('div');
				grid.setAttribute('class', 'control-grid');
				grid.setAttribute('style', gs);
				grid.setAttribute('activ', (index == 0) ? '1' : '0');
				grid.setAttribute('tid', escape(tdata[index]["name"]));
				if(tdata[index]["blocks"] === undefined){ tdata[index]["blocks"] = tdata[index]["buttons"]; }
				for (indexB in tdata[index]["blocks"])
				{
					if(tdata[index]["blocks"][indexB]["type"] === undefined){ tdata[index]["blocks"][indexB]["type"] == "button"; }
					if(tdata[index]["blocks"][indexB]["type"] == "button"){
						bcostyle = "";
						button = document.createElement('div');
						button.setAttribute('class', 'control-grid-button');
						button.setAttribute('bid', escape(tdata[index]["name"]) + '_' + tdata[index]["blocks"][indexB]["id"]);
						button.setAttribute('onclick', 'app.executeMacro("' + tdata[index]["blocks"][indexB]["macro"] + '", "' + tdata[index]["blocks"][indexB]["sound"] + '");');
						if (tdata[index]["blocks"][indexB]["icon"] != "")
						{
							hh = app.purgeFileName(tdata[index]["blocks"][indexB]["icon"]);
							button.setAttribute('image', hh);
							imgp = app.getCachedImg(hh);
							if(imgp == null){ 
								console.log(tdata[index]["blocks"][indexB]["icon"] + " NOT FOUND");
								app.getImageUpdateList(tdata[index]["blocks"][indexB]["icon"]); 
								}
							else{ 
								bcostyle = bcostyle + "background-image:"+"url('" + imgp.data + "')"+";";
								//$('.control-grid-button[image="' + hh + '"]').css('background-image',"url('" + imgp.data + "')"); 
								}
							
							bcostyle = bcostyle + "background-repeat: no-repeat; background-size: 95%; background-position: center;";
						}

						try
						{
							dfg = tdata[index]["blocks"][indexB]["style"].replace(new RegExp(" ", "g"), "").split(";");
							for (hj in dfg)
							{
								if (dfg[hj].search(new RegExp("^color:", "i")) != -1)
								{
									bcostyle = bcostyle + "border-color:" + dfg[hj].replace("color:", "") + ";";
									break;
								}
							}
						}
						catch (err)
						{}
						var w = parseInt(tdata[index]["blocks"][indexB]["width"], 10) * caseWidth; 
						w = w + (4 * (parseInt(tdata[index]["blocks"][indexB]["width"], 10) - 1)) + (4*(parseInt(tdata[index]["blocks"][indexB]["width"], 10) - 1));
						var h = parseInt(tdata[index]["blocks"][indexB]["height"], 10) * caseWidth; 
						h = h + (4 * (parseInt(tdata[index]["blocks"][indexB]["height"], 10) - 1)) + (4*(parseInt(tdata[index]["blocks"][indexB]["height"], 10) - 1));

						button.setAttribute('style', bs +
							'width:' + w + 'px;' +
							'height:' + h + 'px;' +
							'line-height:' + (h + (tdata[index]["blocks"][indexB]["height"] * 12)) + 'px;' +
							tdata[index]["blocks"][indexB]["style"] + bcostyle);
						span = document.createElement('div');
						span.innerText = tdata[index]["blocks"][indexB]["name"];
						button.appendChild(span);
						grid.appendChild(button);
					}
					else if(tdata[index]["blocks"][indexB]["type"] == "module"){
						var module = null;
						var moduleId = null;
						var moduleIdx = null;
						for(indexM=0; indexM<app.storedMods.length; indexM++){
							if(app.storedMods[indexM].id == tdata[index]["blocks"][indexB]["macro"]){
								module = app.storedMods[indexM].data;
								moduleId = tdata[index]["blocks"][indexB]["id"];
								moduleIdx = app.storedMods[indexM].id;
								break;
								}
						}
						if(module == null){ continue; }
						//if(app.storedMods[tdata[index]["blocks"][indexB]["macro"]] === undefined){ continue; }
						//console.log(app.storedMods[tdata[index]["blocks"][indexB]["macro"]]);
						
						var cw = parseInt(tdata[index]["blocks"][indexB]["width"], 10);
						var ch = parseInt(tdata[index]["blocks"][indexB]["height"], 10);
						var ModWidth = cw * caseWidth + (4 * (cw - 1)) + (4*(cw - 1));
						var ModHeight = ch * caseWidth + (4 * (ch - 1)) + (4*(ch - 1));
						block = document.createElement('div');
						block.setAttribute('class', 'control-grid-module');
						block.setAttribute('type', tdata[index]["blocks"][indexB]["macro"]);
						block.setAttribute('style', bs +
							'width:' + ModWidth + 'px;' +
							'height:' + ModHeight + 'px;' +
							'line-height:' + (ModHeight + (tdata[index]["blocks"][indexB]["height"] * 12)) + 'px;' +
							tdata[index]["blocks"][indexB]["style"]);
						
						var styleId = "style__"+moduleId;
						if(document.getElementById(styleId) != null){ document.getElementById(styleId).parentNode.removeChild(document.getElementById(styleId)); }
						var sty = module.getElementsByTagName("Style")[0].childNodes[0].nodeValue;
						var htm = module.getElementsByTagName("Template")[0].childNodes[0].nodeValue;
						htm = htm.replace(new RegExp('%bmid%', 'g'), moduleId);
						sty = sty.replace(new RegExp('%bmid%', 'g'), moduleId);
						htm = htm.replace(new RegExp('%mid%', 'g'), moduleIdx);
						sty = sty.replace(new RegExp('%mid%', 'g'), moduleIdx);
						try{
							var vars = module.getElementsByTagName("Vars")[0].childNodes;
							var declares = module.getElementsByTagName("Declares")[0].childNodes;
							for(var nodei=0; nodei< vars.length; nodei++){
								if(vars[nodei].nodeName == "Var"){
									var isEval = false;
									try{ var val = vars[nodei].getAttribute('eval'); if(val == "true"){ isEval = true; } } catch(errr){ }
									htm = htm.replace(new RegExp('%'+vars[nodei].getAttribute('name')+'%', 'g'), (isEval)?eval(vars[nodei].childNodes[0].nodeValue):vars[nodei].childNodes[0].nodeValue);
									sty = sty.replace(new RegExp('%'+vars[nodei].getAttribute('name')+'%', 'g'), (isEval)?eval(vars[nodei].childNodes[0].nodeValue):vars[nodei].childNodes[0].nodeValue);
									}
								}
							for(var nodei=0; nodei< declares.length; nodei++){
								if(declares[nodei].nodeName == "Declare"){
									app.modsVars[moduleIdx+declares[nodei].childNodes[0].nodeValue] = '';
									}
								}
							}
						catch(erv){ console.log(erv); }
						block.innerHTML = htm;
						//console.log(sty);
						addNewStyle(sty, styleId);
						grid.appendChild(block);
						}
					else{}
				}
				$("#ControlsListGrids").append(grid);
			}
		}
		catch (error)
		{
			console.log(error);
		}
		app.getImages();
	},
	executeMacro: function (name, sname)
	{
		ob =
		{
			macro: name,
			sound: sname
		};
		app.socketSend(JSON.stringify(ob));
	},
	ExchangeTab: function (tid)
	{
		console.log("ExchangeTab");
		$("#ControlsListTabs .control-tab").attr('activ', 0);
		$("#ControlsListGrids .control-grid").attr('activ', 0);
		$('#ControlsListTabs .control-tab[tid="' + tid + '"]').attr('activ', 1);
		$('#ControlsListGrids .control-grid[tid="' + tid + '"]').attr('activ', 1);
	},
	getCrypt: function (data, key)
	{
		return Crytography.Encrypt(JSON.stringify(data), key);
	},
	getImageUpdateList: function (ref)
	{
		app.listImagesToGet.push(ref);
	},
	getImages: function ()
	{
		if (app.listImagesToGet.length == 0) { return; }
		list = clone(app.listImagesToGet);
		app.listImagesToGet = [];
		list3 = [];
		for (id in list) {
			imgp = app.getCachedImg(list[id]);
			if(imgp == null){ list3.push(list[id]); }
			else{  $('.control-grid-button[image="' + list[id] + '"]')[0].style.backgroundImage="url('" + imgp.data + "')"; }
			}
		if(list3.length > 0) { 
			//app.socketSend('{"function":"GetImages","references":' + JSON.stringify(list3) + '}'); 
			app.SendToServerEncoded('{"function":"GetImages","references":' + JSON.stringify(list3) + '}'); 
			}
	}
};

app.init();