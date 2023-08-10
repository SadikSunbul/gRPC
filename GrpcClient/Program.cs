

using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcMessageServer;
using GrpcServer;

var chanel = GrpcChannel.ForAddress("http://localhost:5136");//Buradaki gRPC ye baglan dedik
#region Greeter
/*
 var greeterClient = new Greeter.GreeterClient(chanel);//servisin adı Ahmet ıse ahamet yazmalıydık ılk basta bız protos a greeter adını vemıs tık 

HelloReply result = await greeterClient.SayHelloAsync(new HelloRequest()
{
    Name = "Sadık dan selamlar"
});
 */
#endregion
#region Message burası unary
//var messageClient = new Message.MessageClient(chanel);
//MessageRespons respons=await messageClient.SendMesageAsync(new MesageRequest() { Message="Merhaba ", Name="Sadık"});
#endregion
#region Message burası server stream

//var messageClient = new Message.MessageClient(chanel);

//var respons = messageClient.SendMesage(new MesageRequest { Message = "Selam", Name = "Sadık" });

////durdurmak ıstıye bılırız dıyerekten 
//CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

//while (await respons.ResponseStream.MoveNext(cancellationTokenSource.Token))//streamden gelecek her bır datayı yakalar
//{
//    Console.WriteLine(respons.ResponseStream.Current.Message);//verielre boyle ulasılır
//}

#endregion
#region Message burası clieant stream       
//var messageClient = new Message.MessageClient(chanel);

//var request = messageClient.SendMesage();

//for (int i = 0; i < 10; i++)
//{
//    await Task.Delay(1000);
//    await request.RequestStream.WriteAsync(new MesageRequest()
//    {
//        Message = "Mesaj" + i,
//        Name = "Sadık"
//    }); //burası bıze rısponsu dondurmez drekt 
//}
////burada neden boyle bır calısma yapılmıs cunku ılk once request bıtmesı gerek kı respons gelsın

//await request.RequestStream.CompleteAsync(); //gonderılcek verının bıttıgını soyluyoruz bana responsu gonder der

//Console.WriteLine((await request.ResponseAsync).Message); //gelecek verıyı boyle yakalıyoruz

#endregion


#region Bi directional streaming
var messageClient = new Message.MessageClient(chanel);

var request = messageClient.SendMesage();

//verı gonderıyoruz
var task1= Task.Run(async () =>
{
    for (int i = 0; i < 10; i++)
    {
        await Task.Delay(1000);
        await request.RequestStream.WriteAsync(new MesageRequest()
        {
            Name = "Sadık",
            Message = "mesaj" + i
        });
    }
});

CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

while (await request.ResponseStream.MoveNext(cancellationTokenSource.Token)) //gelecek verıyı alıcaz
{
    Console.WriteLine(request.ResponseStream.Current.Message);
}

await task1; //burada task 1 ı yanı requesti bekliyoruzawa
await request.RequestStream.CompleteAsync(); //burada request bıttı dedık 

#endregion



