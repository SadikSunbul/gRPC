using Grpc.Net.Client;
using grpcFileTransportServer;

var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new FileService.FileServiceClient(channel);

//Dosyanın indirileceği dizin tanımlanıyor.
string downloadPath = @"C:\Users\ngaka\Desktop\gRPCClientExample\gRPCDownloadExample\DownloadFile";

//Server'dan talep edilen dosya bilgileri 'FileInfo' olarak tanımlanıyor.
var fileInfo = new grpcFileTransportServer.FileInfo
{
    FileExtension = ".mp4",
    FileName = "Global Değişkenler"
};

FileStream fileStream = null;

//Server'dan ilgili 'FileInfo' ile talep yapılıyor.
var request = client.FileDowland(fileInfo);

CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

int count = 0;
decimal chunkSize = 0;

//Talep neticesinde stream olarak gelen dosya parçaları okunmaya başlanıyor.
while (await request.ResponseStream.MoveNext(cancellationTokenSource.Token))
{
    //İlk gelen parçada transfer edilen dosyanın ana hatları belirleniyor.
    if (count++ == 0)
    {
        //Transfer edilen dosyanın server'dan gelen bilgiler eşliğinde belirtilen dizine depolanması için konfigürasyon gerçekleştiriliyor.
        fileStream = new FileStream(@$"{downloadPath}\{request.ResponseStream.Current.Info.FileName}{request.ResponseStream.Current.Info.FileExtension}", FileMode.CreateNew);

        //Depolanacak yerde dosya boyutu kadar alan tahsis ediliyor.
        fileStream.SetLength(request.ResponseStream.Current.Filesize);
    }

    //'message.proto' dosyasında belirtilen 'bytes' türüne karşılık olarak 'ByteString' türünde gelen buffer'lar byte dizisine dönüştürülüyor.
    var buffer = request.ResponseStream.Current.Buffer.ToByteArray();

    //İlgili FileStream'e parçalar yazdırılıyor.
    await fileStream.WriteAsync(buffer, 0, request.ResponseStream.Current.ReadedByte);

    Console.WriteLine($"{Math.Round(((chunkSize += request.ResponseStream.Current.ReadedByte) * 100) / request.ResponseStream.Current.Filesize)}%");
}
Console.WriteLine("Yüklendi...");

await fileStream.DisposeAsync();
fileStream.Close();