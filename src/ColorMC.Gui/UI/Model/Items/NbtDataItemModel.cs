using System;
using ColorMC.Core.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// Nbt数据
/// </summary>
public partial class NbtDataItemModel : ObservableObject
{
    /// <summary>
    /// 键
    /// </summary>
    [ObservableProperty]
    private int _key;
    /// <summary>
    /// 值
    /// </summary>
    [ObservableProperty]
    private object _value;

    /// <summary>
    /// 实际数据
    /// </summary>
    private object _valueSave;

    /// <summary>
    /// 是否为hex
    /// </summary>
    private bool _hex;
    /// <summary>
    /// 是否为初始化状态
    /// </summary>
    private bool _init;

    public NbtDataItemModel(int key, object value, bool hex)
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
        {
            return;
        }

        //处理输入数据
        if (value is string str)
        {
            _init = true;
            string outdata = str;
            if (_hex)
            {
                //自动转换类型
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

    /// <summary>
    /// 获取数据
    /// </summary>
    /// <returns></returns>
    public object GetValue()
    {
        return _valueSave;
    }

    /// <summary>
    /// 显示数据
    /// </summary>
    public void Show()
    {
        if (_hex)
        {
            if (_valueSave is byte a)
            {
                Value = StringHelper.ToHex(a);
            }
            else if (_valueSave is int b)
            {
                Value = StringHelper.ToHex(b);
            }
            else if (_valueSave is long c)
            {
                Value = StringHelper.ToHex(c);
            }
        }
        else
        {
            Value = _valueSave;
        }

        _init = false;
    }

    /// <summary>
    /// 切换Hex模式
    /// </summary>
    /// <param name="hex"></param>
    public void ChangeHex(bool hex)
    {
        _hex = hex;
        Show();
    }
}
