using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using MnistReader;

var mlContext = new MLContext();
var mnistTrain = await new MnistDownloader()
    .Download(Constants.TrainingLabelsLink, Constants.TrainingImagesLink)
    .Select(x => new InputData
    {
        PixelValues = x.Image.RawDataArray().Select(pixel => pixel / 255f).ToArray(),
        Number = x.Label,
    })
    .Where(x => x?.PixelValues?.Length == 28 * 28)
    .ToArrayAsync();
var mnistTest = await new MnistDownloader()
    .Download(Constants.TestingLabelsLink, Constants.TestingImagesLink)
    .Select(x => new InputData
    {
        PixelValues = x.Image.RawDataArray().Select(pixel => pixel / 255f).ToArray(),
        Number = x.Label,
    })
    .Where(x => x?.PixelValues?.Length == 28 * 28)
    .ToArrayAsync();

// Data load
var trainingData = mlContext.Data
    .LoadFromEnumerable(mnistTrain);

var testingData = mlContext.Data.LoadFromEnumerable(mnistTest);

// Some transformers

var dataProcessPipeline = mlContext.Transforms.Conversion
    .MapValueToKey("Label", nameof(InputData.Number), keyOrdinality: ValueToKeyMappingEstimator.KeyOrdinality.ByValue)
    .Append(mlContext.Transforms
        .Concatenate("Features", nameof(InputData.PixelValues)));

// Trainers
var trainer = mlContext.MulticlassClassification.Trainers.OneVersusAll(mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression());
var trainingPipeline = dataProcessPipeline.Append(trainer);

//Train

Console.WriteLine("=============== Training the model ===============");
ITransformer trainedModel = trainingPipeline.Fit(trainingData);

Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
var predictions = trainedModel.Transform(testingData);
var metrics = mlContext.MulticlassClassification.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");
Console.WriteLine("===Metrics===");
Common.ConsoleHelper.PrintMultiClassClassificationMetrics(trainer.ToString(), metrics);

mlContext.Model.Save(trainedModel, trainingData.Schema, "super-model.zip");
await using var file = File.OpenWrite("super-model.onnx");
mlContext.Model.ConvertToOnnx(trainedModel, testingData, file);
Console.WriteLine("The model is saved to {0}", "super-model.zip");

class InputData
{
    [VectorType(28 * 28)]
    public float[] PixelValues { get; set; }

    public float Number { get; set; }
}