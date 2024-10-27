using EffectiveMobileTest.Models;
using System.Globalization;
using System.Runtime.CompilerServices;

public class Program
{
    private static void Main(string[] args)
    {
        if(args.Length < 5)
        {
            Console.WriteLine("Ошибка, необходимо ввести 5 аргументов: cityDistrict, firstDeliveryDate, DataPath, _deliveryLog, _deliveryOrder");
            return;
        }
        string cityDistrict = args[0];
        string DataFilePath = args[2];
        string deliveryLog = args[3];
        string deliveryOrder = args[4];
        DateTime firstDeliveryDate;
        if (!DateTime.TryParseExact(args[1], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out firstDeliveryDate))
        {
            Console.WriteLine("Ошибка: Некорректный формат для firstDeliveryDate. Используйте формат: yyyy-MM-dd HH:mm:ss");
            Log(deliveryLog, "Ошибка в формате firstDeliveryDate");
            return;
        }
        
        DateTime endDeliveryDate;
        if(args.Length > 5)
        {
            if(!DateTime.TryParseExact(args[5], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDeliveryDate))
            {
                Console.WriteLine("Ошибка: Некорректный формат для endDeliveryDate. Используйте формат: yyyy-MM-dd HH:mm:ss");
                Log(deliveryLog, "Ошибка в формате endDeliveryDate");
                return;
            }
        }
        else
        {
            endDeliveryDate = DateTime.Now;
            Log(deliveryLog, "Для endDeliveryDate, будет установлена дата запуска приложения");
        }
        Log(deliveryLog, $"Запуск приложения. Фильтр: район - {cityDistrict}, начальная дата - {firstDeliveryDate}, конечная дата - {endDeliveryDate}");
        try
        {
            var orders = ParseDate(DataFilePath, deliveryLog);

            Filter(orders, cityDistrict, deliveryOrder, firstDeliveryDate, endDeliveryDate);
            Log(deliveryLog, "Фильтрация завершена успешно. Результат сохранен.");
        }
        catch (Exception ex)
        {
            Log(deliveryLog, $"Ошибка: {ex.Message}");
        }
    }

    static List<Order> ParseDate(string filepath, string deliveryLog)
    {
        var orders = new List<Order>();

        foreach (var line in File.ReadLines(filepath))
        {
            var parts = line.Split(';');

           
            if (parts.Length < 4)
            {
                Log(deliveryLog, $"Ошибка: строка {line} не содержит ожидаемого количества значений.");
                continue;
            }

            
            if (!int.TryParse(parts[0].Trim(' ', '"'), out int orderId))
            {
                Log(deliveryLog, $"Ошибка: '{parts[0]}' не является корректным идентификатором заказа.");
                continue;
            }

            if (!double.TryParse(parts[1], out double weight))
            {
                Log(deliveryLog, $"Ошибка: '{parts[1]}' не является корректным значением веса.");
                continue;
            }

            string district = parts[2];

            if (!DateTime.TryParseExact(parts[3].Trim(' ', '"'), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime deliveryDateTime))
            {
                Log(deliveryLog, $"Ошибка: '{parts[3]}' не соответствует формату даты 'yyyy-MM-dd HH:mm:ss'.");
                continue;
            }

            
            orders.Add(new Order
            {
                OrderId = orderId,
                Weight = weight,
                District = district,
                DeliveryTime = deliveryDateTime
            });
            Log(deliveryLog, $"Заказ {orderId}, {weight}, {district}, {deliveryDateTime} успешно добавлен");
        }

        return orders;
    }
    static void Filter(List<Order> orders, string cityDistrict, string deliveryOrder, DateTime firstDeliveryDate, DateTime endDeliveryDate)
    {

        var filterorders = (from o in orders
                           where o.District == cityDistrict && o.DeliveryTime >= firstDeliveryDate && o.DeliveryTime <= endDeliveryDate
                           select o).ToList();
        using(var order = new StreamWriter(deliveryOrder))
        {
            foreach (var orderdata in filterorders)
            {
                order.WriteLine($"ID: {orderdata.OrderId}, Вес: {orderdata.Weight}, Время доставки: {orderdata.DeliveryTime}");
            }
            order.WriteLine($"Количество заказов для {cityDistrict}: {filterorders.Count}");
        }
        
    }
    static void Log(string deliveryLog, string message)
    {
        using(var log = new StreamWriter(deliveryLog, true))
        {
            log.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }
    }
}