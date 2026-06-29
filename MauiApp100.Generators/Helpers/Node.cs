using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MauiApp100.Generators.Helpers;

abstract class Node
{
    public List<Node> Children = new();


public int Row = 0;
    public int Column = 0;
    public int RowSpan = 1;
    public int ColumnSpan = 1;

    public float X;
    public float Y;
    public float Width;
    public float Height;


}

// 👇 Nodo base concreto
class ContainerNode : Node { }

class GridNode : Node
{
    public List<string> Columns = new();
    public List<string> Rows = new();
}

class StackNode : Node
{
    public bool Horizontal;
    public float Spacing;
}

class LabelNode : Node
{
    public string Binding = "";
    public string Format = "";
    public bool Bold;
    public string Color = "Black";
    public string Align = "Start";
}

class ButtonNode : Node
{
    public string Action = "";
    public string Glyph = "";
    public string Color = "OrangeRed";
}

static class XamlParser
{
    public static Node Parse(XElement el)
    {
        return ParseNode(el);
    }

private static Node ParseNode(XElement el)
    {
        var name = el.Name.LocalName;

        Node node = name switch
        {
            "Grid" => new GridNode(),
            "HorizontalStackLayout" => new StackNode { Horizontal = true },
            "VerticalStackLayout" => new StackNode { Horizontal = false },
            "Label" => new LabelNode(),
            "Button" => new ButtonNode(),
            _ => new ContainerNode() // ✅ FIX
        };

        // =====================
        // PROPIEDADES GRID SAFE
        // =====================
        node.Row = ParseInt(el, "Grid.Row");
        node.Column = ParseInt(el, "Grid.Column");
        node.RowSpan = ParseInt(el, "Grid.RowSpan", 1);
        node.ColumnSpan = ParseInt(el, "Grid.ColumnSpan", 1);

        // =====================
        // STACK
        // =====================
        if (node is StackNode stack)
        {
            stack.Spacing = ParseFloat(el, "Spacing", 0);
        }

        // =====================
        // LABEL
        // =====================
        if (node is LabelNode lbl)
        {
            var text = el.Attribute("Text")?.Value ?? "";

            if (text.Contains("{Binding"))
            {
                var binding = ExtractBinding(text, out var format);
                lbl.Binding = binding;
                lbl.Format = format;
            }

            lbl.Bold = (el.Attribute("FontAttributes")?.Value ?? "").Contains("Bold");
            lbl.Align = el.Attribute("HorizontalTextAlignment")?.Value ?? "Start";
            lbl.Color = el.Attribute("TextColor")?.Value ?? "Black";
        }

        // =====================
        // BUTTON
        // =====================
        if (node is ButtonNode btn)
        {
            var img = el.Attribute("ImageSource")?.Value ?? "";

            if (!string.IsNullOrEmpty(img))
            {
                btn.Glyph = ExtractValue(img, "Glyph");
                btn.Color = ExtractValue(img, "Color") ?? "OrangeRed";
            }

            var cmd = el.Attribute("Command")?.Value ?? "";
            if (cmd.Contains("Delete")) btn.Action = "DELETE";
            if (cmd.Contains("Gift")) btn.Action = "GIFT";
        }

        // =====================
        // HIJOS
        // =====================
        foreach (var child in el.Elements())
        {
            node.Children.Add(ParseNode(child));
        }

        return node;
    }

    // =====================
    // HELPERS
    // =====================
    private static int ParseInt(XElement el, string attr, int def = 0)
    {
        return int.TryParse(el.Attribute(attr)?.Value, out var v) ? v : def;
    }

    private static float ParseFloat(XElement el, string attr, float def = 0)
    {
        return float.TryParse(el.Attribute(attr)?.Value, out var v) ? v : def;
    }

    private static string ExtractBinding(string text, out string format)
    {
        format = "";

        try
        {
            int start = text.IndexOf("{Binding") + 8;
            int end = text.IndexOf(",", start);

            if (end == -1)
                end = text.IndexOf("}", start);

            if (end == -1)
                return "";

            var binding = text.Substring(start, end - start).Trim();

            if (text.Contains("StringFormat"))
            {
                int sfStart = text.IndexOf("StringFormat=") + 13;
                int sfEnd = text.IndexOf("}", sfStart);

                if (sfEnd > sfStart)
                {
                    format = text.Substring(sfStart, sfEnd - sfStart)
                        .Replace("'", "")
                        .Replace("\"", "");
                }
            }

            return binding;
        }
        catch
        {
            return "";
        }
    }

    private static string ExtractValue(string text, string key)
    {
        if (!text.Contains(key + "=")) return "";

        int start = text.IndexOf(key + "=") + key.Length + 1;
        int end = text.IndexOf(",", start);

        if (end == -1) end = text.Length;

        return text.Substring(start, end - start)
            .Replace("&#x", "\\u")
            .Replace(";", "")
            .Trim();
    }

}
