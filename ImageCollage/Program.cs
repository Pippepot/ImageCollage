using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageCollage
{
	class Program
	{

        static void Main()
        {
            Console.Title = "ImageCollage";
            Console.WriteLine("Type 'tutorial' to get a helping hand");
            Log.Read();
        }

        /// <summary>
        /// Creates png files and meta files storing average RGB value
        /// </summary>
        static public void Convert()
        {
            // Declare the bitmap array which will be the converted pictures
            List<Bitmap> pictures = new List<Bitmap>();
            List<string> picturePaths = new List<string>();
            string[] filters = new string[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg" }; // Filter only image files
            foreach (var filter in filters)
            {
                picturePaths.AddRange(Directory.GetFiles(Properties.AppSettings.Default.ConversionPath, string.Format("*.{0}", filter), SearchOption.TopDirectoryOnly));
            }

            //Create every bitmap with the right size and a metafile to save computing time when creating the entire collage later
            int j = 0; // Used for whileloop if override is false
            for (int i = 0; i < picturePaths.Count; i++)
            {
                string fileName = Properties.AppSettings.Default.ConversionDestination + @"\conversion " + i + ".png";
                pictures.Add(new Bitmap(Image.FromFile(picturePaths[i]), new Size(Properties.AppSettings.Default.ConversionSize, Properties.AppSettings.Default.ConversionSize)));
                // If it should not override existing files, then check for valid filenames so the bitmap does not override
                if (!Properties.AppSettings.Default.Override)
                {
                    while (File.Exists(fileName))
                    {
                        j++;
                        fileName = Properties.AppSettings.Default.ConversionDestination + @"\conversion " + j + ".png";
                    }
                }
                using (MemoryStream memory = new MemoryStream())
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        pictures[i].Save(memory, ImageFormat.Jpeg);
                        byte[] bytes = memory.ToArray();
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }

                // Get the RGB values for the meta file
                int r = 0;
                int g = 0;
                int b = 0;

                for (int y = 0; y < Properties.AppSettings.Default.ConversionSize; y++)
                {
                    for (int x = 0; x < Properties.AppSettings.Default.ConversionSize; x++)
                    {
                        r += pictures[i].GetPixel(x, y).R * pictures[i].GetPixel(x, y).R;
                        g += pictures[i].GetPixel(x, y).G * pictures[i].GetPixel(x, y).G;
                        b += pictures[i].GetPixel(x, y).B * pictures[i].GetPixel(x, y).B;
                    }
                }
                int pixels = Properties.AppSettings.Default.ConversionSize * Properties.AppSettings.Default.ConversionSize;
                r = (int)Math.Sqrt(r / pixels);
                g = (int)Math.Sqrt(g / pixels);
                b = (int)Math.Sqrt(b / pixels);
                File.WriteAllLines(fileName + ".meta", new string[] { r.ToString(), g.ToString(), b.ToString() } );

                Console.WriteLine((int)((float)i * 100 / (float)(picturePaths.Count * Properties.AppSettings.Default.Sections)) + "%");
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
        }


        /// <summary>
        /// Creates a png file by combining convertet images into a collage
        /// </summary>
        static public void Create()
        {
            Console.WriteLine("Creating Image...");
            // Gets the converted images as a palette
            List<Bitmap> convertedImages = new List<Bitmap>();
            List<string> convertedImagesPath = new List<string>();
            convertedImagesPath.AddRange(Directory.GetFiles(Properties.AppSettings.Default.ConversionDestination, string.Format("*.{0}", "png"), SearchOption.TopDirectoryOnly));
            Color[] convetedImagesRGB = new Color[convertedImagesPath.Count];
            for (int i = 0; i < convertedImagesPath.Count; i++)
            {
                convertedImages.Add(new Bitmap(Image.FromFile(convertedImagesPath[i]))); // Get bitmapfiles
                string[] metaString = File.ReadAllLines(convertedImagesPath[i] + ".meta");
                convetedImagesRGB[i] = Color.FromArgb(int.Parse(metaString[0]), int.Parse(metaString[1]), int.Parse(metaString[2]));
            }

            // Declare the image which will be recreated
            Bitmap originalImage = new Bitmap(Image.FromFile(Properties.AppSettings.Default.ImagePath));
            int sectionsWide = (int)Math.Ceiling(Properties.AppSettings.Default.Sections * ((float)originalImage.Height / (float)originalImage.Width));

            // The final image which will be written to file as png
            Bitmap recreatedImage = new Bitmap(originalImage, Properties.AppSettings.Default.ConversionSize * Properties.AppSettings.Default.Sections, Properties.AppSettings.Default.ConversionSize * sectionsWide);

            // Lock the bitmap bits to memory
            Rectangle rect = new Rectangle(0, 0, recreatedImage.Width, recreatedImage.Height);
            BitmapData bmpData = recreatedImage.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // Get address of the first line in memory aka. Array[0]
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * recreatedImage.Height;
            byte[] rgbValues = new byte[bytes];
            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);


            // MAINPULATE THE IMAGE
            // One pixel in the 1-dimesional byte array is 3 bytes, therefore many values will be multiplied by 3.

            int row = 0;
            // For every section
            for (int section = 0; section < Properties.AppSettings.Default.Sections * sectionsWide; section++)
            {
                // If width is reached, jump to next row
                if (section != 0 && section % Properties.AppSettings.Default.Sections == 0)
                {
                    row += rect.Width * Properties.AppSettings.Default.ConversionSize * 3 - Properties.AppSettings.Default.Sections * Properties.AppSettings.Default.ConversionSize * 3;
                    Console.WriteLine((int)((float)section * 100 / (float)(sectionsWide * Properties.AppSettings.Default.Sections)) + "%");
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                }
                int pixel = section * Properties.AppSettings.Default.ConversionSize * 3 + row;

                // Get the RGB values for one section of the original image
                int r = 0;
                int g = 0;
                int b = 0;

                for (int y = 0; y < Properties.AppSettings.Default.ConversionSize; y++)
                {
                    for (int x = pixel; x < pixel + Properties.AppSettings.Default.ConversionSize * 3; x += 3) // *3 because each pixel has 3 values
                    {
                        r += rgbValues[2 + x] * rgbValues[2 + x];
                        g += rgbValues[1 + x] * rgbValues[1 + x];
                        b += rgbValues[x] * rgbValues[x];

                    }
                    pixel += rect.Width * 3;
                }

                pixel = section * Properties.AppSettings.Default.ConversionSize * 3 + row;

                r = (int)Math.Sqrt(r / (Properties.AppSettings.Default.ConversionSize * Properties.AppSettings.Default.ConversionSize));
                g = (int)Math.Sqrt(g / (Properties.AppSettings.Default.ConversionSize * Properties.AppSettings.Default.ConversionSize));
                b = (int)Math.Sqrt(b / (Properties.AppSettings.Default.ConversionSize * Properties.AppSettings.Default.ConversionSize));
                // Compare the average value of one section of the original image's average RGB value
                int[] distance = new int[convertedImages.Count];
                for (int i = 0; i < convertedImages.Count; i++) // Find distance
                {
                    //Do some math to make compare the colors as a human eye would compare colors
                    int multiplier = (r + convetedImagesRGB[i].R) / 2;
                    distance[i] = (int)Math.Sqrt((2 + multiplier / 256) * (r - convetedImagesRGB[i].R) * (r - convetedImagesRGB[i].R)
                        + 4 * (g - convetedImagesRGB[i].G) * (g - convetedImagesRGB[i].G)
                        + (2 + (255 - multiplier) / 256) * (b - convetedImagesRGB[i].B) * (b - convetedImagesRGB[i].B));
                }
                // Find the image with the closest RGB value
                Bitmap closestBmp = convertedImages[Array.IndexOf(distance, distance.Min())];

                // Write the closest image to a variable
                for (int y = 0; y < Properties.AppSettings.Default.ConversionSize; y++)
                {
                    int indexX = 0;
                    for (int x = pixel; x < pixel + Properties.AppSettings.Default.ConversionSize * 3; x += 3) // *3 because each pixel has 3 values
                    {
                        byte red = closestBmp.GetPixel(indexX, y).R;
                        byte green = closestBmp.GetPixel(indexX, y).G;
                        byte blue = closestBmp.GetPixel(indexX, y).B;
                        rgbValues[x + 2] = red;
                        rgbValues[x + 1] = green;
                        rgbValues[x] = blue;
                        indexX++;
                    }

                    pixel += rect.Width * 3;
                }
                // Continues with the next section until the recreated image is complete
            }

            // Copy the RGB values into memory, overriding the original image
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bytes from memory
            recreatedImage.UnlockBits(bmpData);

            // Save the recreated image to file
            int j = 0;
            string fileName = Properties.AppSettings.Default.ImageDestination + @"\Recreated.png";
            // Makes sure no file will be overwritten
            while (File.Exists(fileName))
            {
                j++;
                fileName = Properties.AppSettings.Default.ImageDestination + @"\Recreated " + j + ".png";
            }

            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    recreatedImage.Save(memory, ImageFormat.Png);
                    byte[] imageBytes = memory.ToArray();
                    fs.Write(imageBytes, 0, imageBytes.Length);
                }
            }
        }
    }
}

