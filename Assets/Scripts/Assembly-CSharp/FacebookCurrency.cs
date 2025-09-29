public class FacebookCurrency
{
	[JsonDeserialize("user_currency")]
	public string UserCurrency = string.Empty;

	[JsonDeserialize("currency_exchange")]
	public float CurrencyExchange;

	[JsonDeserialize("currency_exchange_inverse")]
	public float CurrencyExchangeInverse;

	[JsonDeserialize("usd_exchange")]
	public float UsdExchange;

	[JsonDeserialize("usd_exchange_inverse")]
	public float UsdExchangeInverse;

	[JsonDeserialize("currency_offset")]
	public float CurrencyOffset;
}
