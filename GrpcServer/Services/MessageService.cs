using Grpc.Core;
using GrpcMessageServer; //bu proto dosyasýnda yazdýgýmýz namespce kýsmý 


namespace GrpcServer.Services
{
    public class MessageService : Message.MessageBase
    {
        //public override async Task<MessageRespons> SendMesage(MesageRequest request, ServerCallContext context)
        //{
        //    Console.WriteLine($"Mesaj : {request.Message} | Name : {request.Name}");
        //    return new MessageRespons()
        //    {
        //        Message = "Messaj Baþarý ile alýnmýþtýr ...."
        //    };
        //}

        #region Server stream
        //Server stream e gectýgýmýz ýcýn yenýden overýde et

        //public async override Task SendMesage(MesageRequest request, IServerStreamWriter<MessageRespons> responseStream, ServerCallContext context)
        //{
        //    Console.WriteLine($"Mesaj : {request.Message} | Name : {request.Name}");
        //    for (int i = 0; i < 10; i++)
        //    {
        //        await Task.Delay(200);
        //        await responseStream.WriteAsync(new MessageRespons() { Message = "Merhaba" + i }); //ne kadar deger verýrsem o kadar deger clýeanta gýdecektýr
        //    }
        //}
        #endregion

        #region Client Stream

        //public async override Task<MessageRespons> SendMesage(IAsyncStreamReader<MesageRequest> requestStream, ServerCallContext context)
        //{
        //    // burada CancellationToken býszým olusturmamýza gerek yok contexte var
        //    while (await requestStream.MoveNext(context.CancellationToken))
        //    {
        //        Console.WriteLine($"Mesaj : {requestStream.Current.Message} | Name : {requestStream.Current.Name}");
        //    }



        //    return new MessageRespons()
        //    {
        //        Message = "Veri alýnmiþtýr ..."
        //    };

        //}
        #endregion

        #region Bi directional streaming

        public override async Task SendMesage(IAsyncStreamReader<MesageRequest> requestStream, IServerStreamWriter<MessageRespons> responseStream, ServerCallContext context)
        {
            //Hem verý gelýcek sureklý hemde verý gýdýcek sureklý onun ýcýn 2 farklý task run olsutucaz bu ýkýsý ayný anda calýsýcakalr
            var task1= Task.Run(async () =>
             {
                 while (await requestStream.MoveNext(context.CancellationToken))
                 {
                     Console.WriteLine($"Mesaj : {requestStream.Current.Message} | Name : {requestStream.Current.Name}");
                 }
             });

            //burayý task da yazamyta gerek yok drekt bu sayfanýn kendý taskýnda devam etsýn bu
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                await responseStream.WriteAsync(new MessageRespons()
                {
                    Message = "mesaj" + i
                });
            }

            await task1; //yukarýdakýný burada beklýycez
        }

        #endregion


    }
}