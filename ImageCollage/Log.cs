using System;
using System.IO;

namespace ImageCollage
{
    static class Log
    {
        static string tutorialStep;

        static public void Read()
        {
            // Check for viable commands
            string input;
            input = Console.ReadLine();

            if (input.ToLower() == "tutorial")
            {
                tutorialStep = "conversion-path";
                Console.WriteLine("\n|TUTORIAL|");
                Console.WriteLine("Welcome to the tutorial!\n");
                Console.WriteLine("We will create an image out of many smaller images\n");
                Console.WriteLine("First we need to prepare the images that make up the bigger image.");
                Console.WriteLine("To do this, we will make copies of the images in a smaller size and buddy them up with a meta file\n");
                Console.WriteLine("First type 'conversion-path' followed by the path to a folder with many images, which you would like to create your collage out of.");
                
                Read();
                return;
            }

            if (input.ToLower() == "help") // help
            {
                Console.WriteLine("Further explanation\n");
                Console.WriteLine("Command name                 Command                         Summary\n");
                Console.WriteLine("View                         view                            View all saved settings.\n");
                Console.WriteLine("ConversionPath               conversion-path  'path'         Sets the path of a folder, where a copy of each picture will be convertet.\n");
                Console.WriteLine("ConversionDestination        conversion-destination  'path'  Sets the path of a folder, where the convertet pictures will be saved to.\n");
                Console.WriteLine("ImagePath                    image-path  'file'              Sets the path of a file, which will be recreated by convertet pictures.\n");
                Console.WriteLine("ImageDestination             image-destination  'path'       Sets the path of a folder, where the final image will be saved to.\n");
                Console.WriteLine("ConvertedSize                size 'number'                   Sets the size of converted images in pixels.Default is 20. If this is changed and images are not reconverted images will be cut off.\n");
                Console.WriteLine("ImageSections                sections 'number'               Sets the width of recreated images in sections.Default is 80.\n");
                Console.WriteLine("Override                     override 'true/false'           Overrides any converted image with the same name. Uses conversion path and conversionDestination.\n");
                Console.WriteLine("Convert                      convert                         Converts image files from conversionPath into smaller collage pieces with meta files and places them in conversionDestination.\n");
                Console.WriteLine("Create                       create                          Creates a png file in image path by combining convertet images from conversionDestination into a collage located in image destination. Override will not be taken into account. This command will never override existing images.\n");

                Read();
                return;
            }

            if (input.ToLower() == "view") // view
            {
                Console.WriteLine("Conversion path: " + Properties.AppSettings.Default.ConversionPath);
                Console.WriteLine("Conversion destination: " + Properties.AppSettings.Default.ConversionDestination);
                Console.WriteLine("Image path: " + Properties.AppSettings.Default.ImagePath);
                Console.WriteLine("Image destination: " + Properties.AppSettings.Default.ImageDestination);
                Console.WriteLine();
                Console.WriteLine("Conversion size: " + Properties.AppSettings.Default.ConversionSize);
                Console.WriteLine("Image sections: " + Properties.AppSettings.Default.Sections);
                Console.WriteLine("Override: " + Properties.AppSettings.Default.Override);
                Read();
                return;
            }

            if (input.ToLower().StartsWith("conversion-path")) // SetConversionPath
            {
                string path = input.Substring(("conversion-path").Length);

                // Checks for existance and writes into the settings file
                if (Directory.Exists(path))
                {
                    Properties.AppSettings.Default.ConversionPath = path;
                    Properties.AppSettings.Default.Save();
                    LogSucces("Conversion path set");

                    if (tutorialStep == "conversion-path")
                    {
                        Console.WriteLine("\n|TUTORIAL|");
                        Console.WriteLine("Perfect!");
                        Console.WriteLine("Next type 'conversion-destination' followed by the path to another folder where you would like these new images to be located");
                        tutorialStep = "conversion-destination";
                    }

                    Read();
                }
                else { LogError("Path does not exist"); }
                return;
            }


            if (input.ToLower().StartsWith("conversion-destination")) // SetConversionDestination
            {
                string path = input.Substring(("conversion-destination").Length);

                // Checks for existance and writes into the settings file
                if (Directory.Exists(path))
                {
                    Properties.AppSettings.Default.ConversionDestination = path;
                    Properties.AppSettings.Default.Save();
                    LogSucces("Conversion destination set");

                    if (tutorialStep == "conversion-destination")
                    {
                        Console.WriteLine("\n|TUTORIAL|");
                        Console.WriteLine("Nice!");
                        Console.WriteLine("Now we need to convert the images to our prefered size by typing 'convert'");
                        Console.WriteLine("Note that you can change the size of these images by typing 'size' followed by an integer number");
                        Console.WriteLine("To view what the size currently is, type 'view'");
                        tutorialStep = "convert";
                    }

                    Read();
                }
                else { LogError("Path does not exist"); }
                return;
            }


            if (input.ToLower().StartsWith("image-path")) // SetImagePath
            {
                // Gets the path after the command ignoring the first space key
                string path = input.Substring(("image-path").Length);

                // Checks for existance and writes into the settings file
                if (File.Exists(path))
                {
                    Properties.AppSettings.Default.ImagePath = path;
                    Properties.AppSettings.Default.Save();
                    LogSucces("Image path set");
                    if (tutorialStep == "image-path")
                    {
                        Console.WriteLine("\n|TUTORIAL|");
                        Console.WriteLine("Cool!");
                        Console.WriteLine("Then type 'image-destination' followed by the path the folder where the collage will be placed");
                        tutorialStep = "image-destination";
                    }
                    Read();
                }
                else { LogError("File does not exist"); }
                return;
            }


            if (input.ToLower().StartsWith("image-destination")) // SetImageDestination
            {
                // Gets the path after the command ignoring the first space key
                string path = input.Substring(("image-destination").Length);

                // Checks for existance and writes into the settings file
                if (Directory.Exists(path))
                {
                    Properties.AppSettings.Default.ImageDestination = path;
                    Properties.AppSettings.Default.Save();
                    LogSucces("Image destination set");
                    if (tutorialStep == "image-destination")
                    {
                        Console.WriteLine("\n|TUTORIAL|");
                        Console.WriteLine("This is the final step!");
                        Console.WriteLine("Type 'create' to make magic happen");
                        Console.WriteLine("Note you can type 'sections' to change how many converted images make one horizontal line on the collage");
                        Console.WriteLine("To view how many sections there currently are, type 'view'");
                        tutorialStep = "create";
                    }
                    Read();
                }
                else { LogError("Path does not exist"); }
                return;
            }


            if (input.ToLower().StartsWith("size")) // SetConvertionsize
            {
                string size = input.Substring(("size").Length);

                // Writes into the settings file
                if (int.TryParse(size, out int parse))
                {
                    if (parse < 1)
                    {
                        LogError("Interger must be greater than 0");
                        return;
                    }

                    Properties.AppSettings.Default.ConversionSize = parse;
                    LogSucces("Size set");
                    Read();
                }
                else { LogError("Missing integer argument"); }
                return;
            }


            if (input.ToLower().StartsWith("sections")) // Sections
            {
                string sectionCount = input.Substring(("sections").Length);

                // Writes into the settings file
                if (int.TryParse(sectionCount.ToLower(), out int parse))
                {
                    if (parse < 1)
                    {
                        LogError("Interger must be greater than 0");
                        return;
                    }

                    Properties.AppSettings.Default.Sections = parse;
                    Properties.AppSettings.Default.Save();
                    LogSucces("Sections set");
                    Read();
                }
                else { LogError("Missing integer argument"); }
                return;
            }


            if (input.ToLower().StartsWith("override")) // Override
            {
                string shouldOverride = input.Substring(("override").Length);

                // Writes into the settings file
                if (bool.TryParse(shouldOverride.ToLower(), out bool parse))
                {
                    Properties.AppSettings.Default.Override = parse;
                    Properties.AppSettings.Default.Save();
                    LogSucces("Override set");
                    Read();
                }
                else { LogError("Missing boolean argument"); }
                return;
            }


            if (input.ToLower() == "convert") // convert
            {
                if (!Directory.Exists(Properties.AppSettings.Default.ConversionPath))
                {
                    LogError("Unable to find conversion path");
                    return;
                }

                if (!Directory.Exists(Properties.AppSettings.Default.ConversionDestination))
                {
                    LogError("Unable to find conversion destination");
                    return;
                }

                Program.Convert();
                LogSucces("Images converted");

                if (tutorialStep == "convert")
                {
                    Console.WriteLine("\n|TUTORIAL|");
                    Console.WriteLine("You should now see the converted images in your specified path");
                    Console.WriteLine("Now we move on the creating the bigger picture\n");
                    Console.WriteLine("Type 'image-path' followed by the location of the image file that the smaller images will make up");
                    tutorialStep = "image-path";
                }

                Read();
                return;
            }


            if (input.ToLower() == "create") // create
            {
                if (!Directory.Exists(Properties.AppSettings.Default.ConversionDestination))
                {
                    LogError("Unable to find conversion destination");
                    return;
                }

                if (!File.Exists(Properties.AppSettings.Default.ImagePath))
                {
                    LogError("Unable to find image file");
                    return;
                }

                if (!Directory.Exists(Properties.AppSettings.Default.ImageDestination))
                {
                    LogError("Unable to find image destination");
                    return;
                }


                string result = Program.Create();

                if (result != "")
                    LogError(result);

                LogSucces("Image created");
                if (tutorialStep == "create")
                {
                    Console.WriteLine("\n|TUTORIAL|");
                    Console.WriteLine("Awesome!");
                    Console.WriteLine("You are all done now!");
                    Console.WriteLine("Go see you image collage");
                    Console.WriteLine("If you want to know more type 'help'");
                    tutorialStep = "";
                }
                Read();
                return;
            }

            // If input did not match any commands
            LogError("Unable to recognize '" + input + "'");
            return;
        }

        static void LogSucces(string succes)
        {
            Console.WriteLine("[SUCCES] " + succes);
        }

        static void LogError(string error)
        {
            Console.WriteLine("[ERROR] " + error);
            Read();
        }
    }
}
