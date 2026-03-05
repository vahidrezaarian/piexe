using System.Drawing;
using Tesseract;

namespace Piexe.Utilities;

public static class PictureScanner
{
    public static List<AnalyzedItem> Scan(object image, PageIteratorLevel textScanningPageIteratorLevel, out Bitmap sourceImage)
    {
        if (image is string imagePath)
        {
            return Scan(imagePath, textScanningPageIteratorLevel, out sourceImage);
        }
        else if (image is Bitmap bitmap)
        {
            sourceImage = bitmap;
            return Scan(bitmap, textScanningPageIteratorLevel);
        }
        else
        {
            throw new Exception("Invalid type!");
        }
    }

    public static List<AnalyzedItem> Scan(string imagePath, PageIteratorLevel textScanningPageIteratorLevel, out Bitmap sourceImage)
    {
        var result = new List<AnalyzedItem>();

        var barcodes = BarcodeScanner.Scan(imagePath, out sourceImage);
        
        if (barcodes != null && barcodes.Count > 0)
        {
            foreach (var barcode in barcodes)
            {
                if (barcode.IsQrCode)
                {
                    result.Add(new AnalyzedItem(barcode.Text, AnalyzedItemType.QrCode));
                }
                else
                {
                    result.Add(new AnalyzedItem(barcode.Text, AnalyzedItemType.Barcode));
                }
            }
        }

        var texts = TextScanner.Scan(imagePath, textScanningPageIteratorLevel);
        if (texts != null && texts.Length > 0)
        {
            foreach (var text in texts)
            {
                result.Add(new AnalyzedItem(text, AnalyzedItemType.Text));
            }
        }

        return result;
    }

    public static List<AnalyzedItem> Scan(Bitmap? imageBitmap, PageIteratorLevel textScanningPageIteratorLevel)
    {
        if (imageBitmap is null)
        {
            throw new ArgumentNullException(nameof(imageBitmap));
        }

        var result = new List<AnalyzedItem>();

        var barcodes = BarcodeScanner.Scan(imageBitmap);

        if (barcodes != null && barcodes.Count > 0)
        {
            foreach (var barcode in barcodes)
            {
                if (barcode.IsQrCode)
                {
                    result.Add(new AnalyzedItem(barcode.Text, AnalyzedItemType.QrCode));
                }
                else
                {
                    result.Add(new AnalyzedItem(barcode.Text, AnalyzedItemType.Barcode));
                }
            }
        }

        var texts = TextScanner.Scan(imageBitmap, textScanningPageIteratorLevel);
        if (texts != null && texts.Length > 0)
        {
            foreach (var text in texts)
            {
                result.Add(new AnalyzedItem(text, AnalyzedItemType.Text));
            }
        }

        return result;
    }
}
