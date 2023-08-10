using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using grpcFileTransportServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Infrastructure;
using System.Runtime.Serialization;

namespace GrpcServer.Services;

public class FileTransportService : FileService.FileServiceBase
{
    readonly IWebHostEnvironment webHostEnvironment; //bu uygulama ıcerısınde wwwroot a erısmemızı saglıycak path ı verıcek olan bır property nesne envarement ıle ılgılı tum bılgılerı getıren bır 

    public FileTransportService(IWebHostEnvironment webHostEnvironment)
    {
        this.webHostEnvironment = webHostEnvironment;
    }

    public override async Task<Empty> FileUpload(IAsyncStreamReader<BytesContent> requestStream, ServerCallContext context)
    {
        //Sreamen yapılacagı dızını belırlıyelım

        //Stream'in yapılacağı dizin belirleniyor.
        string path = Path.Combine(webHostEnvironment.WebRootPath, "files");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        //Dosyanın stream edileceği hedef FileStream.
        FileStream fileStream = null;

        try
        {
            int count = 0;

            //Yüzdelik hesaplaması için 'chunkSize' değişkeni tanımlanıyor.
            decimal chunkSize = 0;

            //Gelen stream okunmaktadır.
            while (await requestStream.MoveNext())
            {
                //Stream ilk başladığında(ilk adımda) yapılması gereken öncelikli işlevler gerçekleştiriliyor.
                if (count++ == 0)
                {
                    //Stream'de gelen Info nesnesinin FileName özelliğiyle hedef dosyanın adı belirleniyor.
                    fileStream = new FileStream($"{path}/{requestStream.Current.Info.FileName}{requestStream.Current.Info.FileExtension}", FileMode.CreateNew);

                    //Gelecek dosya boyutu kadar alan tahsis ediliyor. Bu işlem zorunlu değildir lakin süreçte farklı bir program tarafından diskin doldurulup, işimize engel olmasının önüne geçiyoruz.
                    fileStream.SetLength(requestStream.Current.Filesize);
                }

                //Buffer, akışta gelen her bir parçanın ta kendisidir. Chunk olarak isimlendirilir.
                var buffer = requestStream.Current.Buffer.ToByteArray();

                //Akışta gelen chunk'ları hedef FileStream nesnesine yazdırıyoruz. Burada, ikinci parametrede ki '0' değeri ile buffer'dan kaçıncı byte'dan itibaren okunacağı ve yazdırılacağı bildirilmektedir.
                await fileStream.WriteAsync(buffer, 0, requestStream.Current.ReadedByte);

                //Akışın yüzdelik olarak ne kadarının aktarıldığı hesaplanıyor.
                //Formülasyon olarak;
                //Okunan parça sayısı(ReadedByte), chunkSize değişkeninde toplanıyor ve 100 ile çarpılıp sonuç toplam boyuta bölünüyor. Nihai sonuç ise yakın olan tam sayıya yuvarlanıyor ve yüzdelik olarak ne kadarlık aktarım gerçekleştirildiği hesaplanmış oluyor.
                Console.WriteLine($"{Math.Round(((chunkSize += requestStream.Current.ReadedByte) * 100) / requestStream.Current.Filesize)}%");
            }
            Console.WriteLine("Yüklendi...");

        }
        catch (Exception ex)
        {
            //Client'ta stream 'CompleteAsync' edildiği vakit burada olası hata meydana gelebilmektedir. Dolayısıyla tüm bu süreci try catch ile kontrol ediyoruz.
        }
        //await fileStream.DisposeAsync();
        //fileStream.Close();
        return new Empty();
    }

    public override async Task FileDowland(grpcFileTransportServer.FileInfo request, IServerStreamWriter<BytesContent> responseStream, ServerCallContext context)
    {
        string path = Path.Combine(webHostEnvironment.WebRootPath, "files");

        //Client tarafından download edilmek istenen dosya bilgileri gönderilmiştir. Bu bilgilere karşılık olan dosya bulunmakta ve FileStream olarka işaretlenmektedir.
        using FileStream fileStream = new FileStream($"{path}/{request.FileName}{request.FileExtension}", FileMode.Open, FileAccess.Read);

        //Her bir akışta gönderilecek veri parçasını belirliyoruz.
        byte[] buffer = new byte[2048];

        //Gönderilecek dosyanın bilgilerini veriyoruz.
        BytesContent content = new BytesContent
        {
            Filesize = fileStream.Length,
            Info = new grpcFileTransportServer.FileInfo { FileName = "video", FileExtension = ".mp4" },
            ReadedByte = 0
        };

        //Her bir buffer, 0. byte'tan itibaren 2048 adet okunmakta ve sonuç 'content.ReadedByte'a atanmaktadır.
        while ((content.ReadedByte = fileStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            //Okunan buffer'ın stream edilebilmesi için 'message.proto' dosyasındaki 'bytes' türüne dönüştürülüyor.
            content.Buffer = ByteString.CopyFrom(buffer);
            //'BytesContent' nesnesi stream olarak gönderiliyor.
            await responseStream.WriteAsync(content);
        }

        fileStream.Close();
    }
}
