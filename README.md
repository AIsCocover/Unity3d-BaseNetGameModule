Unity3d多人网络对战框架
	声明：
		本框架的所有代码都源自书籍《Unity3d网络游戏实战》罗培羽著 机械工业出版社
		本框架的个人笔记博客：blog.csdn.net/MadBam_boo/article/details/79553169?utm_source=blogxgwz5
	
	本项目包含的内容为：服务器框架以及Unity3d客户端前台框架。
	除了网络上的基础功能之外，只提供了登录、注册、房间功能，不包含实际游戏部分。
	如果需要添加游戏部分，可以在服务器框架中添加相关的协议以及处理方法。
	触发游戏的关键点在RoomPanel的Start按钮逻辑中。
	
	注意：运行该框架需要确保vs中的.Net Framework 版本、Unity3d版本与要求一致或者保持兼容。
	数据库的相关连接信息请在服务器端的DataMgr的Connect方法中查看。

	客户端：
		1、客户端运行在Unity3d上，版本为Unity 2017+，现为2018.2.11f1(64 bit)
		2、脚本结构
			核心部分core
				网络部分Net
					协议Protocol
						这部分跟服务器端完全相同，详情可查阅下面
					连接实体Connection
						负责维护跟服务器之间的一个连接，实现客户端跟服务器之间的消息和数据传输。
						主要功能：
							连接 public bool Connect(string host, int port)
							断开连接 public bool Disconnect()
							消息发送 public bool Send(ProtocolBase protoBase)
							消息发送 public bool Send(ProtocolBase protoBase, string cbName, MsgDistribution.Delegate cb) 
							消息发送 public bool Send(ProtocolBase protoBase, MsgDistribution.Delegate cb)
							按帧fps  public void Update()
						生命周期：
							连接方法运行成功后触发。
							接受消息 private ReceiveCb(IAsyncResult ar)
							消息解析 private void ProcessData()
					消息分发器MsgDistribution
						维护一个消息列表msgList、一次性监听器以及持久监听器。消息分发通过C#的代理机制Delegate实现
							msgList用于保存从服务器端传送过来且解析成功的消息
						主要功能：
							添加一次性监听器 public void AddOnceListener(string name, Delegate cb)
							删除一次性监听器 public void DelOnceListener(string name, Delegate cb)
							添加持久性监听器 public void AddListener(string name, Delegate cb)
							删除持久性监听器 public void DelListener(string name, Delegate cb)
						生命周期：
							连接Connection解析消息后会将解析到的消息放进msgList中，消息分发器会对msgList中的消息一条条地处理。（为了防止死锁，需要对msgList加锁）
							根据协议名判断需要触发哪个监听事件。而管理监听事件的监听器有两种：一次性和持久性。
								一次性监听器onceDict只会执行一次，执行完毕就销毁。也就是说对于特定的协议，事件只会被触发一次。
								持久性监听器eventDict会一直存在，无论接收多少次指定的协议都会触发对应的事件。
							处理msgList public void Update()
							分发消息 	private void DispatchMsgEvent(ProtocolBase protoBase)
						
					网络管理器NetMgr
						主要用于管理连接，客户端和服务器之间的连接在复杂的项目中很有可能不止一条。
						主要功能：
							编制心跳消息 public static ProtocolBase GetHeartBeatProtocol()
								客户端需要间隔一段时间向服务器发送心跳消息以证明自己还“活着”
				面板Panel
					面板基类PanelBase
						用于规范所有面板的基本行为。
						主要功能：
							关闭 protected virtual void Close()
						生命周期：
							初始化 		public virutal void Init(params object[] args)
							面板显示前  public virtual void OnShowing()					在面板显示前需要处理的逻辑
							面板显示后  public virtual void OnShowed()					在面板显示后需要处理的逻辑
							更新        public virtual void Update()					在面板显示时需要处理的逻辑
							面板关闭前  public virtual void OnClosing()					在面板关闭前需要处理的逻辑
							面板关闭后  public virtual void OnClosed()					在面板关闭后需要处理的逻辑
					面板管理器PanelMgr
						单例。用于管理面板。
						面板管理器维护了一个面板列表dict，用于保存目前已经实例化的所有面板。
						主要功能：
							打开面板 public void OpenPanel<T> (string skinPath, params object[] args) where T : PanelBase
								使用泛型来实现OpenPanel的功能，参数T必须是继承自PanelBase的类型。
								实例化Unity3d中的预制体Prefab面板
			自定义部分Sample
				面板部分Panel
					这部分的Panel脚本将根据自己的功能需求充分利用面板管理器的面板开启、关闭、使用连接进行消息发送以及消息分发器中监听器的配置。
					详情可看代码。
					登录面板 LoginPanel
					注册面板 RegisterPanel
					大厅面板 LobbyPanel
					房间面板 RoomPanel
					提示面板 TipPanel
		
	服务器端：
		1、项目运行在.net frameworkv4.5.2下
			vs2017中更改.net framework版本:
				右键解决方案下的Server项目 > 属性 > 将目标框架更改为.Net Framework 4.5.2
		2、项目包含客户端和服务器框架以及房间功能，不包含实际场景战斗。
			如需添加战斗功能可自行添加函数，在房间点击Start按钮时留意服务器端控制台的tips
			
		3、项目配置
			IDE: Microsoft Visual Studio 2017
			项目属性配置目标框架：.net framework 4.5.2
			项目引用添加MySql.Data
				引用右键 > 管理NuGet包 > 浏览 > MySql > 安装MySql.Data
				用于连接MySql数据库
		4、项目结构：
			底层框架：
				服务器网络管理器
				数据库管理器
				协议
			应用层框架：
				连接实体
				用户实体
					持久数据实体
					临时数据实体
				房间实体
				系统工具实体
			逻辑层框架：
				连接请求处理器
				玩家消息处理器
				玩家事件处理器
				房间消息处理器
				
			服务器网络管理器通过解析获取到的消息，使用反射机制调用逻辑层的相关方法。
			
		5、详细
			底层框架：
				1、服务器网络管理器
					单例。服务器网络管理器使用socket进行网络通信。
					通过维护一个连接池管理所有客户端的连接，接受客户端的连接请求以及消息请求。
					主要功能：
						启动 public void Start(string host, int port)
						关闭 public void Close()
						发送消息（向单一客户端） public void Send(Conn conn, ProtocolBase protoBase)
						发送消息（广播）		 public void Broadcast(ProtocolBase protoBase)
					次要功能：
						申请连接（向连接池申请一个可用连接） private int NewIndex()
						心跳检测（检测客户端是否还在线上）   private void HeartBeat() && private void HandleMainTimer(object sender, System.Timers.ElapsedEventArgs e)
					生命周期：
						监听客户端连接请求 void AcceptCb(IAsyncResult ar)
						监听客户端消息请求 void ReceiveCb(IAsyncResult ar)
							解析消息并通过映射调用逻辑层相关处理器的方法。
							解析 private void ProcessData(Conn conn)
							消息分发 private void HandleMsg(Conn conn, ProtocolBase protoBase)
				2、数据库管理器
					单例。用于管理数据库数据，对数据库执行操作。
					数据库管理器封装了一系列方法，提供给外部使用。
					主要功能：
						注册 public bool Register(string id, string pw)
						登录 public bool CheckPassword(string id, string pw)
						创建玩家数据 public bool CreatePlayerData(string id)
						获取玩家数据 public PlayerData GetPlayerData(string id)
						更新玩家数据 public bool UpdatePlayerData(Player player)
				3、协议
					属于服务器和客户端之间进行消息通信的一组规则。
					两者在进行消息发送时都需要按照协议对数据进行编码。
					两者在接受消息时都需要按照协议对数据进行解码。
					协议结构：[数据长度][协议名][数据]
					基类协议 ProtocolBase
						用于规范协议所拥有的行为，主要包括：
						解码 		public virtual ProtocolBase Decode(byte[] readBuff, int start, int length)
						编码 		public virtual byte[] Encode()
						获取协议名 	public virtual string GetProtocolName()
						获取内容	public virtual string GetDesc()
					字节型协议 ProtocolBytes 继承自ProtocolBase
						用于字节型数据的消息协议
						数据解析方法：
							String类型
								添加 public void AddString(string str)
								获取 public string GetString(int start, ref int end)	添加ref int end可实现数据的连续读取
								获取 public string GetString(int start) 				调用GetString(int start, ref int end)
							Int类型
								添加 public void AddInt32(Int32 num)
								获取 public string GetInt32(int start, ref int end)	添加ref int end可实现数据的连续读取
								获取 public string GetInt32(int start) 				调用GetInt32(int start, ref int end)
							Float类型
								添加 public void AddFloat(float num)
								获取 public string GetFloat(int start, ref int end)	添加ref int end可实现数据的连续读取
								获取 public string GetFloat(int start) 				调用GetFloat(int start, ref int end)
					字符串型协议 ProtocolStr 继承自ProtocolBase
						用于字符串型数据的消息协议
						数据解析方式可参照字节型协议。
						
			应用层框架：
				1、连接实体Conn
					用于维护一个服务器与客户端之间的连接。
					其中包括一个缓存池用于数据通信。
					方法：
						初始化 				public void Init(Socket socket)
						获取客户端地址 		public string GetAddress()
						获取缓存池剩余长度 	public int BufferRemain()
						连接关闭			public void Close()
						消息发送			public void Send(ProtocolBase protoBase)
				2、用户实体Player
					描述一个用户所拥有的属性以及行为。
					方法：
						消息发送 	public void Send(ProtocolBase protoBase)
						强制下线	public static bool KickOff(string id, ProtocolBase protoBase)
						
					持久数据实体：这些数据将会保存到数据库中，比如等级、经验、胜利/失败场数等。
					临时数据实体：用于保存游戏运行时的临时数据，比如房间信息、游戏状态等。
						登出		public bool Logout()
				3、房间实体Room
					描述一个房间所拥有的属性以及行为。
					房间实体维护一个用户列表用于保存房间中所有玩家的数据。
					主要方法：
						添加用户 	 public bool AddPlayer(Player player)
						删除用户 	 pulbic void DelPlayer(string id)
						广播 		 public void Broadcast(ProtocolBase protoBase)
						获取房间信息 public ProtocolBytes GetRoomInfo()
					次要方法：
						自动换队 private int SwichTeam()
							根据房间当前队伍比例对新进玩家自动换队
						更换房主 private void UpdateOwner()
							默认第一个进入房间的为房主
				4、房间管理器RoomMgr
					单例，用于管理房间内发生的行为。
					方法：	
						新建房间	 public void CreateRoom(Player player)
						离开房间	 public void LeaveRoom(Player player)
						获取房间列表 public ProtocolBytes GetRoomList()
						
			逻辑层框架：
				1、连接请求处理器HandleConnMsg
					用于处理客户端的消息类型的请求
					请求：
						心跳请求 pubilc void MsgHeartBeat(Conn conn, ProtocolBase protoBase)
						注册请求 public void MsgRegister(Conn conn, ProtocolBase protoBase)
						登录请求 public void MsgLogin(Conn conn, ProtocolBase protoBase)
						登出请求 public void MsgLogout(Conn conn, ProtocolBytes protoBase)
				2、玩家消息处理器
					用于处理玩家游戏时的消息
					消息：
						获取分数 public void MsgGetScore(Player player, ProtocolBase protoBase)
						获取成就 public void MsgGetAchieve(Player player, ProtocolBase protoBase)
				3、玩家事件处理器
					用于处理玩家触发的事件
					事件：
						登录事件 public void OnLogin(Player player)
						登出事件 public void OnLogout(Player player)
				4、房间消息处理器
					用于处理游戏房间的消息
					消息：
						获取房间列表 public void MsgGetRoomList(Player player, ProtocolBase protoBase)
						创建房间	 public void MsgCreateRoom(Player player, ProtocolBase protoBase)
						获取房间信息 public void MsgGetRoomInfo(Player player, ProtocolBase protoBase)
						玩家进入房间	 public void MsgEnterRoom(Player player, ProtocolBase protoBase)
						玩家离开房间	 public void MsgLeaveRoom(Player player, ProtocolBase protoBase)
					
	6、总结
		可优化点：
			1、消息传输协议的编码方式可以考虑使用赫夫曼编码来压缩需要传输的数据；
			2、使用设计模式优化代码框架（学习中。。。）
		