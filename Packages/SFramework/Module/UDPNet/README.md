# UDPNetHelper
Unity UDP Server and Client .

可以启动UDP客户端和服务端的，消息进行GZip压缩，并且具有心跳包检测。

可优化点：目前消息轮询在MessageCenter的Update中进行，每帧处理一个消息事件，可能在连接数量过多时存在阻塞情况。  
优化方法：每次消息轮询将接收队列的消息全部取出进行处理。
