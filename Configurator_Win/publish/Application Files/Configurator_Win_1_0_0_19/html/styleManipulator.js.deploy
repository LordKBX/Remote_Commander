
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
