using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace SA4E.scripts.fireflies;

[GlobalClass]
public partial class FireflyStructGridNode2D : Node2D
{
	public static FireflyStructGridNode2D Instance { get; private set; }
	
	[Export] public Vector2I GridSize { get; private set; } = new Vector2I(2, 2);
	[Export] public bool IsTorus { get; private set; } = true;
	[Export] public double Coupling { get; private set; } = 0.1;
	
	public FireflyStruct[,] Fireflies { get; private set; }
	public CancellationTokenSource UpdateCancellationTokenSource { get; private set; }
	public Task[,] FireflyUpdateTasks { get; private set; }	
	
	public void Initialize(int width, int height, bool isTorus, double coupling)
	{
		GridSize = new Vector2I(width, height);
		IsTorus = isTorus;
		Coupling = coupling;
		
		Fireflies = new FireflyStruct[width, height];
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				Fireflies[x, y] = new FireflyStruct()
				{
					ID = y * width + x,
					Index = new Vector2I(x, y),
					
					Base = GD.RandRange(0.00d, 0.02d),
					Coupling = coupling,
					Phase = 0,
					
					AssignedPeerID = -1
				};
			}
		}
	}
	
	public double GetPhase(int x, int y) => Fireflies[x, y].Phase;
	public double GetPhase(Vector2I pos) => Fireflies[pos.X, pos.Y].Phase;
	public void SetPhase(int x, int y, double phase) => Fireflies[x, y].Phase = phase;

	public bool TryGetFireflyIdForPeer(long peerId, out int id, out Vector2I index)
	{
		for (int y = 0; y < GridSize.Y; y++)
		{
			for (int x = 0; x < GridSize.X; x++)
			{
				if (Fireflies[x, y].AssignedPeerID == peerId)
				{
					id = Fireflies[x, y].ID;
					index = Fireflies[x, y].Index;
					return true;
				}
			}
		}

		id = -1;
		index = new Vector2I(-1, -1);
		return false;
	}
	
	public Error TryAssignPeer(long peerId, out int id, out Vector2I index)
	{
		for (int y = 0; y < GridSize.Y; y++)
		{
			for (int x = 0; x < GridSize.X; x++)
			{
				var assignError = Fireflies[x,y].TryAssignPeer(peerId, out id, out index);
				if (assignError == Error.AlreadyInUse)
					return assignError;
				if (assignError == Error.Ok)
					return assignError;
			}
		}
		
		id = -1;
		index = new Vector2I(-1, -1);
		return Error.Failed;
	}
	public Error TryReleasePeer(long peerId)
	{
		for (int y = 0; y < GridSize.Y; y++)
		{
			for (int x = 0; x < GridSize.X; x++)
			{
				var releaseError = Fireflies[x, y].TryReleasePeer(peerId);
				if (releaseError == Error.Ok)
					return Error.Ok;
			}
		}
		
		return Error.Failed;
	}

	private void _GetPhasesSum(Vector2I[] indices, out double phaseSum, out int neighbourCount)
	{
		phaseSum = 0;
		neighbourCount = 0;
		
		foreach (var n in indices)
		{
			if(n.X is -1 || n.Y is -1)
				continue;
			
			phaseSum += GetPhase(n.X, n.Y);
			neighbourCount++;
		}
	}
	
	public void GetPhaseSum4Neighbours(Vector2I pos, out double phaseSum, out int neighbourCount)
	{
		Vector2I[] neighbourIndices = [
			FireflyNeighbour.GetNeighbourIndex(pos, FireflyNeighbour.DirectionEnum.North, GridSize, IsTorus),
			FireflyNeighbour.GetNeighbourIndex(pos, FireflyNeighbour.DirectionEnum.East, GridSize, IsTorus),
			FireflyNeighbour.GetNeighbourIndex(pos, FireflyNeighbour.DirectionEnum.South, GridSize, IsTorus),
			FireflyNeighbour.GetNeighbourIndex(pos, FireflyNeighbour.DirectionEnum.West, GridSize, IsTorus)
		];
		_GetPhasesSum(neighbourIndices, out phaseSum, out neighbourCount);
	}
	public void GetPhaseSum8Neighbours(Vector2I pos, out double phaseSum, out int neighbourCount)
	{
		Vector2I[] neighbourIndices = [
			FireflyNeighbour.GetNeighbourIndex(pos, FireflyNeighbour.DirectionEnum.North, GridSize, IsTorus),
			FireflyNeighbour.GetNeighbourIndex(pos, FireflyNeighbour.DirectionEnum.East, GridSize, IsTorus),
			FireflyNeighbour.GetNeighbourIndex(pos, FireflyNeighbour.DirectionEnum.South, GridSize, IsTorus),
			FireflyNeighbour.GetNeighbourIndex(pos, FireflyNeighbour.DirectionEnum.West, GridSize, IsTorus),
			FireflyNeighbour.GetNeighbourIndex(pos, FireflyNeighbour.DirectionEnum.NorthEast, GridSize, IsTorus),
			FireflyNeighbour.GetNeighbourIndex(pos, FireflyNeighbour.DirectionEnum.SouthEast, GridSize, IsTorus),
			FireflyNeighbour.GetNeighbourIndex(pos, FireflyNeighbour.DirectionEnum.SouthWest, GridSize, IsTorus),
			FireflyNeighbour.GetNeighbourIndex(pos, FireflyNeighbour.DirectionEnum.NorthWest, GridSize, IsTorus)
		];
		_GetPhasesSum(neighbourIndices, out phaseSum, out neighbourCount);
	}

	public void StartUpdatePhaseTasks()
	{
		FireflyUpdateTasks = new Task[GridSize.X, GridSize.Y];
		UpdateCancellationTokenSource = new CancellationTokenSource();
		
		for (int y = 0; y < GridSize.Y; y++)
		{
			for (int x = 0; x < GridSize.X; x++)
			{
				var task = Task.Run(Fireflies[x, y].UpdatePhaseTask);
				FireflyUpdateTasks[x, y] = task;
			}
		}
	}
	
	public void StopTasks()
	{
		UpdateCancellationTokenSource.Cancel();
	}

	public override void _Ready()
	{
		base._Ready();
		Instance = this;
	}
}
