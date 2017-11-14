using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BatchThat.Image.Enums;
using BatchThat.Image.EventArguments;
using BatchThat.Image.Filters;
using ImageMagick;

namespace BatchThat.Image
{
    public class ImageManager
    {
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
                Message = new ChangedEventMessage($"\"File\",\"Frame Count\",\"Width\",\"Height\",\"Horizontal Resolution\",\"Vertical Resolution\"", EnumMessageType.Informational)
            });

            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 }, file =>
            {
                var frames = new List<string>();
                try
                {
                    using (var tiffImage = System.Drawing.Image.FromFile(file))
                    {
                        var pageCount = tiffImage.GetFrameCount(FrameDimension.Page);
                        
                        lock (Mutex)
                        {
                            processed++;
                        }
                        ProgressChanged(this, new ProgressChangedEventArgument
                        {
                            Current = processed,
                            Total = files.Count,
                            Message = new ChangedEventMessage($"\"{file}\",\"{pageCount}\",\"{tiffImage.Width}\",\"{tiffImage.Height}\",\"{tiffImage.HorizontalResolution}\",\"{tiffImage.VerticalResolution}\"", EnumMessageType.Informational)
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
    }
}
