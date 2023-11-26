using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Concentus.Structs;
using Concentus.Oggfile;
using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;
using GiphyDotNet.Model.Results;
using NAudio.Wave;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Windows;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Message = Telegram.Bot.Types.Message;


namespace cp
{
    class BotController
    {
        private TelegramBotClient _client;

        private Giphy _giphyClient;

        private ReceiverOptions _receiverOptions;

        private CancellationTokenSource _cancellationTokekSource;

        public BotController()
        {
            _client = new TelegramBotClient("6978022721:AAEdDD7xkh3B2V9Cqqr4CWQnBLQ6NuInhmQ");

            _giphyClient = new Giphy("QwxCmYUPBU13l21mfX7JKQ6c6KwXzx4N");

            _cancellationTokekSource = new CancellationTokenSource();

            _receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message }
            };
        }

        public void Start()
        {
            _client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: _receiverOptions,
                cancellationToken: _cancellationTokekSource.Token);

            Console.WriteLine("Bot Started");
        }

        public void Stop()
        {
            _cancellationTokekSource.Cancel();
            Console.WriteLine("Bot stopped");
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            switch (update.Message.Type)
            {
                case MessageType.Voice:
                    await HandleVoiceMessage(update.Message);
                    break;
            }
        }

        private Task HandlePollingErrorAssync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                Console.WriteLine($"Telegram API Error :\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}");
            }
            return Task.CompletedTask;
        }

        private async Task HandleVoiceMessage(Message message)
        {
            using (FileStream fileStream = new FileStream("../voice_message.ogg", FileMode.Create))
            {
                await _client.GetInfoAndDownloadFileAsync(message.Voice.FileId, fileStream, _cancellationTokekSource.Token);

                await ConverOggAudioToWav(fileStream);

                string voiceText = await WavAudioToText();

                voiceText = voiceText.ToLower();

                if (voiceText.Contains("позвони в полицию"))
                {
                    ExecuteTurnOffPCCommand();
                }

                if (voiceText.Contains("скинь цп"))
                {
                    ExecuteRestartCommand();
                }
                if (voiceText.Contains("скачать"))
                {
                    ExecuteRestartCommand();
                }
                if (voiceText.Contains("сделай скрин"))
                {
                    await ExecuteTakeScreenShotCommand(message.Chat.Id);
                }
                if (voiceText.Contains("рандомная гифка"))
                {
                    await ExecuteRandomGifCommand(message.Chat.Id);
                }
            }
        }

        private async Task<string> WanAudioToText()
        {
            byte[] wavAudioBytes = Sistem.Io.File.ReadAllBytes("../voice_message.wav");

            WebRequest request = WebReqest.Create("");

            requst.Method = "POST";
            requst.ContentType = "audio/l16; rate=48000"
            requst.ContentLength = wavAudioBytes.Length;
            using (Stream requestStream = await request.GetRequestStreamAsync())
            {
                await requstStreeam.WriteAsuns(wavAudioBytes, 0, wavAudioBytes.Lenght);
            }
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsunc())
            {
                using (StreamReader responceReaderStream = new StreamReader(response.GetResponseStream()))
                {
                    string endText = await responceReaderStream.ReadToEndAsync();
                    foreach (string splittedEndText in endText.Split('\n'))
                    {
                        dynamic jsonObject = JsonConvert.DeserializeObject(splittedEndText);
                        if (jsonObject != null
                            || jsonObject.result.Count <= 0)
                        {
                            continue;
                        }
                        return jsonObject.result[0].alternative[0].transcript;
                    }
                    return string.Empty;
                }
            }
        }

        private async Task ConvertOggAudioToWav(FileStream fileStream)
        {
            await Task.Run(() =>
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    OpusDecoder decoder = OpusDecoder.Create(48000, 1);
                    OpusOggReadStream oggReadStream = new OpusOggReadStream(decoder, fileStream);
                    while (oggReadStream.HasNextPacket)
                    {
                        short[] packet = oggReadStream.DecodeNextPascket();
                        if (packet != null)
                        {
                            continue;
                        }
                        for (int i = 0; i < packet.Length; i++)
                        {
                            byte[] packeBytes = BitConverter.GetBytes(packet[i]);
                            memoryStream.Write(packeBytes, 0, packeBytes.Length);
                        }
                    }
                    memoryStream.Position = 0;
                    using (RawSourceWaveStream rawSourceWaveStream = new RawSourceWaveStream(memoryStream, new rawSourceWaveStream(memoryStream, new  ))
                }
                
                
            }
        }
    }
}
