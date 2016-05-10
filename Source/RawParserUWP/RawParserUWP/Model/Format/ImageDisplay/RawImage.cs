﻿using RawParser.Model.Format;
using RawParser.Model.Format.Base;
using RawParser.Model.Format.Image.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawParser.Model.ImageDisplay
{

    class RawImage
    {
        private string fileName {get;set;}
        private Exif exif;
        protected Image imageData;
        protected Image imagePreviewData;
        public RawImage(Exif e ,Image d, Image p, string name)
        {
            exif = e;
            imageData = d;
            imagePreviewData = p;
            fileName = name;            
        }

        public void setImageData(Image i)
        {
            imageData = i;
        }

        public void setImagePreviewData(Image i)
        {
            imagePreviewData = i;
        }
    }
}