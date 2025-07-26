# Video Compressor with Trim, Scale & CRF Control

A simple yet powerful Windows desktop application built using WPF and C# for compressing videos using FFmpeg. Includes features like video preview, trimming, resolution scaling, CRF-based compression, and file size comparison.

---

## 🔧 Features

- 🎞️ **Video Preview** before compression
- ✂️ **Trim Video** using a range slider (select start & end)
- ⚖️ **Scale Resolution**  
  - `None` (no scaling)  
  - `3/4` (75% of original)  
  - `1/2` (50% of original)  
  - `1/4` (25% of original)
- 🔍 **Seek Slider** for isolated video seeking
- 🔄 **CRF Slider** for setting compression quality
- 📊 **Live Logging** of FFmpeg output and final results
- 📁 **Output File Size Comparison** (before & after)

---

## 📥 Installation Instructions

### 🔽 Download

Grab the latest **Setup EXE** from the [Releases page](https://github.com/mmaarij/FFMPEG-Compressor-UI/releases).

- ✅ Includes FFmpeg already

### ⚙️ Requirements

- **.NET Desktop Runtime 8.0 (x64)**
  Download from: [https://dotnet.microsoft.com/en-us/download/dotnet/8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
> Required only if the app doesn't launch after installation<br>
> Make sure to install the **Desktop App Runtime** (not the SDK or ASP.NET runtime)

---

## 🖼️ UI Overview

- **Seek Slider**: Scrub through the video without affecting trim selection
- **Range Slider**: Select start and end time for trimming
- **Video Preview**: Automatically updates when seeking or scrubbing
- **Compression Options**: Scale, CRF, Start & End time
- **Compress Button**: Runs FFmpeg with selected parameters

---

## 📦 Dependencies

- [.NET 8](https://dotnet.microsoft.com/en-us/)
- [FFmpeg](https://ffmpeg.org/) (bundled or placed in same folder as the executable)
- [Xceed.Wpf.Toolkit](https://github.com/xceedsoftware/wpftoolkit) for range slider control

---

## 🚀 Usage

1. **Launch the application**
2. Select the **input video file** and an **output folder**
3. Use the **seek slider** to preview video
4. Set **start** and **end** time with the **range slider**
5. Select **scale option** (`None`, `3/4`, `1/2`, `1/4`)
6. Adjust the **CRF slider** (lower = better quality, higher = smaller file)
7. Click **Compress**
8. See the output log and compare input/output file sizes

---

## 📄 Final Compression Log Includes

- ✅ Success message
- 📁 Input & Output file sizes in MB
- 🎯 CRF level
- 🖼️ Scale setting (or `None`)
- ⏱️ Start time, end time, and total trimmed video duration

---

## ⚠️ Notes

- The FFmpeg executable (`ffmpeg.exe`) **must be available in the same directory** as the app or in the system PATH.
- If `scale` is set to `None`, the original resolution will be preserved.
- CRF values typically range from 18 (high quality) to 28 (low quality).
- Audio settings are kept default.
