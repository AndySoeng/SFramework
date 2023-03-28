var ChineseInputWebGL = {
	$input:null,
	$canvas:null,
	$unityContainer:null,
	$webglcontent:null,
	$unityGameObjectName : "",
	$unityInputID: "",
	$Inputing: function()
	{
		if(unityGameObjectName!=null&&unityInputID!=null && input != null)
		{
			var strvalue = unityInputID+"|"+input.value+"|"+input.selectionStart+"|"+input.selectionEnd;
			SendMessage(unityGameObjectName, "OnInputText", strvalue);
		}
	},
	$InputEnd: function()
	{
		if(unityGameObjectName!=null&&unityInputID!=null && input != null)
		{
			SendMessage(unityGameObjectName,"OnInputEnd",unityInputID.toString());
		}
		//document.onkeydown=null;
	},
	InputShow: function(GameObjectName_,inputID_,v_,fontsizeT_,indexStr_,inputRectStr_)
	{
		var GameObjectName = Pointer_stringify(GameObjectName_);
		var inputID = Pointer_stringify(inputID_);
		var v = Pointer_stringify(v_);
		var fontsizeT = Pointer_stringify(fontsizeT_);
		var indexStr=Pointer_stringify(indexStr_);
		var inputRectStr = Pointer_stringify(inputRectStr_);

		var indexArr=indexStr.split("|");
		var startIndexT = indexArr[0];
		var endIndexT = indexArr[1];
		
		var inputRectArr=inputRectStr.split("|");
		var posX = inputRectArr[0];
		var posY = inputRectArr[1];
		var width = inputRectArr[2];
		var height = inputRectArr[3];

		if(input==null){
			input = document.createElement("input");
			input.type = "text";
			input.id = "ChineseInputWebGLId";
			input.name = "ChineseInputWebGL";
			input.style = "visibility:hidden;";
			input.oninput = Inputing;
			input.onblur = InputEnd;
			//input.onmousemove=InputOnMouseover;
			//document.body.appendChild(input);
			document.getElementsByClassName("webgl-content")[0].appendChild(input);//需要放在这个下面，这样就方便计算在=相对canvas下的位置
			//document.getElementById("#canvas").appendChild(input);//需要放在这个下面，这样就方便计算在=相对canvas下的位置
			input.style.pointerEvents = "none";//不遮挡鼠标事件
			//input.zIndex=1000;//设置层级使不被遮挡
			//input.style.fontFamily="simkai"
		}

		if(canvas==null)
		{
			canvas = document.getElementById("#canvas");
		}
		this.screenSizeX=canvas.width;
		this.screenSizeY=canvas.height;
		
		if(unityContainer==null)
		{
			unityContainer = document.getElementById("unityContainer");//Unity2019.2.4此处的名称为unityContainer
			if(unityContainer==null)
			{
				unityContainer = document.getElementById("gameContainer");//Unity2017.3.0此处的名称为gameContainer
			}
		}
		this.unityScreenSizeX=unityContainer.style.width.replace("px","");
		this.unityScreenSizeY=unityContainer.style.height.replace("px","");
		if(webglcontent==null)
		{
			webglcontent = document.getElementsByClassName("webgl-content")[0];
		}
		this.webglcontentSizeX=webglcontent.clientWidth;
		this.webglcontentSizeY=webglcontent.clientHeight;
		

		var offsetX=(this.screenSizeX-this.webglcontentSizeX)/2;
		var offsetY=(this.screenSizeY-this.webglcontentSizeY)/2;
		//alert(posX+"-"+this.screenSizeX+"-"+this.unityScreenSizeX+"-"+this.webglcontentSizeX+"-"+offsetX)
		//alert(posY+"-"+this.screenSizeY+"-"+this.unityScreenSizeY+"-"+this.webglcontentSizeY+"-"+offsetY)
		if(offsetX>0)
		{		
			posX=posX-offsetX;
		}
		if(offsetY>0)
		{
			posY=posY-offsetY;
		}		
		input.style.width=width+"px";
		input.style.height=height+"px";
		input.style.left =posX+"px";//左边距
		//posY=posY-60;//测试
		input.style.top =posY+"px";//上边距
		input.value = v;
		input.style.fontSize=fontsizeT+"px";
		unityGameObjectName = GameObjectName;
		unityInputID = inputID;
		input.style.position='absolute';
		input.style.visibility = "visible"; 		
		input.style.opacity = 0;
		//console.log("opacity："+1);

		//input.focus();
		input.selectionStart = startIndexT;
		input.selectionEnd  = endIndexT;
		input.focus();
		//console.log("InputShow："+startIndexT+"|"+endIndexT);
    
		document.onkeydown = function (event) {
			event = event || window.event; //IE suckes
			if (event.keyCode == 65 && event.ctrlKey) {//捕捉Ctrl+A事件
				SendMessage(unityGameObjectName, "SelectAll",unityInputID.toString());
			}
			else if (event.keyCode == 37) {//左方向键
				var selectionindex=input.selectionStart-1;
				var strvalue = unityInputID+"|"+input.value+"|"+selectionindex+"|"+selectionindex;
				//console.log("左方向键："+input.selectionStart+"|"+selectionindex);
				SendMessage(unityGameObjectName, "OnInputText", strvalue);
			}
			else if (event.keyCode == 39) {//右方向键
				var selectionindex=input.selectionStart+1;
				var strvalue = unityInputID+"|"+input.value+"|"+selectionindex+"|"+selectionindex;
				//console.log("右方向键："+input.selectionStart+"|"+selectionindex);
				SendMessage(unityGameObjectName, "OnInputText", strvalue);
				
			}
		}
	}
};


autoAddDeps(ChineseInputWebGL, '$input');
autoAddDeps(ChineseInputWebGL, '$canvas');
autoAddDeps(ChineseInputWebGL, '$unityContainer');
autoAddDeps(ChineseInputWebGL, '$webglcontent');
autoAddDeps(ChineseInputWebGL, '$Inputing');
autoAddDeps(ChineseInputWebGL, '$InputEnd');
autoAddDeps(ChineseInputWebGL, '$unityGameObjectName');
autoAddDeps(ChineseInputWebGL, '$unityInputID');
mergeInto(LibraryManager.library, ChineseInputWebGL);