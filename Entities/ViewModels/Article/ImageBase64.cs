using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Article
{
    public class ImageBase64
    {
        public string ImageData { get; set; }
        public string ImageExtension { get; set; }
    }
    public class VideoBase64
    {
        public string VideoData { get; set; }
        public string VideoExtension { get; set; }
    }
}
