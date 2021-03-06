﻿using System;

namespace RawNet
{
    class TiffDecoder : RawDecoder
    {
        protected IFD ifd;

        public TiffDecoder(IFD rootifd, ref TIFFBinaryReader file) : base(ref file)
        {
            decoderVersion = 1;
            ifd = rootifd;
            //check if no 
        }

        protected override RawImage decodeRawInternal()
        {
            if (!ifd.tags.TryGetValue((TagType)0x0106, out var photoMetricTag)) throw new FormatException("File not correct");
            if (!ifd.tags.TryGetValue((TagType)0x0111, out var imageOffsetTag)) throw new FormatException("File not correct");
            if (!ifd.tags.TryGetValue((TagType)0x0100, out var imageWidthTag)) throw new FormatException("File not correct");
            if (!ifd.tags.TryGetValue((TagType)0x0101, out var imageHeightTag)) throw new FormatException("File not correct");
            if (!ifd.tags.TryGetValue((TagType)0x0103, out var imageCompressedTag)) throw new FormatException("File not correct");
            if (!ifd.tags.TryGetValue((TagType)0x0116, out var rowPerStripTag)) throw new FormatException("File not correct");
            if (!ifd.tags.TryGetValue((TagType)0x0117, out var stripSizeTag)) throw new FormatException("File not correct");

            if ((ushort)photoMetricTag.data[0] == 2)
            {
                if (!ifd.tags.TryGetValue((TagType)0x0102, out var bitPerSampleTag)) throw new FormatException("File not correct");
                if (!ifd.tags.TryGetValue((TagType)0x0115, out var samplesPerPixel)) throw new FormatException("File not correct");
                uint height = Convert.ToUInt32(imageHeightTag.data[0]);
                uint width = Convert.ToUInt32(imageWidthTag.data[0]);
                //suppose that image are always 8,8,8 or 16,16,16
                ushort colorDepth = (ushort)bitPerSampleTag.data[0];
                ushort[] image = new ushort[width * height * 3];
                long strips = height / (ushort)rowPerStripTag.data[0], lastStrip = height % (ushort)rowPerStripTag.data[0];
                long rowperstrip = Convert.ToInt64(rowPerStripTag.data[0]);
                if ((ushort)imageCompressedTag.data[0] == 1)
                {
                    //not compressed
                    for (int i = 0; i < strips + ((lastStrip == 0) ? 0 : 1); i++)
                    {
                        //for each complete strip
                        //move to the offset
                        mFile.Position = Convert.ToInt64(imageOffsetTag.data[i]);
                        for (int y = 0; y < rowperstrip && !(i == strips && y <= lastStrip); y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                //get the pixel
                                //red
                                image[(y + i * rowperstrip) * width * 3 + x * 3] = mFile.ReadByte();
                                //green
                                image[(y + i * rowperstrip) * width * 3 + x * 3 + 1] = mFile.ReadByte();
                                //blue 
                                image[(y + i * rowperstrip) * width * 3 + x * 3 + 2] = mFile.ReadByte();
                                for (int z = 0; z < (Convert.ToInt32(samplesPerPixel.data[0]) - 3); z++)
                                {
                                    //pass the other pixel if more light
                                    mFile.ReadByte();
                                }
                            }
                        }
                    }
                }
                else if ((ushort)imageCompressedTag.data[0] == 32773)
                {
                    //compressed
                    /*Loop until you get the number of unpacked bytes you are expecting:
                    Read the next source byte into n.
                    If n is between 0 and 127 inclusive, copy the next n+1 bytes literally.
                    Else if n is between - 127 and - 1 inclusive, copy the next byte -n + 1
                    times.
                    Else if n is - 128, noop.
                    Endloop
                    */
                    //not compressed
                    for (int i = 0; i < strips + ((lastStrip == 0) ? 0 : 1); i++)
                    {
                        //for each complete strip
                        //move to the offset
                        mFile.Position = Convert.ToInt64(imageOffsetTag.data[i]);
                        for (int y = 0; y < rowperstrip && !(i == strips && y < lastStrip); y++)
                        {
                            //uncompress line by line of pixel
                            ushort[] temp = new ushort[3 * width];
                            short buffer = 0;
                            int count = 0;
                            for (int x = 0; x < width * 3;)
                            {
                                buffer = mFile.ReadByte();
                                count = 0;
                                if (buffer >= 0)
                                {
                                    for (int k = 0; k < count; ++k, ++x)
                                    {
                                        temp[x] = mFile.ReadByte();
                                    }
                                }
                                else
                                {
                                    count = -buffer;
                                    buffer = mFile.ReadByte();
                                    for (int k = 0; k < count; ++k, ++x)
                                    {
                                        temp[x] = (ushort)buffer;
                                    }
                                }
                            }

                            for (int x = 0; x < width * 3; x++)
                            {

                                //red
                                image[(y + i * rowperstrip) * width * 3 + x * 3] = temp[x * 3];
                                //green
                                image[(y + i * rowperstrip) * width + x * 3 + 1] = temp[x * 3 + 1];
                                //blue 
                                image[(y + i * rowperstrip) * width + x * 3 + 2] = temp[x * 3 + 2];
                                for (int z = 0; z < ((int)samplesPerPixel.data[0] - 3); z++)
                                {
                                    //pass the other pixel if more light
                                    mFile.ReadByte();
                                }
                            }
                        }
                    }
                }
                else throw new FormatException("Compression mode " + imageCompressedTag.dataAsString + " not supported yet");
                mRaw.rawData = image;
                return mRaw;
            }
            else throw new FormatException("Photometric interpretation " + photoMetricTag.dataAsString + " not supported yet");
        }

        protected override void decodeMetaDataInternal(CameraMetaData meta)
        {
            throw new NotImplementedException();
        }

        protected override void checkSupportInternal(CameraMetaData meta)
        {
            throw new NotImplementedException();
        }
    }
}
