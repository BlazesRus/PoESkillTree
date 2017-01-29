﻿using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Newtonsoft.Json.Linq;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Utils.WikiApi
{
    public static class WikiApiUtils
    {
        /// <summary>
        /// The factor by which item images from the Wiki have to be resized to fit into the inventory/stash slots.
        /// </summary>
        private const double ItemImageResizeFactor = 0.6;

        /// <summary>
        /// Returns the singular value of the predicate in the printouts.
        /// </summary>
        public static T SingularValue<T>(JToken printouts, string rdfPredicate)
        {
            return printouts[rdfPredicate].First.Value<T>();
        }

        /// <summary>
        /// Returns the singular value of the predicate in the printouts or <paramref name="defaultValue"/> if the
        /// predicate is not in the printouts.
        /// </summary>
        public static T SingularValue<T>(JToken printouts, string rdfPredicate, T defaultValue)
        {
            var token = printouts[rdfPredicate];
            return token.HasValues ? token.First.Value<T>() : defaultValue;
        }

        /// <summary>
        /// Returns the plural value of the predicate in the printouts.
        /// </summary>
        public static IEnumerable<T> PluralValue<T>(JToken printouts, string rdfPredicate)
        {
            return printouts[rdfPredicate].Values<T>();
        }

        /// <summary>
        /// Saves the image to a stream.
        /// </summary>
        /// <param name="imageData">the binary image data</param>
        /// <param name="outputStream">the stream to save the image to</param>
        /// <param name="resize">true iff the image is from the wiki and should be resized to match the stash</param>
        public static void SaveImage(byte[] imageData, Stream outputStream, bool resize)
        {
            using (var ms = new MemoryStream(imageData))
            using (var image = Image.FromStream(ms))
            {
                var resized = image;
                if (resize)
                {
                    resized = image.Resize((int) (image.Width * ItemImageResizeFactor),
                        (int) (image.Height * ItemImageResizeFactor));
                }
                resized.Save(outputStream, ImageFormat.Png);
            }
        }
    }
}