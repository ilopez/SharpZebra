﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Com.SharpZebra.Commands
{
    public partial class ZPLCommands
    {
        public static byte[] ClearPrinter(Printing.PrinterSettings settings)
        {
            //^MMT: Tear off Mode.  ^PRp,s,b: print speed (print, slew, backfeed) (2,4,5,6,8,9,10,11,12).  
            //~TA###: Tear off position (must be 3 digits).  ^LHx,y: Label home. ^SD##x: Set Darkness (00 to 30). ^PWx: Label width
            //^XA^MMT^PR4,12,12~TA000^LH0,12~SD19^PW750
            stringCounter = 0;
            printerSettings = settings;
            return Encoding.GetEncoding(850).GetBytes(string.Format("^XA^MMT^PR{0},12,12~TA{1:000}^LH{2},{3}~SD{4:00}^PW{5}", settings.PrintSpeed, 
                settings.AlignTearOff, settings.AlignLeft, settings.AlignTop, settings.Darkness, settings.Width+settings.AlignLeft));
        }        

        public static byte[] PrintBuffer(int copies)
        {
            return Encoding.GetEncoding(850).GetBytes(copies > 1 ? string.Format("^PQ{0}^XZ", copies) : "^XZ");
        }

        public static byte[] BarCodeWrite(int left, int top, int height, ElementDrawRotation rotation, Barcode barcode, bool readable, string barcodeData)
        {
            string encodedReadable = readable ? "Y" : "N";
            char encodedRotation = Rotation.ZPLRotationMap[(int)rotation];
            switch (barcode.Type)
	        {
                case BarcodeType.CODE39_STD_EXT:
                    return Encoding.GetEncoding(850).GetBytes(string.Format("^FO{0},{1}^BY{2}^B3{3},,{4},{5}^FD{6}^FS", left, top, 
                        barcode.BarWidthNarrow, encodedRotation, height, encodedReadable, barcodeData));
                case BarcodeType.CODE128_AUTO:
                    return Encoding.GetEncoding(850).GetBytes(string.Format("^FO{0},{1}^BY{2}^BC{3},{4},{5}^FD{6}^FS", left, top, 
                        barcode.BarWidthNarrow, encodedRotation, height, encodedReadable, barcodeData));
                case BarcodeType.UPC_A:
                    return Encoding.GetEncoding(850).GetBytes(string.Format("^FO{0},{1}^BY{2}^BU{3},{4},{5}^FD{6}^FS", left, top, 
                        barcode.BarWidthNarrow, encodedRotation, height, encodedReadable, barcodeData));
                case BarcodeType.EAN13:
                    return Encoding.GetEncoding(850).GetBytes(string.Format("^FO{0},{1}^BY{2}^BE{3},{4},{5}^FD{6}^FS", left, top,
                        barcode.BarWidthNarrow, encodedRotation, height, encodedReadable, barcodeData));
		        default:
                    throw new ApplicationException("Barcode not yet supported by SharpZebra library.");                
	        }
        }

        public static byte[] TextWrite(int left, int top, ElementDrawRotation rotation, ZebraFont font, int height, int width, string text, int codepage)
        {
            return Encoding.GetEncoding(codepage).GetBytes(string.Format("^FO{0},{1}^A{2}{3},{4},{5}^FD{6}^FS", left, top, (char)font,
                Rotation.ZPLRotationMap[(int)rotation], height, width, text));
        }

        public static byte[] TextWrite(int left, int top, ElementDrawRotation rotation, ZebraFont font, int height, int width, string text)
        {
            return TextWrite(left, top, rotation, font, height, width, text, 850);
        }

        public static byte[] TextWrite(int left, int top, ElementDrawRotation rotation, string fontName, char storageArea, int height, string text, int codepage)
        {
            return Encoding.GetEncoding(codepage).GetBytes(string.Format("^A@{0},{1},{1},{2}:{3}^FO{4},{5}^FD{6}^FS",
                Rotation.ZPLRotationMap[(int)rotation], height, storageArea, fontName, left, top, text));
        }

        public static byte[] TextWrite(int left, int top, ElementDrawRotation rotation, string fontName, char storageArea, int height, string text)
        {
            return TextWrite(left, top, rotation, fontName, storageArea, height, text, 850);
        }

        public static byte[] TextWrite(int left, int top, ElementDrawRotation rotation, int height, string text, int codepage)
        {
            //uses last specified font
            return Encoding.GetEncoding(codepage).GetBytes(string.Format("^A@{0},{1}^FO{2},{3}^FD{4}^FS", Rotation.ZPLRotationMap[(int)rotation],
                height, left, top, text));
        }

        public static byte[] TextWrite(int left, int top, ElementDrawRotation rotation, int height, string text)
        {
            return TextWrite(left, top, rotation, height, text, 850);
        }

        public static byte[] LineWrite(int left, int top, int lineThickness, int right, int bottom)
        {
            int height = top-bottom;
            int width = right - left;            
            char diagonal = height * width < 0 ? 'L' : 'R';
            int l = Math.Min(left, right);
            int t = Math.Min(top, bottom);
            height = Math.Abs(height);
            width = Math.Abs(width);

            //zpl requires that straight lines are drawn with GB (Graphic-Box)
            if (width < lineThickness)
                return BoxWrite(left-((int)(lineThickness/2)), top, lineThickness, width, height, 0);
            else if (height < lineThickness)
                return BoxWrite(left, top-((int)(lineThickness/2)), lineThickness, width, height, 0);
            else
                return Encoding.GetEncoding(850).GetBytes(string.Format("^FO{0},{1}^GD{2},{3},{4},{5},{6}^FS", l, t, width, height, 
                    lineThickness, "", diagonal));
        }

        public static byte[] BoxWrite(int left, int top, int lineThickness, int width, int height, int rounding)
        {
            return Encoding.GetEncoding(850).GetBytes(string.Format("^FO{0},{1}^GB{2},{3},{4},{5},{6}^FS", left, top, 
                Math.Max(width, lineThickness), Math.Max(height, lineThickness), lineThickness, "", rounding));
        }
        /*
        public static string FormDelete(string formName)
        {
            return string.Format("FK\"{0}\"\n", formName);
        }

        public static string FormCreateBegin(string formName)
        {
            return string.Format("{0}FS\"{1}\"\n", FormDelete(formName), formName);
        }

        public static string FormCreateFinish()
        {
            return string.Format("FE\n");
        }
        */
    }
}
