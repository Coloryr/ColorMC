﻿using CommunityToolkit.Mvvm.ComponentModel;
namespace ColorMC.Gui.UI.Model;

public abstract class TopModel : ObservableObject
{
    public BaseModel Model { get; }
    public TopModel(BaseModel model)
    {
        Model = model;
    }
    public void TopClose()
    {
        Close();
    }
    protected abstract void Close();
}