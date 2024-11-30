using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using SA4E.scripts.fireflies;
using SA4E.scripts.renderer;

namespace SA4E.scripts.mp;

[GlobalClass]
public partial class MultiplayerApp : Node
{
	//SERVER AND CLIENT DATA
	[Export] public bool IsServer;
	[Export] public string ServerAddress;
	[Export] public int ServerPort;
	[Export] public int MaximumClients;
	[Export] public bool IsRunning;
	private ENetMultiplayerPeer _Peer;
	
	//FIREFLY DATA
	[Export] public FireflyStructGridNode2D FireflyGrid { get; private set; }
	
	//MULTIPLAYER DATA for this Peer (Server/Client)
	//Which Firefly is assigned to this Peer
	[Export] public int PeerFireflyID;
	[Export] public Vector2I PeerFireflyIndex;

	//Asynchronous Data
	public CancellationTokenSource UpdateCancellationTokenSource { get; private set; }
	public Task UpdatePhaseTask { get; private set; }
	
	//RENDERER
	[Export] public FireflyInstanceRenderer2D FireflyRenderer { get; private set; }
	
	
	public void RequestAssignPeer(long peerId, out int fireflyId, out Vector2I fireflyIndex)
	{
		var assignError = FireflyGrid.TryAssignPeer(peerId, out fireflyId, out fireflyIndex);
		if(assignError != Error.Ok)
			return;

		GD.Print($"Peer {peerId} assigned to Firefly {fireflyId} at index {fireflyIndex}");
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RequestReleasePeer(long peerId)
	{
		var releaseError = FireflyGrid.TryReleasePeer(peerId);
		if(releaseError != Error.Ok)
			return;

		var id = Multiplayer.GetUniqueId();
		GD.PrintErr($"ID: {id} --> (Peer: {peerId}/Status: {Error.Ok}/ID: {PeerFireflyID}/Index: {PeerFireflyIndex})");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void ResponseAssignPeer(long targetPeerId, int fireflyId, Vector2I fireflyIndex)
	{
		//(1) Update the Peer assigned
		FireflyGrid.Fireflies[fireflyIndex.X, fireflyIndex.Y].AssignedPeerID = targetPeerId;
		
		//(2) If we this peer is not the target, end immediatly
		if(Multiplayer.GetUniqueId() != targetPeerId)
			return;
		
		//(3) Start the Firefly on the target peer
		PeerFireflyID = fireflyId;
		PeerFireflyIndex = fireflyIndex;
		FireflyRenderer.FireflyID = PeerFireflyID;
		FireflyRenderer.FireflyIndex = PeerFireflyIndex;
		GD.Print($"Peer {targetPeerId} submitted Firefly {fireflyId} at index {fireflyIndex}");

		//(4) Start the tasks: Update Phase and Rendering
		UpdateCancellationTokenSource = new CancellationTokenSource();
		UpdatePhaseTask = Task.Run(UpdatePhaseAsync);
		
		FireflyRenderer.StartRenderingTask();
		
		//(5) Update the Window's Title
		DisplayServer.WindowSetSize(new Vector2I (450, 300), 0);
		DisplayServer.WindowSetTitle($"PeerID: {Multiplayer.GetUniqueId()} | FireflyID: {PeerFireflyID} | FireflyIndex: {PeerFireflyIndex}");
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void SetPhaseRpc(long senderPeerId, Vector2I index, double phase)
	{
		FireflyGrid.SetPhase(index.X, index.Y, phase);
		//GD.Print($"Peer {senderPeerId} set phase for Firefly {FireflyGrid.Fireflies[index.X, index.Y].ID} to {phase}");
	}
	
	public void SetPhase()
	{
		var calculatedPhase = FireflyGrid.Fireflies[PeerFireflyIndex.X, PeerFireflyIndex.Y].CalculatePhase();
		Rpc(MethodName.SetPhaseRpc, Multiplayer.GetUniqueId(), PeerFireflyIndex, calculatedPhase);
	}
	public async Task UpdatePhaseAsync()
	{
		GD.Print($"Firefly {PeerFireflyID} started!");
		
		while (!UpdateCancellationTokenSource.Token.IsCancellationRequested && 
		       _Peer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected)
		{
			CallDeferred(MethodName.SetPhase);
			await Task.Delay(33, UpdateCancellationTokenSource.Token);
		}
		
		GD.Print($"Firefly {PeerFireflyID} stopped!");
	}

	private void _Server_OnStarted()
	{
		var peerId = Multiplayer.GetUniqueId();
		GD.Print($"[SERVER] Started with ID: {peerId}");
		
		//(1) The SERVER will assign the peer to a firefly in the grid
		RequestAssignPeer(peerId, out var fireflyId, out var fireflyIndex);
		
		//(2) The SERVER will inform @everyone connected about the assignment
		Rpc(MethodName.ResponseAssignPeer, peerId, fireflyId, fireflyIndex);
	}
	private void _Server_OnMultiplayerServerDisconnected()
	{
		GD.Print("[SERVER] Server disconnected");
	}
	private void _Server_OnMultiplayerPeerDisconnected(long id)
	{
		GD.Print($"[SERVER] Peer {id} disconnected");
	}
	private void _Server_OnMultiplayerPeerConnected(long id)
	{
		GD.Print($"[SERVER] Peer {id} connected");
		
		//(1) The SERVER will assign the peer to a firefly in the grid
		RequestAssignPeer(id, out var fireflyId, out var fireflyIndex);
		
		//(2) The SERVER will inform @everyone connected about the assignment
		Rpc(MethodName.ResponseAssignPeer, id, fireflyId, fireflyIndex);
	}
	private void _Server_OnMultiplayerConnectedToServer()
	{
		GD.Print("[SERVER] Connected to server");
	}
	private void _Server_OnMultiplayerConnectionFailed()
	{
		GD.Print("[SERVER] Connection failed");
	}
	
	private void _Client_OnMultiplayerServerDisconnected()
	{
		GD.Print("[CLIENT] Server disconnected");
	}
	private void _Client_OnMultiplayerPeerDisconnected(long id)
	{
		GD.Print($"[CLIENT] Peer {id} disconnected");
	}
	private void _Client_OnMultiplayerPeerConnected(long id)
	{
		GD.Print($"[CLIENT] Peer {id} connected");
	}
	private void _Client_OnMultiplayerConnectedToServer()
	{
		GD.Print("[CLIENT] Connected to server");	
	}
	private void _Client_OnMultiplayerConnectionFailed()
	{
		GD.Print("[CLIENT] Connection failed");
	}
	
	public bool Start()
	{
		if (IsRunning)
			return false;
		
		//(1) Create a new Multiplayer Peer
		_Peer = new ENetMultiplayerPeer();
		
		//(2) Register Callbacks
		GD.Print("Add callbacks");
		if (IsServer)
		{
			Multiplayer.ConnectionFailed += _Server_OnMultiplayerConnectionFailed;
			Multiplayer.ConnectedToServer += _Server_OnMultiplayerConnectedToServer;
			Multiplayer.PeerConnected += _Server_OnMultiplayerPeerConnected;
			Multiplayer.PeerDisconnected += _Server_OnMultiplayerPeerDisconnected;
			Multiplayer.ServerDisconnected += _Server_OnMultiplayerServerDisconnected;
		}
		else
		{
			Multiplayer.ConnectionFailed += _Client_OnMultiplayerConnectionFailed;
			Multiplayer.ConnectedToServer += _Client_OnMultiplayerConnectedToServer;
			Multiplayer.PeerConnected += _Client_OnMultiplayerPeerConnected;
			Multiplayer.PeerDisconnected += _Client_OnMultiplayerPeerDisconnected;
			Multiplayer.ServerDisconnected += _Client_OnMultiplayerServerDisconnected;
		}

		//(3) Initialize it either as Server OR as Client
		if (IsServer)
		{
			GD.Print($"Create new Peer as Server at {ServerAddress}:{ServerPort}");
			var error = _Peer.CreateServer(ServerPort, MaximumClients);
			if (error != Error.Ok)
			{
				GD.PrintErr($"Error: {error}");
				return false;
			}
			_Peer.SetBindIP(ServerAddress);
			_Server_OnStarted();
		}
		else
		{
			GD.Print($"Create new Peer as Client at {ServerAddress}:{ServerPort}");
			var error = _Peer.CreateClient(ServerAddress, ServerPort);
			if (error != Error.Ok)
			{
				GD.PrintErr($"Error: {error}");
				return false;
			}
		}
		
		//(4) Set is as active peer to start the multiplayer
		GD.Print("Set as active peer");
		Multiplayer.MultiplayerPeer = _Peer;
		
		//(5) Print some information
		var isServerApi = Multiplayer.IsServer();
		GD.Print($"IsServer: {isServerApi}");
		
		var id = Multiplayer.GetUniqueId();
		GD.Print($"My ID: {id}");
		
		var peers = Multiplayer.GetPeers();
		GD.Print($"Peers: {string.Join(", ", peers)}");
		
		IsRunning = true;
		return true;
	}

	public bool Stop()
	{
		if (!IsRunning)
			return false;
		
		Rpc(MethodName.RequestReleasePeer, Multiplayer.GetUniqueId());
		
		Multiplayer.MultiplayerPeer.Close();
		
		if (IsServer)
		{
			Multiplayer.ConnectionFailed -= _Server_OnMultiplayerConnectionFailed;
			Multiplayer.ConnectedToServer -= _Server_OnMultiplayerConnectedToServer;
			Multiplayer.PeerConnected -= _Server_OnMultiplayerPeerConnected;
			Multiplayer.PeerDisconnected -= _Server_OnMultiplayerPeerDisconnected;
			Multiplayer.ServerDisconnected -= _Server_OnMultiplayerServerDisconnected;
		}
		else
		{
			Multiplayer.ConnectionFailed -= _Client_OnMultiplayerConnectionFailed;
			Multiplayer.ConnectedToServer -= _Client_OnMultiplayerConnectedToServer;
			Multiplayer.PeerConnected -= _Client_OnMultiplayerPeerConnected;
			Multiplayer.PeerDisconnected -= _Client_OnMultiplayerPeerDisconnected;
			Multiplayer.ServerDisconnected -= _Client_OnMultiplayerServerDisconnected;
		}
		
		UpdateCancellationTokenSource.Cancel();
		Multiplayer.MultiplayerPeer = null;
		
		IsRunning = false;
		return true;
	}

	public void Main(int gridWidth, int gridHeight, bool isTorus, double coupling, bool isServer)
	{
		IsServer = isServer;
		MaximumClients = gridWidth * gridHeight;
		FireflyGrid.Initialize(gridWidth, gridHeight, isTorus, coupling);
		
		Start();
	}
}
