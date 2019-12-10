using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BatchThat.Image.Enums;
using BatchThat.Image.EventArguments;
using BatchThat.Image.Filters;
using ImageMagick;
using Encoder = System.Drawing.Imaging.Encoder;

namespace BatchThat.Image
{
    public class ImageManager
    {
        private readonly ImageFormat[] formatList =
        {
            ImageFormat.Gif,
            ImageFormat.Jpeg,
            ImageFormat.Png,
            ImageFormat.Tiff
        };
        private enum TiffCompressionTypes
        {
            Unspecified = 0,
            Uncompressed = 1,
            CCIT1D = 2,
            CCIT2D = 3,
            CCITT6 = 4,
            LZW = 5,
            JPEG = 7
        }
        public EventHandler<ProgressChangedEventArgument> ProgressChanged;
        private static readonly object Mutex = new object();

        public void ApplyFilters(IList<Filter> filters, IList<string> files, string destinationFolder, string sourceFolder)
        {
            var processed = 0;

            Parallel.ForEach(files, new ParallelOptions {MaxDegreeOfParallelism = Environment.ProcessorCount - 1}, file =>
            {
                var frames = new List<string>();
                try
                {
                    using (var tiffImage = System.Drawing.Image.FromFile(file))
                    {
                        var pageCount = tiffImage.GetFrameCount(FrameDimension.Page);
                        for (var j = 0; j < pageCount; j++)
                        {
                            tiffImage.SelectActiveFrame(FrameDimension.Page, j);
                            using (var bitmap = new Bitmap(tiffImage))
                            {
                                var image = new MagickImage(bitmap);

                                image = filters.Aggregate(image, (current, filter) => filter.ApplyFilter(current));
                                image.Density = new Density(tiffImage.HorizontalResolution,
                                    tiffImage.VerticalResolution);

                                var tempPath = Path.GetTempFileName();
                                frames.Add(tempPath);
                                image.Write(tempPath);
                                image.Dispose();
                            }
                        }
                        var relativePath = file.Replace(sourceFolder, "");
                        if (relativePath.StartsWith(@"\"))
                            relativePath = relativePath.Substring(1, relativePath.Length - 1);

                        var savePath = Path.Combine(destinationFolder, relativePath);
                        if (savePath.Contains(@"\"))
                        {
                            var directory = new DirectoryInfo(savePath.Substring(0, savePath.LastIndexOf(@"\", StringComparison.Ordinal)));
                            if (!directory.Exists)
                                directory.Create();
                        }
                        Save(savePath, frames, tiffImage.PixelFormat);
                        lock (Mutex)
                        {
                            processed++;
                        }
                        ProgressChanged(this, new ProgressChangedEventArgument
                        {
                            Current = processed,
                            Total = files.Count,
                            Message = new ChangedEventMessage($"Processed '{file}' ({savePath})", EnumMessageType.Informational)
                        });
                    }
                }
                catch (Exception ex)
                {
                    lock (Mutex)
                    {
                        processed++;
                    }
                    ProgressChanged(this, new ProgressChangedEventArgument
                    {
                        Current = processed,
                        Total = files.Count,
                        Message = new ChangedEventMessage($"Unable to process '{file}' ({ex.Message})", EnumMessageType.Error)
                    });
                }
                finally
                {
                    Clean(frames);
                }
            });
        }

        public void ApplyFilters(IList<Filter> filters, string file, string saveFile)
        {
            using (var tiffImage = System.Drawing.Image.FromFile(file))
            {
                var frames = new List<string>();
                var pageCount = tiffImage.GetFrameCount(FrameDimension.Page);
                var total = pageCount * filters.Count;
                for (var j = 0; j < pageCount; j++)
                {
                    tiffImage.SelectActiveFrame(FrameDimension.Page, j);
                    using (var bitmap = new Bitmap(tiffImage))
                    {
                        var image = new MagickImage(bitmap);
                        ProgressChanged(this, new ProgressChangedEventArgument
                        {
                            Current = (j * filters.Count),
                            Total = total,
                            Image = image.ToBitmap(),
                        });

                        for (var i = 0; i < filters.Count; i++)
                        {
                            image = filters[i].ApplyFilter(image);
                            ProgressChanged(this, new ProgressChangedEventArgument
                            {
                                Current = (j * filters.Count) + (i + 1),
                                Total = total,
                                Image = image.ToBitmap(),
                            });
                        }
                        var tempPath = Path.GetTempFileName();
                        frames.Add(tempPath);
                        image.Write(tempPath);
                        image.Dispose();
                    }
                }
                Save(saveFile, frames, tiffImage.PixelFormat);
                Clean(frames);
            }
        }

        public void Save(string output, List<string> frames, PixelFormat pixelFormat)
        {
            var codecInfo = ImageCodecInfo.GetImageEncoders().First(imageEncoder => imageEncoder.MimeType == "image/tiff");
            var parameters = new EncoderParameters
            {
                Param = new[]
                {
                    new EncoderParameter(Encoder.SaveFlag, (long) EncoderValue.MultiFrame),
                    new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW)
                }
            };
            var frameParameters = new EncoderParameters
            {
                Param = new[]
                {
                    new EncoderParameter(Encoder.SaveFlag, (long) EncoderValue.FrameDimensionPage),
                    new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW),
                    new EncoderParameter(Encoder.ColorDepth, (long)pixelFormat)
                }
            };

            using (var image = System.Drawing.Image.FromFile(frames[0]))
            {
                image.Save(output, codecInfo, parameters);
                for (var i = 1; i < frames.Count; i++)
                {
                    using (var frame = System.Drawing.Image.FromFile(frames[i]))
                    {
                        image.SaveAdd(frame, frameParameters);
                    }
                }
            }
        }

        public void Clean(List<string> frames)
        {
            foreach (var frame in frames)
            {
                File.Delete(frame);
            }
            frames.Clear();
        }

        public void GetFilesProperties(List<string> files, string exportPath)
        {
            var processed = 0;

            ProgressChanged(this, new ProgressChangedEventArgument
            {
                Current = processed,
                Total = files.Count,
                Message = new ChangedEventMessage($"\"File\",\"Frame Count\",\"Width\",\"Height\",\"Horizontal Resolution\",\"Vertical Resolution\",\"File Size\",\"File Format\",\"Compression Type\",\"File Creation Date\",\"Color Space\",\"Camera Model or Capture Hardware\"", EnumMessageType.Informational)
            });

            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 }, file =>
            {
                var frames = new List<string>();
                try
                {
                    using (var tiffImage = System.Drawing.Image.FromFile(file))
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        var pageCount = tiffImage.GetFrameCount(FrameDimension.Page);
                        var imageFormat = formatList.FirstOrDefault(f => f.Equals(tiffImage.RawFormat)).ToString();
                        string equipmentMake = tiffImage.PropertyItems.Any(p => p.Id == 271) ? System.Text.Encoding.Default.GetString(tiffImage.GetPropertyItem(271).Value).TrimEnd(new char['0']) : "";
                        string equipmentModel = tiffImage.PropertyItems.Any(p => p.Id == 272) ? System.Text.Encoding.Default.GetString(tiffImage.GetPropertyItem(272).Value).TrimEnd(new char['0']) : "";
                        TiffCompressionTypes compressionType = tiffImage.PropertyItems.Any(p => p.Id == 259) ? (TiffCompressionTypes)tiffImage.GetPropertyItem(259).Value[0] : TiffCompressionTypes.Unspecified;
                        string colorSpace = tiffImage.PropertyItems.Any(p => p.Id == 40961) ? GetColorspaceName(tiffImage.GetPropertyItem(40961).Value[0]) : "";
                        if (tiffImage.PropertyIdList.Any(p => p == 34675))
                        {
                            byte[] valueArray = tiffImage.GetPropertyItem(34675).Value;
                            AssignDescriptionFromICCProfile(valueArray, ref colorSpace);
                        }
                        string fileSizeString = GetHumanReadableFileSize(fileInfo.Length);
                        lock (Mutex)
                        {
                            processed++;
                        }
                        ProgressChanged(this, new ProgressChangedEventArgument
                        {
                            Current = processed,
                            Total = files.Count,
                            Message = new ChangedEventMessage($"\"{file}\",\"{pageCount}\",\"{tiffImage.Width}\",\"{tiffImage.Height}\",\"{tiffImage.HorizontalResolution}\",\"{tiffImage.VerticalResolution}\",\"{fileSizeString}\",\"{imageFormat}\",\"{compressionType}\",\"{fileInfo.CreationTime}\",\"{colorSpace}\",\"{equipmentMake}{(equipmentMake.Length > 0 ? " ": "")}{equipmentModel}\"", EnumMessageType.Informational)
                        });
                    }
                }
                catch (Exception)
                {
                    lock (Mutex)
                    {
                        processed++;
                    }
                    ProgressChanged(this, new ProgressChangedEventArgument
                    {
                        Current = processed,
                        Total = files.Count,
                        Message = new ChangedEventMessage($"\"{file}\",\"\",\"\",\"\",\"\",\"\"", EnumMessageType.Error)
                    });
                }
                finally
                {
                    Clean(frames);
                }
            });
        }

        public string GetHumanReadableFileSize(long length)
        {
            decimal scaledLength = length;
            string[] sizeSpecifiers = new[] {"bytes","KB","MB","GB","TB"};
            double order = Math.Floor(Math.Log(length, 1024));
            double number = (length / Math.Pow(1024, order));
            StringBuilder builder = new StringBuilder().Append(Math.Round(number, 1).ToString()).Append(" ").Append(sizeSpecifiers[(int)order]);
            return builder.ToString();
        }
        private string GetColorspaceName(int identifier)
        {
            string colorspaceName = "Uncalibrated";
            switch (identifier)
            {
                case 1:
                    colorspaceName = "sRGB";
                    break;
                case 2:
                    colorspaceName = "Adobe RGB";
                    break;
                default:
                    colorspaceName = "Uncalibrated";
                    break;
            }
            return colorspaceName;
        }

        public void AssignDescriptionFromICCProfile(byte[] profileByteArray, ref string profileDescription)
        {
            byte[] descriptionMarkerBytes = Encoding.ASCII.GetBytes("desc");
            int startIndex = 0;
            int tagIndex = -1;
            int charIndex = Array.IndexOf(profileByteArray, descriptionMarkerBytes[0], startIndex);
            while (charIndex != -1)
            {
                bool arrayMatch = true;
                for (int i = 0; i < descriptionMarkerBytes.Length; i++)
                {
                    if (profileByteArray[charIndex + i] != descriptionMarkerBytes[i])
                    {
                        arrayMatch = false;
                    }
                }
                if (arrayMatch == true && charIndex < profileByteArray.Length - (descriptionMarkerBytes.Length + 4))
                {
                    Array.Reverse(profileByteArray, charIndex + descriptionMarkerBytes.Length, 4);
                    tagIndex = BitConverter.ToInt32(profileByteArray, charIndex + descriptionMarkerBytes.Length);
                    break;
                }

            }
            if (tagIndex != -1)
            {
                Array.Reverse(profileByteArray, tagIndex + 8, 4);
                int descriptionLength = BitConverter.ToInt32(profileByteArray, tagIndex + 8);
                string description =
                    System.Text.Encoding.ASCII.GetString(profileByteArray, tagIndex + 12, descriptionLength - 1);
                profileDescription = description;
            }
        }
    }
}
