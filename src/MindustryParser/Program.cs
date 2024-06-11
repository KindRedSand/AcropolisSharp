// using System.Diagnostics;
// using System.Globalization;
// using Playground.Mindustry;
// using Playground.Mindustry.Blocks;
// using Playground.Mindustry.Maps;
// using SixLabors.ImageSharp;
//
// //Why this isn't default?
// CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
//
//
// Blocks.Load();
// Items.Load();
//
// // byte[] header = "msch"u8.ToArray();
// //
// // using var str = File.OpenRead("lambda.msav");
// // var ctx = SaveIO.load(str);
// // ctx!.GetMapImage().SaveAsPng("map.png");
// // //
// // using var str2 = File.OpenRead("test.msav");
// // var ctx2 = SaveIO.load(str2);
// // ctx2!.GetMapImage().SaveAsPng("map2.png");
// //
// // using var str3 = File.OpenRead("oh_no.msav");
// // var ctx3 = SaveIO.load(str3);
// // ctx3!.GetMapImage().SaveAsPng("map3.png");
// // //Console.ReadKey();
// // return;
// //var filename = args.Length == 1 ? args[0] : "scheme.msch";
// //var filename = "itemChart.msch";
// //var filename = "bridgeChain.msch";
// //var filename = "conveyorMadnes.msch";
//
// //var filename = "conveyorTest.msch";
// //var filename = "ereConduitTest.msch";
// var filename = "generalTest.msch";
// //var filename = "shadowTest.msch";
// // var filename = "thorium_test.msch";
//
// //var filename = "3-way.msch";
// //var filename = "3-way-hor.msch";
// //var filename = "directions.msch";
// //var filename = "FlowDirections.msch";
// // var filename = "drills.msch";
// //
// using var _file = File.OpenRead(filename);
//
//
// var scheme = Schematics.Read(_file);
// foreach (var tag in scheme.tags)
// {
//     Console.WriteLine($"{tag.Key} => {tag.Value}");
// }
//
// Schematics.Write(scheme, File.OpenWrite("test.msch"));
//
// //Console.WriteLine();
// //Console.WriteLine();
// //Console.WriteLine(scheme);
//
// var img = SchematicDrawer.DrawSchemePreview(scheme);
//
// img.SaveAsPng("test.png");
// //
// // var base64 = "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";
// // //base64 = "bXNjaAF4nCXKsQ7CMAwE0EscKqGOfAdf026ogwkeItKkalIk/p4aD76nsw4DAiEUXgXXqXN8z9I6xpe0uKetp1oADJmfkhv8Y3G4bZlb55KO9R5r+ci37udmPA9ewxkapDjDG6SQjej/RrBlsHZRflpPFXc=";
// // base64 = "bXNjaAF4nG1T227TQBCdXV/i2LEd20kqJIT4gbykCL6EJ8SDG5tilNiRYxf6DTzBG+U3qYBAa+bskLZCtNqcnZ05cznepRmd2GTX+bYk72VddfunFysa7bsy31YF+avT1erZ6YvnpysKinK/bqtdVzU1Ebmb/Kzc7Em/ej2lpKu6vK767XLd1BflZdNSdN42fV0s3+TrrmkvaZq326YtiwcRu7f5vry3XSZ0ZUveu75emyonXdm1eVddlMu25Lh91/bIRtNdfrlp8gfJgrxq72ot8qKo/kPz+hosruHvmvdlu6yboqT4rK2K8weNPCo/7Jq6rLsq3/yT4fG233TVblOt/9dWuOs3MlHRVx2L9JEXRWT+YrIAU4FEIBXIBGYCc7IUaQoV7zUvJVwlXCVcJVwlXCVcxVytmWQIWghaCFoImiM1YA4w9Sz+V6NhGJ7wulI2m4ZvCd8C32FIhZHJ4UwsTuOSRzbpETkUEu8dmqJ1xSQbJM2QIdoWki1dOpLQEZ8jPkd8rvhc8bnic+Ebc4EQXY+kyRGadBgSAR7SZ8gcojFhN+Odq7Gb887RASeIPQcJhxtSw2G4JT0cuCuWj30JcbBHqQNbcxIuAzBajFHGZ4AbuvEeyp3w+swlAvSG03iEHvn0YNlsTmH5QvYNecwHgflliEk7DKZEIFHBXdQEvwFD7Knht7kSEyT0GRCjtYWqaGPG66uesKmO5pUOOdiMG3IiK+JiESYKWSrOERqpIFBopPLNGaSamN3CQXGLOSblNRpW0N5Hs5HcrUjuVsSfy1z4mcBcYCEjqr+i3w4DxCab0/1EJ+wzWWJksRkynMaSJZYsMbJgbNPFDY40BEs9vruc0DL/tsYDy+SdzQTmAgu5yIYe8/piwRwfzU/G9I9f7buFzxBwx4TrgWqMrBO+qEZocgz9YeE1m/kTeRuJlE6kdILSGI11AyhSeIpjjJqyihoWFGaYCIQCkUAsMIUoqSiVSr1U6qXy+FOpl8qomdyIg3ngMM3kv+7M8X3/MIOjeW3M+dH8ZszFvfcPBen5+A==";
// // base64 = "bXNjaAF4nGNgZmBmZmDJS8xNZeBOKs3MSSkpLSpKLWHgTkktTi7KLCjJzM9jYGBgy0lMSs0pZmCKjmWEKtQtyS9PLQLKMYIQkAAACCgSzw==";
// // base64 =
// //     "bXNjaAF4nB2OUY7CMAxEZykktN0Fae+xt+EP7UfauhApaVDiqNdngiI9O5rx2PjF0OG4uSgYbjVn0ZsUxbhImbN/qU8bABPcJKHgcP+/wkQX/GODdesa/KwwUxY3PzFO1YflT9MuGbbMTpVNt9QEU+aUabGLX9daBIbhryC0ySPKprBaKs/w6Eudgo9Om7i7HJkxrClLebol7TipV9eOugIHPnwRBh/w17GcgCNh8QGLIc4g+mY5s9DSs6E2YGzaiG/OEmPTfnBpmRcu4cAbnYk68A==";
// // base64 = "bXNjaAF4nCWKSw6AIAxERyDGz1m8iit3xgVqFyQIBEw8vWLRSdtM3wwkFI/TB6Ed/U5ponSiZ7NFE07jHYDa6pVsgpiXCl3wF8XBcZkjhaKmgsAv2YjPi3yXy5ksND8Ar/wYd8rP5MYLRogbcA==";
// //
// // var fidgetScheme = Schematics.Read(base64);
// // var fidgetImage = SchemeDrawer.DrawSchemePreview(fidgetScheme);
// //
// // fidgetImage.SaveAsPng("fidget.png");
//
