// SceneManager.cs
using Godot;

public partial class SceneManager : CanvasLayer
{
    // Добавьте это статическое поле
    public static SceneManager Instance { get; private set; }
    
    private ColorRect _fadeRect;
    private bool _isBusy = false;
    
    private const float FADE_TIME = 0.28f;

    public override void _Ready()
    {
        Instance = this;  // Сохраняем ссылку на себя
        
        Layer = 128;
        
        _fadeRect = new ColorRect
        {
            Color = new Color(0, 0, 0, 0),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_fadeRect);
        
        GD.Print("SceneManager (C#) загружен!");
    }

    public async void ChangeScene(string path)
    {
        if (_isBusy)
            return;
        
        _isBusy = true;
        _fadeRect.MouseFilter = Control.MouseFilterEnum.Stop;
        
        var tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", 1.0f, FADE_TIME);
        await ToSignal(tween, Tween.SignalName.Finished);
        
        GetTree().ChangeSceneToFile(path);
        
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        
        var tween2 = CreateTween();
        tween2.TweenProperty(_fadeRect, "color:a", 0.0f, FADE_TIME);
        await ToSignal(tween2, Tween.SignalName.Finished);
        
        _fadeRect.MouseFilter = Control.MouseFilterEnum.Ignore;
        _isBusy = false;
    }

    public async void QuitGame()
    {
        var tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", 1.0f, FADE_TIME);
        await ToSignal(tween, Tween.SignalName.Finished);
        
        GetTree().Quit();
    }
}