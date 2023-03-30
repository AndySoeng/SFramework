
mergeInto(LibraryManager.library, {
  LoadParams: function () {
   var username=window.sessionStorage.getItem("username");
   var token=window.sessionStorage.getItem("token");
   var path=window.sessionStorage.getItem("path");
   var scoreapi=window.sessionStorage.getItem("scoreapi");
   var returnStr = username+'~'+ token+'~'+path+'~'+scoreapi;
   var bufferSize = lengthBytesUTF8(returnStr) + 1;
   var buffer = _malloc(bufferSize);
   stringToUTF8(returnStr, buffer, bufferSize);
   return buffer;
   },
   
   getUrlParams: function (defaultValue) {
   		var name = Pointer_stringify(defaultValue);
     		var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
     		var r = window.location.search.substr(1).match(reg);
     		if (r != null){
   			var returnStr = unescape(r[2]);
   			var bufferSize = lengthBytesUTF8( returnStr) + 1;
   			var buffer = _malloc(bufferSize);
   			stringToUTF8(returnStr, buffer, bufferSize);
   			 return buffer;
   		} 
   		else return null;
   	},
   	
   	OpenPage: function (str) {
   		window.open(Pointer_stringify(str));
    },
   
 });