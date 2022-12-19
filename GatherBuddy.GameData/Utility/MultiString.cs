using System;
using System.Globalization;
using Dalamud;
using Dalamud.Data;
using Lumina.Text;

namespace GatherBuddy.Utility;

public readonly struct MultiString
{
    public static string ParseSeStringLumina(SeString? luminaString)
        => luminaString == null ? string.Empty : Dalamud.Game.Text.SeStringHandling.SeString.Parse(luminaString.RawData).TextValue;

    public readonly string English;
    public readonly string German;
    public readonly string French;
    public readonly string Japanese;
    public readonly string Chinese;

    public string this[ClientLanguage lang]
        => Name(lang);

    public override string ToString()
        => Name(ClientLanguage.ChineseSimplified);

    public string ToWholeString()
        => $"{English}|{German}|{French}|{Japanese}|{Chinese}";

    public MultiString(string en, string de, string fr, string jp, string cn)
    {
        English  = en;
        German   = de;
        French   = fr;
        Japanese = jp;
        Chinese  = cn;
    }


    public static MultiString FromPlaceName(DataManager gameData, uint id)
    {
        var en = string.Empty;
        var de = string.Empty;
        var fr = string.Empty;
        var jp = string.Empty;
        var cn = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.PlaceName>(ClientLanguage.ChineseSimplified)!.GetRow(id)?.Name);
        return new MultiString(en, de, fr, jp,cn);
    }

    public static MultiString FromItem(DataManager gameData, uint id)
    {
        var en = string.Empty;
        var de = string.Empty;
        var fr = string.Empty;
        var jp = string.Empty;
        var cn = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>(ClientLanguage.ChineseSimplified)!.GetRow(id)?.Name);
        return new MultiString(en, de, fr, jp,cn);
    }

    private string Name(ClientLanguage lang)
        => lang switch
        {
            ClientLanguage.English  => English,
            ClientLanguage.German   => German,
            ClientLanguage.Japanese => Japanese,
            ClientLanguage.French   => French,
            ClientLanguage.ChineseSimplified => Chinese,
            _                       => throw new ArgumentException(),
        };

    public static readonly MultiString Empty = new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
}
