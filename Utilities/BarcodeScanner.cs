using ZXing;
using System.Drawing;

namespace Piexe.Utilities;

public class ScannedBarcode
{
    public string Text = string.Empty;
    public bool IsQrCode;
}

public static class BarcodeScanner
{
    public static List<ScannedBarcode>? Scan(string imagePath, out Bitmap sourceImage)
    {
        sourceImage = (Bitmap)Image.FromFile(imagePath);
        return Scan(sourceImage);
    }

    public static List<ScannedBarcode>? Scan(Bitmap imageBitmap)
    {
        var reader = new BarcodeReaderGeneric();
        var scannedItems = reader.DecodeMultiple(imageBitmap);
        if (scannedItems != null && scannedItems.Length > 0)
        {
            var finalResult = new List<ScannedBarcode>();
            foreach (var item in scannedItems)
            {
                finalResult.Add(new ScannedBarcode()
                {
                    Text = item.Text,
                    IsQrCode = item.BarcodeFormat == BarcodeFormat.QR_CODE
                });
            }
            return finalResult;
        }
        return null;
    }
}
