using BatchThat.Image.Enums;
using ImageMagick;

namespace BatchThat.Image.Filters
{
    public class AutoCrop : Filter
    {
        private const int PixelSamplingPercentage = 100;

        private int _pixelSkipRateH;
        private int _pixelSkipRateV;

        private const int LineSkipRate = 20;
        private const int DetectPaperBrightness = 85;
        private const int EdgeSafetyBuffer = 30;

        public override MagickImage ApplyFilter(MagickImage image)
        {
            using (var pixels = image.GetPixels())
            {
                SetPixelSkipRates(image.Width, image.Height);

                var prescanH = preScan_H(pixels, image.Width, image.Height);
                var prescanV = preScan_V(pixels, image.Width, image.Height);

                var rangeResult = FindPreScanEdgeRange(prescanH);
                var edgeTop = FindEdge(pixels, rangeResult[0], rangeResult[1], EnumAxis.Horiztontal, image.Width, EnumScanDirection.Forward);

                rangeResult = FindPreScanEdgeRange(FlipPreScanResults(prescanH));
                var edgeBottom = FindEdge(pixels, rangeResult[1], rangeResult[0], EnumAxis.Horiztontal, image.Width, EnumScanDirection.Backward);

                rangeResult = FindPreScanEdgeRange(prescanV);
                var edgeLeft = FindEdge(pixels, rangeResult[0], rangeResult[1], EnumAxis.Vertical, image.Height, EnumScanDirection.Forward);

                rangeResult = FindPreScanEdgeRange(FlipPreScanResults(prescanV));
                var edgeRight = FindEdge(pixels, rangeResult[1], rangeResult[0], EnumAxis.Vertical, image.Height, EnumScanDirection.Backward);

                var cropWidth = edgeRight - edgeLeft;
                var cropHeight = edgeBottom - edgeTop;

                image.Crop(edgeLeft, edgeTop, cropWidth, cropHeight);

                return image;
            }
        }

        private int[,] preScan_H(PixelCollection pPixels, int pWidth, int pHeight)
        {
            var totalScans = (pHeight / LineSkipRate) + 1;

            var scanCount = 0;

            var _return = new int[totalScans, 2];

            for (var x = 0; x < pHeight; x += LineSkipRate)
            {
                var scanReturn = lineScan_H(pPixels, x, pWidth);
                _return[scanCount, 0] = x; _return[scanCount, 1] = scanReturn;
                scanCount++;
            }

            return _return;
        }

        private int[] lineRangeScan_V(PixelCollection pPixels, int pBeginLine, int pEndLine, int pHeight)
        {
            var _return = new int[pEndLine - pBeginLine];

            for (var x = 0; x < (pEndLine - pBeginLine); x++)
            {
                _return[x] = lineScan_V(pPixels, pBeginLine + x, pHeight);
            }

            return _return;
        }

        private int lineScan_V(PixelCollection pPixels, int pLine, int pHeight)
        {
            var pixelTotal = 0;
            var scanCount = 0;

            for (var x = 0; x < pHeight; x += _pixelSkipRateV)
            {
                pixelTotal += pPixels.GetValue(pLine, x)[0];
                scanCount++;
            }
            scanCount--;

            var _return = pixelTotal / scanCount;

            return _return;
        }

        private int FindEdge(PixelCollection pPixels, int pBeginLine, int pEndLine, EnumAxis pAxis, int pAxisMax, EnumScanDirection pDirection)
        {
            if (pBeginLine > pEndLine && pAxis == EnumAxis.Horiztontal && pDirection == EnumScanDirection.Backward)
                return pBeginLine + EdgeSafetyBuffer;

            if (pBeginLine > pEndLine && pAxis == EnumAxis.Vertical && pDirection == EnumScanDirection.Backward)
                return pBeginLine + EdgeSafetyBuffer;

            var _return = 0;

            var highest = 0;

            // get all the RGB readings from the range
            var scanResults = pAxis == EnumAxis.Horiztontal ? lineRangeScan_H(pPixels, pBeginLine, pEndLine, pAxisMax) : lineRangeScan_V(pPixels, pBeginLine, pEndLine, pAxisMax);

            // find the highest value
            for (var x = 0; x < scanResults.Length; x++)
            {
                if (scanResults[x] > highest)
                {
                    highest = scanResults[x];
                    _return = pBeginLine + x;
                }
            }

            // count back the safety buffer
            if (pDirection == EnumScanDirection.Forward)
            {
                _return -= EdgeSafetyBuffer;
            }
            else
            {
                _return += EdgeSafetyBuffer;
            }

            return _return;
        }

        private int[] lineRangeScan_H(PixelCollection pPixels, int pBeginLine, int pEndLine, int pWidth)
        {
            var _return = new int[pEndLine - pBeginLine];

            for (var x = 0; x < (pEndLine - pBeginLine); x++)
            {
                _return[x] = lineScan_H(pPixels, pBeginLine + x, pWidth);
            }

            return _return;
        }

        private int lineScan_H(PixelCollection pPixels, int pLine, int pWidth)
        {
            var pixelTotal = 0;
            var scanCount = 0;

            for (var x = 0; x < pWidth; x += _pixelSkipRateH)
            {
                pixelTotal += pPixels.GetValue(x, pLine)[0];
                scanCount++;
            }

            var _return = pixelTotal / scanCount;

            return _return;
        }

        private int[,] FlipPreScanResults(int[,] pPreScanResults)
        {
            var firstElementCount = pPreScanResults.GetUpperBound(0) + 1;

            var _return = new int[firstElementCount, 2];

            for (var x = 0; x < firstElementCount; x++)
            {
                _return[firstElementCount - x - 1, 0] = pPreScanResults[x, 0];
                _return[firstElementCount - x - 1, 1] = pPreScanResults[x, 1];
            }

            return _return;
        }

        private int[] FindPreScanEdgeRange(int[,] pPreScanResults)
        {
            var _return = new[] { -1, -1 };

            for (var x = 1; x < pPreScanResults.GetUpperBound(0); x++)
            {
                if ((pPreScanResults[x, 1] > pPreScanResults[x - 1, 1]) && (pPreScanResults[x, 1] >= DetectPaperBrightness))
                {
                    _return[0] = pPreScanResults[x - 1, 0];
                    _return[1] = pPreScanResults[x, 0];

                    break;
                }
            }

            return _return;
        }

        private void SetPixelSkipRates(int x, int y)
        {
            _pixelSkipRateH = x * PixelSamplingPercentage / 100;
            _pixelSkipRateV = y * PixelSamplingPercentage / 100;

            _pixelSkipRateH = x / _pixelSkipRateH;
            _pixelSkipRateV = y / _pixelSkipRateV;
        }

        private int[,] preScan_V(PixelCollection pPixels, int pWidth, int pHeight)
        {
            var totalScans = pWidth / LineSkipRate;
            totalScans++;

            var scanCount = 0;

            var _return = new int[totalScans, 2];

            for (var x = 0; x < pWidth; x += LineSkipRate)
            {
                var scanReturn = lineScan_V(pPixels, x, pHeight);
                _return[scanCount, 0] = x; _return[scanCount, 1] = scanReturn;
                scanCount++;
            }

            return _return;
        }
    }
}
