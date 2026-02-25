using System.Windows.Media;

namespace Piexe.Assets;

public static class CustomIcons
{
    public static DrawingImage Copy
    {
        get
        {
            var drawingGroup = new DrawingGroup();

            var geometry1 = Geometry.Parse(
                "M16 12.9V17.1C16 20.6 14.6 22 11.1 22H6.9C3.4 22 2 20.6 2 17.1V12.9C2 9.4 3.4 8 6.9 8H11.1C14.6 8 16 9.4 16 12.9Z");
            var drawing1 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry1);
            drawingGroup.Children.Add(drawing1);

            var geometry2 = Geometry.Parse(
                "M17.0998 2H12.8998C9.81668 2 8.37074 3.09409 8.06951 5.73901C8.00649 6.29235 8.46476 6.75 9.02167 6.75H11.0998C15.2998 6.75 17.2498 8.7 17.2498 12.9V14.9781C17.2498 15.535 17.7074 15.9933 18.2608 15.9303C20.9057 15.629 21.9998 14.1831 21.9998 11.1V6.9C21.9998 3.4 20.5998 2 17.0998 2Z");
            var drawing2 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry2);
            drawingGroup.Children.Add(drawing2);

            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();

            return drawingImage;
        }
    }

    public static DrawingImage Copied
    {
        get
        {
            var drawingGroup = new DrawingGroup();

            var geometry1 = Geometry.Parse(
                "M17.0998 2H12.8998C9.81668 2 8.37074 3.09409 8.06951 5.73901C8.00649 6.29235 8.46476 6.75 9.02167 6.75H11.0998C15.2998 6.75 17.2498 8.7 17.2498 12.9V14.9781C17.2498 15.535 17.7074 15.9933 18.2608 15.9303C20.9057 15.629 21.9998 14.1831 21.9998 11.1V6.9C21.9998 3.4 20.5998 2 17.0998 2Z");
            var drawing1 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry1);
            drawingGroup.Children.Add(drawing1);

            var geometry2 = Geometry.Parse(
                "M11.1 8H6.9C3.4 8 2 9.4 2 12.9V17.1C2 20.6 3.4 22 6.9 22H11.1C14.6 22 16 20.6 16 17.1V12.9C16 9.4 14.6 8 11.1 8ZM12.29 13.65L8.58 17.36C8.44 17.5 8.26 17.57 8.07 17.57C7.88 17.57 7.7 17.5 7.56 17.36L5.7 15.5C5.42 15.22 5.42 14.77 5.7 14.49C5.98 14.21 6.43 14.21 6.71 14.49L8.06 15.84L11.27 12.63C11.55 12.35 12 12.35 12.28 12.63C12.56 12.91 12.57 13.37 12.29 13.65Z");
            var drawing2 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry2);
            drawingGroup.Children.Add(drawing2);

            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();

            return drawingImage;
        }
    }

    public static DrawingImage Barcode
    {
        get
        {
            var drawingGroup = new DrawingGroup();
            var mainGeometry = Geometry.Parse(
                "M2 4h2v16H2V4zm4 0h2v16H6V4zm3 0h3v16H9V4zm4 0h2v16h-2V4zm3 0h2v16h-2V4zm3 0h3v16h-3V4z");
            var mainDrawing = new GeometryDrawing(CustomColors.PrimaryVariant, null, mainGeometry);
            drawingGroup.Children.Add(mainDrawing);
            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();
            return drawingImage;
        }
    }

    public static DrawingImage QRCode
    {
        get
        {
            var drawingGroup = new DrawingGroup();
            var mainGeometry = Geometry.Parse(
                "M16 17v-1h-3v-3h3v2h2v2h-1v2h-2v2h-2v-3h2v-1h1zm5 4h-4v-2h2v-2h2v4zM3 3h8v8H3V3zm10 0h8v8h-8V3zM3 13h8v8H3v-8zm15 0h3v2h-3v-2zM6 6v2h2V6H6zm0 10v2h2v-2H6zM16 6v2h2V6h-2z");
            var mainDrawing = new GeometryDrawing(CustomColors.PrimaryVariant, null, mainGeometry);
            drawingGroup.Children.Add(mainDrawing);
            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();
            return drawingImage;
        }
    }

    public static DrawingImage Document
    {
        get
        {
            var drawingGroup = new DrawingGroup();
            var mainGeometry = Geometry.Parse(
                "M125,109 C123.896,109 123,108.104 123,107 L123,103 L129,109 L125,109 Z " +
                "M124,119 L112,119 C111.448,119 111,118.553 111,118 C111,117.448 111.448,117 112,117 L124,117 " +
                "C124.552,117 125,117.448 125,118 C125,118.553 124.552,119 124,119 Z " +
                "M124,125 L112,125 C111.448,125 111,124.553 111,124 C111,123.447 111.448,123 112,123 L124,123 " +
                "C124.552,123 125,123.447 125,124 C125,124.553 124.552,125 124,125 Z " +
                "M123,101.028 C122.872,101.028 109,101 109,101 C106.791,101 105,102.791 105,105 L105,129 " +
                "C105,131.209 106.791,133 109,133 L127,133 C129.209,133 131,131.209 131,129 L131,109 L123,101.028 Z");
            var mainDrawing = new GeometryDrawing(CustomColors.PrimaryVariant, null, mainGeometry);
            drawingGroup.Children.Add(mainDrawing);
            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();
            return drawingImage;
        }
    }

    public static DrawingImage PowerShell
    {
        get
        {
            var drawingGroup = new DrawingGroup();
            var mainGeometry = Geometry.Parse(
                "M16.19 2H7.81C4.17 2 2 4.17 2 7.81V16.18C2 19.83 4.17 22 7.81 22H16.18C19.82 22 21.99 19.83 21.99 16.19V7.81C22 4.17 19.83 2 16.19 2Z " +
                "M9.94 13.27C9.26 14.29 8.32 15.12 7.22 15.67C7.12 15.72 7 15.75 6.89 15.75C6.61 15.75 6.35 15.6 6.22 15.34C6.03 14.97 6.18 14.52 6.56 14.33 " +
                "C7.43 13.9 8.17 13.24 8.7 12.44C8.88 12.17 8.88 11.83 8.7 11.56C8.16 10.76 7.42 10.1 6.56 9.67C6.18 9.49 6.03 9.04 6.22 8.66C6.4 8.29 6.85 8.14 7.22 8.33 " +
                "C8.32 8.88 9.26 9.71 9.94 10.73C10.46 11.5 10.46 12.5 9.94 13.27Z " +
                "M17 15.75H13C12.59 15.75 12.25 15.41 12.25 15C12.25 14.59 12.59 14.25 13 14.25H17C17.41 14.25 17.75 14.59 17.75 15C17.75 15.41 17.41 15.75 17 15.75Z");
            var mainDrawing = new GeometryDrawing(CustomColors.PrimaryVariant, null, mainGeometry);
            drawingGroup.Children.Add(mainDrawing);
            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();
            return drawingImage;
        }
    }

    public static DrawingImage OpenFolder
    {
        get
        {
            var drawingGroup = new DrawingGroup();
            var bottomGeometry = Geometry.Parse(
                "M21.0582 11.8216L20.8982 11.6016C20.6182 11.2616 20.2882 10.9916 19.9082 10.7916 " +
                "C19.3982 10.5016 18.8182 10.3516 18.2182 10.3516H5.76824C5.16824 10.3516 4.59824 10.5016 " +
                "4.07824 10.7916C3.68824 11.0016 3.33824 11.2916 3.04824 11.6516C2.47824 12.3816 " +
                "2.20824 13.2816 2.29824 14.1816L2.66824 18.8516C2.79824 20.2616 2.96824 22.0016 " +
                "6.13824 22.0016H17.8582C21.0282 22.0016 21.1882 20.2616 21.3282 18.8416L21.6982 14.1916 " +
                "C21.7882 13.3516 21.5682 12.5116 21.0582 11.8216Z " +
                "M14.3882 17.3416H9.59824C9.20824 17.3416 8.89824 17.0216 8.89824 16.6416 " +
                "C8.89824 16.2616 9.20824 15.9416 9.59824 15.9416H14.3882C14.7782 15.9416 " +
                "15.0882 16.2616 15.0882 16.6416C15.0882 17.0316 14.7782 17.3416 14.3882 17.3416Z");
            drawingGroup.Children.Add(new GeometryDrawing(CustomColors.PrimaryVariant, null, bottomGeometry));
            var topGeometry = Geometry.Parse(
                "M20.56 8.59643C20.5976 8.97928 20.1823 9.23561 19.8175 9.11348 " +
                "C19.3127 8.94449 18.7814 8.86 18.2289 8.86H5.76891C5.21206 8.86 4.66381 8.95012 " +
                "4.15225 9.12194C3.79185 9.24298 3.37891 8.99507 3.37891 8.61489V6.66 " +
                "C3.37891 3.09 4.46891 2 8.03891 2H9.21891C10.6489 2 11.0989 2.46 11.6789 3.21 " +
                "L12.8789 4.81C13.1289 5.15 13.1389 5.17 13.5789 5.17H15.9589 " +
                "C19.0846 5.17 20.3059 6.00724 20.56 8.59643Z");
            drawingGroup.Children.Add(new GeometryDrawing(CustomColors.PrimaryVariant, null, topGeometry));
            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();
            return drawingImage;
        }
    }

    public static DrawingImage Link
    {
        get
        {
            var drawingGroup = new DrawingGroup();
            var mainGeometry = Geometry.Parse(
                "M16.19 2H7.81C4.17 2 2 4.17 2 7.81V16.18C2 19.83 4.17 22 7.81 22H16.18C19.82 22 21.99 19.83 21.99 16.19V7.81C22 4.17 19.83 2 16.19 2Z " +
                "M8.18 16.77C8.16 16.77 8.13 16.77 8.11 16.77C7.14 16.68 6.23 16.23 5.55 15.51C3.95 13.83 3.95 11.1 5.55 9.42L7.74 7.12 " +
                "C8.52 6.3 9.57 5.84 10.69 5.84C11.81 5.84 12.86 6.29 13.64 7.12C15.24 8.8 15.24 11.53 13.64 13.21L12.55 14.36 " +
                "C12.26 14.66 11.79 14.67 11.49 14.39C11.19 14.1 11.18 13.63 11.46 13.33L12.55 12.18C13.61 11.07 13.61 9.26 12.55 8.16 " +
                "C11.56 7.12 9.82 7.12 8.82 8.16L6.63 10.46C5.57 11.57 5.57 13.38 6.63 14.48C7.06 14.94 7.64 15.22 8.25 15.28 " +
                "C8.66 15.32 8.96 15.69 8.92 16.1C8.89 16.48 8.56 16.77 8.18 16.77Z " +
                "M18.45 14.59L16.26 16.89C15.48 17.71 14.43 18.17 13.31 18.17C12.19 18.17 11.14 17.72 10.36 16.89C8.76 15.21 8.76 12.48 10.36 10.8 " +
                "L11.45 9.65C11.74 9.35 12.21 9.34 12.51 9.62C12.81 9.91 12.82 10.38 12.54 10.68L11.45 11.83C10.39 12.94 10.39 14.75 11.45 15.85 " +
                "C12.44 16.89 14.18 16.9 15.18 15.85L17.37 13.55C18.43 12.44 18.43 10.63 17.37 9.53C16.94 9.07 16.36 8.79 15.75 8.73 " +
                "C15.34 8.69 15.04 8.32 15.08 7.91C15.12 7.5 15.48 7.19 15.9 7.24C16.87 7.34 17.78 7.78 18.46 8.5C20.05 10.17 20.05 12.91 18.45 14.59Z");
            var mainDrawing = new GeometryDrawing(CustomColors.PrimaryVariant, null, mainGeometry);
            drawingGroup.Children.Add(mainDrawing);
            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();
            return drawingImage;
        }
    }

    public static DrawingImage PictureFrame
    {
        get
        {
            var drawingGroup = new DrawingGroup();

            var geometry1 = Geometry.Parse(
                "M10.51 11.22 L8.31 2.39 C8.26 2.16 8.05 2 7.81 2 " +
                "C4.6 2 2 4.6 2 7.81 V13.51 C2 13.85 2.33 14.1 2.66 14 " +
                "L10.16 11.83 C10.42 11.76 10.58 11.49 10.51 11.22 Z");
            var drawing1 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry1);
            drawingGroup.Children.Add(drawing1);

            var geometry2 = Geometry.Parse(
                "M11.12 13.6789 C11.05 13.3989 10.76 13.2289 10.48 13.3089 " +
                "L2.37 15.6689 C2.15 15.7389 2 15.9389 2 16.1689 V16.1889 " +
                "C2 19.3989 4.6 21.9989 7.81 21.9989 H12.53 C12.86 21.9989 " +
                "13.11 21.6889 13.03 21.3589 L11.12 13.6789 Z");
            var drawing2 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry2);
            drawingGroup.Children.Add(drawing2);

            var geometry3 = Geometry.Parse(
                "M16.1908 2 H10.4408 C10.1108 2 9.86081 2.31 9.94081 2.64 " +
                "L14.6808 21.61 C14.7408 21.84 14.9408 22 15.1808 22 H16.1808 " +
                "C19.4008 22 22.0008 19.4 22.0008 16.19 V7.81 " +
                "C22.0008 4.6 19.4008 2 16.1908 2 Z");
            var drawing3 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry3);
            drawingGroup.Children.Add(drawing3);

            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();

            return drawingImage;
        }
    }

    public static DrawingImage PictureScan
    {
        get
        {
            var drawingGroup = new DrawingGroup();

            var geometry1 = Geometry.Parse(
                "M2.77 10 C2.34 10 2 9.66 2 9.23 V6.92 C2 4.21 4.21 2 6.92 2 " +
                "H9.23 C9.66 2 10 2.34 10 2.77 C10 3.2 9.66 3.54 9.23 3.54 " +
                "H6.92 C5.05 3.54 3.54 5.06 3.54 6.92 V9.23 C3.54 9.66 3.19 10 2.77 10 Z");
            var drawing1 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry1);
            drawingGroup.Children.Add(drawing1);

            var geometry2 = Geometry.Parse(
                "M21.23 10 C20.81 10 20.46 9.66 20.46 9.23 V6.92 " +
                "C20.46 5.05 18.94 3.54 17.08 3.54 H14.77 C14.34 3.54 14 3.19 14 2.77 " +
                "C14 2.35 14.34 2 14.77 2 H17.08 C19.79 2 22 4.21 22 6.92 V9.23 " +
                "C22 9.66 21.66 10 21.23 10 Z");
            var drawing2 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry2);
            drawingGroup.Children.Add(drawing2);

            var geometry3 = Geometry.Parse(
                "M17.0819 21.9997 H15.6919 C15.2719 21.9997 14.9219 21.6597 14.9219 21.2297 " +
                "C14.9219 20.8097 15.2619 20.4597 15.6919 20.4597 H17.0819 " +
                "C18.9519 20.4597 20.4619 18.9397 20.4619 17.0797 V15.6997 " +
                "C20.4619 15.2797 20.8019 14.9297 21.2319 14.9297 C21.6519 14.9297 22.0019 15.2697 22.0019 15.6997 " +
                "V17.0797 C22.0019 19.7897 19.7919 21.9997 17.0819 21.9997 Z");
            var drawing3 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry3);
            drawingGroup.Children.Add(drawing3);

            var geometry4 = Geometry.Parse(
                "M9.23 22 H6.92 C4.21 22 2 19.79 2 17.08 V14.77 C2 14.34 2.34 14 2.77 14 " +
                "C3.2 14 3.54 14.34 3.54 14.77 V17.08 C3.54 18.95 5.06 20.46 6.92 20.46 " +
                "H9.23 C9.65 20.46 10 20.8 10 21.23 C10 21.66 9.66 22 9.23 22 Z");
            var drawing4 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry4);
            drawingGroup.Children.Add(drawing4);

            var geometry5 = Geometry.Parse(
                "M18.4595 11.2305 H17.0995 H6.89953 H5.53953 " +
                "C5.10953 11.2305 4.76953 11.5805 4.76953 12.0005 " +
                "C4.76953 12.4205 5.10953 12.7705 5.53953 12.7705 " +
                "H6.89953 H17.0995 H18.4595 C18.8895 12.7705 19.2295 12.4205 19.2295 12.0005 " +
                "C19.2295 11.5805 18.8895 11.2305 18.4595 11.2305 Z");
            var drawing5 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry5);
            drawingGroup.Children.Add(drawing5);

            var geometry6 = Geometry.Parse(
                "M6.89844 13.9405 V14.2705 C6.89844 15.9305 8.23844 17.2705 9.89844 17.2705 " +
                "H14.0984 C15.7584 17.2705 17.0984 15.9305 17.0984 14.2705 V13.9405 " +
                "C17.0984 13.8205 17.0084 13.7305 16.8884 13.7305 H7.10844 " +
                "C6.98844 13.7305 6.89844 13.8205 6.89844 13.9405 Z");
            var drawing6 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry6);
            drawingGroup.Children.Add(drawing6);

            var geometry7 = Geometry.Parse(
                "M6.89844 10.0605 V9.73047 C6.89844 8.07047 8.23844 6.73047 9.89844 6.73047 " +
                "H14.0984 C15.7584 6.73047 17.0984 8.07047 17.0984 9.73047 V10.0605 " +
                "C17.0984 10.1805 17.0084 10.2705 16.8884 10.2705 H7.10844 " +
                "C6.98844 10.2705 6.89844 10.1805 6.89844 10.0605 Z");
            var drawing7 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry7);
            drawingGroup.Children.Add(drawing7);

            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();

            return drawingImage;
        }
    }

    public static DrawingImage Gallery
    {
        get
        {
            var drawingGroup = new DrawingGroup();

            var geometry1 = Geometry.Parse(
                "M2.58078 19.0112 L2.56078 19.0312 " +
                "C2.29078 18.4413 2.12078 17.7713 2.05078 17.0312 " +
                "C2.12078 17.7613 2.31078 18.4212 2.58078 19.0112 Z");
            var drawing1 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry1);
            drawingGroup.Children.Add(drawing1);

            var geometry2 = Geometry.Parse(
                "M9.00109 10.3811 " +
                "C10.3155 10.3811 11.3811 9.31553 11.3811 8.00109 " +
                "C11.3811 6.68666 10.3155 5.62109 9.00109 5.62109 " +
                "C7.68666 5.62109 6.62109 6.68666 6.62109 8.00109 " +
                "C6.62109 9.31553 7.68666 10.3811 9.00109 10.3811 Z");
            var drawing2 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry2);
            drawingGroup.Children.Add(drawing2);

            var geometry3 = Geometry.Parse(
                "M16.19 2 H7.81 C4.17 2 2 4.17 2 7.81 V16.19 " +
                "C2 17.28 2.19 18.23 2.56 19.03 C3.42 20.93 5.26 22 7.81 22 " +
                "H16.19 C19.83 22 22 19.83 22 16.19 V13.9 V7.81 " +
                "C22 4.17 19.83 2 16.19 2 Z " +
                "M20.37 12.5 C19.59 11.83 18.33 11.83 17.55 12.5 " +
                "L13.39 16.07 C12.61 16.74 11.35 16.74 10.57 16.07 " +
                "L10.23 15.79 C9.52 15.17 8.39 15.11 7.59 15.65 " +
                "L3.85 18.16 C3.63 17.6 3.5 16.95 3.5 16.19 V7.81 " +
                "C3.5 4.99 4.99 3.5 7.81 3.5 H16.19 C19.01 3.5 20.5 4.99 20.5 7.81 " +
                "V12.61 L20.37 12.5 Z");
            var drawing3 = new GeometryDrawing(CustomColors.PrimaryVariant, null, geometry3);
            drawingGroup.Children.Add(drawing3);

            var drawingImage = new DrawingImage(drawingGroup);
            drawingImage.Freeze();

            return drawingImage;
        }
    }
}