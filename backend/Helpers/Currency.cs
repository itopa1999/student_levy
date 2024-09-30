public class Currency
{
    private decimal _value;

    public Currency(decimal value)
    {
        _value = value;
    }

    public decimal Value => _value;

    public string ToFormattedString(string format = "F2")
    {
        return _value.ToString(format);
    }

    public override string ToString()
    {
        return ToFormattedString();
    }
}
