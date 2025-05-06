using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace Infrastructure.Services
{
    public interface ITelegramBotService
    {
        Task SendOrderNotificationAsync(Order order);
    }
    public class TelegramBotService : ITelegramBotService
    {

        private readonly TelegramBotClient _botClient;
        private readonly string _adminChatId;

        public TelegramBotService(IConfiguration config)
        {
            _botClient = new TelegramBotClient(config["Telegram:BotToken"]);
            _adminChatId = config["Telegram:AdminChatId"];
        }

        public async Task SendOrderNotificationAsync(Order order)
        {
            var itemsText = string.Join("\n", order.Items.Select(item =>
                $"🍕 {item.Pizza.Name} x{item.Quantity} — {item.PriceAtOrder * item.Quantity:F2}₴"));

            var message = $"🛒 Новый заказ №{order.Id}\n" +
                          $"👤 Клиент: {order.CustomerName}\n" +
                          $"📞 Телефон: {order.Phone}\n" +
                          $"🏠 Адрес: {order.Address}\n" +
                          $"🧾 Заказ:\n{itemsText}\n" +
                          $"💰 Итог: {order.TotalPrice}₴\n" +
                          $"🕒 Дата: {order.CreatedAt:dd.MM.yyyy HH:mm}";

            await _botClient.SendMessage(chatId: _adminChatId, text: message,

                    protectContent: true



                );
        }

    }
}
