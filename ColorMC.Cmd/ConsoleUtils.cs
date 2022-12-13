using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Cmd;

public static class ConsoleUtils
{
    private static Action<int> OnSelect;
    private static Thread thread;
    private static List<string> Items;
    private static int Index;
    private static int Max;
    private static int Page;
    private static int MaxPage;

    public static void Init()
    {
        Console.Clear();
        Console.CursorVisible = false;
        Console.ForegroundColor = ConsoleColor.White;
        thread = new(Run);
        thread.Start();
    }

    public static void Reset()
    {
        Console.Clear();
        Console.CursorVisible = false;
        Items = null;
        Index = 0;
        Max = 0;
        Page = 0;
    }

    public static void ShowTitle(string title)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($">>ColorMC<<  [");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(title);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("]");
    }

    public static void ShowTitle1(string title)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"    {title}");
    }

    public static void SetItems(List<string> items, Action<int> select)
    {
        OnSelect = select;
        Items = items;
        Index = 0;
        Page = 0;
        MaxPage = Items.Count / 10;

        ShowItems();
    }

    public static void Input(string title)
    {
        Console.SetCursorPosition(0, Items.Count + 2);
        Console.WriteLine(title);
        Console.CursorVisible = true;
    }

    public static string? ReadLine(string input)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("- ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(input + ": ");
        return Console.ReadLine();
    }

    public static string ReadPassword(string input)
    {
        string a = "";
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("- ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(input + ": ");
        int start = Console.CursorLeft;
        int line = Console.CursorTop;
        while (true)
        {
            var key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.Backspace:
                    if (a.Length == 0)
                    {
                        Console.SetCursorPosition(start, line);
                    }
                    if (a.Length != 0)
                    {
                        a = a[..^1];
                        Console.SetCursorPosition(start + a.Length, line);
                        Console.Write(" ");
                        Console.SetCursorPosition(start + a.Length, line);
                    }
                    break;
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return a;
                default:
                    a += key.KeyChar;
                    Console.SetCursorPosition(start, line);
                    Console.Write(new string('*', a.Length));
                    break;
            }
        }
    }

    public static void Keep()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(">>任意键继续<<");
        Console.ReadKey(true);
    }

    public static string Edit(string name, string input)
    {
        Console.CursorVisible = true;
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("- ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(name + ": ");
        int start = Console.CursorLeft;
        int line = Console.CursorTop;
        Console.Write(input);
        while (true)
        {
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.Backspace:
                    if (Console.CursorLeft <= start)
                    {
                        Console.SetCursorPosition(start, line);
                    }
                    if (input.Length != 0)
                    {
                        input = input[..^1];
                        Console.SetCursorPosition(start + input.Length, line);
                        Console.Write(" ");
                        Console.SetCursorPosition(start + input.Length, line);
                    }
                    break;
                case ConsoleKey.Enter:
                    Console.CursorVisible = false;
                    Console.WriteLine();
                    return input;
                default:
                    input += key.KeyChar;
                    Console.SetCursorPosition(start, line);
                    Console.Write(input);
                    break;
            }
        }
    }

    public static void ToEnd()
    {
        if (Items == null)
            Console.WriteLine();
        else
            Console.SetCursorPosition(0, Items.Count + 2);
    }

    public static void Info1(string info)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(info);
    }

    public static void Info(string info)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(info);
    }

    public static void Error(string info)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(info);
    }
    public static void Ok(string info)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(info);
    }

    public static bool YesNo(string info)
    {
        ToEnd();
        int state = 0;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(info);
        int line = Console.CursorTop;
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(" >取消<    确定  ");
        Console.SetCursorPosition(0, line);
        while (true)
        {
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                    state = 0;
                    Console.SetCursorPosition(0, line);
                    Console.WriteLine(" >取消<    确定  ");
                    break;
                case ConsoleKey.RightArrow:
                    state = 1;
                    Console.SetCursorPosition(0, line);
                    Console.WriteLine("  取消    >确定< ");
                    break;
                case ConsoleKey.Enter:
                    return state == 1;
            }
        }
    }

    private static void ShowItems()
    {
        Console.SetCursorPosition(0, 2);

        string temp = new(' ', Console.WindowWidth);
        for (int a = 0; a < 12; a++)
        {
            Console.WriteLine(temp);
        }

        Console.SetCursorPosition(0, 2);

        Max = 0;

        if (Page != 0)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("..." + new string(' ', Console.WindowWidth - 3));
        }

        for (int a = 0; a < Math.Min(10, Items.Count - Page * 10); a++)
        {
            var item = Items[(Page * 10) + a];
            Max = item.Length > Max ? item.Length : Max;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("> ");
            Console.WriteLine(item);
        }

        if (MaxPage != Page)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("..." + new string(' ', Console.WindowWidth - 3));
        }

        Max += 10;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.SetCursorPosition(Max, Index + (Page != 0 ? 3 : 2));
        Console.Write("<");
    }

    private static void Run()
    {
        while (true)
        {
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    if (Index != 0)
                    {
                        Index--;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.SetCursorPosition(Max, Index + (Page != 0 ? 4 : 3));
                        Console.Write(" ");
                        Console.SetCursorPosition(Max, Index + (Page != 0 ? 3 : 2));
                        Console.Write("<");
                    }
                    else if (Page > 0)
                    {
                        Page--;
                        Index = 0;
                        ShowItems();
                    }
                    break;
                case ConsoleKey.DownArrow:
                    if (Index < Math.Min(9, (Items.Count - Page * 10) - 1))
                    {
                        Index++;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.SetCursorPosition(Max, Index + (Page != 0 ? 2 : 1));
                        Console.Write(" ");
                        Console.SetCursorPosition(Max, Index + (Page != 0 ? 3 : 2));
                        Console.Write("<");
                    }
                    else if (Page < MaxPage)
                    {
                        Page++;
                        Index = 0;
                        ShowItems();
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    if (Page > 0)
                    {
                        Page--;
                        Index = 0;
                        ShowItems();
                    }
                    break;
                case ConsoleKey.RightArrow:
                    if (Page < MaxPage)
                    {
                        Page++;
                        Index = 0;
                        ShowItems();
                    }
                    break;
                case ConsoleKey.Enter:
                    OnSelect(Index + Page * 10);
                    break;
            }

        }
    }

}
