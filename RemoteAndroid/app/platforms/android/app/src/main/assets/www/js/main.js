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

app =
{
	Params: {},
	Serverkey: null,
	servPort: 25000,
	servAddr: "192.168.1.64",
	waitRegister: false,
	waitLog: false,
	lastPing: 0,
	timerPing: null,
	storedGrids: null,
	storedActiveGrid: 1,
	listImages: {},
	listImagesToGet: [],
	init: function ()
	{
		var options = { name: 'config.db', location: 'default' };
		if (device.platform == "Android") { options.androidDatabaseProvider = 'system'; }
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
						tx.executeSql("PRAGMA table_info('SERVERS')", [], function (tx, res)
						{
							if (res.rows.length === 0)
							{
								console.log('CREA TABLE SERVERS');
								tx.executeSql("CREATE TABLE 'SERVERS' (" +
									"'NAME' TEXT PRIMARY KEY NOT NULL, " +
									"'IP' TEXT NOT NULL, " +
									"'PORT' TEXT NOT NULL, " +
									"'REC_TIME' TEXT NOT NULL, " +
									"'LAST_USE' TEXT NOT NULL" +
									")");
								//tx.executeSql("INSERT INTO 'SERVERS'('NAME','IP','PORT','REC_TIME','LAST_USE') VALUES('TEST EKATON','192.168.1.174','25000','0','0')");
								//tx.executeSql("INSERT INTO 'SERVERS'('NAME','IP','PORT','REC_TIME','LAST_USE') VALUES('TEST HOME','192.168.1.64','25000','0','0')");
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
							}, 
							function (e) { console.log("ERROR TEST DB 4:", e); }
							);
							tx.executeSql("SELECT 'NAME','VALUE' FROM PARAMS", [], function (tx, res)
								{
									if (res.rows.length !== 0)
									{
										for (i = 0; i < res.rows.length; i++) { app.Params[res.rows.item(i).NAME] = res.rows.item(i).VALUE; }
									}
									app.init_P2();
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
		Crytography.Init();
		app.Serverkey = null;
		app.timerImages = setInterval(app.getImages, 100);
		$('#PannelHomeList').html('');
		$('#side_menu').bind('click', function(e){
			$('#side_menu').css('left','-250px');
		});

		//$('#PannelHomeIP').val(app.servAddr);
		//$('#PannelHomePORT').val(app.servPort);
		app.db.transaction(
			function (tx)
			{
				tx.executeSql("SELECT NAME,IP,PORT FROM SERVERS", [], function (tx, res)
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
							btn.setAttribute('onclick', "app.TestServer('" + res.rows.item(i).IP + "'," + res.rows.item(i).PORT + ");");
							btn.innerHTML = "Connect";
							doc.appendChild(p);
							doc.appendChild(btn);
							$('#PannelHomeList').append(doc);
						}
						console.log(res.rows);
					}
				}
				);
				tx.executeSql("SELECT 'NAME','VALUE','LAST_UPDATE' FROM 'CACHE'", [], function (tx, res)
				{
					if (res.rows.length !== 0)
					{
						var reftime = Date.now() - 3600000;
						for (i = 0; i < res.rows.length; i++)
						{
							if(reftime < res.rows.item(i).LAST_UPDATE){
								app.listImages[res.rows.item(i).NAME] = {
									data: res.rows.item(i).VALUE,
									last_update: res.rows.item(i).LAST_UPDATE
								};
							}
						}
						console.log(res.rows);
						tx.executeSql("DELETE FROM 'CACHE' WHERE LAST_UPDATE < ?", [reftime]);
						//tx.executeSql("DELETE FROM 'CACHE'", []);
					}
				}
				);
			}
		);
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
		datagram = cordova.require("cordova-plugin-datagram4.datagram");
		app.socket = datagram.createSocket("udp4");

		app.socket.bind(0, function (data)
			{
			console.log("bind \n" + data);
			if (data !== null) { location.reload(); }
			}
		);

		app.socket.on("message", 
			function (data, info)
			{
				try
				{
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
					if (tdata["function"] !== undefined)
					{
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
											tx.executeSql("SELECT NAME,IP,PORT FROM SERVERS WHERE NAME = ? OR IP = ?", [name, info.address], function (tx, res)
											{
												if (res.rows.length !== 0)
												{
													st = "";
													if (res.rows.item(i).NAME == name)
													{
														st = "Host Name already recorded";
													}
													if (res.rows.item(i).IP == info.address)
													{
														st = "Host Address already recorded";
													}
													alert("Error: " + st);
												}
												else
												{
													tx.executeSql("INSERT INTO SERVERS(NAME,IP,PORT,REC_TIME,LAST_USE) VALUES(?,?,?,?,?)", [name, info.address, info.port, 0, 0], function (tx, res)  {}
													);
												}
											}
											);
										}
									);
								}

								app.waitRegister = false;
								app.waitLog = false;
								app.servPort = info.port;
								app.servAddr = info.address;
								app.lastPing = Date.now();

								password = "";
								while (password === "")
								{
									password = prompt("Please enter the host password", "").replace(new RegExp(" ", "g"), "");
								}

								obl =
								{
									"function": "Login",
									"password": password
								};
								app.SendToServerEncoded(info.address, info.port, JSON.stringify(obl));
								//
							}
						}
						if (tdata["function"] == "Login")
						{
							if(tdata["status"] == "OK"){ console.log("Session OK"); app.gotoControls(); }
							else{ alert("invalid login"); }
						}
						if (tdata["function"] == "Ping")
						{
							app.lastPing = Date.now();
							app.SendToServerEncoded(app.servAddr, app.servPort, '{"function":"Pong"}');
						}
						if (tdata["function"] == "SendGrids")
						{
							app.storedGrids = tdata["grids"];
							app.storedActiveGrid = 1;
							app.drawGrids();
						}
						if (tdata["function"] == "RetGetImage")
						{
							if (tdata["result"] != "ERROR")
							{
								$('.control-grid-button[image="' + tdata["reference"] + '"]').css('background-image', "url('" + tdata["result"] + "')");
								var ins = Date.now();
								if(app.listImages[tdata["reference"]] === undefined){
									try{
										app.db.transaction(
											function (tx)
											{
												tx.executeSql(
													"INSERT INTO 'CACHE'('NAME','VALUE','LAST_UPDATE') VALUES(?,?,?)",
													[tdata["reference"], tdata["result"], ins],
													function (tx, res)  {},
													function (e) { console.log("ERROR DB 2:", e); }
												);
											},
											function (e) { console.error(tdata["reference"] + " => ERROR DB 1:", e); }
										);
										app.listImages[tdata["reference"]] = {
											data: tdata["result"],
											last_update: ins
										};
									}
									catch(error){}
								}
							}
						}
					}
				}
				catch (error)
				{
					console.log(data);
					console.log(info);
					console.error(error);
				}
			}
		);

		app.timerPing = setInterval(app.loopDetectDisconnected, 5000);
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
		console.log("TestAndRegister");
		app.waitLog = true;
		app.waitRegister = true;
		addr = $('#PannelHomeIP').val();
		port = $('#PannelHomePORT').val();
		app.GetServerInfos(addr, port);
	},
	TestServer: function (addr, port)
	{
		console.log("TestServer");
		app.waitLog = true;
		app.waitRegister = false;
		app.GetServerInfos(addr, port);
	},
	GetServerInfos: function (addr, port)
	{
		console.log("GetServerInfos");
		obl =
		{
			"function": "GetInfo",
			"keyPU": Crytography.GetPublicKeyString(),
			"time": Date.now
		};
		app.socket.send(JSON.stringify(obl), addr, port);
		console.log("GetServerInfos");
	},
	SendToServerEncoded: function (addr, port, st)
	{
		//st = JSON.stringify(modata);
		console.log(st);
		mob =
		{
			type: "encoded",
			data: Crytography.Encrypt(st, app.Serverkey)
		};
		app.socket.send(JSON.stringify(mob), addr, port);
	},
	gotoHome: function ()
	{
		app.lastPing = 0;
		app.servPort = null;
		app.servAddr = null;
		$("#PannelControl").css("display", "none");
		$("#PannelHome").css("display", "block");
		$("#toolbar_text").text("HOME");
	},
	gotoControls: function ()
	{
		$("#PannelHome").css("display", "none");
		$("#PannelControl").css("display", "block");
		$("#toolbar_text").text("CONTROLS");
		app.SendToServerEncoded(app.servAddr, app.servPort, "{\"function\":\"GetGrids\"}");
	},
	loopDetectDisconnected: function ()
	{
		if (app.lastPing + 25000 <= Date.now())
		{
			app.gotoHome();
		}
	},
	drawGrids: function ()
	{
		console.log("drawGrids");
		$("#ControlsListTabs").html('');
		$("#ControlsListGrids").html('');
		ww = window.innerWidth - 4;
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

				caseWidth = ww / tdata[index]["width"];
				grid = document.createElement('div');
				grid.setAttribute('class', 'control-grid');
				grid.setAttribute('style', gs);
				grid.setAttribute('activ', (index == 0) ? '1' : '0');
				grid.setAttribute('tid', escape(tdata[index]["name"]));
				for (indexB in tdata[index]["buttons"])
				{
					button = document.createElement('div');
					button.setAttribute('class', 'control-grid-button');
					button.setAttribute('bid', escape(tdata[index]["name"]) + '_' + tdata[index]["buttons"][indexB]["id"]);
					button.setAttribute('onclick', 'app.executeMacro("' + tdata[index]["buttons"][indexB]["macro"] + '", "' + tdata[index]["buttons"][indexB]["sound"] + '");');
					if (tdata[index]["buttons"][indexB]["icon"] != "")
					{
						button.setAttribute('image', tdata[index]["buttons"][indexB]["icon"]);
						app.getImageUpdateList(tdata[index]["buttons"][indexB]["icon"]);
						//setTimeout("app.SendToServerEncoded(app.servAddr, app.servPort, '"+JSON.stringify(mobj)+"');", 100);
					}

					bcostyle = "";
					try
					{
						dfg = tdata[index]["buttons"][indexB]["style"].replace(new RegExp(" ", "g"), "").split(";");
						for (hj in dfg)
						{
							if (dfg[hj].search(new RegExp("^color:", "i")) != -1)
							{
								bcostyle = "border-color:" + dfg[hj].replace("color:", "") + ";";
								break;
							}
						}
					}
					catch (err)
					{}

					button.setAttribute('style', bs +
						'width:' + ((caseWidth * tdata[index]["buttons"][indexB]["width"]) - 4) + 'px;' +
						'height:' + ((caseWidth * tdata[index]["buttons"][indexB]["height"]) - 4) + 'px;' +
						'line-height:' + ((caseWidth * tdata[index]["buttons"][indexB]["height"]) + (tdata[index]["buttons"][indexB]["height"] * 12)) + 'px;' +
						tdata[index]["buttons"][indexB]["style"] + bcostyle);
					span = document.createElement('div');
					span.innerText = tdata[index]["buttons"][indexB]["name"];
					button.appendChild(span);
					grid.appendChild(button);
				}
				$("#ControlsListGrids").append(grid);
			}
		}
		catch (error)
		{
			console.log(error);
		}
	},
	executeMacro: function (name, sname)
	{
		ob =
		{
			macro: name,
			sound: sname
		};
		app.socket.send(JSON.stringify(ob), app.servAddr, app.servPort);
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
			if(app.listImages[list[id]] === undefined){ list3.push(list[id]); }
			else{ $('.control-grid-button[image="' + list[id] + '"]').css('background-image', "url('" + app.listImages[list[id]].data + "')"); }
			}
		if(list3.length > 0) { app.SendToServerEncoded(app.servAddr, app.servPort, '{"function":"GetImages","references":' + JSON.stringify(list3) + '}'); }
	}
};
