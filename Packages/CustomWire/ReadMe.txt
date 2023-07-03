****************************************
CustomWire 
版本:1.3 
作者:万剑飞
日期:2017.8.3
****************************************

使用方法：
点击菜单栏中的 "CustomWire -> Create A New Wire" 新建一个CustomWire物体，通过修改线段配置、创建并拖拽节点来建立需要的线。
//******2017.8.2*****//
如果当前选择了某个物体，则该物体将作为新创建的CustomWire物体的父物体
创建的CustomWire物体设有默认材质，初始时即有三个节点（创建曲线至少需要三个节点）
//******************//



部分方法说明：
//**********CustomWire**********//
void GetNode(int index)							    				- 获取第index个节点
void AddNode()									    				- 在末尾添加新节点，若没有节点，则新节点为第一个节点
void AddNode(CustomWireNode node)				    				- 在末尾添加指定节点，若节点为空或已存在则不添加
void AddNodeRange(CustomWireNode[] nodeCollection)					- 添加节点集合
void AddNodeRange(GameObject[] goCollection)						- 添加节点集合（重载，参数：GameObject数组）
void AddNodeRange(Transform[] tfCollection)							- 添加节点集合（重载，参数：Transform数组）
void AddNodeRange(Vector3[] v3Collection)							- 添加节点集合（重载，参数：Vector3数组）
void InsertNode(int index)											- 在index位置插入新节点
void InsertNode(int index, bool atFront)		    				- 插入新节点，新节点将作为index位置节点的前置(atFront==true)/后置(atFront==false)节点
void InsertNode(int index, CustomWireNode node)						- 在index位置插入节点node
void InsertNodeRange(int index, CustomWireNode[] nodeCollection)	- 插入节点集合
void InsertNodeRange(int index, GameObject[] goCollection)			- 插入节点集合（重载，参数：GameObject数组）
void InsertNodeRange(int index, Transform[] tfCollection)			- 插入节点集合（重载，参数：Transform数组）
void InsertNodeRange(int index, Vector3[] v3Collection)				- 插入节点集合（重载，参数：Vector3数组）
void RemoveNode(int index)   										- 删除index位置的节点
void RemoveNode(CustomWireNode node) 								- 删除指定节点
void RemoveAllNodes() 												- 删除所有节点
void UpdateWire()													- 更新线段

//**********CustomWireNode**********//
void AddNode(bool atFront) - 在当前节点的前(atFront==true)/后(atFront==false)添加新节点



Inspector面板变量说明：
//**********CustomWire**********//
▼ Wire Configuration
Line Renderer	  - 目标线段
Wire Type		  - 曲线(Curve，至少3个节点及以上时才绘制曲线)、折线(Linear)
Set On Update	  - 是否实时更新线段
Close Wire		  - 是否闭合，即线段是否收尾相连

▼ Node Manager
Draw Nodes		  - 是否显示节点
Node Radius		  - 表示节点的小球的半径
Create Node		  - 在线段末尾新建一个节点，默认为CustomWire物体的子物体，该节点的坐标为前一个节点的坐标，如果是第一个节点则localPosition=Vector3.zero
Remove All Nodes  - 删除所有节点 

▼ Node List
Wire Node X		  - 节点名称
▲/▼	     		  - 上移/下移节点，即与前/后节点交换位置，节点的顺序会影响线段的计算结果，改变节点顺序将改变线段形状
√  	     		  - 选择节点
X  	     		  - 删除节点

//**********CustomWireNode**********//
Front Node:xxx    - 前置节点名称
<<                - 选择前置节点
Back Node:xxx     - 后置节点名称
>>                - 选择后置节点
Add Node At Front - 在当前节点的前面添加节点
Add Node At Back  - 在当前节点的后面添加节点
Go To Wire Object - 选择CustomWire物体
Remove This Node  - 删除当前节点



注意事项：
1、可以对一个或几个节点动态改变位置（制作节点动画、代码控制、或将节点作为其他动画物体的子物体随父物体进行移动）来获得一条动态改变的线；
2、选择CustomWire物体后会显示节点名称，可以鼠标直接点击选择节点；
3、请使用Inspector面板上的按键对节点进行操作，手动新建、复制或撤销删除后的节点将不在节点列表中，可使用Inspector面板上的"Add To Node List"按键将节点添加到节点列表末尾再进行操作；
4、可以复制多个CustomWire物体；
5、不可以对多个CustomWire物体同时编辑；
6、使用折线（Linear）类型时折点处尽量平滑，否则会出现折线不均匀问题；
7、线段的材质会根据线段变长而被拉伸，尽量使用纯色材质。