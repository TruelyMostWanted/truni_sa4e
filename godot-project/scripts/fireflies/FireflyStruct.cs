using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace SA4E.scripts.fireflies;

public struct FireflyStruct
{
	//SINGLEPLAYER DATA
	public int ID;
	public Vector2I Index;

	public double Base;         //random value from 0.00 to 0.25
	public double Coupling;     //a fixed value of 0.1
	public double Phase;        //random value from 0.00 to 1.00
	
	//MULTIPLAYER DATA
	public long AssignedPeerID = -1;

	public FireflyStruct(int id, Vector2I index, double @base, double coupling, double phase, long assignedPeerId)
	{
		ID = id;
		Index = index;
		Base = @base;
		Coupling = coupling;
		Phase = phase;
		AssignedPeerID = assignedPeerId;
	}

	public double CalculatePhase()
	{
		var grid = FireflyStructGridNode2D.Instance;
		
		// (1) Get the sum of the phases of the neighbours and calculate the average
		grid.GetPhaseSum4Neighbours(Index, out double phaseSum, out int neighbourCount);
		double averagePhase = phaseSum / neighbourCount;
			
		//(2) The new phase is defined as base value + coupling * average phase
		double deltaPhase = Base + Coupling * averagePhase;
			
		//(3) Update the phase
		double phase = grid.GetPhase(Index);
		phase += deltaPhase;
		phase %= 1.0;
		grid.SetPhase(Index.X, Index.Y, phase);
		
		return phase;
	}

	public async Task UpdatePhaseTask()
	{
		GD.Print($"Firefly {ID} started!");
		var token = FireflyStructGridNode2D.Instance.UpdateCancellationTokenSource.Token;
		
		while (!token.IsCancellationRequested)
		{
			//(1-3) Calculate the new phase
			CalculatePhase();
			
			//(4) wait for the next "Frame" (1000ms/33s = 30fps)
			await Task.Delay(33, token);
		}
		
		GD.Print($"Firefly {ID} stopped!");
	}

	public Error TryAssignPeer(long peerId, out int id, out Vector2I index)
	{
		id = ID;
		index = Index;

		if (AssignedPeerID == peerId)
			return Error.AlreadyInUse;

		if (AssignedPeerID is > -1)
			return Error.Failed;
		
		AssignedPeerID = peerId;
		return Error.Ok;
	}
	public Error TryReleasePeer(long peerId)
	{
		if(AssignedPeerID <= -1)
			return Error.Skip;

		if (AssignedPeerID != peerId) 
			return Error.InvalidData;
		
		AssignedPeerID = -1;
		return Error.Ok;
	}
}
