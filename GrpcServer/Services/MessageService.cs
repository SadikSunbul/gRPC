using Grpc.Core;
using GrpcMessageServer; //bu proto dosyas�nda yazd�g�m�z namespce k�sm� 


namespace GrpcServer.Services
{
    public class MessageService : Message.MessageBase
    {
        //public override async Task<MessageRespons> SendMesage(MesageRequest request, ServerCallContext context)
        //{
        //    Console.WriteLine($"Mesaj : {request.Message} | Name : {request.Name}");
        //    return new MessageRespons()
        //    {
        //        Message = "Messaj Ba�ar� ile al�nm��t�r ...."
        //    };
        //}

        #region Server stream
        //Server stream e gect�g�m�z �c�n yen�den over�de et

        //public async override Task SendMesage(MesageRequest request, IServerStreamWriter<MessageRespons> responseStream, ServerCallContext context)
        //{
        //    Console.WriteLine($"Mesaj : {request.Message} | Name : {request.Name}");
        //    for (int i = 0; i < 10; i++)
        //    {
        //        await Task.Delay(200);
        //        await responseStream.WriteAsync(new MessageRespons() { Message = "Merhaba" + i }); //ne kadar deger ver�rsem o kadar deger cl�eanta g�decekt�r
        //    }
        //}
        #endregion

        #region Client Stream

        //public async override Task<MessageRespons> SendMesage(IAsyncStreamReader<MesageRequest> requestStream, ServerCallContext context)
        //{
        //    // burada CancellationToken b�sz�m olusturmam�za gerek yok contexte var
        //    while (await requestStream.MoveNext(context.CancellationToken))
        //    {
        //        Console.WriteLine($"Mesaj : {requestStream.Current.Message} | Name : {requestStream.Current.Name}");
        //    }



        //    return new MessageRespons()
        //    {
        //        Message = "Veri al�nmi�t�r ..."
        //    };

        //}
        #endregion

        #region Bi directional streaming

        public override async Task SendMesage(IAsyncStreamReader<MesageRequest> requestStream, IServerStreamWriter<MessageRespons> responseStream, ServerCallContext context)
        {
            //Hem ver� gel�cek surekl� hemde ver� g�d�cek surekl� onun �c�n 2 farkl� task run olsutucaz bu �k�s� ayn� anda cal�s�cakalr
            var task1= Task.Run(async () =>
             {
                 while (await requestStream.MoveNext(context.CancellationToken))
                 {
                     Console.WriteLine($"Mesaj : {requestStream.Current.Message} | Name : {requestStream.Current.Name}");
                 }
             });

            //buray� task da yazamyta gerek yok drekt bu sayfan�n kend� task�nda devam ets�n bu
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                await responseStream.WriteAsync(new MessageRespons()
                {
                    Message = "mesaj" + i
                });
            }

            await task1; //yukar�dak�n� burada bekl�ycez
        }

        #endregion


    }
}