using ColorMC.Core.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ColorMC.Gui.UI.Model.Items;

public partial class NbtDataItem : ObservableObject
{
    [ObservableProperty]
    private int _key;
    [ObservableProperty]
    private object _value;

    private object _valueSave;

    private bool _hex;
    private bool _init;

    public NbtDataItem(int key, object value, bool hex)
    {
        _key = key;
        _valueSave = value;
        _hex = hex;
        _init = true;
        Show();
    }

    partial void OnValueChanged(object value)
    {
        if (_init)
            return;

        if (value is string str)
        {
            _init = true;
            string outdata = str;
            if (_hex)
            {
                var temp = str.Trim().Replace(" ", "");
                foreach (var item in temp)
                {
                    if (item is not ('0' or '1'))
                    {
                        Show();
                        return;
                    }
                }
                if ((_valueSave is byte && temp.Length != 8)
                    || (_valueSave is int && temp.Length != 32)
                    || (_valueSave is long && temp.Length != 64))
                {
                    Show();
                    return;
                }
                outdata = Convert.ToInt64(temp, 2).ToString();
            }

            if (_valueSave is byte)
            {
                _valueSave = byte.Parse(outdata);
            }
            else if (_valueSave is int)
            {
                _valueSave = int.Parse(outdata);
            }
            else if (_valueSave is long)
            {
                _valueSave = long.Parse(outdata);
            }
            Show();
        }
    }

    public object GetValue()
    {
        return _valueSave;
    }

    public void Show()
    {
        if (_hex)
        {
            if (_valueSave is byte a)
            {
                Value = FuntionUtils.ToHex(a);
            }
            else if (_valueSave is int b)
            {
                Value = FuntionUtils.ToHex(b);
            }
            else if (_valueSave is long c)
            {
                Value = FuntionUtils.ToHex(c);
            }
        }
        else
        {
            Value = _valueSave;
        }

        _init = false;
    }

    public void ChangeHex(bool hex)
    {
        _hex = hex;
        Show();
    }
}
