using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.ML;
using Microsoft.ML.Data;
using MnistReader;

Console.WriteLine("Initializing model");
var mlContext = new MLContext();
var transformer = mlContext.Model.Load("super-model.zip", out var schema);
var predictionEngine = mlContext.Model.CreatePredictionEngine<InputData, OutputData>(transformer);
Console.WriteLine("Enter path to image");
while (true)
{
    using var stream = System.IO.File.OpenRead(Console.ReadLine());
    var bitmap = new Bitmap(stream);
    var pixels = new List<float>(28 * 28);
    for (var y = 0; y < 28; y++)
    {
        for (var x = 0; x < 28; x++)
        {
            var pixel = bitmap.GetPixel(x, y);
            pixels.Add(1f - (pixel.R + pixel.G + pixel.B) / 3f / 255f);
        }
    }

    var myBitmap = new MyBitmap(28, 28, new Memory<byte>(pixels.Select(x => (byte)(x * 255)).ToArray()));
    Console.WriteLine(FormatToAsciiArt(myBitmap));
    var prediction = predictionEngine.Predict(new InputData
    {
        PixelValues = pixels.ToArray(),
    });

    Console.WriteLine(string.Join("\n",
        prediction.Score.OrderByDescending(x => x).Take(3).Select(x => (Array.IndexOf(prediction.Score, x), x))
            .Select(x => $"{x.Item1}={x.x:P}")));
}

static string FormatToAsciiArt(MyBitmap bitmap)
{
    var sb = new StringBuilder();
    for (var r = 0; r < bitmap.Rows; r++)
    {
        for (var c = 0; c < bitmap.Columns; c++)
        {
            var pixel = bitmap[r, c];
            sb.Append(pixel switch
            {
                0 => ' ',
                >0 and <200 => '░',
                _ => '█',
            });
        }

        sb.Append('\n');
    }

    return sb.ToString();
}

class InputData
{
    [VectorType(28 * 28)]
    public float[] PixelValues { get; set; }

    public float Number { get; set; }
}

record OutputData
{
    public float[] Score { get; set; }
    public uint PredictedLabel { get; set; }
}