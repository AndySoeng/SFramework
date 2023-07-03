****************************************
CustomWire 
�汾:1.3 
����:�򽣷�
����:2017.8.3
****************************************

ʹ�÷�����
����˵����е� "CustomWire -> Create A New Wire" �½�һ��CustomWire���壬ͨ���޸��߶����á���������ק�ڵ���������Ҫ���ߡ�
//******2017.8.2*****//
�����ǰѡ����ĳ�����壬������彫��Ϊ�´�����CustomWire����ĸ�����
������CustomWire��������Ĭ�ϲ��ʣ���ʼʱ���������ڵ㣨��������������Ҫ�����ڵ㣩
//******************//



���ַ���˵����
//**********CustomWire**********//
void GetNode(int index)							    				- ��ȡ��index���ڵ�
void AddNode()									    				- ��ĩβ����½ڵ㣬��û�нڵ㣬���½ڵ�Ϊ��һ���ڵ�
void AddNode(CustomWireNode node)				    				- ��ĩβ���ָ���ڵ㣬���ڵ�Ϊ�ջ��Ѵ��������
void AddNodeRange(CustomWireNode[] nodeCollection)					- ��ӽڵ㼯��
void AddNodeRange(GameObject[] goCollection)						- ��ӽڵ㼯�ϣ����أ�������GameObject���飩
void AddNodeRange(Transform[] tfCollection)							- ��ӽڵ㼯�ϣ����أ�������Transform���飩
void AddNodeRange(Vector3[] v3Collection)							- ��ӽڵ㼯�ϣ����أ�������Vector3���飩
void InsertNode(int index)											- ��indexλ�ò����½ڵ�
void InsertNode(int index, bool atFront)		    				- �����½ڵ㣬�½ڵ㽫��Ϊindexλ�ýڵ��ǰ��(atFront==true)/����(atFront==false)�ڵ�
void InsertNode(int index, CustomWireNode node)						- ��indexλ�ò���ڵ�node
void InsertNodeRange(int index, CustomWireNode[] nodeCollection)	- ����ڵ㼯��
void InsertNodeRange(int index, GameObject[] goCollection)			- ����ڵ㼯�ϣ����أ�������GameObject���飩
void InsertNodeRange(int index, Transform[] tfCollection)			- ����ڵ㼯�ϣ����أ�������Transform���飩
void InsertNodeRange(int index, Vector3[] v3Collection)				- ����ڵ㼯�ϣ����أ�������Vector3���飩
void RemoveNode(int index)   										- ɾ��indexλ�õĽڵ�
void RemoveNode(CustomWireNode node) 								- ɾ��ָ���ڵ�
void RemoveAllNodes() 												- ɾ�����нڵ�
void UpdateWire()													- �����߶�

//**********CustomWireNode**********//
void AddNode(bool atFront) - �ڵ�ǰ�ڵ��ǰ(atFront==true)/��(atFront==false)����½ڵ�



Inspector������˵����
//**********CustomWire**********//
�� Wire Configuration
Line Renderer	  - Ŀ���߶�
Wire Type		  - ����(Curve������3���ڵ㼰����ʱ�Ż�������)������(Linear)
Set On Update	  - �Ƿ�ʵʱ�����߶�
Close Wire		  - �Ƿ�պϣ����߶��Ƿ���β����

�� Node Manager
Draw Nodes		  - �Ƿ���ʾ�ڵ�
Node Radius		  - ��ʾ�ڵ��С��İ뾶
Create Node		  - ���߶�ĩβ�½�һ���ڵ㣬Ĭ��ΪCustomWire����������壬�ýڵ������Ϊǰһ���ڵ�����꣬����ǵ�һ���ڵ���localPosition=Vector3.zero
Remove All Nodes  - ɾ�����нڵ� 

�� Node List
Wire Node X		  - �ڵ�����
��/��	     		  - ����/���ƽڵ㣬����ǰ/��ڵ㽻��λ�ã��ڵ��˳���Ӱ���߶εļ��������ı�ڵ�˳�򽫸ı��߶���״
��  	     		  - ѡ��ڵ�
X  	     		  - ɾ���ڵ�

//**********CustomWireNode**********//
Front Node:xxx    - ǰ�ýڵ�����
<<                - ѡ��ǰ�ýڵ�
Back Node:xxx     - ���ýڵ�����
>>                - ѡ����ýڵ�
Add Node At Front - �ڵ�ǰ�ڵ��ǰ����ӽڵ�
Add Node At Back  - �ڵ�ǰ�ڵ�ĺ�����ӽڵ�
Go To Wire Object - ѡ��CustomWire����
Remove This Node  - ɾ����ǰ�ڵ�



ע�����
1�����Զ�һ���򼸸��ڵ㶯̬�ı�λ�ã������ڵ㶯����������ơ��򽫽ڵ���Ϊ��������������������游��������ƶ��������һ����̬�ı���ߣ�
2��ѡ��CustomWire��������ʾ�ڵ����ƣ��������ֱ�ӵ��ѡ��ڵ㣻
3����ʹ��Inspector����ϵİ����Խڵ���в������ֶ��½������ƻ���ɾ����Ľڵ㽫���ڽڵ��б��У���ʹ��Inspector����ϵ�"Add To Node List"�������ڵ���ӵ��ڵ��б�ĩβ�ٽ��в�����
4�����Ը��ƶ��CustomWire���壻
5�������ԶԶ��CustomWire����ͬʱ�༭��
6��ʹ�����ߣ�Linear������ʱ�۵㴦����ƽ���������������߲��������⣻
7���߶εĲ��ʻ�����߶α䳤�������죬����ʹ�ô�ɫ���ʡ�