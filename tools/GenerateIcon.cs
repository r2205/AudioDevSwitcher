using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

var outputPath = args.Length > 0 ? args[0] : "app.ico";
var sizes = new[] { 16, 24, 32, 48, 64, 256 };

using var ms = new MemoryStream();
using var writer = new BinaryWriter(ms);

// ICO header
writer.Write((short)0);       // reserved
writer.Write((short)1);       // type: icon
writer.Write((short)sizes.Length);

var imageDataList = new List<byte[]>();
foreach (var size in sizes)
{
    var pngBytes = RenderIcon(size);
    imageDataList.Add(pngBytes);
}

// Directory entries (offset computed after all headers)
int dataOffset = 6 + sizes.Length * 16;
for (int i = 0; i < sizes.Length; i++)
{
    var size = sizes[i];
    var data = imageDataList[i];
    writer.Write((byte)(size < 256 ? size : 0)); // width
    writer.Write((byte)(size < 256 ? size : 0)); // height
    writer.Write((byte)0);  // palette
    writer.Write((byte)0);  // reserved
    writer.Write((short)1); // color planes
    writer.Write((short)32); // bits per pixel
    writer.Write(data.Length);
    writer.Write(dataOffset);
    dataOffset += data.Length;
}

// Image data
foreach (var data in imageDataList)
    writer.Write(data);

File.WriteAllBytes(outputPath, ms.ToArray());
Console.WriteLine($"Generated {outputPath} with {sizes.Length} sizes.");

static byte[] RenderIcon(int size)
{
    using var bitmap = new Bitmap(size, size);
    using (var g = Graphics.FromImage(bitmap))
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.Clear(Color.Transparent);

        using var bgBrush = new SolidBrush(Color.FromArgb(0, 120, 212));
        g.FillEllipse(bgBrush, 0, 0, size - 1, size - 1);

        float fontSize = size * 0.5f;
        using var font = new Font("Segoe MDL2 Assets", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
        using var textBrush = new SolidBrush(Color.White);
        var sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
        };
        g.DrawString("\uE767", font, textBrush, new RectangleF(0, 0, size, size), sf);
    }

    using var ms = new MemoryStream();
    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
    return ms.ToArray();
}
