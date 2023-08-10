


using Google.Protobuf;
using Grpc.Net.Client;
using grpcFileTransportServer;
using System.Net.Mime;
using System.Threading.Channels;

var chanel = GrpcChannel.ForAddress("http://localhost:5268");


var client = new FileService.FileServiceClient(chanel);

string file = @"C:\Users\ngaka\Desktop\gRPCClientExample\gRPCClientExample\File\Global Değişkenler.mp4";

//Stream yapılacak dosya belirleniyor.
using FileStream fileStream = new FileStream(file, FileMode.Open);

//Dosyanın tüm bilgileri ediniliyor. Bu nesne stream ile birlikte gönderilecektir.
var content = new BytesContent
{
    Filesize = fileStream.Length,
    ReadedByte = 0,
    Info = new grpcFileTransportServer.FileInfo { FileName = "Global Değişkenler", FileExtension = ".mp4" }
};

//Stream için server'da ki FileUpload fonksiyonu çağrılıyor.
var upload = client.FileUpload();

//Akışta ne kadar parça gideceği önceden ayarlanıyor. Burada 2048'lik bir alan tahsis edilmektedir. Gönderilecek dosyanın boyutu ne olursa olsun en fazla 2048'lik parça gönderilebileceğinden dolayı bu şekilde ayarlanmıştır.
byte[] buffer = new byte[2048];

//Her bir buffer, 0. byte'tan itibaren 2048 adet okunmakta ve sonuç 'content.ReadedByte'a atanmaktadır.
while ((content.ReadedByte = fileStream.Read(buffer, 0, buffer.Length)) > 0)
{
    //Okunan buffer'ın stream edilebilmesi için 'message.proto' dosyasındaki 'bytes' türüne dönüştürülüyor.
    content.Buffer = ByteString.CopyFrom(buffer);
    //'BytesContent' nesnesi stream olarak gönderiliyor.
    await upload.RequestStream.WriteAsync(content);
}
await upload.RequestStream.CompleteAsync();

fileStream.Close();