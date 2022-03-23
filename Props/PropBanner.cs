using System;
using System.Collections.Generic;
using Godot;

[Tool]
public class PropBanner : Spatial, IPropElement
{
    Action IPropElement.OnChangeHandle { get; set; }
    [Export]
    public bool ShowIcon;
    public override void _Ready()
    {

    }

    List<Action<PersonalTheme>> IPropElement.GetThemeSetters()
    {
        throw new NotImplementedException();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
