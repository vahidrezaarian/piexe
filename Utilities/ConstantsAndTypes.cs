namespace Piexe.Utilities;

public enum AnalyzedItemType
{
    QrCode,
    Barcode,
    Text
}

public class AnalyzedItem(string value, AnalyzedItemType type)
{
    public readonly AnalyzedItemType Type = type;
    public readonly string Value = value;
}
