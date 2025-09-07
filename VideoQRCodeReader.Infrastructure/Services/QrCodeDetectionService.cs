using VideoQRCodeReader.Infrastructure.Interfaces;
using ZXing;
using ZXing.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.Extensions.Logging;

namespace VideoQRCodeReader.Infrastructure.Services
{
    /// <summary>
    /// Implementation of QR code detection using ZXing.Net library
    /// Follows Single Responsibility Principle - only handles QR code detection
    /// </summary>
    public class QrCodeDetectionService : IQrCodeDetectionService
    {
        private readonly ILogger<QrCodeDetectionService> _logger;
        private readonly BarcodeReaderGeneric _barcodeReader;

        public QrCodeDetectionService(ILogger<QrCodeDetectionService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Configure the barcode reader for QR codes
            _barcodeReader = new BarcodeReaderGeneric
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new[] { BarcodeFormat.QR_CODE },
                    TryInverted = true
                }
            };
        }

        public async Task<string?> DetectQrCodeAsync(string imagePath)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(imagePath))
                    throw new ArgumentException("Image path cannot be null or empty.", nameof(imagePath));

                if (!File.Exists(imagePath))
                    throw new FileNotFoundException($"Image file not found: {imagePath}");

                _logger.LogDebug("Detecting QR code in image: {ImagePath}", imagePath);

                // Load and process the image
                using var image = await Image.LoadAsync<Rgb24>(imagePath);
                
                // Convert image to format suitable for ZXing
                var luminanceSource = new RgbLuminanceSource(GetImageBytes(image), image.Width, image.Height);
                var binaryBitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));

                // Detect QR code
                var result = _barcodeReader.Decode(luminanceSource);
                
                if (result != null)
                {
                    _logger.LogInformation("QR code detected in {ImagePath}: {QrContent}", imagePath, result.Text);
                    return result.Text;
                }

                _logger.LogDebug("No QR code found in image: {ImagePath}", imagePath);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting QR code in image: {ImagePath}", imagePath);
                throw;
            }
        }

        public async Task<List<string>> DetectMultipleQrCodesAsync(string imagePath)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(imagePath))
                    throw new ArgumentException("Image path cannot be null or empty.", nameof(imagePath));

                if (!File.Exists(imagePath))
                    throw new FileNotFoundException($"Image file not found: {imagePath}");

                _logger.LogDebug("Detecting multiple QR codes in image: {ImagePath}", imagePath);

                // Configure reader for multiple detection
                var multiReader = new BarcodeReaderGeneric
                {
                    AutoRotate = true,
                    Options = new DecodingOptions
                    {
                        TryHarder = true,
                        PossibleFormats = new[] { BarcodeFormat.QR_CODE },
                        TryInverted = true
                    }
                };

                // Load and process the image
                using var image = await Image.LoadAsync<Rgb24>(imagePath);
                
                // Convert image to format suitable for ZXing
                var luminanceSource = new RgbLuminanceSource(GetImageBytes(image), image.Width, image.Height);
                var binaryBitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));

                // Detect multiple QR codes
                var results = multiReader.DecodeMultiple(luminanceSource);
                
                var qrCodes = new List<string>();
                if (results != null && results.Length > 0)
                {
                    foreach (var result in results)
                    {
                        qrCodes.Add(result.Text);
                    }
                    
                    _logger.LogInformation("Found {Count} QR codes in {ImagePath}", qrCodes.Count, imagePath);
                }
                else
                {
                    _logger.LogDebug("No QR codes found in image: {ImagePath}", imagePath);
                }

                return qrCodes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting multiple QR codes in image: {ImagePath}", imagePath);
                throw;
            }
        }

        /// <summary>
        /// Converts ImageSharp image to byte array for ZXing processing
        /// </summary>
        private static byte[] GetImageBytes(Image<Rgb24> image)
        {
            var bytes = new byte[image.Width * image.Height * 3];
            var index = 0;

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < image.Width; x++)
                    {
                        var pixel = row[x];
                        bytes[index++] = pixel.R;
                        bytes[index++] = pixel.G;
                        bytes[index++] = pixel.B;
                    }
                }
            });

            return bytes;
        }
    }

    /// <summary>
    /// Custom luminance source for ZXing to work with RGB byte arrays
    /// </summary>
    public class RgbLuminanceSource : BaseLuminanceSource
    {
        private readonly byte[] _rgbData;

        public RgbLuminanceSource(byte[] rgbData, int width, int height) : base(width, height)
        {
            _rgbData = rgbData;
        }

        public override byte[] Matrix => GetLuminanceArray();

        public override byte[] getRow(int y, byte[] row)
        {
            if (row == null || row.Length < Width)
                row = new byte[Width];

            for (int x = 0; x < Width; x++)
            {
                var offset = (y * Width + x) * 3;
                var r = _rgbData[offset];
                var g = _rgbData[offset + 1];
                var b = _rgbData[offset + 2];
                
                // Convert RGB to luminance (grayscale)
                row[x] = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
            }

            return row;
        }

        protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
        {
            // Convert luminance back to RGB format for consistency
            var rgbData = new byte[newLuminances.Length * 3];
            for (int i = 0; i < newLuminances.Length; i++)
            {
                var luminance = newLuminances[i];
                rgbData[i * 3] = luminance;     // R
                rgbData[i * 3 + 1] = luminance; // G  
                rgbData[i * 3 + 2] = luminance; // B
            }
            return new RgbLuminanceSource(rgbData, width, height);
        }

        private byte[] GetLuminanceArray()
        {
            var luminance = new byte[Width * Height];
            for (int i = 0; i < luminance.Length; i++)
            {
                var offset = i * 3;
                var r = _rgbData[offset];
                var g = _rgbData[offset + 1];
                var b = _rgbData[offset + 2];
                
                // Convert RGB to luminance (grayscale)
                luminance[i] = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
            }
            return luminance;
        }
    }
}
