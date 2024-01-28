using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JoltXServer.Models;


// JSON serialiser / deserialser for candle data for transfer
// Binance GET request for historical kline data returns an 
// array of klines (candles), each kline is a mixed type array
// so we must use a serialiser to convert to a list of structs

[JsonConverter(typeof(CandleConverter))]
public struct Candle
{
    public long Time { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
};

// Received char interval as either 'h' hour or 'm' minute, and validates the time
// between each candle matches this interval
// returns true if time series has a consistent interval or is empty
public static class CandleTimeSeriesValidator
{
    public static bool Validate(List<Candle> candles)
    {
        int intervalInMilliSeconds = 60_000;
        long previousTime = candles[0].Time;

        for(int i = 1; i < candles.Count; i++)
        {
            if(previousTime + intervalInMilliSeconds != candles[i].Time)
                return false;

            previousTime = candles[i].Time;
        }

        return candles.Count >= 0;
    }
}

public class CandleConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(Candle));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JArray ja = JArray.Load(reader);
        Candle candle = new()
        {
            Time = (long)ja[0],
            Open = (decimal)ja[1],
            High = (decimal)ja[2],
            Low = (decimal)ja[3],
            Close = (decimal)ja[4],
            Volume = (decimal)ja[5]
        };

        return candle;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if(value == null) return;

        JArray ja = new();
        Candle candle = (Candle)value;
        ja.Add(candle.Time);
        ja.Add(candle.Open);
        ja.Add(candle.High);
        ja.Add(candle.Low);
        ja.Add(candle.Close);
        ja.Add(candle.Volume);
        ja.WriteTo(writer);
    }
}
