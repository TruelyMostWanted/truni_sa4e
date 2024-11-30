using Godot;
using SA4E.scripts.app;
using SA4E.scripts.mp;

/// <summary>
/// The Application class is the main entry point for the application.
/// It handles the initialization and configuration of both singleplayer and multiplayer modes.
/// </summary>
public partial class Application : Node
{
    /// <summary>
    /// Singleton instance of the Application.
    /// </summary>
    public static Application Instance { get; private set; }

    /// <summary>
    /// Indicates whether the online mode is enabled.
    /// </summary>
    [Export] public bool IsOnlineEnabled;
    
    /// <summary>
    /// Default grid width for the application.
    /// </summary>
    [Export] public int DefaultGridWidth { get; set; } = 2;
    
    /// <summary>
    /// Default grid height for the application.
    /// </summary>
    [Export] public int DefaultGridHeight { get; set; } = 2;
    
    /// <summary>
    /// Default coupling value for the application.
    /// </summary>
    [Export] public double DefaultCoupling { get; set; } = 0.1;
    
    /// <summary>
    /// Instance of the SingleplayerApp.
    /// </summary>
    [Export] public SingleplayerApp SingleplayerApp { get; private set; }
    
    /// <summary>
    /// Instance of the MultiplayerApp.
    /// </summary>
    [Export] public MultiplayerApp MultiplayerApp { get; private set; }

    /// <summary>
    /// Main method to initialize the application.
    /// </summary>
    public void Main()
    {
        // (0) Create a singleton instance
        Instance = this;
        
        // (1) Read in the cmd arguments
        ApplicationArgumentsController.ReadArguments();
        foreach(var kvp in ApplicationArgumentsController.Arguments)
            GD.Print($"KEY: {kvp.Key}, VALUE: {kvp.Value}");
        
        // (2) Get the arguments as matching types
        var canGetGridWidth = ApplicationArgumentsController.TryParseArgumentToInt("grid_width", out int gridWidth);
        var canGetGridHeight = ApplicationArgumentsController.TryParseArgumentToInt("grid_height", out int gridHeight);
        var canGetIsOnline = ApplicationArgumentsController.TryParseArgumentToBool("online", out bool isOnline);
        var canGetIsServer = ApplicationArgumentsController.TryParseArgumentToBool("is_server", out bool isServer);
        
        // (3) If arguments cannot be parsed, set them to the default values
        if (!canGetGridWidth)
            gridWidth = DefaultGridWidth;
        if (!canGetGridHeight)
            gridHeight = DefaultGridHeight;

        // (4) Start the offline or online app
        if (!isOnline)
        {
            IsOnlineEnabled = false;
            SingleplayerApp.Main(gridWidth, gridHeight, true, DefaultCoupling);
        }
        else
        {
            IsOnlineEnabled = true;
            isServer = !canGetIsServer || isServer;
            MultiplayerApp.Main(gridWidth, gridHeight, true, DefaultCoupling, isServer);
        }
    }

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// </summary>
    public override void _Ready() //<-- ENTRY POINT
    {
        Main();
    }
    
    /// <summary>
    /// Handles notifications for the application.
    /// </summary>
    /// <param name="what">The notification type.</param>
    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest ||
            what == NotificationExitTree)
        {
            if (!IsOnlineEnabled)
            {
                SingleplayerApp.Stop();
            }
            else
            {
                MultiplayerApp.Stop();
            }
        }
    }
}